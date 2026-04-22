using UnityEngine;

public class JavelinFlight : MonoBehaviour
{
    private Rigidbody rb;
    private bool canCollide = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb.linearVelocity.magnitude > 0.5f)
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);

        Invoke(nameof(EnableCollision), 0.1f);
    }

    void EnableCollision()
    {
        canCollide = true;
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.magnitude > 0.5f)
        {
            Quaternion target = Quaternion.LookRotation(rb.linearVelocity);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, target, Time.fixedDeltaTime * 8f));
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!canCollide) return;

        rb.isKinematic = true;
    }
}