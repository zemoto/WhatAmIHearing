using System.Collections.Generic;

namespace WhatAmIHearing.Api.Shazam;

internal sealed class ShazamApiClient : ApiClient
{
   protected override Dictionary<string, string> ApiHeaders { get; } = new()
   {
      ["x-rapidapi-host"] = "shazam.p.rapidapi.com",
      ["x-rapidapi-key"] = ApiConstants.ShazamApiKey
   };
}
