namespace IPlayStreetDice.Server.Core;

public sealed record CeeLoResolution(
    CeeLoOutcomeType Outcome,
    CeeLoRoll Roll,
    int? Point,
    int Rank,
    string Message);
