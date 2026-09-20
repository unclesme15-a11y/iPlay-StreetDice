using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public partial class StreetDiceGreyboxController
{
    private enum CatchStyle { Tap, Plant, Wave }
    private bool fadeInProgress;
    private int selectedFadeStyle;
    private Vector3 catchViewCenter;
    private CatchStyle catchStyle;
    private VideoPlayer catchVideo;
    private GameObject catchVideoSurface;
    private bool klingCatchActive, catchVideoFinished;

    private IEnumerator AnimateCatch()
    {
        // A fade kills the roll immediately; only the hand remains in the display.
        dieA.SetActive(false);
        dieB.SetActive(false);
        dieC.SetActive(false);
        handRig.SetActive(false);
        skyCamVisible = false;
        skyResultVisible = false;
        if (catchVideo == null)
        {
            catchVideoSurface = GameObject.CreatePrimitive(PrimitiveType.Quad);
            catchVideoSurface.name = "Kling hand-only fade";
            Destroy(catchVideoSurface.GetComponent<Collider>());
            catchVideoSurface.layer = 31;
            catchVideoSurface.GetComponent<Renderer>().sharedMaterial = new Material(Resources.Load<Shader>("Fades/CatchVideo"));
            catchVideo = catchVideoSurface.AddComponent<VideoPlayer>();
            catchVideo.playOnAwake = false;
            catchVideo.isLooping = false;
            catchVideo.waitForFirstFrame = true;
            catchVideo.loopPointReached += _ => catchVideoFinished = true;
            catchVideo.audioOutputMode = VideoAudioOutputMode.None;
            catchVideo.renderMode = VideoRenderMode.MaterialOverride;
            catchVideo.targetMaterialRenderer = catchVideoSurface.GetComponent<Renderer>();
            catchVideo.targetMaterialProperty = "_MainTex";
            catchVideoSurface.SetActive(false);
        }
        var clip = Resources.Load<VideoClip>(catchStyle == CatchStyle.Tap ? "Fades/catch-tap-kling" :
            catchStyle == CatchStyle.Wave ? "Fades/catch-wave-kling" : "Fades/catch-plant-kling");
        if (catchVideo.clip != clip) catchVideo.clip = clip;
        var renderer = catchVideoSurface.GetComponent<Renderer>();
        int shade = catcherId == "p1" ? selectedHandSkin : Mathf.Abs(catcherId[catcherId.Length - 1] - '2') % 3;
        var grade = shade == 0 ? new Vector4(2.7f, 3.2f, 3.6f, 1) : shade == 1 ? new Vector4(1.6f, 1.75f, 1.85f, 1) : Vector4.one;
        renderer.sharedMaterial.SetVector("_SkinGrade", grade);
        renderer.enabled = false;
        catchVideoSurface.SetActive(true);
        catchVideo.Prepare();
        float deadline = Time.realtimeSinceStartup + 5;
        while (!catchVideo.isPrepared && Time.realtimeSinceStartup < deadline) yield return null;
        if (!catchVideo.isPrepared)
        {
            catchVideoSurface.SetActive(false);
            Debug.LogError("Kling fade video could not be prepared.");
            yield break;
        }
        catchViewCenter = Vector3.zero;
        catchVideoSurface.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(90, 0, 0));
        catchVideoSurface.transform.localScale = Vector3.one * 0.48f;
        int previousMask = skyCamera.cullingMask;
        Camera.main.cullingMask &= ~(1 << 31);
        skyCamera.cullingMask = 1 << 31;
        skyCamera.aspect = 1;
        skyCamera.orthographicSize = 0.24f;
        skyCamera.transform.position = new Vector3(0, 6, 0);
        klingCatchActive = true;
        catchVideoFinished = false;
        catchVideo.Play();
        float started = Time.realtimeSinceStartup;
        try
        {
            while (!catchVideoFinished && Time.realtimeSinceStartup - started <
                Mathf.Max(3, (float)catchVideo.length / Mathf.Max(0.1f, catchVideo.playbackSpeed) + 2))
            {
                renderer.enabled = catchVideo.frame >= 0;
                skyCamVisible = renderer.enabled;
                yield return null;
            }
            if (!catchVideoFinished) Debug.LogWarning("Kling fade playback timed out; restoring betting.");
        }
        finally
        {
            catchVideo.Stop();
            renderer.enabled = false;
            catchVideoSurface.SetActive(false);
            klingCatchActive = false;
            skyCamVisible = false;
            skyCamera.aspect = 2;
            skyCamera.cullingMask = previousMask;
        }
    }
}
