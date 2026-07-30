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
- `7` before point: Shooter loses.

Open decision:

- Confirm whether Shooter keeps the dice after losing during point phase, or only after come-out crap-out.

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
- Whether Shooter keeps dice after losing during point phase.
- Exact number of fades before momentum starts, currently assumed as after 3 fades.
- Whether Double Up requires one Catcher to cover full amount or can be split.
- Whether side bets have a timeout before roll lock.
