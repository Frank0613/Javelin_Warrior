using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum BodyPart
{
    Head,
    Body,
    Arm_Left
}

public class BossHealth : MonoBehaviour
{
    [Header("References")]
    public Slider healthSlider;
    public GameObject healthCanvas;
    public CanvasGroup healthCanvasGroup;

    [Header("Settings")]
    public float fadeDuration = 1f;

    [Header("Simple Boss")]
    public float uniformDamage = 0f;

    [Header("Death")]
    public GameObject[] bodyParts;

    [Header("Audio")]
    public SfxCue hurtSfx;
    public SfxCue deathSfx;     // played once when the boss dies

    private float currentHealth = 100f;
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        healthSlider.maxValue = 100f;
        healthSlider.value = 100f;
        healthCanvasGroup.alpha = 0f;
    }

    public void TakeHit(BodyPart partName)
    {
        if (isDead) return;

        float damage = 0f;

        if (uniformDamage > 0f)
        {
            damage = uniformDamage;
        }
        else
        {
            switch (partName)
            {
                case BodyPart.Head:
                    Debug.Log("Head");
                    damage = 50f;
                    break;
                case BodyPart.Body:
                    Debug.Log("Body");
                    damage = 40f;
                    break;
                default:
                    Debug.Log("Arm&Leg");
                    damage = 20f;
                    break;
            }
        }

        currentHealth -= damage;
        if (currentHealth < 0f) currentHealth = 0f;
        healthSlider.value = currentHealth;

        if (currentHealth <= 0f)
        {
            isDead = true;
            healthCanvas.SetActive(false);
            StartCoroutine(FadeCanvas(0f));
            animator.SetTrigger("die");
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(deathSfx);
            var patrol = GetComponent<BossPatrol>();
            if (patrol != null) patrol.StopPatrol();
            DisableBodyColliders();
        }
        else
        {
            animator.SetTrigger("hurt");
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(hurtSfx);
        }
    }
    public void ShowHealthBar()
    {
        StartCoroutine(FadeCanvas(1f));
    }

    IEnumerator FadeCanvas(float targetAlpha)
    {
        float startAlpha = healthCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            healthCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        healthCanvasGroup.alpha = targetAlpha;

        if (targetAlpha == 0f && healthCanvas != null)
            healthCanvas.SetActive(false);
    }
    public bool IsDead() => isDead;

    void DisableBodyColliders()
    {
        for (int i = 0; i < bodyParts.Length; i++)
        {
            if (bodyParts[i] == null) continue;
            Collider[] cols = bodyParts[i].GetComponents<Collider>();
            for (int j = 0; j < cols.Length; j++)
                cols[j].enabled = false;
        }
    }
}