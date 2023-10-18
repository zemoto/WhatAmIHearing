# WhatAmIHearing

Program that will record system audio, resample it into a form that is required by the Shazam API, and send it off to said API to identify the song. For when your phone's microphone just can't quite capture the song well enough for Shazam to figure it out.

Written in C# with WPF for the UI.

NAudio (https://github.com/naudio/NAudio) used for recording, resampling, and playback.

ZemotoCommon (https://github.com/zemoto/ZemotoCommon/tree/master) is my utility submodule containing classes that I use in all of my projects.

Uses POST calls to Shazam's API (see documentation at https://rapidapi.com/apidojo/api/shazam) with the raw audio in the body for song detection.

## Spotify Integration

User has the option to authenticate with Spotify, allowing the app to add any detected songs to the private playlist "What Did I Hear?".

Authentication is handled by [SpotifyAuthenticator](https://github.com/zemoto/WhatAmIHearing/blob/main/WhatAmIHearing/Api/Spotify/SpotifyAuthenticator.cs). Uses the user's default browser, using their existing credentials if they are already signed in with the browser, only requiring them to allow the app to access their playlist info.

HTTP requests to API are made in [SpotifyApi](https://github.com/zemoto/WhatAmIHearing/blob/main/WhatAmIHearing/Api/Spotify/SpotifyApi.cs).

## Audio recording
See [Recorder](https://github.com/zemoto/WhatAmIHearing/blob/main/WhatAmIHearing/Audio/Recorder.cs)

Uses NAudio's `WasapiLoopbackCapture` class to record system audio. Shazam allows for up to 500KB of audio to be sent to their API, so we record the maximum amount we can. As of NAudio v2.1 the recorder supports resampling the audio during recording so it is already in a format that Shazam will take once we are done. The WaveFormat is provided by the [ShazamSpecProvider](https://github.com/zemoto/WhatAmIHearing/blob/main/WhatAmIHearing/Api/Shazam/ShazamSpecProvider.cs).
