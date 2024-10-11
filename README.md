# WhatAmIHearing

An app that will record system audio and send it off to the Shazam API to be identified. For when your phone's microphone just can't quite capture the song well enough for Shazam to figure it out.

<img src="https://github.com/user-attachments/assets/5a51cf0f-642c-41a8-bec5-8b016d09178e" Width="358" />

## How do I Download it?

Go to the Releases Page on the right side of the GitHub repo and on the latest release you'll see a zip file attached named `WhatAmIHearing.zip`:

<img src="https://github.com/user-attachments/assets/826267ec-c643-4f5f-90e2-cf00ccffd1c1" Width="600" />

Click that link to download the zip file, unzip it, and run `WhatAmIHearing.exe` to open the app.

#### Updating
Settings are stored in a `config.json` and history is stored in `history.json`. Both files are kept right next to your `WhatAmIHearing.exe`. When updating all you need to do is take those files from your previous version and put them next to the new `WhatAmIHearing.exe`. If you're using your own API key, be sure to also bring in your `ShazamApiKey.json` file.

## Rate Limiting

Shazam's API has a monthly quota of 500 requests per month on the free tier. To work around this the app supports using your own API key in place of the one I bundle with the app. 

To get an API key:
1. Go to https://rapidapi.com and make an account.
2. Go to https://rapidapi.com/apidojo/api/shazam and subscribe to the free tier. This should not require any additional info.
3. Go to https://rapidapi.com/developer/dashboard to see a new Shazam app created for you. Should be named something like `default-application_#######`.
4. The Authorization section of that App contains the API key which can be pasted in the API Key text box in the app. WhatAmIHearing will immediately start using the new key without having to restart. 

Feel free to ask me any question or report any issues by creating a [new Issue](https://github.com/zemoto/WhatAmIHearing/issues).

## Technical Details

Written in C# with WPF for the UI. Compiled with Visual Studio Community 2022 against .NET 6.0.

ZemotoCommon (https://github.com/zemoto/ZemotoCommon/tree/master) is my utility submodule containing classes that I use in all of my projects.

Uses POST calls to Shazam's API (see documentation at https://rapidapi.com/apidojo/api/shazam) with the raw audio in the body for song detection.

### Audio recording
NAudio (https://github.com/naudio/NAudio) used for recording, resampling, and playback. See the [Recorder](https://github.com/zemoto/WhatAmIHearing/blob/main/WhatAmIHearing/Audio/Recorder.cs) class for the meat of the logic.

Uses NAudio's `WasapiLoopbackCapture` class to record system audio. Shazam allows for up to 500KB of audio to be sent to their API, so we record the maximum amount we can. As of NAudio v2.1 the recorder supports resampling the audio during recording so it is already in a format that Shazam will take once we are done.
