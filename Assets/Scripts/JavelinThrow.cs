using UnityEngine;

public class JavelinThrow : MonoBehaviour
{
    [Header("References")]
    public OVRInput.Controller controllerHand = OVRInput.Controller.LTouch;
    public Renderer[] javelinMeshRenderers;
    public GameObject javelinProjectilePrefab;
    public Accumulation_test accumulationSystem;

    [Header("Throw Settings")]
    public float velocityMultiplier = 6f;
    public float minThrowSpeed = 0.8f;

    [Header("Debug")]
    public bool debugMode = true;
    public float debugThrowSpeed = 10f;

    private bool hasThrown = false;

    void Update()
    {
        if (debugMode && Input.GetKeyDown(KeyCode.T) && !hasThrown)
        {
            Throw(transform.forward * debugThrowSpeed);
            return;
        }

        if (!accumulationSystem.IsCharged() || hasThrown) return;

        bool grabReleased = OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, controllerHand);
        Vector3 localVelocity = OVRInput.GetLocalControllerVelocity(controllerHand);
        Vector3 worldVelocity = transform.root.TransformDirection(localVelocity);

        if (grabReleased && worldVelocity.magnitude > minThrowSpeed)
            Throw(worldVelocity);
    }

    void Throw(Vector3 realVelocity)
    {
        hasThrown = true;

        foreach (var r in javelinMeshRenderers)
            r.enabled = false;

        Vector3 throwDirection = transform.forward;
        float throwSpeed = realVelocity.magnitude * velocityMultiplier;

        GameObject projectile = Instantiate(
            javelinProjectilePrefab,
            transform.position,
            Quaternion.LookRotation(throwDirection)
        );

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.linearVelocity = throwDirection * throwSpeed;

        accumulationSystem.ConsumeCharge();
        Invoke(nameof(ResetJavelin), 3f);
    }

    void ResetJavelin()
    {
        hasThrown = false;
        foreach (var r in javelinMeshRenderers)
            r.enabled = true;
    }
}