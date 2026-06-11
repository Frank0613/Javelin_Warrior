using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;

// Opening intro: Villager looks around, switches to idle, turns to face the
// target, runs to it, then turns to face the player. The player does nothing;
// pacing is driven by this script while the Animator only swaps clips.
[RequireComponent(typeof(Animator))]
public class VillagerIntro : MonoBehaviour
{
    [Header("Animator trigger names (must match the Animator)")]
    public string idleTrigger = "Idle";
    public string runTrigger = "Run";
    public string cryTrigger = "Cry";

    [Header("Timing")]
    public float lookAroundDuration = 5f;   // entry hold so the player can look at the scene
    public float turnToRunDelay = 0.5f;     // pause after facing the target, before running
    public float arriveDelay = 0.5f;        // pause after arriving, before turning to the player

    [Header("Movement")]
    public Transform target;                // destination to run to
    public float moveSpeed = 2f;            // running speed (m/s)
    public float turnSpeed = 360f;          // turning angular speed (deg/s)
    public float arriveDistance = 0.1f;     // distance to target counted as "arrived" (m)
    public float faceAngleThreshold = 1f;   // angle within which a turn is counted as done (deg)

    [Header("Facing the player")]
    public Transform player;                // who to face after arriving; defaults to Camera.main

    [Header("Tree Boss")]
    public GameObject treeBoss;             // appears when the villager first turns; hidden at start
    public SfxCue bossShowSfx;              // played once as the Tree Boss appears

    [Header("Dialogue")]
    public CanvasGroup dialogueCanvas;      // Dialogue_Canvas group; starts hidden (alpha 0)
    public GameObject dialogue01;           // first dialogue button (shown first)
    public GameObject dialogue02;           // second dialogue button (shown after 01)
    public float cryToDialogueDelay = 2f;   // wait after crying starts before the dialogue appears
    public float dialogueFadeDuration = 0.5f; // fade in/out time for the dialogue canvas
    public Transform lookTargetAfterDialogue; // who/what to face after the dialogue ends
    public SfxCue villageTalkSfx;           // played once each time a dialogue button appears

    [Header("Scene transition")]
    public FadeUI fadeUI;                   // fade-to-black overlay; reused from the rest of the game
    public string nextSceneName;            // scene to load after the intro ends
    public float endHoldDuration = 3f;      // hold after the final turn before fading out

    [Header("On complete")]
    public UnityEvent onComplete;           // invoked after the whole intro, just before the fade

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        // Position is driven by script, so disable root motion to avoid conflicts.
        animator.applyRootMotion = false;

        if (player == null && Camera.main != null)
            player = Camera.main.transform;
    }

    void Start()
    {
        // Dialogue canvas starts fully hidden and non-interactive.
        if (dialogueCanvas != null)
        {
            dialogueCanvas.alpha = 0f;
            dialogueCanvas.interactable = false;
            dialogueCanvas.blocksRaycasts = false;
        }
        if (dialogue01 != null) dialogue01.SetActive(true);
        if (dialogue02 != null) dialogue02.SetActive(false);

        // Tree Boss is hidden until the villager first turns.
        if (treeBoss != null) treeBoss.SetActive(false);

        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        // 1. Look around on entry.
        yield return new WaitForSeconds(lookAroundDuration);

        // 2. Switch to idle, then turn (code-driven) to face the target.
        animator.SetTrigger(idleTrigger);
        yield return StartCoroutine(FaceTowards(target));

        // 3. Short pause, then start running.
        yield return new WaitForSeconds(turnToRunDelay);

        // 4. Run to the target while playing the run animation.
        animator.SetTrigger(runTrigger);
        yield return StartCoroutine(RunToTarget());

        // 5. Pause after arriving.
        yield return new WaitForSeconds(arriveDelay);

        // 6. Switch back to idle and turn to face the player.
        animator.SetTrigger(idleTrigger);
        yield return StartCoroutine(FaceTowards(player));

        // 7. Cry.
        animator.SetTrigger(cryTrigger);

        // 8. After crying for a moment, fade in the dialogue and hand control to the buttons.
        yield return new WaitForSeconds(cryToDialogueDelay);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(villageTalkSfx);
        yield return StartCoroutine(FadeCanvas(0f, 1f));

        if (dialogueCanvas != null)
        {
            dialogueCanvas.interactable = true;
            dialogueCanvas.blocksRaycasts = true;
        }
        // The rest of the flow continues from the button click handlers below.
    }

    // Hook to dialogue01's Button OnClick: hide 01, show 02.
    public void OnDialogue01Clicked()
    {
        if (dialogue01 != null) dialogue01.SetActive(false);
        if (dialogue02 != null) dialogue02.SetActive(true);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(villageTalkSfx);
    }

    // Hook to dialogue02's Button OnClick: close dialogue, return to idle, face the target.
    public void OnDialogue02Clicked()
    {
        StartCoroutine(EndDialogue());
    }

    IEnumerator EndDialogue()
    {
        if (dialogueCanvas != null)
        {
            dialogueCanvas.interactable = false;
            dialogueCanvas.blocksRaycasts = false;
        }

        // Fade the dialogue out and switch from crying back to idle, facing the chosen target.
        // The Tree Boss appears the moment the villager starts this final turn.
        yield return StartCoroutine(FadeCanvas(1f, 0f));
        animator.SetTrigger(idleTrigger);
        if (treeBoss != null) treeBoss.SetActive(true);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(bossShowSfx);
        yield return StartCoroutine(FaceTowards(lookTargetAfterDialogue));

        // Hold on the final pose, then fade out and load the next scene.
        yield return new WaitForSeconds(endHoldDuration);
        onComplete?.Invoke();
        yield return StartCoroutine(FadeOutAndLoad());
    }

    IEnumerator FadeOutAndLoad()
    {
        if (fadeUI != null)
        {
            fadeUI.FadeOut();
            if (AudioManager.Instance != null)
                AudioManager.Instance.FadeOutMusic(fadeUI.fadeDuration);
            yield return new WaitForSeconds(fadeUI.fadeDuration);
        }

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    // Lerp the dialogue canvas alpha from -> to over dialogueFadeDuration.
    IEnumerator FadeCanvas(float from, float to)
    {
        if (dialogueCanvas == null) yield break;

        float t = 0f;
        dialogueCanvas.alpha = from;
        while (t < dialogueFadeDuration)
        {
            t += Time.deltaTime;
            dialogueCanvas.alpha = Mathf.Lerp(from, to, t / dialogueFadeDuration);
            yield return null;
        }
        dialogueCanvas.alpha = to;
    }

    // Rotate in place until facing the given transform (within faceAngleThreshold).
    IEnumerator FaceTowards(Transform t)
    {
        if (t == null) yield break;

        while (true)
        {
            Vector3 dir = t.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) yield break;

            Quaternion want = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, want, turnSpeed * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, want) <= faceAngleThreshold)
                yield break;

            yield return null;
        }
    }

    // Run toward the target until within arriveDistance, keeping facing the target.
    IEnumerator RunToTarget()
    {
        if (target == null) yield break;

        while (true)
        {
            Vector3 pos = transform.position;
            // Move on the horizontal plane only, keeping current height (assumes flat ground).
            Vector3 dest = new Vector3(target.position.x, pos.y, target.position.z);

            if ((dest - pos).sqrMagnitude <= arriveDistance * arriveDistance)
                break;

            Vector3 dir = dest - pos;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion want = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, want, turnSpeed * Time.deltaTime);
            }

            transform.position = Vector3.MoveTowards(pos, dest, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
