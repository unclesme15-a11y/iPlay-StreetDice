# Revenue Opportunity — Street Dice Founding Table Pass

**Prepared:** 2026-08-19 00:19 EDT  
**Status:** INTERNAL / REVIEW ONLY — NOT APPROVED TO SEND, PUBLISH, BILL, OR PROMISE ACCESS

## Opportunity

Validate willingness to pay before funding production clips, voice, and multiplayer polish with a small **Founding Table Pass** cohort.

**Pricing hypothesis for validation:** $19 one-time per player, capped at 20 players. The pass would fund a moderated closed-alpha table session and include non-cash founder recognition plus one cosmetic dice-color entitlement at launch. Table chips remain play-only, have no cash value, cannot be purchased, withdrawn, transferred, or redeemed, and the test must not be marketed as wagering.

This is a demand test, not a launch offer. No external outreach, billing, public copy, or access promise is approved.

## Why this is the nearest money path

- The server-authoritative rules prototype and greybox already exist, so the next expensive work should be gated by proof that the Shooter/Catcher, Fade/Catch, streak, and Double Up loop attracts paying players.
- A capped cohort can test both monetization and multiplayer table energy before commissioning the five production clip families or wiring production voice.
- Maximum hypothesis revenue is **$380** at the proposed cap; the more valuable output is evidence for or against further production spend.

## Evidence checked

- `dotnet test IPlayStreetDice.sln --nologo`: **19/19 passed** on 2026-08-19.
- `tools/verify-street-dice-local.ps1 -StartServer`: **ok=True**; table flow completed; expected voice gate returned **501** because production signing is not wired.
- Unity `compile.log`: script build succeeded and batch mode exited **0**, with two warnings and no compile errors.
- Roadmap: rules lock, backend prototype, and Unity greybox are marked complete; production clips, voice UI, side-bet UI, device testing, commercial audit, and legal review remain open.

## Readiness and confidence

- **Prototype/mechanics readiness: 60%** — core rules, API, automated tests, smoke flow, and compiling greybox exist; no real-device or live multi-human proof was found.
- **Paid-cohort readiness: 35%** — no packaged tester build, production voice, device QA, external offer approval, billing flow, or legal review is evidenced.
- **Confidence this is the right next revenue experiment: 78%** — it measures willingness to pay before the costly clip/voice lanes, but target-customer interviews and acquisition-channel evidence are not yet present.

## Internal validation card

### Target cohort

18+ players who already understand informal street-dice culture and can join a scheduled moderated remote test. Do not recruit minors. Do not position the test as real-money play.

### Pass/fail thresholds

- **GO:** at least 8 of 20 qualified prospects explicitly accept the $19 offer concept, and at least 5 complete a no-charge mechanics session without a critical rule-flow failure.
- **HOLD:** 3–7 accept the concept, or mechanics are engaging but onboarding/voice blocks completion.
- **STOP/REPOSITION:** fewer than 3 accept, players interpret the product primarily as real-money wagering, or the core Fade/Catch loop is not understood after one moderated round.

### Evidence to collect

1. Prior familiarity with street dice.
2. Whether Shooter/Catcher and Fade/Catch are understood without coaching.
3. Whether the player would pay $19 for a capped founding pass; record exact words, not inferred interest.
4. Preferred benefit: cosmetic dice, founder badge/credit, private table access, or early seasons.
5. Any expectation of cash-out, purchasing chips, or real-money wagering.
6. Device, session completion, critical defects, and replay intent.

## Bell's safe next actions

1. Package the current greybox into an internal Windows/Android tester candidate and record path, bytes, SHA256, build source commit, and QA result.
2. Run a two-human local/LAN mechanics session; capture comprehension and defect evidence against the thresholds above.
3. Draft a 20-prospect lead-source map and private outreach variants, still marked NOT APPROVED TO SEND.
4. Prepare a no-charge first-session intake form and results sheet so payment is not collected before mechanics and compliance assumptions are validated.
5. Return an approval bundle with exact candidate artifact proof, test result, final proposed wording, and a recommended GO/HOLD/STOP call.

## Smallest SME approval gate

Approve or reject this single hypothesis: **“Bell may prepare (but not send) a $19, 20-player Founding Table Pass validation package, explicitly play-only/no-cash-value, with billing and outreach still requiring separate approval.”**
