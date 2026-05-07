using UnityEngine;
using System.Collections;

public class BossAnimEvent : MonoBehaviour
{
    [Header("Effect")]
    public ParticleSystem attackParticle;
    public ParticleSystem smokeParticle;

    private Animator animator;
    private int attackHash;

    void Start()
    {
        animator = GetComponent<Animator>();
        attackHash = Animator.StringToHash("attack");
    }

    public void WaitForAttackEnd()
    {
        StartCoroutine(CheckAttackEnd());
    }

    IEnumerator CheckAttackEnd()
    {
        float timeout = 2f;
        float waited = 0f;

        while (waited < timeout)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash == attackHash)
                break;

            waited += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Attack started!");

        while (true)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash != attackHash || stateInfo.normalizedTime >= 1f)
                break;

            yield return null;
        }

        Debug.Log("Attack ended!");
        GameManager.Instance.EndBossTurn();
    }

    // 由 Animation Event 呼叫，攻擊特效
    public void AttackEffect()
    {
        if (attackParticle != null)
            attackParticle.Play();
        Debug.Log("Attack effect played!");
    }

    // 由 Animation Event 呼叫，計算 Boss 攻擊次數（在 attack 動畫尾端觸發）
    public void CountBossAttack()
    {
        GameManager.Instance.IncrementBossAttackCount();
    }

    public void StopSmokeEffect()
    {
        if (smokeParticle != null)
            smokeParticle.Stop();
    }
}