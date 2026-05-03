using UnityEngine;

public class BossHitBox : MonoBehaviour
{
    public BodyPart partName;
    private BossHealth bossHealth;

    void Start()
    {
        bossHealth = GetComponentInParent<BossHealth>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<JavelinFlight>() != null)
        {
            bossHealth.TakeHit(partName);

            if (GameManager.Instance != null)
                GameManager.Instance.OnPlayerAttacked();
        }
    }
}