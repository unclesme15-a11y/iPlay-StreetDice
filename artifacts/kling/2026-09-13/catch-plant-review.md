# Kling Fade Hand Review

- Model: kling-video-v3_0, image-to-video, 5 seconds, 1080p, no audio.
- Generation ID: ATTC5ZyADPa73OjMLYqSq4M_bEcXz7u-qSyrH5Mo0MIgRL7XzlyqdMoeL8SY9ijoV0b-6f6Y
- Status: completed; charged 40 credits.
- Prompt: catch-plant-prompt.txt
- Local clip: catch-plant-kling-v1.mp4 (default watermarked output).
- Reference: Assets/Resources/Environments/bodega-pavement-exact-crop.png.
- Review: photographic hand enters from top-right, fingers pointing left; plants and withdraws. No dice are included. Entry direction differs from the prompt. Not integrated or approved yet.
- Integration must preserve the no-contact fade rule and avoid stretching the square clip into the wide sky-camera display.

## Speed revision

User accepted the clip's look but rejected its speed, including 2x playback, and requires dice visible alongside the hand. Created catch-plant-kling-fast-v2.mp4 from the existing completed clip, not another paid generation. Entry segment 0-0.9 seconds compressed to 0.18; planted segment 0.9-3.5 compressed to 0.30; withdrawal 3.5-5 compressed to 0.25. This is a timing-only asset; actual in-game dice composition remains outstanding. Do not describe the hand-only clip as the finished fade.

Sky-camera framing was tightened separately in StreetDicePlayExperience.cs and StreetDiceFadePresentation.cs. unity/readiness-close-sky.log reports READINESS PASSED, including 180 roll cases and nonblank sky camera. An editor indexing exception also appears in the log. These checks do not constitute visual approval of the hand. No APK built.
