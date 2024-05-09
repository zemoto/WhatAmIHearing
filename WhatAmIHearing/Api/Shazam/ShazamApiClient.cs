using System.Collections.Generic;
using System.Windows;

namespace WhatAmIHearing.Api.Shazam;

internal sealed class ShazamApiClient : ApiClient
{
   protected override Dictionary<string, string> ApiHeaders { get; } = new()
   {
      ["x-rapidapi-host"] = "shazam.p.rapidapi.com",
      ["x-rapidapi-key"] = ApiConstants.GetShazamApiKey()
   };

   protected override void OnRateLimited()
   {
      const string rateLimitMessage = "Shazam's API is indicating this app's monthly quota has been hit. To continue using the app edit the \"ShazamApiKey.json\" file to use your own API key.";

      Application.Current.Dispatcher.Invoke( () => MessageBox.Show( Application.Current.MainWindow, rateLimitMessage ) );
   }
}
