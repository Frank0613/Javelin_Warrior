using UnityEngine;
using System.Collections;

public class BossPatrol : MonoBehaviour
{
    [Header("Settings")]
    public float walkDistance = 2f;       // 從中心走多遠（左右各）
    public float walkSpeed = 1f;
    public float turnSpeed = 360f;
    public int anglesPerCycle = 4;
    public float idleAfterWalk = 1f;
    public float idleBeforeAngry = 0.3f;

    private Animator animator;
    private BossHealth bossHealth;
    private bool isPatrolling = false;
    private bool currentDirRight = true;
    private int walkCount = 0;
    private Quaternion initialRotation;
    private Vector3 initialPosition;
    private Vector3 worldRight;

    private Coroutine patrolCoroutine;

    void Start()
    {
        animator = GetComponent<Animator>();
        bossHealth = GetComponent<BossHealth>();
        initialRotation = transform.rotation;
        initialPosition = transform.position;
        worldRight = transform.right;
    }

    public void StartPatrol()
    {
        if (patrolCoroutine != null) StopCoroutine(patrolCoroutine);
        isPatrolling = true;
        animator.SetBool("walk", false);
        animator.ResetTrigger("angry");
        patrolCoroutine = StartCoroutine(PatrolLoop());
    }

    public void StopPatrol()
    {
        isPatrolling = false;
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }
        animator.SetBool("walk", false);
        animator.ResetTrigger("angry");
    }

    public IEnumerator FaceFront()
    {
        isPatrolling = false;
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }
        animator.SetBool("walk", false);
        animator.ResetTrigger("angry");

        Quaternion targetRot = initialRotation;
        while (Quaternion.Angle(transform.rotation, targetRot) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRot;
    }

    IEnumerator PatrolLoop()
    {
        while (isPatrolling)
        {
            if (bossHealth != null && bossHealth.IsDead()) yield break;

            // 1. 算出目標位置（中心點 ± walkDistance）
            Vector3 targetPos = initialPosition + (currentDirRight ? worldRight : -worldRight) * walkDistance;

            // 2. 轉向行走方向
            float targetYaw = currentDirRight ? 90f : -90f;
            yield return TurnTo(targetYaw);
            if (!isPatrolling) yield break;

            // 3. 走到目標位置
            animator.SetBool("walk", true);
            Vector3 moveDir = currentDirRight ? worldRight : -worldRight;

            while (Vector3.Dot(targetPos - transform.position, moveDir) > 0.05f && isPatrolling)
            {
                transform.position += moveDir * walkSpeed * Time.deltaTime;
                yield return null;
            }

            // 4. 停止走
            animator.SetBool("walk", false);
            walkCount++;

            // 5. Idle
            yield return new WaitForSeconds(idleAfterWalk);
            if (!isPatrolling) yield break;

            // 6. Angry
            if (walkCount >= anglesPerCycle)
            {
                walkCount = 0;

                yield return TurnTo(0f);
                if (!isPatrolling) yield break;

                yield return new WaitForSeconds(idleBeforeAngry);
                if (!isPatrolling) yield break;

                animator.SetTrigger("angry");
                yield return WaitForAngryEnd();
                if (!isPatrolling) yield break;
            }

            currentDirRight = !currentDirRight;
        }
    }

    IEnumerator TurnTo(float relativeYaw)
    {
        Quaternion targetRot = initialRotation * Quaternion.Euler(0f, relativeYaw, 0f);

        while (Quaternion.Angle(transform.rotation, targetRot) > 1f && isPatrolling)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            yield return null;
        }

        if (isPatrolling)
            transform.rotation = targetRot;
    }

    IEnumerator WaitForAngryEnd()
    {
        int angryHash = Animator.StringToHash("angry");

        float timeout = 2f;
        float waited = 0f;
        while (waited < timeout && isPatrolling)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == angryHash)
                break;
            waited += Time.deltaTime;
            yield return null;
        }

        while (isPatrolling
               && animator.GetCurrentAnimatorStateInfo(0).shortNameHash == angryHash
               && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.95f)
        {
            yield return null;
        }
    }
}