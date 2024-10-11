using System.Collections.Generic;

namespace WhatAmIHearing.Api.Shazam;

internal sealed class ShazamApiClient( ShazamApiSettings settings ) : ApiClient
{
   protected override Dictionary<string, string> ApiHeaders => new()
   {
      ["x-rapidapi-host"] = "shazam.p.rapidapi.com",
      ["x-rapidapi-key"] = settings.ShouldUseDefaultKey ? ShazamApiSettings.DefaultShazamApiKey : settings.ShazamApiKey
   };
}
