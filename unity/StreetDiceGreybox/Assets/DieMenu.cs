// ─────────────────────────────────────────────────────────────────────────────
//  DieMenu.cs
//
//  A six-sided main menu built on a die.
//
//  HOW IT MOVES
//    One swipe = one quarter turn. You never have to drag the die all the way
//    round; a short flick is enough. The die follows your finger a little while
//    you swipe (so it feels alive), then either completes the turn or springs
//    back when you let go.
//
//      swipe left / right  →  steps through the four SIDE faces, like a carousel
//      swipe up / down     →  rolls to the TOP or BOTTOM face
//
//  HOW IT SELECTS
//    Whichever face is pointing at the camera is the live selection. Tapping
//    that face fires it. Tapping any other face just turns to it.
//
//  SCENE SETUP (about five minutes)
//    1. Make your die a GameObject at the world origin with a BoxCollider on it.
//       Put it on its own layer — call it "DieMenu".
//    2. Create an empty GameObject called "MenuController" and add this script.
//    3. Drag the die into `Die`, and your menu camera into `Cam`.
//    4. Rotate the die in the Scene view until it looks how you want at rest.
//       Whatever pose you leave it in becomes the menu's home position — the
//       script measures everything from there, so you don't have to do maths.
//    5. Set `Die Layer` to the layer from step 1.
//    6. The six faces are pre-filled with the six cube normals. Give each one a
//       label and hook its On Selected event to whatever it should do.
//
//  A NOTE ON THE FACE NORMALS
//    Each face is identified by the direction it points in the die's OWN space:
//    (0,0,1) is the die's front, (0,1,0) its top, and so on. That never changes
//    no matter how the die is turned, which is what makes it a safe id to
//    hard-code against. If you follow real dice convention, opposite faces sum
//    to seven, so the pip count doubles as a stable face number.
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[Serializable]
public class MenuFaceEvent : UnityEvent<int> { }

[Serializable]
public class MenuFace
{
    [Tooltip("Shown in your UI. Write it as the thing that happens: \"Start Game\", not \"Play menu\".")]
    public string label = "Play";

    [Tooltip("Which way this face points, in the die's own space. One axis, value 1 or -1.")]
    public Vector3 localNormal = Vector3.forward;

    [Tooltip("The die's number for this face. Doubles as the 1-6 keyboard shortcut.")]
    [Range(1, 6)] public int pip = 1;

    [Tooltip("Decorative faces such as the iPlay logo can turn forward but cannot launch a screen.")]
    public bool selectable = true;

    [Tooltip("Optional. An object to switch on while this face is the live selection — a glow, an outline, a label.")]
    public GameObject highlight;

    [Tooltip("What happens when this face is confirmed.")]
    public UnityEvent onSelected;
}

[DisallowMultipleComponent]
public class DieMenu : MonoBehaviour
{
    // ── Scene references ────────────────────────────────────────────────────
    [Header("Scene")]
    [Tooltip("The die itself. Needs a collider if you want tapping to work.")]
    public Transform die;

    [Tooltip("The camera looking at the menu. Leave empty to use Camera.main.")]
    public Camera cam;

    [Tooltip("The layer your die is on, so taps don't hit anything else.")]
    public LayerMask dieLayer = ~0;

    // ── Faces ───────────────────────────────────────────────────────────────
    [Header("Faces")]
    public List<MenuFace> faces = new List<MenuFace>();


    // ── Swipe feel ──────────────────────────────────────────────────────────
    [Header("Swipe feel")]
    [Tooltip("How far a swipe must travel to count, as a fraction of the screen's short edge. 0.06 is about a thumb-flick on a phone.")]
    [Range(0.02f, 0.25f)] public float swipeDistance = 0.06f;

    [Tooltip("A flick faster than this turns the die even if it was short. Pixels per second.")]
    public float flickSpeed = 600f;

