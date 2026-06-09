using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// Closing sequence, driven independently of Meteorite. Hook Begin() to
// Meteorite's onSequenceComplete event. Flow: wait -> fade out -> rotate the
// camera rig and reveal the villager -> fade in -> villager runs to a spot and
// idles -> dialogue -> on click, fade out -> credits -> return to a scene.
public class VillagerClose : MonoBehaviour
{
    [Header("Start")]
    public float startDelay = 3f;           // wait after Meteorite finishes before fading

    [Header("Fade (reused FadeUI)")]
    public FadeUI fadeUI;                    // full-screen fade-to-black overlay

    [Header("Player camera")]
    public Transform cameraRig;             // rotated while the screen is black
    public float cameraTurnAngle = 90f;     // degrees to rotate the rig (around Y)

    [Header("Villager")]
    public GameObject villager;             // revealed while the screen is black
    public Animator villagerAnimator;       // drives run/idle (auto-fetched if empty)
    public string runTrigger = "Run";
    public string idleTrigger = "Idle";
    public Transform runTarget;             // where the villager runs to
    public float moveSpeed = 2f;            // running speed (m/s)
    public float turnSpeed = 360f;          // turning angular speed (deg/s)
    public float arriveDistance = 0.1f;     // distance to target counted as "arrived" (m)

    [Header("Dialogue")]
    public CanvasGroup dialogueCanvas;      // group; starts hidden (alpha 0)
    public GameObject dialogueButton;       // single dialogue button
    public float dialogueFadeDuration = 0.5f;
    public float afterClickDelay = 1f;      // wait after the player clicks before fading out
    public KeyCode dialogueKey = KeyCode.K; // keyboard fallback for testing without VR

    private bool awaitingDialogueClick = false;

    [Header("Credits")]
    public GameObject creditObject;         // Credit image under FadeUI; off until the final fade
    public float creditHoldDuration = 5f;   // how long credits stay before returning
    public string returnSceneName;          // scene to load after the credits

    void Start()
    {
        if (villager != null)
        {
            if (villagerAnimator == null)
                villagerAnimator = villager.GetComponentInChildren<Animator>(true);
            if (villagerAnimator != null)
                villagerAnimator.applyRootMotion = false;
            villager.SetActive(false);
        }

        HideCanvas(dialogueCanvas);

        // Credits live under FadeUI; keep the object off so the early fade-in
        // shows only black. It is revealed for the final fade-out.
        if (creditObject != null) creditObject.SetActive(false);
    }

    void Update()
    {
        // Keyboard fallback so the dialogue can be advanced without VR.
        if (awaitingDialogueClick && Input.GetKeyDown(dialogueKey))
            OnDialogueClicked();
    }

    // Hook this to Meteorite's onSequenceComplete event in the Inspector.
    public void Begin()
    {
        StartCoroutine(PlayClose());
    }

    IEnumerator PlayClose()
    {
        // 1. Hold after the meteorite sequence finishes.
        yield return new WaitForSeconds(startDelay);

        // 2. Fade to black.
        yield return StartCoroutine(FadeScreen(true));

        // 3. While black: rotate the camera rig and reveal the villager (in idle).
        if (cameraRig != null) cameraRig.Rotate(0f, cameraTurnAngle, 0f);
        if (villager != null) villager.SetActive(true);
        if (villagerAnimator != null) villagerAnimator.SetTrigger(idleTrigger);

        // 4. Fade back in.
        yield return StartCoroutine(FadeScreen(false));

        // 5. Villager runs to the target, then idles.
        if (villagerAnimator != null) villagerAnimator.SetTrigger(runTrigger);
        yield return StartCoroutine(RunToTarget());
        if (villagerAnimator != null) villagerAnimator.SetTrigger(idleTrigger);

        // 6. Show the dialogue; the rest continues from the button click handler.
        yield return StartCoroutine(FadeCanvas(dialogueCanvas, 0f, 1f, dialogueFadeDuration));
        if (dialogueCanvas != null)
        {
            dialogueCanvas.interactable = true;
            dialogueCanvas.blocksRaycasts = true;
        }
        awaitingDialogueClick = true;
    }

    // Hook to the dialogue button's OnClick (also fired by dialogueKey).
    public void OnDialogueClicked()
    {
        if (!awaitingDialogueClick) return;  // ignore double/early triggers
        awaitingDialogueClick = false;
        StartCoroutine(EndDialogueAndCredits());
    }

    IEnumerator EndDialogueAndCredits()
    {
        if (dialogueCanvas != null)
        {
            dialogueCanvas.interactable = false;
            dialogueCanvas.blocksRaycasts = false;
        }

        // Wait a beat after the click, then reveal the credits so they fade in
        // together with the black screen (they share FadeUI's CanvasGroup).
        yield return new WaitForSeconds(afterClickDelay);
        if (dialogueCanvas != null) dialogueCanvas.alpha = 0f;
        if (creditObject != null) creditObject.SetActive(true);

        if (AudioManager.Instance != null && fadeUI != null)
            AudioManager.Instance.FadeOutMusic(fadeUI.fadeDuration);
        yield return StartCoroutine(FadeScreen(true));

        // Credits are now fully visible on the black screen; hold, then return.
        yield return new WaitForSeconds(creditHoldDuration);

        if (!string.IsNullOrEmpty(returnSceneName))
            SceneManager.LoadScene(returnSceneName);
    }

    // Run toward runTarget until within arriveDistance, keeping facing it.
    IEnumerator RunToTarget()
    {
        if (villager == null || runTarget == null) yield break;
        Transform t = villager.transform;

        while (true)
        {
            Vector3 pos = t.position;
            // Move on the horizontal plane only, keeping current height (assumes flat ground).
            Vector3 dest = new Vector3(runTarget.position.x, pos.y, runTarget.position.z);

            if ((dest - pos).sqrMagnitude <= arriveDistance * arriveDistance)
                break;

            Vector3 dir = dest - pos;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion want = Quaternion.LookRotation(dir);
                t.rotation = Quaternion.RotateTowards(
                    t.rotation, want, turnSpeed * Time.deltaTime);
            }

            t.position = Vector3.MoveTowards(pos, dest, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    // Drive the shared FadeUI overlay and wait for it to finish.
    IEnumerator FadeScreen(bool toBlack)
    {
        if (fadeUI == null) yield break;
        if (toBlack) fadeUI.FadeOut();
        else fadeUI.FadeIn();
        yield return new WaitForSeconds(fadeUI.fadeDuration);
    }

    // Lerp a canvas group's alpha from -> to over duration.
    IEnumerator FadeCanvas(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;

        float t = 0f;
        cg.alpha = from;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    void HideCanvas(CanvasGroup cg)
    {
        if (cg == null) return;
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}
