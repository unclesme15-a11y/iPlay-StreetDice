namespace IPlayStreetDice.Server.Core;

public sealed record RollResolution(
    RollResultType Result,
    DiceRoll? Roll,
    int? Point,
    string Message);
