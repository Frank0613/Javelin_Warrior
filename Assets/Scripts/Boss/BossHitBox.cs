using UnityEngine;

public class BossHitBox : MonoBehaviour
{
    public BodyPart partName; // Inspector 裡填：Head、Body、Arm_Left 等
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
        }
    }
}