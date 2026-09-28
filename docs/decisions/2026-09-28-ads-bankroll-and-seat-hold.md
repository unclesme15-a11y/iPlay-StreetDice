# Ads, Bankroll and Seat Hold -- Agreed Design

Owner-approved 2026-09-28. **None of this is built yet.** Claude owns the ad
and revenue planning; Codex builds. Uses the host rule from
`2026-09-27-rank-host-and-rewards.md`.

## Ad network: AppLovin MAX (mediation)

- Use **AppLovin MAX** mediation, with Google AdMob, Unity Ads and other
  networks plugged in as bidders, so several networks compete for every slot.
- The SDK key and ad unit IDs come from the owner's MAX account. Use MAX's
  test mode during development; never ship test IDs.
- Rewarded ads must use MAX's server-side verification callback to grant the
  refill (see below).
- Use MAX's built-in consent flow (privacy consent where the law requires it)
  and, on iPhone, Apple's tracking prompt with this wording (Info.plist
  `NSUserTrackingUsageDescription`): *"Allowing tracking helps keep iPlay free
  by showing ads that fit you better. You'll see ads either way."*
- **No purchases.** No "remove ads" or any other in-app purchase (owner
  decision).

## Ad rules (Google's social casino requirements)

Google's ad network accepts simulated-gambling games where nothing of real
value can be won, if the game has an adults-only notice and says it offers no
real-money gambling and no real-world prizes. The existing 18+ screen
(`StreetDiceStartup.cs`, `DrawAdultGate`) says "for adults, play money only";
extend its text to also say **"No real money. Nothing of real value can be
won."** Promoting the game with Google's own ads later needs a separate social
casino certification -- that's a marketing step, not a build step.

## Ad layout

| Where | Format | Rule |
| --- | --- | --- |
| Every screen that isn't a live game (main menu, online lobby, settings, credits) | Banner | Always on. Every menu needs a clear strip so the banner never covers a button. No banners inside a game, including the in-game drawer. |
| Party ad break (Craps) | Full-screen, skippable after 15 seconds | **The whole party at once**, after every **2nd seven-out at the party, no matter who threw it** -- see "Party ad break" below. **Never** counts a come-out 2/3/12 (the shooter keeps the dice). |
| Party ad break (Cee-lo) | Full-screen, skippable after 15 seconds | *Proposed, owner to confirm:* the whole party after every **2nd time the bank passes** (Cee-lo's version of the dice passing), same timer rules. |
| Player is broke | Rewarded video, 15-30 seconds, can't skip once started | Player chooses to watch (store rules require opt-in). See refill rules. |

### Party ad break (owner-defined 2026-09-28)

Like a TV commercial break: everyone at the party watches at the same time,
so nobody waits on anybody.

- **Trigger:** every 2nd seven-out at the party, whoever threw it. Example: 5
  players each seven-out once -> breaks after the 2nd and 4th seven-outs, and
  the next one after the first shooter of round two sevens out (6th).
- **Time floor:** a break needs at least **90 seconds** since the last break
  ended. If the 2nd seven-out comes sooner (a quick seven-out), the break waits
  for the next seven-out instead.
- **Cap:** never more than **3 seven-outs** without a break, even if 90 seconds
  haven't passed. This guarantees the owner's rule that 2 breaks have played
  before all 5 players have shot and sevened out.
- **Why these numbers (simulated 100,000 turns of real craps odds with the
  2026-09-28 roll timing):** a shooter's turn averages about 2 minutes and a
  5-player rotation about 9.5 minutes; with the 90-second floor and the
  3-seven-out cap, **100% of rotations got at least 2 breaks**, averaging about
  **14 breaks per hour** (one every ~4 minutes). A 120-second floor dropped
  below 100%.
- **Mechanics:** the server pauses the party (no rolls, no bets, betting
  timers frozen), marks everyone "away" so the disconnect timer can't remove
  anyone, and resumes when every player's ad has closed or after 35 seconds,
  whichever comes first. Never starts mid-roll -- only right after a
  seven-out resolves.
- Offline vs the computer: same trigger and timer; bots' seven-outs count.

**Offline play vs the computer uses the same rules** as online (banners,
loss ad, broke-player ad). Offline, the player is the host, so the refill goes
up to their own max bet.

### Owner setup (before ads can go live)

1. Create the AppLovin MAX account (and tax info), add the app, send the SDK
   key and ad unit IDs.
2. Put an `app-ads.txt` file (MAX provides the contents) on the website listed
   in the store pages.
3. Update the privacy policy to cover ads and the ad networks' data use.

### Broke-player refill

- Each ad pays **half of the player's own level's max bet** ($50 at Level 1 up
  to $500 at Level 5) -- the one exception to the host rule.
- The player can keep watching until their balance reaches the **host's** max
  bet, and no further. Example: a Level 1 player at a Level 3 host's party gets
  $50 per ad, up to $500 (10 ads).
- Ad completion must be confirmed server-side (the ad network's server-side
  verification callback), not trusted from the app, or the refill can be faked.

## Bankroll: one per account

- Money follows the account from party to party. Going broke anywhere means the
  refill ad is the way back. This is what makes the broke-player ad earn.
- Today each seat starts at a fresh $1,000, which lets a broke player leave and
  rejoin for free money. This design closes that.
- Starting balance for a brand-new account: $1,000 (current default; confirm
  with the owner if it should change).

## Seat hold ("be right back")

- Covers both ad breaks and phones that die: the player's seat and money are
  held **until the party ends**. The rest of the party keeps playing and skips
  them in the shooter rotation.
- Coming back with the same party code on the same account puts them back in
  their old seat with their old balance.
- **Only the host can share or re-share the invite.** If a friend left for
  good, the host re-sharing frees that held seat for someone new.
- Before an ad starts, the app tells the server "on an ad break," so the
  server's 20-second disconnect timer doesn't remove them. That timer only
  exists today so a dead phone doesn't freeze the party; with seat hold, a
  held player is skipped instead of removed.
- Only allowed when the player has no live bet and isn't holding the dice (a
  broke player naturally has neither).

- **If the host's own seat is on hold,** the next-highest rank covers as host
  (music, invites) and the original host gets the role back when they return.
  (A host who actually leaves hands it off for good -- already built.)

## Sign-in

Online parties require signing in (owner decision), so money, XP and rank stay
honest. Offline play vs AI stays open to everyone. Enforce it server-side at
`join-real` (reject without a valid account token) together with the client's
sign-in prompt, so neither ships without the other.
