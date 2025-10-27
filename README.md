# WhatAmIHearing

An open-source Shazam client for identifying music on your PC. For when your phone's microphone just doesn't cut it. 

<img src="https://github.com/user-attachments/assets/5a51cf0f-642c-41a8-bec5-8b016d09178e" Width="358" />

## How do I get it?

### Direct Download

Go to the [Releases Page](https://github.com/zemoto/WhatAmIHearing/releases) and on each release there is a zip file attached named `WhatAmIHearing.zip`:

<img src="https://github.com/user-attachments/assets/826267ec-c643-4f5f-90e2-cf00ccffd1c1" Width="600" />

Click it to download the zip file, unzip it, and run `WhatAmIHearing.exe` to open the app.

### WinGet
WhatAmIHearing is available to install via WinGet with the following command:
```
winget install zemoto.WhatAmIHearing
```
By default, it will install to `%LOCALAPPDATA%\Microsoft\WinGet\Packages` and a `WhatAmIHearing` command will be added to your command line to launch the app from there.


### Updating
Settings are stored in a `config.json` and history is stored in `history.json`. Both files are kept next to your `WhatAmIHearing.exe`. When updating, to keep your settings you need to move those files from your previous version and put them next to the new `WhatAmIHearing.exe`.

## Rate Limiting

Shazam's API has a monthly quota of 500 requests per month on the free tier. To work around this the app supports using your own API key instead of the one bundled with the app. 

To get an API key:
1. Go to https://rapidapi.com and make an account.
2. Go to https://rapidapi.com/apidojo/api/shazam and subscribe to the free tier. This should not require any additional info.
3. Go to https://rapidapi.com/developer/dashboard to see a new Shazam app created for you. Should be named something like `default-application_#######`.
4. The Authorization section of that app contains the API key which can be pasted in the API Key text box in the app. WhatAmIHearing will immediately start using the new key without having to restart. 

Feel free to ask any questions or report any issues by creating a [new Issue](https://github.com/zemoto/WhatAmIHearing/issues).

## Technical Details

Written in C# with WPF for the UI.

[ZemotoCommon](https://github.com/zemoto/ZemotoCommon/tree/master) is my utility submodule containing classes that I use in all of my projects.

Uses POST calls to [Shazam's API](https://rapidapi.com/apidojo/api/shazam) with the raw audio in the body for song detection. API calls are defined in the [Api](https://github.com/zemoto/WhatAmIHearing/blob/main/WhatAmIHearing/Shazam/Api.cs) class. HTTP calls are made via the built-in `HttpClient` class and handled in the [ApiClient](https://github.com/zemoto/WhatAmIHearing/blob/main/WhatAmIHearing/Shazam/ApiClient.cs) wrapper class.

The [NAudio](https://github.com/naudio/NAudio) package is used for recording, resampling, and playback. The [Recorder](https://github.com/zemoto/WhatAmIHearing/blob/main/WhatAmIHearing/Audio/Recorder.cs) class handles wrapping all the recording logic.

### Compiling
The app is compiled with Visual Studio Community 2022. For building the release version, I use the `Publish` feature in Visual Studio with the following settings:
- Configuration: `Release` (default configuration provided by Visual Studio)
- Target Framework: `net6.0-windows`
- Target Runtime: `win-x64`
- Deployment Mode: `Framework-dependant` or `Self-contained` depending on the version I am building.