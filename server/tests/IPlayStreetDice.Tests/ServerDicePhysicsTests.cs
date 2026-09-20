using IPlayStreetDice.Server.Core;

namespace IPlayStreetDice.Tests;

public sealed class ServerDicePhysicsTests
{
    [Theory]
    [InlineData(2, 713)]
    [InlineData(3, 947)]
    public void PhysicalThrowSettlesOnCountedFaces(int diceCount, int seed)
    {
        var throwResult = ServerDicePhysics.Simulate(new DiceGesture(0.5f, 0f), diceCount, seed);
        Assert.Equal(diceCount, throwResult.Faces.Count);
        Assert.True(throwResult.IsCounted);
        Assert.True(throwResult.Frames.Count > 60);
        Assert.All(throwResult.Frames, frame => Assert.Equal(diceCount, frame.Length));
        Assert.All(throwResult.Frames[^1], die => Assert.InRange(die.Position.Y, 0.03f, 0.10f));
        var transport = PhysicalRollTransport.Frames(throwResult);
        Assert.Equal(diceCount, transport[0].Dice.Length);
        Assert.Equal(diceCount, transport[^1].Dice.Length);
        Assert.Equal(throwResult.Duration, transport[^1].Time, 4);
    }

    [Fact]
    public void SameSeedIsRepeatableAndGestureChangesEarlyTrajectory()
    {
        var weak = ServerDicePhysics.Simulate(new DiceGesture(0.15f, 0f), 2, 713);
        var again = ServerDicePhysics.Simulate(new DiceGesture(0.15f, 0f), 2, 713);
        var strong = ServerDicePhysics.Simulate(new DiceGesture(1f, 0f), 2, 713);
        Assert.Equal(weak.Faces, again.Faces);
        Assert.Equal(weak.Frames[18][0], again.Frames[18][0]);
        Assert.True(strong.Frames[18][0].Position.Z > weak.Frames[18][0].Position.Z + 0.35f);
    }

    [Theory]
    [InlineData(false, 0.31f, 0.22f)]
    [InlineData(true, -0.22f, -0.31f)]
    public void PublicLaunchMatchesHeldHandAndIsIndependentOfPrivateSpin(bool left, float firstX, float secondX)
    {
        var first = ServerDicePhysics.Simulate(new DiceGesture(0.5f, 0f, left), 2, 713, 947);
        var otherSpin = ServerDicePhysics.Simulate(new DiceGesture(0.5f, 0f, left), 2, 714, 947);
        var otherLaunch = ServerDicePhysics.Simulate(new DiceGesture(0.5f, 0f, left), 2, 713, 948);
        Assert.Equal(first.Frames[0], otherSpin.Frames[0]);
        Assert.NotEqual(first.Frames[0][0].Rotation, otherLaunch.Frames[0][0].Rotation);
        Assert.InRange(first.Frames[0][0].Position.X, firstX - 0.001f, firstX + 0.001f);
        Assert.InRange(first.Frames[0][1].Position.X, secondX - 0.001f, secondX + 0.001f);
        Assert.InRange(first.Frames[0][0].Position.Z, -2.731f, -2.729f);
        Assert.InRange(first.Frames[0][0].Position.Y, 0.674f, 0.676f);
    }

    [Fact]
    public void SeedSampleDoesNotFavorOneFaceOrProduceFrequentNoCounts()
    {
        var faces = new int[7];
        int noCounts = 0;
        for (int seed = 0; seed < 256; seed++)
        {
            var throwResult = ServerDicePhysics.Simulate(new DiceGesture(0.5f, 0f), 2, seed);
            foreach (int face in throwResult.Faces)
            {
                faces[face]++;
                if (face == 0) noCounts++;
            }
        }
        Assert.True(noCounts <= 25, $"Too many no-count dice: {noCounts}/512");
        for (int face = 1; face <= 6; face++)
            Assert.True(faces[face] is >= 45 and <= 125,
                $"Face {face}: {faces[face]}/512; all faces: {string.Join(",", faces)}");
    }

    [Theory]
    [InlineData(float.NaN, 0f)]
    [InlineData(0.5f, 2f)]
    [InlineData(-0.1f, 0f)]
    public void RejectsInvalidGesture(float power, float aim)
        => Assert.Throws<ArgumentOutOfRangeException>(() =>
            ServerDicePhysics.Simulate(new DiceGesture(power, aim), 2, 713));
}
