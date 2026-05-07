using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum Turn { Player, Boss }

    [Header("References")]
    public BossHealth bossHealth;
    public Animator bossAnimator;
    public JavelinThrow javelinThrow;
    public BossAnimEvent bossAnimEvent;
    public BossPatrol bossPatrol;

    [Header("Settings")]
    public float bossAttackDelay = 1f;

    [Header("Game Over")]
    public FadeUI fadeUI;
    public PostProcessingController postProcessingController;
    public float gameOverDelay = 2f;
    public int maxBossAttacks = 3;
    private int bossAttackCount = 0;

    private Turn currentTurn = Turn.Player;
    private bool isBossAttacking = false;
    private bool gameStarted = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(WaitForShowup());
    }

    IEnumerator WaitForShowup()
    {
        if (javelinThrow != null)
        {
            foreach (var r in javelinThrow.javelinMeshRenderers)
                r.enabled = false;
            if (javelinThrow.angleUI != null)
                javelinThrow.angleUI.SetActive(false);
        }

        yield return new WaitForSeconds(0.1f);

        int idleHash = Animator.StringToHash("Idle");
        while (true)
        {
            var stateInfo = bossAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash == idleHash && !bossAnimator.IsInTransition(0))
                break;
            yield return null;
        }

        bossHealth.ShowHealthBar();
        if (javelinThrow != null)
            javelinThrow.ResetJavelin();

        if (bossPatrol != null)
            bossPatrol.StartPatrol();

        gameStarted = true;
        Debug.Log("Game Start! Player Turn.");
    }

    public bool IsPlayerTurn() => gameStarted && currentTurn == Turn.Player && !isBossAttacking;

    public void OnPlayerAttacked()
    {
        if (currentTurn != Turn.Player) return;

        currentTurn = Turn.Boss;
        StartCoroutine(WaitHurtThenAttack());
    }

    public void OnPlayerMissed()
    {
        if (currentTurn != Turn.Player) return;

        currentTurn = Turn.Boss;
        StartCoroutine(WaitThenAttack());
    }

    IEnumerator WaitHurtThenAttack()
    {
        if (bossPatrol != null)
            yield return StartCoroutine(bossPatrol.FaceFront());

        int idleHash = Animator.StringToHash("Idle");
        while (true)
        {
            var stateInfo = bossAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash == idleHash && !bossAnimator.IsInTransition(0))
                break;
            yield return null;
        }

        yield return new WaitForSeconds(bossAttackDelay);

        if (bossHealth.IsDead()) yield break;

        isBossAttacking = true;
        bossAnimator.SetTrigger("attack");
        bossAnimEvent.WaitForAttackEnd();
    }

    IEnumerator WaitThenAttack()
    {
        if (bossPatrol != null)
            yield return StartCoroutine(bossPatrol.FaceFront());

        yield return new WaitForSeconds(bossAttackDelay);

        if (bossHealth.IsDead()) yield break;

        isBossAttacking = true;
        bossAnimator.SetTrigger("attack");
        bossAnimEvent.WaitForAttackEnd();
    }

    // 由 Animation Event 呼叫，計算 Boss 攻擊次數
    public void IncrementBossAttackCount()
    {
        bossAttackCount++;
        Debug.Log($"Boss attack count: {bossAttackCount}");

        if (bossAttackCount >= maxBossAttacks)
        {
            StartCoroutine(GameOverSequence());
        }
    }
    public void EndBossTurn()
    {
        isBossAttacking = false;
        currentTurn = Turn.Player;

        // 已經 GameOver 就不繼續
        if (bossAttackCount >= maxBossAttacks)
            return;

        if (javelinThrow != null)
            javelinThrow.ResetJavelin();

        if (bossPatrol != null)
            bossPatrol.StartPatrol();
    }
    IEnumerator GameOverSequence()
    {
        if (postProcessingController != null)
            yield return StartCoroutine(postProcessingController.GameOverEffect(gameOverDelay));
        else
            yield return new WaitForSeconds(gameOverDelay);

        if (fadeUI != null)
        {
            fadeUI.FadeOut();
            yield return new WaitForSeconds(fadeUI.fadeDuration);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}