    [Tooltip("How far the die leans toward the next face while your finger is still down. 0 = no preview, 1 = follows all the way.")]
    [Range(0f, 1f)] public float dragFollow = 0.35f;

    [Tooltip("Seconds for one quarter turn. Under 0.25 feels frantic, over 0.45 feels sluggish.")]
    public float turnDuration = 0.32f;

    [Tooltip("The shape of the turn. The default overshoots slightly, which is what gives the die its weight.")]
    public AnimationCurve turnEase = DefaultEase();

    [Tooltip("Swipe up rolls the die's face upward and brings the underside round, like a real die. Tick this to flip it.")]
    public bool invertVertical = false;

    // ── Tapping ─────────────────────────────────────────────────────────────
    [Header("Tapping")]
    [Tooltip("Tapping the face that is already pointing at you fires it.")]
    public bool tapFrontConfirms = true;

    [Tooltip("Tapping a side face turns to it instead of firing it. Leave this on — firing a face you can barely see is the fastest way to make the menu feel broken.")]
    public bool tapSideTurns = true;

    // ── Idle ────────────────────────────────────────────────────────────────
    [Header("Idle")]
    [Tooltip("A slow drift while nobody is touching it, so the screen doesn't look frozen.")]
    public bool idleDrift = true;
    public float idleDelay = 2.5f;
    [Range(0f, 8f)] public float idleAmount = 2.5f;

    // ── Events ──────────────────────────────────────────────────────────────
    [Header("Events")]
    [Tooltip("Fires whenever a different face comes to the front. Passes the face's index.")]
    public MenuFaceEvent onFaceChanged;

    [Tooltip("Fires when a face is confirmed. Passes the face's index. The face's own On Selected fires too.")]
    public MenuFaceEvent onFaceConfirmed;

    // ── Internal state ──────────────────────────────────────────────────────
    // The die's pose is just two integers. yaw is unbounded and wraps every 4;
    // pitch is -1 (top face forward), 0 (a side face), or 1 (bottom forward).
    int yaw;
    int pitch;

    Quaternion homeRotation;      // the pose you left the die in, in the Scene view
    Quaternion turnFrom, turnTo;
    float turnT = 1f;             // 1 = finished

    bool pressed;
    bool axisLocked;
    bool horizontal;
    Vector2 pressPos, lastPos;
    float pressTime, lastMoveTime;
    float pointerSpeed;
    int previewDir;               // -1, 0 or +1 along the locked axis

    float idleTimer;
    int frontIndex = -1;
    Rect viewport;

    bool Busy => turnT < 1f;

    public void SetViewport(Rect pixelRect) => viewport = pixelRect;

    // ── Lifecycle ───────────────────────────────────────────────────────────

    void Reset()
    {
        // Standard die: opposite faces sum to seven.
        faces = new List<MenuFace>
        {
            NewFace("Craps",           Vector3.back,    1),
            NewFace("Cee-lo",          Vector3.right,   2),
            NewFace("Global Settings", Vector3.forward, 3),
            NewFace("Exit",            Vector3.left,    4),
            NewFace("iPlay",           Vector3.up,      5),
            NewFace("iPlay",           Vector3.down,    6),
        };
        faces[4].selectable = faces[5].selectable = false;
        turnEase = DefaultEase();
    }

    static MenuFace NewFace(string label, Vector3 normal, int pip)
    {
        return new MenuFace { label = label, localNormal = normal, pip = pip };
    }

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (die == null) die = transform;
        if (turnEase == null || turnEase.length == 0) turnEase = DefaultEase();

