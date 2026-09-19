using System.Security.Cryptography;

namespace IPlayStreetDice.Server.Core;

public static class DiceRollFairness
{
    public static DiceRoll ResolveRoll(bool allowClientSuppliedRoll, int? clientDie1, int? clientDie2)
    {
        if (allowClientSuppliedRoll && clientDie1.HasValue && clientDie2.HasValue)
        {
            return new DiceRoll(clientDie1.Value, clientDie2.Value);
        }

        return new DiceRoll(RandomNumberGenerator.GetInt32(1, 7), RandomNumberGenerator.GetInt32(1, 7));
    }
}
