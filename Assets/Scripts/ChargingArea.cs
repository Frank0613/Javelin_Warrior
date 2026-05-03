using UnityEngine;

public class ChargingArea : MonoBehaviour
{
    [Header("References")]
    public OVRInput.Controller controllerHand = OVRInput.Controller.LTouch;
    public ParticleSystem circleEffect;
    public JavelinThrow javelinThrow;

    [Header("Colors")]
    public Color defaultColor = new Color(1f, 1f, 1f, 15f / 255f);
    public Color inZoneColor = new Color(1f, 1f, 0f, 15f / 255f);

    [Header("Effects")]
    public GameObject chargeEffect;
    public GameObject[] finishEffects;

    private bool isJavelinInZone = false;
    private bool isCharged = false;
    private float accumulateTimer = 0f;
    private JavelinEffects javelinEffects;

    void Update()
    {
        if (javelinThrow != null && javelinThrow.HasThrown()) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsPlayerTurn()) return;

        bool grabHeld = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controllerHand)
                        || Input.GetKey(KeyCode.Y);

        if (isJavelinInZone && grabHeld && !isCharged)
        {
            accumulateTimer += Time.deltaTime;

            if (javelinEffects != null)
                javelinEffects.ShowCharge(true);

            if (accumulateTimer >= 3f)
            {
                isCharged = true;

                if (javelinEffects != null)
                    javelinEffects.ShowFinish(true);

                Debug.Log("success");
            }
        }
        else if (!grabHeld && !isCharged)
        {
            ResetAccumulation();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var javelin = other.GetComponentInParent<JavelinThrow>();
        if (javelin != null)
        {
            isJavelinInZone = true;
            javelinEffects = javelin.GetComponent<JavelinEffects>();
            SetCircleColor(inZoneColor);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<JavelinThrow>() != null)
        {
            isJavelinInZone = false;
            SetCircleColor(defaultColor);
            if (!isCharged) ResetAccumulation();
        }
    }

    void SetCircleColor(Color color)
    {
        if (circleEffect != null)
        {
            var main = circleEffect.main;
            main.startColor = color;

            // 強制已存在的粒子也變色
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[circleEffect.particleCount];
            int count = circleEffect.GetParticles(particles);
            for (int i = 0; i < count; i++)
            {
                particles[i].startColor = color;
            }
            circleEffect.SetParticles(particles, count);
        }
    }

    void ResetAccumulation()
    {
        accumulateTimer = 0f;

        if (javelinEffects != null)
        {
            javelinEffects.ShowCharge(false);
            javelinEffects.ShowFinish(false);
        }
    }

    public bool IsCharged() => isCharged;

    public void ConsumeCharge()
    {
        isCharged = false;
        SetCircleColor(defaultColor);
        ResetAccumulation();
    }
}