using System.Collections.Generic;

// Dice-sale auction removed. Losing the shot is now just Shoot or Pass: Pass
// hands the dice to the next player (PassLocalDice / the server's /pass
// route), no bidding, no money changing hands, no forced-buyer "come-out
// protection". This file used to hold that whole system; localTurnOrder is
// the one piece still used elsewhere (CycleDemoShooter, seat ordering).
public sealed partial class StreetDiceGreyboxController
{
    private readonly List<string> localTurnOrder = new();

    private void ResetTurnOrder()
    {
        localTurnOrder.Clear();
        localTurnOrder.AddRange(DemoShooterOrder);
    }
}
