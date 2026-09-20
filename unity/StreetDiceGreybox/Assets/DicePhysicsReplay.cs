using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Simulate unnumbered collision shapes, then replay the poses with the confirmed faces.
// A constant cube-symmetry visual rotation avoids changing the result at the last frame.
public sealed class DicePhysicsReplay
{
    public const float Step = 1f / 120f;
    public readonly List<Pose[]> Frames = new();
    public readonly List<Impact> Impacts = new();
    public readonly struct Impact
    {
        public readonly float Time;
        public readonly string Surface;
        public readonly float Strength;
        public Impact(float time, string surface, float strength) { Time = time; Surface = surface; Strength = strength; }
    }
    public float Duration => (Frames.Count - 1) * Step;

    public static DicePhysicsReplay Create(Vector3[] starts, Quaternion[] rotations, float size, float groundY, Vector3 direction, int seed, float power = 0.5f)
    {
        var replay = new DicePhysicsReplay();
        var scene = SceneManager.CreateScene("Dice roll simulation " + Guid.NewGuid(), new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        var physics = scene.GetPhysicsScene();
        var material = new PhysicsMaterial("Dice on pavement")
        {
            dynamicFriction = 0.65f, staticFriction = 0.75f, bounciness = 0.24f,
            frictionCombine = PhysicsMaterialCombine.Maximum, bounceCombine = PhysicsMaterialCombine.Minimum
        };
        var bodies = new Rigidbody[starts.Length];
        var collisionMesh = StreetDiceGreyboxController.CreateRoundedCubeMesh(size * 0.5f, size * 0.115f, 3);
        try
        {
            var ground = new GameObject("Simulation pavement");
            SceneManager.MoveGameObjectToScene(ground, scene);
            ground.transform.position = new Vector3(0f, groundY - 0.05f, 0f);
            var floor = ground.AddComponent<BoxCollider>();
            floor.size = new Vector3(40f, 0.1f, 40f);
            floor.sharedMaterial = material;
            // These collision-only surfaces align with the approved photographic door and brick.
            AddWall(scene, "Metal", new Vector3(0, groundY + 1.5f, 1.04f), new Vector3(2.9f, 3, 0.12f), material);
            AddWall(scene, "Brick", new Vector3(-2.2f, groundY + 1.5f, 1.04f), new Vector3(1.5f, 3, 0.12f), material);
            AddWall(scene, "Brick", new Vector3(2.2f, groundY + 1.5f, 1.04f), new Vector3(1.5f, 3, 0.12f), material);
            AddWall(scene, "Brick", new Vector3(-2.85f, groundY + 1.5f, -1.7f), new Vector3(0.12f, 3, 5.5f), material);
            AddWall(scene, "Brick", new Vector3(2.85f, groundY + 1.5f, -1.7f), new Vector3(0.12f, 3, 5.5f), material);
            var random = new System.Random(seed);
            for (int i = 0; i < starts.Length; i++)
            {
                var die = new GameObject("Simulation die " + i);
                SceneManager.MoveGameObjectToScene(die, scene);
                die.transform.SetPositionAndRotation(starts[i], rotations[i]);
                var collider = die.AddComponent<MeshCollider>();
                collider.sharedMesh = collisionMesh;
                collider.convex = true;
                collider.contactOffset = size * 0.015f;
                collider.sharedMaterial = material;
                var body = die.AddComponent<Rigidbody>();
                bodies[i] = body;
                body.mass = 0.006f;
                body.linearDamping = 0.06f;
                body.angularDamping = 0.12f;
                body.maxAngularVelocity = 40f;
                body.solverIterations = 12;
                body.solverVelocityIterations = 8;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                float jitter = (float)random.NextDouble();
                body.linearVelocity = direction.normalized * (Mathf.Lerp(2.0f, 8.6f, Mathf.Clamp01(power)) + jitter * 0.3f) + Vector3.up * (0.45f + jitter * 0.15f)
                    + Vector3.right * ((float)random.NextDouble() - 0.5f) * 0.45f;
                body.angularVelocity = new Vector3(9f + jitter * 9f, 5f - jitter * 10f, 7f + jitter * 8f);
            }
            replay.Record(bodies);
            int quietSteps = 0;
            var previousVelocity = new Vector3[bodies.Length];
            var lastImpact = new float[bodies.Length];
            for (int step = 0; step < 720; step++)
            {
                for (int i = 0; i < bodies.Length; i++) previousVelocity[i] = bodies[i].linearVelocity;
                physics.Simulate(Step);
                replay.Record(bodies);
                for (int i = 0; i < bodies.Length; i++)
                {
                    var p = bodies[i].position;
                    float time = (step + 1) * Step;
                    bool hitBack = previousVelocity[i].z > 0.3f && bodies[i].linearVelocity.z < 0 && p.z > 0.80f;
                    bool hitSide = Mathf.Abs(p.x) > 2.6f && previousVelocity[i].x * bodies[i].linearVelocity.x < 0;
                    bool hitGround = previousVelocity[i].y < -0.4f && bodies[i].linearVelocity.y >= -0.05f && p.y < groundY + size;
                    if ((hitBack || hitSide || hitGround) && time - lastImpact[i] > 0.04f)
                    {
                        string surface = hitBack ? Mathf.Abs(p.x) < 1.45f ? "Metal" : "Brick" : hitSide ? "Brick" : "Pavement";
                        float strength = Mathf.Clamp01((previousVelocity[i] - bodies[i].linearVelocity).magnitude / 8f);
                        replay.Impacts.Add(new Impact(time, surface, strength));
                        lastImpact[i] = time;
                    }
                }
                bool quiet = step > 60;
                foreach (var body in bodies)
                    quiet &= body.IsSleeping() || (body.linearVelocity.sqrMagnitude < 0.0001f && body.angularVelocity.sqrMagnitude < 0.001f);
                quietSteps = quiet ? quietSteps + 1 : 0;
                if (quietSteps >= 18) break;
            }
            if (quietSteps < 18) throw new InvalidOperationException("Dice simulation did not settle.");
            return replay;
        }
        finally
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(root);
                else UnityEngine.Object.DestroyImmediate(root);
            }
            SceneManager.UnloadSceneAsync(scene);
            if (Application.isPlaying) UnityEngine.Object.Destroy(material);
            else UnityEngine.Object.DestroyImmediate(material);
            if (Application.isPlaying) UnityEngine.Object.Destroy(collisionMesh);
            else UnityEngine.Object.DestroyImmediate(collisionMesh);
        }
    }

    private static void AddWall(Scene scene, string name, Vector3 position, Vector3 size, PhysicsMaterial material)
    {
        var wall = new GameObject(name);
        SceneManager.MoveGameObjectToScene(wall, scene);
        wall.transform.position = position;
        var collider = wall.AddComponent<BoxCollider>();
        collider.size = size;
        collider.sharedMaterial = material;
    }

    private void Record(Rigidbody[] bodies)
    {
        var frame = new Pose[bodies.Length];
        for (int i = 0; i < bodies.Length; i++) frame[i] = new Pose(bodies[i].position, bodies[i].rotation);
        Frames.Add(frame);
    }

    public Pose Sample(float elapsed, int index)
    {
        float frame = Mathf.Clamp(elapsed / Step, 0f, Frames.Count - 1);
        int a = Mathf.FloorToInt(frame), b = Mathf.Min(a + 1, Frames.Count - 1);
        return new Pose(Vector3.Lerp(Frames[a][index].position, Frames[b][index].position, frame - a),
            Quaternion.Slerp(Frames[a][index].rotation, Frames[b][index].rotation, frame - a));
    }

    public Quaternion FaceCorrection(int index, Vector3 desiredFace)
    {
        var final = Frames[Frames.Count - 1][index].rotation;
        var axes = new[] { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        var top = Vector3.up;
        float best = -1f;
        foreach (var axis in axes)
        {
            float dot = Vector3.Dot(final * axis, Vector3.up);
            if (dot > best) { best = dot; top = axis; }
        }
        if (best < 0.985f) throw new InvalidOperationException("Dice simulation ended cocked.");
        return Quaternion.FromToRotation(desiredFace, top);
    }
}
