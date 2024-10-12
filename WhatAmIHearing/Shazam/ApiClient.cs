using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WhatAmIHearing.Shazam;

internal sealed class ApiClient : IDisposable
{
   private readonly ApiSettings _settings;
   private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;
   private readonly HttpClient _client = new();

   private readonly List<CancellationTokenSource> _cancelTokenSources = [];
   private readonly object _cancelTokenLock = new();

   public ApiClient( ApiSettings settings )
   {
      _settings = settings;

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
         message.Headers.Add( "x-rapidapi-key", _settings.UseDefaultKey ? ApiSettings.DefaultShazamApiKey : _settings.ShazamApiKey );
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

         return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : string.Empty;
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
}
