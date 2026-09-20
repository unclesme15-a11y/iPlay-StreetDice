using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;

namespace IPlayStreetDice.Server.Core;

public readonly record struct DiceGesture(float Power, float Aim, bool LeftHanded = false)
{
    public bool IsValid => float.IsFinite(Power) && float.IsFinite(Aim)
        && Power is >= 0 and <= 1 && Aim is >= -1 and <= 1;
}

public readonly record struct PhysicalDiePose(Vector3 Position, Quaternion Rotation);
public sealed record PhysicalDiceThrow(IReadOnlyList<int> Faces, IReadOnlyList<PhysicalDiePose[]> Frames)
{
    public bool IsCounted => Faces.All(face => face is >= 1 and <= 6);
    public float Duration => (Frames.Count - 1) * ServerDicePhysics.Step;
}

public static class ServerDicePhysics
{
    public const float Step = 1f / 120f;
    private const float DieSize = 0.09f;
    private const float DoorHalfWidth = 1.45f;

    public static PhysicalDiceThrow Simulate(DiceGesture gesture, int diceCount, int seed, int? launchSeed = null)
    {
        if (!gesture.IsValid) throw new ArgumentOutOfRangeException(nameof(gesture));
        if (diceCount is not (2 or 3)) throw new ArgumentOutOfRangeException(nameof(diceCount));

        var pool = new BufferPool();
        var simulation = Simulation.Create(pool, new DiceContacts(), new DiceGravity(), new SolveDescription(12, 2));
        try
        {
            var floorShape = simulation.Shapes.Add(new Box(8f, 0.1f, 8f));
            simulation.Statics.Add(new StaticDescription(new Vector3(0, -0.05f, 0), floorShape));
            AddWall(simulation, new Vector3(0, 1.5f, 1.04f), new Box(2.9f, 3f, 0.12f));
            AddWall(simulation, new Vector3(-2.2f, 1.5f, 1.04f), new Box(1.5f, 3f, 0.12f));
            AddWall(simulation, new Vector3(2.2f, 1.5f, 1.04f), new Box(1.5f, 3f, 0.12f));
            AddWall(simulation, new Vector3(-2.85f, 1.5f, -1.7f), new Box(0.12f, 3f, 5.5f));
            AddWall(simulation, new Vector3(2.85f, 1.5f, -1.7f), new Box(0.12f, 3f, 5.5f));

            var shape = new Box(DieSize, DieSize, DieSize);
            var shapeIndex = simulation.Shapes.Add(shape);
            var inertia = shape.ComputeInertia(0.006f);
            var handles = new BodyHandle[diceCount];
            var random = new Random(seed);
            var launchRandom = new Random(launchSeed ?? (seed ^ 0x5A3C71D));
            Vector3 direction = Vector3.Normalize(new Vector3(gesture.Aim * 0.6f, 0, 1));
            for (int i = 0; i < diceCount; i++)
            {
                float across = i == 0 ? 0.045f : i == 1 ? -0.045f : 0f;
                var start = new Vector3((gesture.LeftHanded ? -0.265f : 0.265f) + across,
                    i == 0 ? 0.675f : 0.665f, i == 2 ? -2.65f : -2.73f - i * 0.01f);
                handles[i] = simulation.Bodies.Add(BodyDescription.CreateDynamic(start, inertia, shapeIndex, 0.01f));
                var body = simulation.Bodies[handles[i]];
                float jitter = (float)random.NextDouble();
                var heldOrientation = Quaternion.Normalize(new Quaternion(-0.26658f,
                    gesture.LeftHanded ? 0.07656f : -0.07656f,
                    gesture.LeftHanded ? -0.02125f : 0.02125f, 0.96053f));
                body.Pose.Orientation = Quaternion.Normalize(heldOrientation
                    * Quaternion.CreateFromYawPitchRoll(
                        (float)launchRandom.NextDouble() * MathF.Tau,
                        (float)launchRandom.NextDouble() * MathF.Tau,
                        (float)launchRandom.NextDouble() * MathF.Tau));
                body.Velocity.Linear = direction * (2f + gesture.Power * 6.6f + jitter * 0.3f)
                    + Vector3.UnitY * (1.1f + jitter * 0.55f)
                    + Vector3.UnitX * ((float)random.NextDouble() - 0.5f) * 0.45f;
                body.Velocity.Angular = new Vector3(
                    ((float)random.NextDouble() - 0.5f) * 80f,
                    ((float)random.NextDouble() - 0.5f) * 80f,
                    ((float)random.NextDouble() - 0.5f) * 80f);
            }

            var frames = new List<PhysicalDiePose[]>(721);
            Record(simulation, handles, frames);
            int quietSteps = 0;
            for (int step = 0; step < 720; step++)
            {
                simulation.Timestep(Step);
                Record(simulation, handles, frames);
                bool quiet = step > 60;
                foreach (var handle in handles)
                {
                    var body = simulation.Bodies[handle];
                    quiet &= body.Velocity.Linear.LengthSquared() < 0.0001f
                        && body.Velocity.Angular.LengthSquared() < 0.001f;
                }
                quietSteps = quiet ? quietSteps + 1 : 0;
                if (quietSteps >= 18) break;
            }
            if (quietSteps < 18) throw new InvalidOperationException("Physical dice did not settle.");

            var faces = new int[diceCount];
            for (int i = 0; i < diceCount; i++)
            {
                var pose = frames[^1][i];
                faces[i] = MathF.Abs(pose.Position.X) > DoorHalfWidth || pose.Position.Z > 1.10f
                    ? 0 : TopFace(pose.Rotation);
            }
            return new PhysicalDiceThrow(faces, frames);
        }
        finally
        {
            simulation.Dispose();
            pool.Clear();
        }
    }

