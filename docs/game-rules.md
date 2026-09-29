# Game Rules Contract

Working title: **iPlay Street Dice**

Current product name: **iPlay Cee-lo & Craps**.

## Shoot, Pass and Leaving

- A player offered the dice chooses Shoot or Pass before committing a wager.
- Pass hands the dice straight to the next player in the active turn queue -- no auction, no charge, no forced buyer.
- Leaving as shooter with a committed wager counts as a crap/forfeit loss, not a new random roll.
- During a point, the main wager loses; point-hit bets lose and point/group-miss bets win, as on a point-phase loss. Already settled bets are not paid again.
- Leaving as a side bettor forfeits that player's open bets without resetting another shooter's point.
- Leave Game requires confirmation and is the last in-game drawer option.
- After a seven-out, the next shooter receives a choice before a new wager is committed.

## Players

- 2 to 5 players.
- One player is the Shooter.
- Shooter must always be shooting against another player.
- The opposing player is the Catcher.
- Other seated players may participate through side betting if the table state allows it.

## Come-Out Roll

The Shooter rolls two dice.

- `7` or `11`: Shooter wins immediately.
- `2`, `3`, or `12`: Shooter loses and pays, but keeps the dice. Their next shot is a brand-new come-out (new come-out lock and countdown).
- Any other total becomes the Shooter's point.

## Point Phase

After a point is established:

- Shooter keeps rolling until the point is hit or a `7` is rolled.
- Point hit: Shooter wins.
- `7` before point: Shooter loses, craps out, and gives up the dice.
- Clockwise handoff: the next player in the active turn queue becomes Shooter. The old Shooter becomes Catcher.

## Fade / Catch

Fade/Catch is the iPlay active defensive mechanic.

- The Catcher has a **Fade/Catch** button while a roll is still in the fade window.
- If Catcher fades/catches the roll before lock:
  - Roll is nullified.
  - No dice result is shown as final.
  - No payout happens.
  - No side bet resolves.
  - Shooter shoots again.

Fade/Catch is not just accepting the action. It means the Catcher stops that roll from counting.

Fade presentation is hand-only. Once the fade kills the roll, dice disappear and the overhead display plays the catcher's selected Kling hand animation: Tap, Plant or Wave. The selection is saved in main settings alongside hand appearance. No dice appear in the fade animation; normal rolls still use the magnified dice camera. The catcher never touches dice.

## Fade Momentum

- First 3 fades are allowed without increasing Shooter momentum.
- Starting after the 3rd fade, additional fades increase Shooter momentum.
- Momentum should benefit the Shooter's streak/heat if the Shooter later wins.
- Momentum exists to prevent Catcher from abusing fade/catch as a pure delay.

Example:

1. Fade 1: roll nullified.
2. Fade 2: roll nullified.
3. Fade 3: roll nullified.
4. Fade 4: roll nullified, Shooter momentum increases.
5. Shooter later hits point: streak/heat reward is larger than normal.

## Side Betting

Side betting replaces the earlier "Call Out" language.

Possible side bets (owner rule, confirmed 2026-09-29):

- **Come-out: CRAP 2/3/12 only.** That's the only come-out bet. Nobody can bet that the shooter *won't* crap -- there is no come-out HIT.
- **Point: CRAP** the point or its paired number (`CRAP 10` / `CRAP 4` on a point of 10), offered to the shooter or to another player.
- **Point: HIT** the point or its paired number (`HIT 10` / `HIT 4`), offered to **anyone but the shooter** -- you're betting a bystander that the shooter hits.
- The shooter can't bet CRAP on their own roll.

Side bets do not resolve on faded rolls.

