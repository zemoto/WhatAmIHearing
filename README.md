# WhatAmIHearing

Program that will record system audio, resample it into a form that is required by the Shazam API, and send it off to said API to identify the song. For when your phone's microphone just can't quite capture the song well enough for Shazam to figure it out.

Written in C# with WPF for the UI.

NAudio (https://github.com/naudio/NAudio) used for recording, resampling, and playback.

Uses simple POST calls to Shazam's API (see documentation at https://rapidapi.com/apidojo/api/shazam) with the raw audio in the body for song detection.

## Notable Classes

### [Recorder](https://github.com/zemoto/WhatAmIHearing/blob/main/WhatAmIHearing/Audio/Recorder.cs)

Uses NAudio's `WasapiLoopbackCapture` class to record system audio. Shazam allows for up to 500KB of audio to be sent to their API, so we record the maximum amount we can. The audio we record from the system audio device will be at a different bitrate than the required audio. Depending on if the system audio comes in at a higher or lower bitrate, we determine if we can record more or less audio and still meet the API's specification.

See the [ShazamSpecEnforcer](https://github.com/zemoto/WhatAmIHearing/blob/main/WhatAmIHearing/Api/ShazamSpecEnforcer.cs) for logic on getting the maximum amount we can record, and how to resample the audio to match Shazam's specification.

### [Player](https://github.com/zemoto/WhatAmIHearing/blob/main/WhatAmIHearing/Audio/Player.cs)

Simple class for playing back the audio if the API didn't return any matches. User's are given the chance to hear their recorded audio played back to them to ensure there was no noice or unintended audio.