    private static void AddWall(Simulation simulation, Vector3 position, Box box)
        => simulation.Statics.Add(new StaticDescription(position, simulation.Shapes.Add(box)));

    private static void Record(Simulation simulation, BodyHandle[] handles, List<PhysicalDiePose[]> frames)
    {
        var frame = new PhysicalDiePose[handles.Length];
        for (int i = 0; i < handles.Length; i++)
        {
            var pose = simulation.Bodies[handles[i]].Pose;
            frame[i] = new PhysicalDiePose(pose.Position, pose.Orientation);
        }
        frames.Add(frame);
    }

    private static int TopFace(Quaternion orientation)
    {
        var normals = new[] { Vector3.UnitX, -Vector3.UnitX, -Vector3.UnitY,
            Vector3.UnitY, -Vector3.UnitZ, Vector3.UnitZ };
        float best = -1;
        int face = 0;
        for (int i = 0; i < normals.Length; i++)
        {
            float dot = Vector3.Dot(Vector3.Transform(normals[i], orientation), Vector3.UnitY);
            if (dot > best) { best = dot; face = i + 1; }
        }
        return best >= 0.985f ? face : 0;
    }

    private unsafe struct DiceContacts : INarrowPhaseCallbacks
    {
        public void Initialize(Simulation simulation) { }
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b,
            ref float speculativeMargin) => a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB) => true;
        public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair,
            ref TManifold manifold, out PairMaterialProperties pairMaterial)
            where TManifold : unmanaged, IContactManifold<TManifold>
        {
            pairMaterial.FrictionCoefficient = 0.72f;
            pairMaterial.MaximumRecoveryVelocity = 2f;
            pairMaterial.SpringSettings = new SpringSettings(30, 1);
            return true;
        }
        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA,
            int childIndexB, ref ConvexContactManifold manifold) => true;
        public void Dispose() { }
    }

    private struct DiceGravity : IPoseIntegratorCallbacks
    {
        public void Initialize(Simulation simulation) { }
        public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
        public readonly bool AllowSubstepsForUnconstrainedBodies => false;
        public readonly bool IntegrateVelocityForKinematics => false;
        public void PrepareForIntegration(float dt) { gravityDt = Vector3Wide.Broadcast(new Vector3(0, -9.81f * dt, 0)); }
        private Vector3Wide gravityDt;
        public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation,
            BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt,
            ref BodyVelocityWide velocity) => velocity.Linear += gravityDt;
    }
}