- A public 10-second betting window opens at two moments only: the come-out (after the main shot is committed) and when the point is set. Street dice is quick -- there is no countdown before the other rolls.
- **Come-out overlay:** the only come-out bet is `CRAP 2/3/12`, so instead of the menu every non-shooter gets the come-out overlay: a `CRAP 2/3/12` lock about twice the standard lock size, center screen, with a **NO BET** tab beside it and a glowing line around it that runs down over the 10 seconds (green -> yellow -> red). Press the lock, pick the bills, and the lock drops to the ground as a proposed bet. Press **NO BET** to sit the come-out out (that player is done). The overlay stays whether the bet menu is open or not -- the **BET** button never hides it.
- **Point set:** the bet menu pops open by itself for everyone but the shooter. This is the only time the menu opens by itself. Betting against the point is a **CRAP** bet: the lock reads `CRAP` plus the shooter's point, whatever it is (`CRAP 6` on a point of 6, `CRAP 10` on a point of 10), or the point's paired number.
- When the propose time ends, the menu closes and only the locks (fully proposed bets) stay on screen. The **BET** button in the top left opens or closes the menu any time betting is allowed.
- **Ends early when everyone's done.** A player is done once they propose a bet, press **NO BET** on the come-out overlay, or (at the point) close the bet menu. When every non-shooter is done, the propose time ends right away. If a lock is still waiting on the shooter they keep up to 5 more seconds to take it; once nothing is waiting on them they can throw immediately. Example: 4 opponents have bet and you close the menu by accident -- the countdown skips and the shooter can roll. (Owner rule 2026-09-28.)
- **Late bet after the point window:** a player with no live bet against the shooter (for example, they closed the menu by accident) can bring the menu back with BET and propose one bet against the point or its paired number (`CRAP 10` or `CRAP 4` for point 10) between rolls. Like an add-on, the shooter takes it before throwing or it expires when the next roll starts.
- Every non-shooter sees a real `10` through `1` countdown. The shooter sees one continuous `15` through `1` countdown because only the shooter receives the additional five-second acceptance period.
- The shooter cannot begin the throw until the 15-second countdown reaches zero.
- New side-bet offers and non-shooter acceptances received after 10 seconds are rejected. The shooter may accept an already offered wager during the additional five seconds; no new offer can be created then.
- After the point window, the shooter keeps rolling with no countdown until the point is hit or they seven-out. Between those rolls the only bets anyone can propose are **Double Up** and the **paired number** add-ons below, plus the one late bet above for a player who has none, which the shooter accepts or ignores before throwing again. (Changed 2026-09-28 by the owner; previously a fresh window opened before every point roll, which made a shot take over a minute.)

Point group side bets use these street dice groups:

- `4/10`
- `6/8`
- `5/9`

Example:

- Shooter establishes point `10`.
- Another player side bets against the `4/10` group by targeting `4`.
- If Shooter rolls `4`, that grouped side bet loses, but Shooter still keeps shooting for `10`.
- If Shooter rolls `10`, that grouped side bet also loses, and the main shot resolves as a point hit.
- If Shooter rolls `7` before either grouped number, the grouped miss bet wins.

During a point, CRAP side bets explicitly target the point or its grouped mate (`CRAP 10` or `CRAP 4` for point `10`). The target stays on the wager lock so two offers can be distinguished. Under the grouped rule above, either `4` or `10` defeats those CRAP offers; `7` wins them. On come-out, the CRAP target is `2/3/12` instead of a point number.

After an eligible bet against the Shooter is accepted during a point, its bettor may make two independent add-on requests before a later roll:

- **Double Up** requests one additional wager equal to that accepted bet on the same number.
- **Paired Number** requests one additional wager of the same amount on the point's grouped mate.
- Each request is separate. The Shooter may accept one, both, or neither.
- **A Double Up is its own proposal, and it can't be split** (owner rule 2026-09-29). Whoever accepts it owes the whole doubled amount: on a $100 bet, accepting the Double Up means owing $200 on that bet. Nobody else can cover part of it. Another player who wants to double has to have their own bet and double that.

**Side-bet amounts:** side bets stack bills like the main shot. Tap bills to build the amount -- $1, $5, $10, $20, plus $50 and $100 once the host has unlocked them -- then tap the lock to propose it. **Clear** starts over. The most any single side bet can be is the host's bet cap ($100 / $250 / $500 / $750 / $1,000 by host level), and never more than you can cover. (Owner 2026-09-29; before this, side bets were a single $1-$20 bill.)
- An unaccepted add-on expires when the next roll begins.

## Cee-lo Street / Banker Rules

Cee-lo uses three dice and is evaluated separately from the two-dice craps flow.

- `4-5-6`: automatic win.
- Trips: automatic win.
- Pair plus `6`: automatic win.
- `1-2-3`: automatic loss.
- Pair plus `1`: automatic loss.
- Pair plus `2`, `3`, `4`, or `5`: point.
- Anything else: no count, roll again.

MVP comparison model:

- Banker rolls first until automatic win, automatic loss, or point.
- If Banker sets a point, each player rolls until automatic win, automatic loss, or point.
- Player point higher than Banker point wins.
- Player point lower than Banker point loses.
- Same point pushes.

