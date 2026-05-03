using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum Turn { Player, Boss }

    [Header("References")]
    public BossHealth bossHealth;
    public Animator bossAnimator;
    public JavelinThrow javelinThrow;
    public BossAnimEvent bossAnimEvent;

    [Header("Settings")]
    public float bossAttackDelay = 1f;
    public float angryInterval = 5f;

    [Header("Game Over")]
    public GameOverMenu gameOverMenu;
    public int maxBossAttacks = 3;
    private int bossAttackCount = 0;

    private Turn currentTurn = Turn.Player;
    private bool isBossAttacking = false;
    private bool gameStarted = false;
    private float angryTimer = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(WaitForShowup());
    }

    void Update()
    {
        if (gameStarted && currentTurn == Turn.Player && !isBossAttacking
            && !bossHealth.IsDead())
        {
            angryTimer += Time.deltaTime;
            if (angryTimer >= angryInterval)
            {
                angryTimer = 0f;
                bossAnimator.SetTrigger("angry");
            }
        }
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
        yield return new WaitForSeconds(0.1f);

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
        Debug.Log("Boss Attack!");

        bossAnimEvent.WaitForAttackEnd();
    }

    IEnumerator WaitThenAttack()
    {
        yield return new WaitForSeconds(bossAttackDelay);

        if (bossHealth.IsDead()) yield break;

        isBossAttacking = true;
        bossAnimator.SetTrigger("attack");
        Debug.Log("Boss Attack!");

        bossAnimEvent.WaitForAttackEnd();
    }

    public void EndBossTurn()
    {
        isBossAttacking = false;
        currentTurn = Turn.Player;
        angryTimer = 0f;

        bossAttackCount++;

        if (bossAttackCount >= maxBossAttacks)
        {
            Debug.Log("Game Over!");
            if (gameOverMenu != null)
                gameOverMenu.TriggerGameOver();
            return;
        }

        if (javelinThrow != null)
            javelinThrow.ResetJavelin();

    }
}