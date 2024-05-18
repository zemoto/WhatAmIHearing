using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WhatAmIHearing.Api;

internal abstract class ApiClient : IDisposable
{
   private static readonly HttpClient _client = new();
   public static void StaticDispose() => _client.Dispose();

   protected abstract Dictionary<string, string> ApiHeaders { get; }

   private readonly List<CancellationTokenSource> _cancelTokenSources = new();
   private readonly object _cancelTokenLock = new();
   private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

   protected ApiClient()
   {
      var retryStrategy = new RetryStrategyOptions<HttpResponseMessage>
      {
         Delay = TimeSpan.FromSeconds( 3 ),
         BackoffType = DelayBackoffType.Constant,
         MaxRetryAttempts = 2,
         ShouldHandle = new PredicateBuilder<HttpResponseMessage>().HandleResult( x => x.StatusCode == HttpStatusCode.TooManyRequests ),
      };
      _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry( retryStrategy ).Build();
   }

   public void CancelRequests()
   {
      lock ( _cancelTokenLock )
      {
         _cancelTokenSources.ForEach( x => x.Cancel() );
      }
   }

   public void Dispose() => CancelRequests();

   public async Task<string> SendPostRequestAsync( string endpoint )
   {
      var messageBuilder = CreateMessageBuilder( HttpMethod.Post, endpoint );
      return await SendMessageAsync( messageBuilder );
   }

   public async Task<string> SendPostRequestAsync( string endpoint, string body )
   {
      var messageBuilder = CreateMessageBuilder( HttpMethod.Post, endpoint, () => new StringContent( body ) );
      return await SendMessageAsync( messageBuilder );
   }

   public async Task<string> SendPostRequestAsync( string endpoint, byte[] data )
   {
      var messageBuilder = CreateMessageBuilder( HttpMethod.Post, endpoint, () => new StringContent( Convert.ToBase64String( data ) ) );
      return await SendMessageAsync( messageBuilder );
   }

   public async Task<string> SendPostRequestAsync( string endpoint, Dictionary<string, string> formUrlEncodedData )
   {
      var messageBuilder = CreateMessageBuilder( HttpMethod.Post, endpoint, () => new FormUrlEncodedContent( formUrlEncodedData ) );
      return await SendMessageAsync( messageBuilder );
   }

   public async Task<string> SendGetRequestAsync( string endpoint )
   {
      var messageBuilder = CreateMessageBuilder( HttpMethod.Get, endpoint );
      return await SendMessageAsync( messageBuilder );
   }

   private Func<HttpRequestMessage> CreateMessageBuilder( HttpMethod method, string endpoint, Func<HttpContent> contentBuilder = null )
   {
      return () =>
      {
         var message = new HttpRequestMessage( method, endpoint );
         if ( contentBuilder is not null )
         {
            message.Content = contentBuilder();
         }

         foreach ( var (key, value) in ApiHeaders )
         {
            message.Headers.Add( key, value );
         }

         return message;
      };
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

         if ( response.StatusCode is HttpStatusCode.TooManyRequests )
         {
            OnRateLimited();
         }

         return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : string.Empty;
      }
      finally
      {
         lock ( _cancelTokenLock )
         {
            cancelTokenSource.Dispose();
            _cancelTokenSources.Remove( cancelTokenSource );
         }
      }
   }

   protected virtual void OnRateLimited() { }
}
