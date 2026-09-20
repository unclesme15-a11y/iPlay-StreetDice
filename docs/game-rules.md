# Game Rules Contract

Working title: **iPlay Street Dice**

Current product name: **iPlay Cee-lo & Craps**.

## Shoot, Sell and Leaving

- A player offered the dice chooses Shoot or Sell before committing a wager.
- Sell is available only with at least three active players and never interrupts a point or unsettled wager.
- A five-second public auction opens. Bids start at $1, have no fixed maximum, and cannot exceed the bidder's available balance.
- The current Catcher holds a forced $1 default bid. If nobody outbids it, the Catcher buys the dice for $1 and begins with +1.5 heat.
- As a disclosed iPlay sale perk, that forced-default buyer cannot roll 2/3/12 while establishing the first point. The protection ends as soon as the point is set; 7 remains possible on come-out and during the point phase.
- The winner pays the seller immediately, becomes Shooter, and the sale price is public. Remaining balances stay private.
- The seller sits out the buyer's main bet and moves to the back of the turn queue, but may watch, use voice chat, and place eligible side bets.
- After the buyer seven-outs, the dice go to the next player from the original clockwise order rather than back to the seller.
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
- `2`, `3`, or `12`: Shooter loses and pays, but keeps the dice.
- Any other total becomes the Shooter's point.

## Point Phase

After a point is established:

- Shooter keeps rolling until the point is hit or a `7` is rolled.
- Point hit: Shooter wins.
- `7` before point: Shooter loses, craps out, and gives up the dice.
- Clockwise handoff: the next player in the active turn queue becomes Shooter. The old Shooter becomes Catcher unless a Sell substitution excludes that seller from the buyer's main bet.

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

Possible side bets:

- Come-out win/loss.
- Shooter hits point.
- Shooter misses point.
- Point group hit/miss.

Side bets do not resolve on faded rolls.

- Every craps roll opens a public 10-second betting window after the main shot is committed.
- Every non-shooter sees a real `10` through `1` countdown. The shooter sees one continuous `15` through `1` countdown because only the shooter receives the additional five-second acceptance period.
- The shooter cannot begin the throw until the 15-second countdown reaches zero.
- New side-bet offers and non-shooter acceptances received after 10 seconds are rejected. The shooter may accept an already offered wager during the additional five seconds; no new offer can be created then.
- If the point remains active after a roll, a fresh 10/15-second role-specific window opens before the shooter's next roll.

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

## Fair Play

Your bankroll is private. Other players see the bets you place and public dice-sale prices, not your remaining balance. The server decides counted dice results and settles each wager once; no player's phone can choose an outcome. The only designed outcome adjustment is the disclosed forced-$1 buyer protection described in Shoot, Sell and Leaving.

## Dice Colors

Selectable player dice colors:

- Black
- White
- Green
- Blue

Reserved streak color:

- Red/orange hot dice.

## Open Rule Questions

- Exact payout multipliers.
- Exact number of fades before momentum starts: MVP prototype uses after 3 fades.
- Whether Double Up requires one Catcher to cover full amount or can be split.