        homeRotation = die.rotation;
        turnFrom = turnTo = PoseFor(yaw, pitch);
        die.rotation = turnTo;
    }

    void Start()
    {
        RefreshFrontFace(force: true);
    }

    void Update()
    {
        if (die == null || cam == null) return;

        ReadKeyboard();
        ReadPointer();
        Animate();
    }

    // ── The pose maths ──────────────────────────────────────────────────────
    //
    //  Everything hangs off one idea: yaw turns the die around the axis that
    //  points UP on screen, and pitch turns it around the axis that points
    //  RIGHT on screen. Both are taken from the camera, so the die always turns
    //  the way the player's thumb moved, no matter where the camera sits.
    //
    //  The two rotations are applied BEFORE the home pose (pre-multiplied), so
    //  the home pose stays a purely visual choice — tilt the die however you
    //  like in the Scene view and the swipes still line up.

    Quaternion PoseFor(int yawSteps, int pitchSteps)
    {
        Vector3 up = cam.transform.up;
        Vector3 right = cam.transform.right;

        return Quaternion.AngleAxis(pitchSteps * 90f, right)
             * Quaternion.AngleAxis(yawSteps * 90f, up)
             * homeRotation;
    }

    /// <summary>Which face is pointing at the camera in the given pose?</summary>
    public int FaceAt(int yawSteps, int pitchSteps)
    {
        Quaternion pose = PoseFor(yawSteps, pitchSteps);
        Vector3 toCam = -cam.transform.forward;

        int best = -1;
        float bestDot = float.NegativeInfinity;
        for (int i = 0; i < faces.Count; i++)
        {
            float d = Vector3.Dot(pose * faces[i].localNormal.normalized, toCam);
            if (d > bestDot) { bestDot = d; best = i; }
        }
        return best;
    }

    /// <summary>The live selection — the face currently turned toward the camera.</summary>
    public int FrontFaceIndex => frontIndex;
    public MenuFace FrontFace => frontIndex >= 0 && frontIndex < faces.Count ? faces[frontIndex] : null;

    // ── Moving ──────────────────────────────────────────────────────────────

    /// <summary>One quarter turn. dx is -1/0/+1 for left/right, dy for up/down.</summary>
    public void Step(int dx, int dy)
    {
        if (dx != 0)
        {
            // A horizontal swipe always lands you back on the equator. Coming
            // off the top or bottom face, the die rolls down and round in one
            // move, which reads better than making the player swipe twice.
            pitch = 0;
            yaw += dx;
        }
        else if (dy != 0)
        {
            int wanted = Mathf.Clamp(pitch + dy, -1, 1);
            if (wanted == pitch) { Nudge(0, dy); return; }   // already at a pole
            pitch = wanted;
        }
        else return;

        BeginTurn(PoseFor(yaw, pitch));
        RefreshFrontFace();
    }

    /// <summary>Turn to a specific face, taking the shortest route.</summary>
    public void GoToFace(int index)
    {
        if (index < 0 || index >= faces.Count) return;
        if (!TryFindPose(index, out int ty, out int tp)) return;

        yaw = ty; pitch = tp;
        BeginTurn(PoseFor(yaw, pitch));
        RefreshFrontFace();
    }

    /// <summary>Turn to the face carrying this pip number (1-6).</summary>
    public void GoToPip(int pipNumber)
    {
        int i = faces.FindIndex(f => f.pip == pipNumber);
        if (i >= 0) GoToFace(i);
    }

    // Search the small neighbourhood of poses for the one that brings `index`
    // forward with the fewest steps. Six faces, nine candidate poses — this is
    // far cheaper and far more robust than trying to invert the rotation.
    bool TryFindPose(int index, out int bestYaw, out int bestPitch)
    {
        bestYaw = yaw; bestPitch = pitch;
        int bestCost = int.MaxValue;

        for (int p = -1; p <= 1; p++)
        {
            for (int offset = -2; offset <= 2; offset++)
            {
                int y = yaw + offset;
                if (FaceAt(y, p) != index) continue;

                int cost = Mathf.Abs(offset) + Mathf.Abs(p - pitch);
                if (cost < bestCost) { bestCost = cost; bestYaw = y; bestPitch = p; }
            }
        }
        return bestCost != int.MaxValue;
    }

    /// <summary>Fire the face currently facing the camera.</summary>
    public void Confirm()
    {
        MenuFace f = FrontFace;
        if (f == null) return;
        if (!f.selectable) { Nudge(0, 1); return; }

        f.onSelected?.Invoke();
        onFaceConfirmed?.Invoke(frontIndex);
    }

    void BeginTurn(Quaternion target)
    {
        turnFrom = die.rotation;
        turnTo = target;
        turnT = 0f;
        idleTimer = 0f;
    }

    // A small bounce when you swipe past the top or bottom face, so the die
    // answers you instead of ignoring you.
    void Nudge(int dx, int dy)
    {
        Vector3 axis = dx != 0 ? cam.transform.up : cam.transform.right;
        float amount = (dx != 0 ? dx : dy) * 7f;
        turnFrom = Quaternion.AngleAxis(amount, axis) * PoseFor(yaw, pitch);
        turnTo = PoseFor(yaw, pitch);
        turnT = 0f;
        die.rotation = turnFrom;
    }

    void RefreshFrontFace(bool force = false)
    {
        int now = FaceAt(yaw, pitch);
        if (now == frontIndex && !force) return;

        frontIndex = now;
        for (int i = 0; i < faces.Count; i++)
            if (faces[i].highlight != null)
                faces[i].highlight.SetActive(i == frontIndex);

        onFaceChanged?.Invoke(frontIndex);
    }

    // ── Animation ───────────────────────────────────────────────────────────

    void Animate()
    {
        if (pressed) return;   // the finger is driving; leave it alone

        if (Busy)
        {
            turnT += Time.unscaledDeltaTime / Mathf.Max(0.01f, turnDuration);
            float e = turnEase.Evaluate(Mathf.Clamp01(turnT));
            // Unclamped so an overshooting curve actually overshoots.
            die.rotation = Quaternion.SlerpUnclamped(turnFrom, turnTo, e);

            if (turnT >= 1f)
            {
                turnT = 1f;
                die.rotation = turnTo;
                idleTimer = 0f;
            }
            return;
        }

        idleTimer += Time.unscaledDeltaTime;
        if (!idleDrift || idleTimer < idleDelay) { die.rotation = PoseFor(yaw, pitch); return; }

        // A lazy figure-of-eight, eased in so it doesn't lurch into motion.
        float t = idleTimer - idleDelay;
        float ramp = Mathf.Clamp01(t / 1.5f) * idleAmount;
        float a = Mathf.Sin(t * 0.62f) * ramp;
        float b = Mathf.Sin(t * 0.44f + 1.1f) * ramp;

        die.rotation = Quaternion.AngleAxis(a, cam.transform.right)
                     * Quaternion.AngleAxis(b, cam.transform.up)
                     * PoseFor(yaw, pitch);
    }

    // ── Input ───────────────────────────────────────────────────────────────

    void ReadPointer()
    {
        PointerState p = SamplePointer();
        float threshold = swipeDistance * Mathf.Min(Screen.width, Screen.height);

        if (p.down)
        {
            if (!viewport.Contains(p.position)) return;
            pressed = true;
            axisLocked = false;
            previewDir = 0;
            pressPos = lastPos = p.position;
            pressTime = lastMoveTime = Time.unscaledTime;
            pointerSpeed = 0f;
            idleTimer = 0f;
            turnT = 1f;                       // cancel any turn in flight
            return;
        }

        if (p.held && pressed)
        {
            Vector2 delta = p.position - pressPos;

            // Track speed over the last moment, so a flick is measured by how
            // fast the finger was moving when it left, not by the whole gesture.
            float dt = Time.unscaledTime - lastMoveTime;
            if (dt > 0.001f)
            {
                pointerSpeed = Vector2.Distance(p.position, lastPos) / dt;
                lastPos = p.position;
                lastMoveTime = Time.unscaledTime;
            }

            // Decide once whether this is a horizontal or a vertical gesture.
            if (!axisLocked && delta.magnitude > threshold * 0.25f)
            {
                axisLocked = true;
                horizontal = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y);
            }

            if (axisLocked && dragFollow > 0f)
            {
                float along = horizontal ? delta.x : delta.y;
                previewDir = along > 0 ? 1 : -1;

                int dx = 0, dy = 0;
                if (horizontal) dx = -previewDir;              // drag right → the left face comes round
                else dy = invertVertical ? -previewDir : previewDir;

                int previewYaw = yaw, previewPitch = pitch;
                if (dx != 0) { previewPitch = 0; previewYaw = yaw + dx; }
                else previewPitch = Mathf.Clamp(pitch + dy, -1, 1);

                float progress = Mathf.Clamp01(Mathf.Abs(along) / threshold);
                die.rotation = Quaternion.SlerpUnclamped(
                    PoseFor(yaw, pitch),
                    PoseFor(previewYaw, previewPitch),
                    progress * dragFollow);
            }
            return;
        }

        if (p.up && pressed)
        {
            pressed = false;
            Vector2 delta = p.position - pressPos;
            float held = Time.unscaledTime - pressTime;

            bool isTap = delta.magnitude < threshold * 0.3f && held < 0.35f;
            if (isTap) { HandleTap(p.position); return; }

            bool far = Mathf.Abs(horizontal ? delta.x : delta.y) >= threshold;
            bool flick = pointerSpeed >= flickSpeed;

            if (axisLocked && (far || flick))
            {
                int dir = (horizontal ? delta.x : delta.y) > 0 ? 1 : -1;
                if (horizontal) Step(-dir, 0);
                else Step(0, invertVertical ? -dir : dir);
            }
            else
            {
                BeginTurn(PoseFor(yaw, pitch));   // didn't make it — spring back
            }
        }
    }

    void HandleTap(Vector2 screenPos)
    {
        Vector2 uv = new Vector2((screenPos.x - viewport.x) / viewport.width,
            (screenPos.y - viewport.y) / viewport.height);
        Ray ray = cam.ViewportPointToRay(new Vector3(uv.x, uv.y, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, 200f, dieLayer))
        {
            BeginTurn(PoseFor(yaw, pitch));
            return;
        }

        // The face normal, expressed in the die's own space. This is the bit
        // that makes one collider enough for six buttons.
        Vector3 n = hit.transform.InverseTransformDirection(hit.normal);
        int tapped = NearestFace(n);
        if (tapped < 0) return;

        if (tapped == frontIndex)
        {
            if (tapFrontConfirms) Confirm();
        }
        else if (tapSideTurns)
        {
            GoToFace(tapped);
        }
    }

    int NearestFace(Vector3 localNormal)
    {
        int best = -1;
        float bestDot = 0.5f;   // must be at least roughly aligned
        for (int i = 0; i < faces.Count; i++)
        {
            float d = Vector3.Dot(localNormal.normalized, faces[i].localNormal.normalized);
            if (d > bestDot) { bestDot = d; best = i; }
        }
        return best;
    }

    void ReadKeyboard()
    {
        if (Busy || pressed) return;

        // Arrows are deliberately the opposite of swipes. Pressing Left picks
        // the face on the left; swiping left pushes the die left and brings the
        // right-hand face round. That is the standard pairing on every carousel,
        // and players read it correctly without being told.
        if (KeyLeft())  Step(-1, 0);
        if (KeyRight()) Step(1, 0);
        if (KeyUp())    Step(0, invertVertical ? 1 : -1);   // -1 = top face forward
        if (KeyDown())  Step(0, invertVertical ? -1 : 1);
        if (KeyConfirm()) Confirm();

        int pipKey = PipKey();
        if (pipKey > 0) GoToPip(pipKey);
    }

    // ── Input backend ───────────────────────────────────────────────────────
    //
    //  Unity has two input systems and projects use either or both. These few
    //  wrappers mean the rest of the file doesn't have to care which one you
    //  picked in Project Settings → Player → Active Input Handling.

    struct PointerState
    {
        public bool down, held, up;
        public Vector2 position;
    }

    PointerState SamplePointer()
    {
        PointerState p = default;

#if ENABLE_INPUT_SYSTEM
        Touchscreen ts = Touchscreen.current;
        if (ts != null)
        {
            var t = ts.primaryTouch;
            bool any = t.press.isPressed || t.press.wasPressedThisFrame || t.press.wasReleasedThisFrame;
            if (any)
            {
                p.down = t.press.wasPressedThisFrame;
                p.held = t.press.isPressed;
                p.up = t.press.wasReleasedThisFrame;
                p.position = t.position.ReadValue();
                return p;
            }
        }

        Mouse m = Mouse.current;
        if (m != null)
        {
            p.down = m.leftButton.wasPressedThisFrame;
            p.held = m.leftButton.isPressed;
            p.up = m.leftButton.wasReleasedThisFrame;
            p.position = m.position.ReadValue();
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            p.position = t.position;
            p.down = t.phase == TouchPhase.Began;
            p.held = t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Began;
            p.up = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
            return p;
        }

        p.down = Input.GetMouseButtonDown(0);
        p.held = Input.GetMouseButton(0);
        p.up = Input.GetMouseButtonUp(0);
        p.position = Input.mousePosition;
#endif
        return p;
    }

