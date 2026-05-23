using UnityEngine;
using TMPro;

public class JavelinThrow : MonoBehaviour
{
    [Header("References")]
    public OVRInput.Controller controllerHand = OVRInput.Controller.LTouch;
    public Renderer[] javelinMeshRenderers;
    public GameObject javelinProjectilePrefab;
    public ChargingArea accumulationSystem;

    public JavelinEffects javelinEffects;

    public GameObject angleUI;
    public TMP_Text debugText;

    [Header("Throw Settings")]
    public float minThrowAcceleration = 3f;
    public float minThrowSpeed = 0.5f;
    public float minLaunchSpeed = 10f;
    public float maxLaunchSpeed = 16f;
    public float referenceVelocity = 3f;

    [Header("Debug")]
    public bool debugMode = true;
    public float debugThrowSpeed = 10f;

    [Header("Peak Velocity Window")]
    public float peakWindow = 0.12f;

    private bool hasThrown = false;
    private Vector3 prevControllerVelocity;
    private bool seekingPeak = false;
    private Vector3 peakVelocity;
    private float peakTimer = 0f;
    private bool reinitVelocity = true;

    public bool HasThrown() => hasThrown;

    void Start()
    {
        if (debugText != null)
            debugText.text = "-- / -- / --";
    }

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

        Vector3 localVelocity = OVRInput.GetLocalControllerVelocity(controllerHand);
        Vector3 worldVelocity = transform.root.TransformDirection(localVelocity);

        if (reinitVelocity)
        {
            prevControllerVelocity = worldVelocity;
            reinitVelocity = false;
            return;
        }

        if (!seekingPeak)
        {
            Vector3 acceleration = (worldVelocity - prevControllerVelocity) / Time.deltaTime;
            if (acceleration.magnitude > minThrowAcceleration)
            {
                seekingPeak = true;
                peakVelocity = worldVelocity;
                peakTimer = 0f;
            }
        }
        else
        {
            peakTimer += Time.deltaTime;
            if (worldVelocity.magnitude > peakVelocity.magnitude)
                peakVelocity = worldVelocity;

            if (peakTimer >= peakWindow)
            {
                seekingPeak = false;
                if (peakVelocity.magnitude > minThrowSpeed)
                    Throw(peakVelocity);
            }
        }

        prevControllerVelocity = worldVelocity;
    }

    void Throw(Vector3 realVelocity)
    {
        hasThrown = true;

        foreach (var r in javelinMeshRenderers)
            r.enabled = false;

        GetComponent<JavelinEffects>().HideAll();
        if (angleUI != null) angleUI.SetActive(false);

        Vector3 throwDirection = realVelocity.normalized;
        float t = Mathf.Clamp01(realVelocity.magnitude / referenceVelocity);
        float throwSpeed = Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, t);
        Debug.Log($"[Throw] inputVel={realVelocity.magnitude:F2} t={t:F2} throwSpeed={throwSpeed:F2} dir={throwDirection}");
        if (debugText != null)
            debugText.text = $"{realVelocity.magnitude:F2} / {t:F2} / {throwSpeed:F2}";

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
        seekingPeak = false;
        peakVelocity = Vector3.zero;
        reinitVelocity = true;
        foreach (var r in javelinMeshRenderers)
            r.enabled = true;

        if (javelinEffects != null)
            javelinEffects.Showup(true);
        if (angleUI != null) angleUI.SetActive(true);
    }
}