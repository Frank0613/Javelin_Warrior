using UnityEngine;

public class JavelinFlight : MonoBehaviour
{
    public GameObject explosionPrefab;

    [Header("Flight Settings")]
    [Range(0f, 1f)]
    public float gravityScale = 1.0f;

    private Rigidbody rb;
    private bool canCollide = false;
    private Vector3 horizontalForward;
    private bool directionInitialized = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        Debug.Log($"[JavelinFlight] gravityScale={gravityScale} mass={rb.mass} drag={rb.linearDamping}");
        Invoke(nameof(EnableCollision), 0.1f);
        Invoke(nameof(MissedDestroy), 5f);
    }

    void EnableCollision()
    {
        canCollide = true;
    }

    void FixedUpdate()
    {
        if (!directionInitialized)
        {
            if (rb.linearVelocity.magnitude > 0.5f)
            {
                Vector3 vel = rb.linearVelocity;
                horizontalForward = new Vector3(vel.x, 0f, vel.z).normalized;
                transform.rotation = Quaternion.LookRotation(vel);
                directionInitialized = true;
            }
            return;
        }

        rb.AddForce(Physics.gravity * gravityScale * rb.mass);

        if (rb.linearVelocity.magnitude > 0.5f)
        {
            float hSpeed = Vector3.Dot(rb.linearVelocity, horizontalForward);
            float vSpeed = rb.linearVelocity.y;
            Vector3 planarVelocity = horizontalForward * hSpeed + Vector3.up * vSpeed;

            Quaternion target = Quaternion.LookRotation(planarVelocity.normalized);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, target, Time.fixedDeltaTime * 8f));
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!canCollide) return;

        bool hitBoss = collision.gameObject.GetComponent<BossHitBox>() != null;

        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 3f);
        }

        if (!hitBoss && GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerMissed();
        }

        CancelInvoke(nameof(MissedDestroy));
        Destroy(gameObject);
    }

    void MissedDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerMissed();
        Destroy(gameObject);
    }
}