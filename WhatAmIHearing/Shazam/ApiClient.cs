using Polly;
using Polly.Retry;
using System.Net;
using System.Net.Http;

namespace WhatAmIHearing.Shazam;

internal sealed class ApiClient : IDisposable
{
   private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;
   private readonly HttpClient _client = new();

   private readonly List<CancellationTokenSource> _cancelTokenSources = [];
   private readonly object _cancelTokenLock = new();

   public ApiClient()
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
      string base64Data = Convert.ToBase64String( data );
      StringContent contentBuilder() => new( base64Data );

      var appSettings = AppSettings.Instance;
      HttpRequestMessage messageBuilder()
      {
         var message = new HttpRequestMessage( HttpMethod.Post, endpoint ) { Content = contentBuilder() };
         message.Headers.Add( "x-rapidapi-host", "shazam.p.rapidapi.com" );
         message.Headers.Add( "x-rapidapi-key", appSettings.KeyData.UseDefaultKey ? ApiKeyData.DefaultShazamApiKey : appSettings.KeyData.ShazamApiKey );
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
         using var response = await _pipeline.ExecuteAsync( async token =>
         {
            using var message = messageBuilder();
            return await _client.SendAsync( message, token );
         }, cancelTokenSource.Token );

         LastStatusCode = response.StatusCode;
         UpdateRateLimitValues( response.Headers );

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

   private void UpdateRateLimitValues( System.Net.Http.Headers.HttpResponseHeaders headers )
   {
      var appSettings = AppSettings.Instance;
      if ( appSettings.KeyData.UseDefaultKey )
      {
         return;
      }

      if ( headers.TryGetValues( "X-RateLimit-Requests-Limit", out var limitValues ) && limitValues?.Any() == true && int.TryParse( limitValues.First(), out int quotaLimit ) &&
           headers.TryGetValues( "X-RateLimit-Requests-Remaining", out var remainingValues ) && remainingValues?.Any() == true && int.TryParse( remainingValues.First(), out int quotaRemaining ) )
      {
         appSettings.KeyData.QuotaLimit = quotaLimit;
         appSettings.KeyData.QuotaUsed = quotaLimit - quotaRemaining;
      }
   }
}