#if ENABLE_INPUT_SYSTEM
    static bool Pressed(KeyControl k) => k != null && k.wasPressedThisFrame;
    bool KeyLeft()    => Keyboard.current != null && Pressed(Keyboard.current.leftArrowKey);
    bool KeyRight()   => Keyboard.current != null && Pressed(Keyboard.current.rightArrowKey);
    bool KeyUp()      => Keyboard.current != null && Pressed(Keyboard.current.upArrowKey);
    bool KeyDown()    => Keyboard.current != null && Pressed(Keyboard.current.downArrowKey);
    bool KeyConfirm() => Keyboard.current != null &&
                         (Pressed(Keyboard.current.enterKey) || Pressed(Keyboard.current.spaceKey));

    int PipKey()
    {
        Keyboard k = Keyboard.current;
        if (k == null) return 0;
        if (Pressed(k.digit1Key)) return 1;
        if (Pressed(k.digit2Key)) return 2;
        if (Pressed(k.digit3Key)) return 3;
        if (Pressed(k.digit4Key)) return 4;
        if (Pressed(k.digit5Key)) return 5;
        if (Pressed(k.digit6Key)) return 6;
        return 0;
    }
#elif ENABLE_LEGACY_INPUT_MANAGER
    bool KeyLeft()    => Input.GetKeyDown(KeyCode.LeftArrow);
    bool KeyRight()   => Input.GetKeyDown(KeyCode.RightArrow);
    bool KeyUp()      => Input.GetKeyDown(KeyCode.UpArrow);
    bool KeyDown()    => Input.GetKeyDown(KeyCode.DownArrow);
    bool KeyConfirm() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);

    int PipKey()
    {
        for (int i = 1; i <= 6; i++)
            if (Input.GetKeyDown(KeyCode.Alpha0 + i)) return i;
        return 0;
    }
#else
    bool KeyLeft() => false;
    bool KeyRight() => false;
    bool KeyUp() => false;
    bool KeyDown() => false;
    bool KeyConfirm() => false;
    int PipKey() => 0;
#endif

    // ── Defaults ────────────────────────────────────────────────────────────

    static AnimationCurve DefaultEase()
    {
        // Quick out of the gate, a touch past the target, then settles.
        return new AnimationCurve(
            new Keyframe(0f,    0f,    0f,   2.2f),
            new Keyframe(0.62f, 1.06f, 0.4f, 0.4f),
            new Keyframe(1f,    1f,    0f,   0f));
    }
}
