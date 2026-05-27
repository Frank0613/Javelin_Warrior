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

    [Header("Audio")]
    public AudioSource chargeSource; // 蓄力音效（進入範圍時播一次，不 loop）

    private bool isJavelinInZone = false;
    private bool isCharged = false;
    private float accumulateTimer = 0f;
    private JavelinEffects javelinEffects;

    private bool wasChargeStart = false;
    private bool wasInZone = false;

    void Awake()
    {
        if (chargeSource != null)
        {
            chargeSource.loop = false;
            chargeSource.playOnAwake = false;
        }
    }

    void Update()
    {
        bool active = !(javelinThrow != null && javelinThrow.HasThrown())
                   && !(GameManager.Instance != null && !GameManager.Instance.IsPlayerTurn());

        if (active)
        {
            if (Input.GetKeyDown(KeyCode.Y) && !isCharged)
            {
                isCharged = true;
                if (javelinEffects != null) javelinEffects.ShowFinish(true);
                Debug.Log("debug charge");
            }

            if (isJavelinInZone && !isCharged)
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
        }

        UpdateChargeSound(active);
    }

    void UpdateChargeSound(bool active)
    {
        if (chargeSource == null) return;

        bool chargeStart = active && isJavelinInZone && !isCharged; // 可開始蓄力的狀態
        bool inZone = active && isJavelinInZone;

        // 進入蓄力狀態的瞬間 → 播一次（已蓄滿時進範圍不會觸發）
        if (chargeStart && !wasChargeStart)
            chargeSource.Play();

        // 離開區域 → 停止（留在範圍內則讓它自然播完）
        if (!inZone && wasInZone)
            chargeSource.Stop();

        wasChargeStart = chargeStart;
        wasInZone = inZone;
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