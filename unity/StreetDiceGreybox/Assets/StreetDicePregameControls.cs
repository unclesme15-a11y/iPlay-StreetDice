using UnityEngine;

public partial class StreetDiceGreyboxController
{
    private GUIStyle pregameVolumeTrack, pregameVolumeThumb;
    private Texture2D pregameThumbTexture;

    private float DrawPregameVolume(Rect rect, float value)
    {
        if (pregameVolumeTrack == null)
        {
            pregameThumbTexture = new Texture2D(3, 3, TextureFormat.RGBA32, false);
            pregameThumbTexture.name = "Pregame volume thumb";
            pregameThumbTexture.filterMode = FilterMode.Point;
            var pixels = new Color[9];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0.82f, 0.7f, 0.48f);
            pixels[4] = new Color(0.19f, 0.21f, 0.22f);
            pregameThumbTexture.SetPixels(pixels);
            pregameThumbTexture.Apply();
            pregameVolumeTrack = new GUIStyle(GUIStyle.none) { fixedHeight = 40 };
            pregameVolumeThumb = new GUIStyle(GUIStyle.none)
            {
                fixedWidth = 28, fixedHeight = 32, margin = new RectOffset(0, 0, 4, 4),
                border = new RectOffset(1, 1, 1, 1)
            };
            pregameVolumeThumb.normal.background = pregameThumbTexture;
            pregameVolumeThumb.hover.background = pregameThumbTexture;
            pregameVolumeThumb.active.background = pregameThumbTexture;
        }
        Color tint = GUI.color;
        GUI.color = new Color(0.5f, 0.54f, 0.55f);
        GUI.DrawTexture(new Rect(rect.x + 14, rect.y + 15, rect.width - 28, 10), Texture2D.whiteTexture);
        GUI.color = new Color(0.08f, 0.1f, 0.11f);
        GUI.DrawTexture(new Rect(rect.x + 15, rect.y + 16, rect.width - 30, 8), Texture2D.whiteTexture);
        GUI.color = new Color(0.05f, 0.8f, 0.95f);
        GUI.DrawTexture(new Rect(rect.x + 15, rect.y + 16, (rect.width - 30) * value, 8), Texture2D.whiteTexture);
        GUI.color = Color.white;
        float changed = GUI.HorizontalSlider(rect, value, 0, 1, pregameVolumeTrack, pregameVolumeThumb);
        GUI.color = tint;
        return changed;
    }

    private bool DrawPregameTutorial(Rect rect, bool enabled)
    {
        return DrawOptionSwitch(rect, enabled, "Tutorial", "Tutorial mode");
    }

    private bool DrawOptionSwitch(Rect rect, bool value, string label, string tooltip = "")
    {
        bool active = GUI.enabled;
        bool next = value;
        if (GUI.Button(rect, new GUIContent("", tooltip), GUIStyle.none) && active) next = !value;

        Color original = GUI.color;
        float opacity = active ? 1f : 0.4f;
        var labelStyle = new GUIStyle(GUI.skin.label)
        {
            font = GUI.skin.label.font,
            fontSize = 18,
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip
        };
        labelStyle.normal.textColor = new Color(0.9f, 0.91f, 0.91f, opacity);
        GUI.color = new Color(original.r, original.g, original.b, original.a * opacity);
        GUI.Label(new Rect(rect.x + 2f, rect.y, rect.width - 106f, rect.height), label, labelStyle);

        float width = 92f;
        float height = Mathf.Min(40f, rect.height);
        var frame = new Rect(rect.xMax - width, rect.y + (rect.height - height) * 0.5f, width, height);
        DrawSwitchFill(new Rect(frame.x - 3f, frame.y + 4f, frame.width + 6f, frame.height), new Color(0.025f, 0.025f, 0.025f));
        DrawSwitchFill(frame, new Color(0.10f, 0.10f, 0.10f));
        var left = new Rect(frame.x + 3f, frame.y + (next ? 7f : 3f), 42f, next ? height - 11f : height - 8f);
        var right = new Rect(frame.x + 47f, frame.y + (next ? 3f : 7f), 42f, next ? height - 8f : height - 11f);
        DrawSwitchFill(left, next ? new Color(0.13f, 0.13f, 0.13f) : new Color(0.20f, 0.20f, 0.20f));
        DrawSwitchFill(right, next ? new Color(0.20f, 0.20f, 0.20f) : new Color(0.13f, 0.13f, 0.13f));
        DrawSwitchFill(new Rect(left.x, left.y, left.width, 2f), next ? new Color(0.18f, 0.18f, 0.18f) : new Color(0.27f, 0.27f, 0.27f));
        DrawSwitchFill(new Rect(right.x, right.y, right.width, 2f), next ? new Color(0.27f, 0.27f, 0.27f) : new Color(0.18f, 0.18f, 0.18f));

        var sideStyle = new GUIStyle(labelStyle)
        {
            font = cardMenuDisplayFont,
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16
        };
        sideStyle.normal.textColor = next ? new Color(0.31f, 0.31f, 0.31f) : Color.white;
        GUI.Label(left, "OFF", sideStyle);
        sideStyle.normal.textColor = next ? new Color(0.216f, 0.839f, 0.894f) : new Color(0.31f, 0.31f, 0.31f);
        GUI.Label(right, "ON", sideStyle);
        GUI.color = original;
        return next;
    }

    private static void DrawSwitchFill(Rect rect, Color color)
    {
        Color original = GUI.color;
        GUI.color = new Color(color.r, color.g, color.b, color.a * original.a);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = original;
    }
}
