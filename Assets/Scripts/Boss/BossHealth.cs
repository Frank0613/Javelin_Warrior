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

        currentHealth -= damage;
        if (currentHealth < 0f) currentHealth = 0f;
        healthSlider.value = currentHealth;

        if (currentHealth <= 0f)
        {
            isDead = true;
            healthCanvas.SetActive(false);
            StartCoroutine(FadeCanvas(0f));
            animator.SetTrigger("die");
        }
        else
        {
            animator.SetTrigger("hurt");
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
}