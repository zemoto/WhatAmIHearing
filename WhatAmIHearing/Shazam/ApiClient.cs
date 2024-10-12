using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WhatAmIHearing.Shazam;

internal sealed class ApiClient : IDisposable
{
   private readonly ApiViewModel _apiVm;
   private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;
   private readonly HttpClient _client = new();

   private readonly List<CancellationTokenSource> _cancelTokenSources = [];
   private readonly object _cancelTokenLock = new();

   public ApiClient( ApiViewModel apiVm )
   {
      _apiVm = apiVm;

      var retryStrategy = new RetryStrategyOptions<HttpResponseMessage>
      {
         Delay = TimeSpan.FromSeconds( 3 ),
         BackoffType = DelayBackoffType.Constant,
         MaxRetryAttempts = 2,
         ShouldHandle = new PredicateBuilder<HttpResponseMessage>().HandleResult( x => x.StatusCode == HttpStatusCode.TooManyRequests ),
      };

      _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry( retryStrategy ).Build();
   }

   public void Dispose()
   {
      CancelRequests();
      _client.Dispose();
   }

   public void CancelRequests()
   {
      lock ( _cancelTokenLock )
      {
         _cancelTokenSources.ForEach( x => x.Cancel() );
      }
   }

   public HttpStatusCode LastStatusCode { get; private set; }

   public async Task<string> SendPostRequestAsync( string endpoint, byte[] data )
   {
      StringContent contentBuilder()
      {
         return new( Convert.ToBase64String( data ) );
      }

      HttpRequestMessage messageBuilder()
      {
         var message = new HttpRequestMessage( HttpMethod.Post, endpoint ) { Content = contentBuilder() };
         message.Headers.Add( "x-rapidapi-host", "shazam.p.rapidapi.com" );
         message.Headers.Add( "x-rapidapi-key", _apiVm.UseDefaultKey ? ApiViewModel.DefaultShazamApiKey : _apiVm.ShazamApiKey );
         return message;
      }

      return await SendMessageAsync( messageBuilder );
   }

   private async Task<string> SendMessageAsync( Func<HttpRequestMessage> messageBuilder )
   {
      CancellationTokenSource cancelTokenSource;
      lock ( _cancelTokenLock )
      {
         cancelTokenSource = new CancellationTokenSource();
         _cancelTokenSources.Add( cancelTokenSource );
      }

      try
      {
         var response = await _pipeline.ExecuteAsync( async token =>
         {
            using var message = messageBuilder();
            return await _client.SendAsync( message, token );
         }, cancelTokenSource.Token );

         LastStatusCode = response.StatusCode;

         if ( response.IsSuccessStatusCode )
         {
            UpdateRateLimitValues( response.Headers );
            return await response.Content.ReadAsStringAsync();
         }

         return string.Empty;
      }
      finally
      {
         lock ( _cancelTokenLock )
         {
            cancelTokenSource.Dispose();
            _ = _cancelTokenSources.Remove( cancelTokenSource );
         }
      }
   }

   private void UpdateRateLimitValues( System.Net.Http.Headers.HttpResponseHeaders headers )
   {
      if ( headers.TryGetValues( "X-RateLimit-Requests-Limit", out var limitValues ) && limitValues?.Any() == true && int.TryParse( limitValues.First(), out int quotaLimit ) )
      {
         _apiVm.QuotaLimit = quotaLimit;
      }

      if ( headers.TryGetValues( "X-RateLimit-Requests-Remaining", out var remainingValues ) && remainingValues?.Any() == true && int.TryParse( remainingValues.First(), out int quotaRemaining ) )
      {
         _apiVm.QuotaRemaining = quotaRemaining;
      }
   }
}
