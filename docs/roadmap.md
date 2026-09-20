# Roadmap

## Phase 1 - Rules Lock

- Confirm final shooter-retains-dice rule after point-phase losses. **Done: seven-out after point gives up dice.**
- Confirm payout model. **Done for MVP: table chip wallet.**
- Confirm side-bet menu. **Done for MVP: come-out win/loss and hit/miss point.**
- Confirm fade timing window. **Done for MVP API: Catcher can Fade/Catch while shot is live.**
- Confirm streak meter thresholds. **Done for prototype: 10-point hot meter with half-point side-bet awards; red dice begin on the next throw.**
- Confirm Double Up risk model. **Done: next shot amount doubles only after a win.**

## Phase 2 - Backend Prototype

- Create minimal server-authoritative dice backend. **Done for MVP prototype.**
- Add deterministic test mode for known dice outcomes. **Done via fixed test rolls.**
- Add full rule tests. **Done for core rule contract.**
- Add table voice token gate. **Done as configuration-gated endpoint.**
- Add simple local smoke verifier. **Done.**

## Phase 3 - Unity Greybox

- Build top-down angled dice lane. **Done in Unity greybox source.**
- Add 5-seat layout. **Done in Unity greybox source.**
- Add dice color selection. **Done as selected dice color plus hot override.**
- Add Roll, Fade/Catch, Run Same, Double Up buttons. **Done in Unity greybox source.**
- Add magnifier/result HUD. **Done in Unity greybox source.**
- Add streak meter and hot dice mode. **Done in Unity greybox source.**

## Phase 4 - Real Clip / Kling Lane

- Produce reference-safe clips for:
  - Shooter ready.
  - Come-out roll.
  - Point roll.
  - Catcher fade.
  - Hot dice activation.
- Keep Unity dice/result authoritative over the clip.

## Phase 5 - Multiplayer Feel

- Add voice UI.
- Add side-betting UI.
- Add table talk indicators.
- Add player reactions.
- Add spectator-like camera cuts while preserving gameplay readability.

## Phase 6 - Production Readiness

- Security and abuse controls.
- Mobile performance checks.
- Device testing.
- Commercial asset audit.
- Wagering model decided 2026-09-19: virtual currency only, no cash value,
  no cash-out, no real-money purchase of chips.
- Platform order decided 2026-09-19, revised same day after store-policy
  research: Apple App Store first (17+ age rating path). Google Play
  deprioritized - its policy reads as an outright ban on this genre, not
  just an age-gate, even with virtual currency. Amazon Appstore and Samsung
  Galaxy Store are viable secondary Android targets. Steam later. See
  `docs/post-production-handoff.md` (P5, P6) for the concrete punch list and
  sourcing.
