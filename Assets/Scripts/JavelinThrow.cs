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

    private bool hasThrown = false;

    void Update()
    {
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

        // Z 軸是長軸，用 transform.forward 就對了
        Vector3 throwDirection = -transform.forward;
        float throwSpeed = realVelocity.magnitude * velocityMultiplier;

        GameObject projectile = Instantiate(
            javelinProjectilePrefab,
            transform.position,
            transform.rotation  // 直接用 Javelin 的旋轉，Z 軸已經對齊
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