## Payout / Score Models

MVP model: **Table Chip Wallet**

- Every player starts with 1,000 table chips.
- Shooter and Catcher settle the shot amount directly.
- Come-out win, point hit, and seven-out pay the shot amount 1:1.
- Side bets are tracked separately and resolve 1:1 for the first prototype.
- Double Up doubles the next shot amount only.
- This model is best for testing the real street-table feel because balances, risk, and pressure are visible immediately.

Alternate model: **Round Points Race**

- No wallet is required.
- Players earn score points for shooter wins, point hits, side-bet wins, and streak bonuses.
- Crapping out resets the Shooter streak and passes dice, but does not create a negative wallet.
- The table can end at a target score such as 50 or after a fixed number of hands.
- This model is better for compliance-friendly mobile testing if chip language needs to be avoided.

## After Shooter Wins

After Shooter wins by come-out or point:

1. Payout resolves.
2. Streak meter increases.
3. Shooter keeps dice.
4. Shooter chooses next action:
   - **Run Same**: shoot the same amount again.
   - **Double Up**: shoot double the previous shot amount.

## Run Same

Shooter continues with the same shot amount.

Example:

- Shooter wins a `$20` shot.
- Shooter chooses **Run Same**.
- Next shot is `$20`.

## Double Up

Shooter increases the next shot amount to double the previous shot.

Recommended interpretation:

- Previous win is locked.
- Double Up affects the next shot only.
- One catcher covers the whole doubled shot. It can't be split between players.
- If Shooter loses the next shot, Shooter loses the new doubled shot amount, not the previous already-paid win.

Example:

- Shooter wins a `$20` shot.
- Shooter chooses **Double Up**.
- Next shot is `$40`.
- If Shooter wins, streak reward increases.
- If Shooter loses, Shooter loses `$40`.

## Streak Meter

The streak meter is a central table drama mechanic.

Streak should build from:

- Hitting your point: +2.
- Winning on the come-out: +1.
- Winning one accepted side bet on one roll: +0.5; winning two or more on that same roll: +1 total.
- Winning a Double Up wager: +3 in addition to the normal shooter win.
- Winning after fade momentum has built, beginning after the third fade.

The meter has 10 points and belongs to the current Shooter. Side-bet heat counts only when the Shooter wins those bets. Merely lasting several rolls and rolling doubles do not add heat. Come-out 2/3/12 empties the meter even though the Shooter keeps the dice; seven-out empties it and passes the dice.

At full streak:

- Dice enter red/orange hot mode on the next throw, never midway through the roll that filled the meter.
- Red/orange is reserved for streak and cannot be selected as a normal dice color.
- The in-game UI shows only a fire-filled hot meter, not the individual point awards.

## Money on the Ground

Bills on the ground only ever show money that is riding: the shooter's and catcher's shot, proposed bets and locked bets. Money only moves on screen when it actually changes hands:

- **You win:** the bills lift off the loser's spot and gather right above where your hand comes out. Your hand rises palm up and the bills are laid into it one at a time, like a bank teller counting them out. The money sound plays right before each bill touches your palm, and your balance goes up as each one lands. Then your hand closes and drops out.
- **You lose, or money moves between other players:** the bills fly from the loser's spot to the winner's and disappear. (Your balance doesn't jump when you lose -- that money already left your available balance when the bet was made.) No loose or decorative bills lying around -- with bets everywhere, stray money would make it unclear what is being bet. (Owner rule 2026-09-29.)

## Fair Play

Your bankroll is private. Other players see the bets you place, not your remaining balance. The server decides counted dice results and settles each wager once; no player's phone can choose an outcome.

## Dice Colors

Selectable player dice colors:

- Black
- White
- Green
- Blue

Reserved streak color:

- Red/orange hot dice.

## Open Rule Questions

- Payouts: every bet pays even money (1:1) for now, including point bets like `CRAP 10`. Even money favors the CRAP bettor (a 7 comes twice as often as a 10), and the owner knows that. **Street odds are a later addition:** the CRAP bettor puts up more to win less -- $20 to win $10 on 4/10, $15 to win $10 on 5/9, $12 to win $10 on 6/8.
- Fades before momentum starts: **3** (owner confirmed 2026-09-29).
- Double Up: one person covers the full doubled amount, never split (owner confirmed 2026-09-29).
