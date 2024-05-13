# WhatAmIHearing

<img src="https://github.com/zemoto/WhatAmIHearing/assets/27508773/78aa41bb-5af5-4a97-bb39-8cfd51f3d4ca" Width="358" />

App that will record system audio and send it off to the Shazam API to be identified. For when your phone's microphone just can't quite capture the song well enough for Shazam to figure it out.

Written in C# with WPF for the UI.

NAudio (https://github.com/naudio/NAudio) used for recording, resampling, and playback.

ZemotoCommon (https://github.com/zemoto/ZemotoCommon/tree/master) is my utility submodule containing classes that I use in all of my projects.

Uses POST calls to Shazam's API (see documentation at https://rapidapi.com/apidojo/api/shazam) with the raw audio in the body for song detection.

## Rate Limiting

Shazam's API has a rather stingy 500 requests per month on the free tier, and the higher tiers are unreasonably expensive for a non-business. To work around this the app supports using your own API key in place of the one I bundle with the app. 

To get an API key:
1. Go to https://rapidapi.com and make an account.
2. Go to https://rapidapi.com/apidojo/api/shazam and under the "Pricing" section subscribe to the free tier. This should not require any additional info.
3. Click "Apps" at the top right of the page to find your newly created Shazam App on the left side of the page. Should be named something like `default-application_#######`.
4. The Authorization section of that App contains the API key which you can then paste into the `ShazamApiKey.json` file bundled with the WhatAmIHearing.

Sorry I can't do much to make this easier other than spend a bunch of money to upgrade the API tier.

## Spotify Integration

User has the option to authenticate with Spotify, allowing the app to add any detected songs to the private playlist "What Did I Hear?".

Authentication is handled by [SpotifyAuthenticator](https://github.com/zemoto/WhatAmIHearing/blob/main/WhatAmIHearing/Api/Spotify/SpotifyAuthenticator.cs). Uses the user's default browser, using their existing credentials if they are already signed in with the browser, only requiring them to allow the app to access their playlist info.

HTTP requests to API are made in [SpotifyApi](https://github.com/zemoto/WhatAmIHearing/blob/main/WhatAmIHearing/Api/Spotify/SpotifyApi.cs).

## Audio recording
See [Recorder](https://github.com/zemoto/WhatAmIHearing/blob/main/WhatAmIHearing/Audio/Recorder.cs)

Uses NAudio's `WasapiLoopbackCapture` class to record system audio. Shazam allows for up to 500KB of audio to be sent to their API, so we record the maximum amount we can. As of NAudio v2.1 the recorder supports resampling the audio during recording so it is already in a format that Shazam will take once we are done. The WaveFormat is provided by the [ShazamSpecProvider](https://github.com/zemoto/WhatAmIHearing/blob/main/WhatAmIHearing/Api/Shazam/ShazamSpecProvider.cs).
