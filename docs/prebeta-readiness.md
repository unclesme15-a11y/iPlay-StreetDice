# Pre-beta readiness

The playable Street Dice code is prepared without building an APK. The online craps flow has a host/join menu, one seat and session token per player, role-specific controls, server-owned dice results, fixed-camera remote replay, paired wagers, and a 20-second reconnect window driven by heartbeats. Balances remain private to their owners. The rules screen includes a short fair-play note.

Shoot/Sell is implemented with a five-second auction, forced $1 Catcher default, immediate private-wallet settlement, public sale price, seller main-bet exclusion, and adjusted clockwise queue. The disclosed default-buyer come-out protection is server-owned. Existing locked point bets can independently request Double Up and a paired-number add-on; each still requires Shooter acceptance.

The ten-point hot meter, next-throw red dice, subtle smoke, bottom-left fire capsule, and COME OUT door cue are implemented. Heat scoring stays off the play screen. Red/orange are temporary hot-dice colors; permanent unlock requirements for the other colors still need a product decision.

## Checks completed

- Server test suite: 90 passing tests.
- Live local API check: two separate seats, server roll commit, replay frames, and no other player's balance in public state.
- Unity editor checks: online wager/physical throw and real host/guest seat flow passed. Full rendered readiness passed without an APK. The remote replay capture is `artifacts/unity-smoke/readiness/real-online-guest-roll.png`.
- These are local/editor checks, not two physical phones or broad Android compatibility testing.

## External setup before voice can be proven

- Link the Unity project to the intended Unity Gaming Services cloud project. Its cloud project ID is currently blank.
- Configure the server-side Vivox issuer, domain, signing key, and Unity environment ID. Keep the signing key on the server only. Supported environment variable names are `IPLAY_VIVOX_ISSUER`, `IPLAY_VIVOX_DOMAIN`, `IPLAY_VIVOX_SIGNING_KEY`, and `IPLAY_UNITY_ENVIRONMENT_ID`.
- Run a two-device open-mic call and verify Android microphone permission, speaker activity, background/reconnect behavior, and mute. These cannot be confirmed in the editor alone.

The legacy one-client/multiple-seat and bot-fill endpoints are development-only. No APK was built or approved.
Both devices must enter the same reachable backend address in the Online menu; `localhost` on a phone refers to that phone, not the developer's computer.
