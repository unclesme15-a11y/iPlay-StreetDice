# Game Rules Contract

Working title: **iPlay Street Dice**

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
- MVP handoff: the Catcher becomes the next Shooter, and the old Shooter becomes the Catcher on the same shot amount.

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
- Exact point number.
- Seven before point.

Side bets do not resolve on faded rolls.

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

- Hitting your point.
- Going longer without crapping out.
- Taking and winning side bets.
- Winning after fade momentum has built.
- Winning after Double Up.

Streak should not build from doubles by default.

At full streak:

- Dice enter red/orange hot mode.
- Red/orange is reserved for streak and cannot be selected as a normal dice color.

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
- Whether side bets have a timeout before roll lock.
