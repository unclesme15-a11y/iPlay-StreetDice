using IPlayStreetDice.Server.Core;

namespace IPlayStreetDice.Tests;

public class DiceRollFairnessTests
{
    [Fact]
    public void ClientSuppliedDiceAreIgnoredByDefault()
    {
        var sawDifferentResult = false;
        for (var i = 0; i < 200; i++)
        {
            var roll = DiceRollFairness.ResolveRoll(allowClientSuppliedRoll: false, clientDie1: 3, clientDie2: 4);
            Assert.True(roll.IsValid);
            if (roll.Die1 != 3 || roll.Die2 != 4)
            {
                sawDifferentResult = true;
                break;
            }
        }

        Assert.True(sawDifferentResult, "Server should generate its own dice values when client-supplied rolls are not explicitly allowed.");
    }

    [Fact]
    public void ClientSuppliedDiceAreHonoredWhenExplicitlyAllowed()
    {
        var roll = DiceRollFairness.ResolveRoll(allowClientSuppliedRoll: true, clientDie1: 2, clientDie2: 5);

        Assert.Equal(new DiceRoll(2, 5), roll);
    }

    [Fact]
    public void MissingClientDiceFallBackToServerGeneratedEvenWhenAllowed()
    {
        var roll = DiceRollFairness.ResolveRoll(allowClientSuppliedRoll: true, clientDie1: null, clientDie2: null);

        Assert.True(roll.IsValid);
    }

    [Fact]
    public void ServerGeneratedRollsCoverTheFullOneToSixRange()
    {
        var seenValues = new HashSet<int>();
        for (var i = 0; i < 500; i++)
        {
            var roll = DiceRollFairness.ResolveRoll(allowClientSuppliedRoll: false, clientDie1: null, clientDie2: null);
            seenValues.Add(roll.Die1);
            seenValues.Add(roll.Die2);
        }

        Assert.Equal(new HashSet<int> { 1, 2, 3, 4, 5, 6 }, seenValues);
    }
}
