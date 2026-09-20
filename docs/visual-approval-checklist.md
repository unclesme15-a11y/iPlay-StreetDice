# Visual approval before APK

No APK build until the user has seen and approved current Unity captures or motion previews of every item below. Concept art establishes style but does not count as a runtime capture. Exclude the already customized bill artwork from a new redesign; check only its placement where it interacts with UI.

## Style reference

- Shared iPlay settings reference: `C:/Users/uncle/MoneyOS/02_iPlay/artifacts/pre-game-screen-redesign/shared-settings-card-ui-v1.png`.
- Dice adaptation: `artifacts/ui/dice-pregame-settings-graffiti-concept-v1.png`.
- Target: distressed dark surfaces, restrained cyan glow and gold edges, brush/graffiti lettering, realistic dice and natural hand shades. Controls must remain functional UI, not a flattened picture.
- The current Unity global settings and in-game drawer captures are plainer than the target. They need a visual pass and new captures before approval.

## Launch and pregame

| Review item | Current evidence | Status |
| --- | --- | --- |
| Logo animation: start, cyan trace, symbol reveal, strike, fade | Runtime implementation in `StreetDiceStartup.cs`; `01-intro.png` currently shows the age gate, so it is not valid animation evidence | Needs frame sequence/video |
| 18+ gate and under-18 exit | `artifacts/unity-smoke/startup/02-adult-gate.png` | Current capture; redesign/review needed |
| Die start menu, including Online, Settings and Exit | `artifacts/unity-smoke/startup/03-die-menu.png` predates the Online button | New capture needed |
| Online host/join panel, server and code entry, loading/error states | Implemented in `StreetDiceStartup.cs` | No review capture yet |
| Global settings: hand shade, fade style, dice color, sound, tutorial | `artifacts/unity-smoke/startup/04-global-settings.png` | Current capture; target style not reached |
| Credits and exit confirmation | Implemented | No review capture yet |

## Table and betting

| Review item | Current evidence | Status |
| --- | --- | --- |
| Table waiting state, shareable code, player slots and mic indicators | Runtime table view | New capture needed |
| Shooter's Shoot / Sell choice | Implemented in the live HUD | New approval capture still needed |
| Sell: waiting for bidder, five-second countdown, bidder keypad, submitted/high bid, winner/default states, AI bids | `artifacts/unity-smoke/sale/01-seller-waiting.png`, `02-bid-pad.png`, `03-sale-complete.png` | Implemented; review needed |
| Main wager selection and role-specific betting controls | `artifacts/unity-smoke/readiness/phone-1280x720-bets.png` | Current capture; review needed |
| Paired-number add-on / side-bet Double Up on an existing eligible locked bet | `artifacts/unity-smoke/add-ons/01-digital-add-ons.png` | Implemented as separate requests; review needed |
| Offered, pending, accepted, declined and expired wager locks; wager amounts and outcome die fan | `artifacts/unity-smoke/readiness/online-incoming-crap-lock.png`, `online-accepted-crap-lock.png`, `wager-outcomes.png` | Current captures; review needed |
| Ten-second bettor and fifteen-second shooter countdowns | `artifacts/unity-smoke/readiness/betting-countdown-player-10.png`, `betting-countdown-shooter-15.png` | Current captures; motion review needed |

## Live play and overlays

| Review item | Current evidence | Status |
| --- | --- | --- |
| Idle table, first-person hand, throw, dice impact, sky cam and return | `artifacts/unity-smoke/readiness/in-game.png`, `sky-display.png` | Current captures; motion review needed |
| Catcher's Fade button and each selected hand-only fade style | `artifacts/unity-smoke/readiness/fade-circular-control.png`, `fade-1.png` through `fade-8.png` | Current captures; motion review needed |
| Come-out cue, point/countdown text, quiet roll result | `artifacts/unity-smoke/readiness/betting-countdown-player-10.png`, `roll-result-static.png` | Current captures; motion review needed |
| Hot meter, red dice and subtle smoke | `artifacts/unity-smoke/readiness/hot-meter-half.png`, `hot-meter-full.png`, `hot-dice-smoke-in-flight.png` | Current captures; review needed |
| Game drawer: Options, My Money & Bets, Voice & Sound, Rules | `artifacts/unity-smoke/readiness/drawer.png`, `voice-sound-switches.png` | Current captures; target style not reached |
| Leave confirmation, connection loss/rejoin, full table, low funds and unavailable controls | `artifacts/unity-smoke/readiness/leave-confirmation.png` for leave only | Other states need captures |
| Icon and control sheet: die menu, settings, exit, mic/activity, locks, Fade, sound/toggles and sell/bid controls | Scattered runtime captures | Consolidated review sheet needed |

Capture at a representative phone resolution and at least one narrower layout. User approval of stills does not replace the later real-device usability test. No money-art redesign is included here.
