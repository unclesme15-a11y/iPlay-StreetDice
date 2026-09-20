# Store Submission Checklists

Researched 2026-09-20. Store fees, steps, and UI change over time — re-verify
against each store's own current documentation right before you actually do
this, rather than trusting this snapshot months later.

Everything in this file is either a step only you can do (it needs a real
identity, a payment method, or a physical Mac) or a step that depends on one
of those happening first. Nothing here can be done by an AI agent on your
behalf.

## Before any store: what's already ready

- Bundle identifiers are set: `com.iplay.ceelocraps` for both Android and
  iOS (`unity/StreetDiceGreybox/ProjectSettings/ProjectSettings.asset`).
- Privacy Policy / Terms of Use / Support page is live:
  https://claude.ai/artifact/Qm6GkeYCcDEgeyFkmBrGUy (`legal-site/index.html`
  in the repo) — **fill in its placeholders (support email, legal entity
  name, jurisdiction, liability clause — the last two need a lawyer) before
  using this URL in a real store listing.**
- Wagering model is virtual-currency-only with no purchase/cash-out path —
  confirmed in code, not just policy. See `docs/post-production-handoff.md`.
- An 18+ self-attestation age gate already exists in the app's startup flow.

## Apple App Store

- **Cost**: $99/year (Apple Developer Program membership).
  ([Apple](https://developer.apple.com/programs/enroll/))
- **You need**: a Mac with Xcode. Unity can export an Xcode project for iOS,
  but only Xcode running on macOS can actually compile and upload the final
  `.ipa` to App Store Connect — this sandbox has neither, so that whole
  export → build → upload → submit sequence has to happen on a real Mac,
  not here.
- **Signup steps**: enroll at developer.apple.com, verify your identity with
  a government-issued photo ID, provide your legal name (this becomes the
  public "seller" name on the App Store), agree to the Developer Agreement,
  pay the $99.
- **Age rating**: expect **17+**, because the app has "frequent or intense"
  simulated gambling content (shooter/catcher wagering, streaks, hot dice).
  Apple's own rating questionnaire in App Store Connect will ask about
  gambling-adjacent content directly — answer it honestly, don't undersell it
  to try for a lower rating.
- **App Store Connect listing needs**: app name, description, screenshots
  (per device size), Privacy Policy URL (above), Support URL (above), age
  rating questionnaire, and — since this app requests microphone access for
  voice chat — a clear purpose string explaining why (Unity's iOS
  `NSMicrophoneUsageDescription` build setting; needs to say something like
  "Used for table voice chat with other players").
- **South Korea note**: Apple requires a government Rating Classification
  Number (RCN) for apps with frequent/intense simulated gambling content
  specifically when listing there — skip that region at first if you don't
  want to deal with it yet.

## Amazon Appstore

- **Cost**: free developer account.
  ([Amazon](https://developer.amazon.com/docs/app-submission/getting-started.html))
- **You need**: nothing exotic — this ships the same Android build
  (APK/AAB) as any other Android store, buildable from Unity without a Mac.
- **Signup steps**: create a developer account at developer.amazon.com,
  verify by email, fill in developer profile (name, phone, company name/
  address, support email — must match any tax forms you file later), agree
  to the Amazon Developer Services Agreement.
- **Policy note**: Amazon explicitly allows simulated gambling using
  currency with no real value — more permissive than Google Play here (see
  `docs/post-production-handoff.md` P6), so no genre-level blocker.
- **Before submitting**: review Amazon's own Appstore Presubmission
  Checklist in their docs — it covers things like required icon sizes and
  content ratings that change independently of anything in this repo.

## Samsung Galaxy Store

- **Cost**: free — no annual fee (as of the May 2025 revenue-share update
  Samsung carried into 2026, developers keep 80% on regular sales).
  ([Samsung](https://developer.samsung.com/galaxy-store/prepare.html))
- **You need**: same Android build as Amazon/Google Play — no Mac required.
- **Signup steps**: go to seller.samsungapps.com, sign up with a Samsung
  account, register with the Seller Portal, apply for commercial seller
  status if you intend to charge for anything (not needed here since
  there's nothing to purchase).
- **Policy note**: Samsung allows gambling-themed play as long as no real
  cash is being bet — consistent with this app's virtual-currency design.

## Google Play — deliberately deprioritized

Not included as an active target. Google Play's policy reads as an outright
ban on this genre (simulated gambling / social-casino-style content),
independent of virtual currency — see `docs/post-production-handoff.md` P6
for the sourced reasoning. Revisit only if you're willing to redesign the
core wagering loop, or if Google's policy changes.

## Steam — not yet researched

Deliberately out of scope until the mobile stores above are live, per the
platform-order decision in `docs/roadmap.md`. When you're ready for it,
that's a fresh research pass (Steamworks' own content policy, the $100
one-time fee, SteamPipe build upload), not something covered here yet.

## Vivox / Unity Gaming Services (needed for real voice chat)

The voice signer in this repo (`VivoxTokenSigner.cs`) is real and correct,
but it needs actual credentials to do anything — right now there's no
Vivox/UGS account behind it.

- **Cost**: Vivox is free for indie-scale use, up to 5,000 peak concurrent
  users. ([Unity](https://unity.com/products/vivox))
- **Steps**: create/link a Unity ID, create a Unity Gaming Services project
  (or use the existing Unity project's org if one exists), enable the Vivox
  service for it, and pull the real Issuer/Key/Domain/Environment ID values
  from the Unity Cloud dashboard into this server's config
  (`Vivox:Issuer`, `Vivox:Domain`, `Vivox:SigningKey`,
  `Vivox:UnityEnvironmentId` — see `VivoxTokenSigner.FromConfiguration`).
  Never commit these values to the repo; set them as environment variables
  or secrets on wherever the server actually runs.

## What's not on this list

Deliberately absent: anything requiring a hosted backend to already be
running (that's a separate decision — see `docs/post-production-handoff.md`
P5 on Docker/hosting), and anything requiring the app to already be built
and running on a real device, since neither exists yet in this sandbox.
