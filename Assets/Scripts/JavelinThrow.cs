using UnityEngine;

public class JavelinThrow : MonoBehaviour
{
    [Header("References")]
    public OVRInput.Controller controllerHand = OVRInput.Controller.LTouch;
    public Renderer[] javelinMeshRenderers;
    public GameObject javelinProjectilePrefab;
    public ChargingArea accumulationSystem;

    public JavelinEffects javelinEffects;

    public GameObject angleUI;

    [Header("Throw Settings")]
    public float velocityMultiplier = 6f;
    public float minThrowSpeed = 0.8f;

    [Header("Debug")]
    public bool debugMode = true;
    public float debugThrowSpeed = 10f;

    private bool hasThrown = false;

    public bool HasThrown() => hasThrown;

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlayerTurn())
            return;

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

        GetComponent<JavelinEffects>().HideAll();
        if (angleUI != null) angleUI.SetActive(false);

        Vector3 throwDirection = realVelocity.normalized;
        float throwSpeed = realVelocity.magnitude * velocityMultiplier;

        Vector3 euler = Quaternion.LookRotation(throwDirection).eulerAngles;
        euler.z = 0f;

        GameObject projectile = Instantiate(
            javelinProjectilePrefab,
            transform.position,
            Quaternion.Euler(euler)
        );

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.linearVelocity = throwDirection * throwSpeed;

        accumulationSystem.ConsumeCharge();
    }

    public void ResetJavelin()
    {
        hasThrown = false;
        foreach (var r in javelinMeshRenderers)
            r.enabled = true;

        if (javelinEffects != null)
            javelinEffects.Showup(true);
        if (angleUI != null) angleUI.SetActive(true);
    }
}