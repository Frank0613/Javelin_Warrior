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

    [Header("Settings")]
    public float deathAnimDuration = 2f; // 死亡動畫長度，自己調

    private float currentHealth = 100f;
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        healthSlider.maxValue = 100f;
        healthSlider.value = 100f;
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
            animator.SetTrigger("die");
            Debug.Log("Boss Dead");
            Invoke(nameof(DestroyBoss), deathAnimDuration);
        }
        else
        {
            animator.SetTrigger("hurt");
        }
    }

    void DestroyBoss()
    {
        Destroy(gameObject);
    }
}