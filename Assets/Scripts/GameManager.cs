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
    public float meteoriteJavelinCooldown = 2f;

    [Header("Game Over")]
    public FadeUI fadeUI;
    public PostProcessingController postProcessingController;
    public float gameOverDelay = 2f;
    public int maxBossAttacks = 3;
    private int bossAttackCount = 0;

    [Header("Audio")]
    public SfxCue playerDeathSfx;

    [Header("Boss Defeat / Next Scene")]
    [Tooltip("勾選：Boss 死後切到下個場景；不勾：留在原場景")]
    public bool switchSceneAfterDeath = false;
    public string nextSceneName;
    [Tooltip("死亡動畫播完後，等幾秒再切場景")]
    public float delayAfterDeath = 0f;

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

    public bool IsPlayerTurn()
    {
        if (!gameStarted) return false;
        if (bossHealth != null && bossHealth.IsDead())
            return !isBossAttacking; // boss 死後：isBossAttacking 當 javelin CD 用
        return currentTurn == Turn.Player && !isBossAttacking;
    }

    public void OnPlayerAttacked()
    {
        if (bossHealth != null && bossHealth.IsDead()) { OnJavelinLanded(); return; }
        if (currentTurn != Turn.Player) return;

        currentTurn = Turn.Boss;
        StartCoroutine(WaitHurtThenAttack());
    }

    public void OnPlayerMissed()
    {
        if (bossHealth != null && bossHealth.IsDead()) { OnJavelinLanded(); return; }
        if (currentTurn != Turn.Player) return;

        currentTurn = Turn.Boss;
        StartCoroutine(WaitThenAttack());
    }

    public void OnJavelinLanded()
    {
        if (isBossAttacking) return;
        StartCoroutine(MeteoriteJavelinReset());
    }

    IEnumerator WaitHurtThenAttack()
    {
        if (bossPatrol != null)
            yield return StartCoroutine(bossPatrol.FaceFront());

        yield return null; // let hurt transition begin before checking for idle

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
    IEnumerator MeteoriteJavelinReset()
    {
        isBossAttacking = true;
        yield return new WaitForSeconds(meteoriteJavelinCooldown);
        isBossAttacking = false;
        if (javelinThrow != null)
            javelinThrow.ResetJavelin();
    }

    // 由死亡動畫尾端的 Animation Event 觸發
    public void OnBossDefeated()
    {
        if (!switchSceneAfterDeath) return; // 選擇留在原場景

        StartCoroutine(NextSceneSequence());
    }

    IEnumerator NextSceneSequence()
    {
        if (delayAfterDeath > 0f)
            yield return new WaitForSeconds(delayAfterDeath);


        fadeUI.FadeOut();
        if (AudioManager.Instance != null)
            AudioManager.Instance.FadeOutMusic(fadeUI.fadeDuration);
        yield return new WaitForSeconds(fadeUI.fadeDuration);


        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("nextSceneName 未設定，無法切換場景。");
            yield break;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator GameOverSequence()
    {
        // 記錄死亡音效預計播完的時間點（含延遲 + 音檔長度）
        float deathSfxEnd = 0f;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(playerDeathSfx);
            if (playerDeathSfx != null && playerDeathSfx.clip != null)
                deathSfxEnd = Time.unscaledTime + playerDeathSfx.delay + playerDeathSfx.clip.length;
        }

        if (postProcessingController != null)
            yield return StartCoroutine(postProcessingController.GameOverEffect(gameOverDelay));
        else
            yield return new WaitForSeconds(gameOverDelay);

        if (fadeUI != null)
        {
            fadeUI.FadeOut();
            if (AudioManager.Instance != null)
                AudioManager.Instance.FadeOutMusic(fadeUI.fadeDuration);
            yield return new WaitForSeconds(fadeUI.fadeDuration);
        }

        // 維持黑畫面，等死亡音效播完再重置（避免重生後還聽到殘音）
        while (Time.unscaledTime < deathSfxEnd)
            yield return null;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}