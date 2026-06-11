using UnityEngine;

public class ChargingArea : MonoBehaviour
{
    [Header("References")]
    public OVRInput.Controller controllerHand = OVRInput.Controller.LTouch;
    public JavelinThrow javelinThrow;

    [Tooltip("頭部參考 Transform（指派 OVRCameraRig 的 CenterEyeAnchor）。若為 null 會 fallback 到 Camera.main。用於蓄力區的「位置原點」。")]
    public Transform head;

    [Tooltip("方向參考 Transform（指派 OVRCameraRig 根節點 / TrackingSpace）。蓄力區的朝向(yaw)會跟著它，而不是跟著頭部轉動，避免轉頭就誤觸發。留空則 fallback 回 head（舊行為）。")]
    public Transform orientationReference;

    [Header("Throw Pose Zone (head-relative, yaw-only)")]
    [Tooltip("至少在頭部右側多少公尺（0 = 只要在後面、不限定多右）")]
    public float rightMin = 0.15f;
    [Tooltip("至少在頭部後方多少公尺")]
    public float behindMin = 0.05f;
    [Tooltip("相對頭部高度下限（約耳/肩高以上即可）")]
    public float heightMin = -0.3f;
    [Tooltip("與頭部的最大距離（手臂可及範圍，避免遠處誤判）。橘色球的半徑就是這個值。")]
    public float maxDistance = 1.5f;
    public bool drawDebug = true;

    [Header("Effects")]
    public GameObject chargeEffect;
    public GameObject[] finishEffects;

    [Header("Audio")]
    public AudioSource chargeSource;
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

    void Start()
    {
        if (javelinThrow != null)
            javelinEffects = javelinThrow.GetComponent<JavelinEffects>();
        if (head == null && Camera.main != null)
            head = Camera.main.transform;
    }

    void Update()
    {
        bool active = !(javelinThrow != null && javelinThrow.HasThrown())
                   && !(GameManager.Instance != null && !GameManager.Instance.IsPlayerTurn());

        bool inZoneNow = active && IsJavelinInThrowPose();

        if (!inZoneNow && isJavelinInZone && !isCharged)
            ResetAccumulation();

        isJavelinInZone = inZoneNow;

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

    // 朝向(yaw)基底用身體/遊玩空間，而非頭部視線；位置原點仍用頭部。
    Transform OrientationRef => orientationReference != null ? orientationReference : head;

    bool IsJavelinInThrowPose()
    {
        if (head == null || javelinThrow == null) return false;

        Vector3 offset = javelinThrow.transform.position - head.position;

        // 只取水平(yaw)基底，忽略 pitch/roll；用身體朝向，轉頭不影響蓄力區
        Vector3 fwd = OrientationRef.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, fwd);

        float rightAmt = Vector3.Dot(offset, right);
        float fwdAmt = Vector3.Dot(offset, fwd);
        float upAmt = offset.y;

        return rightAmt >= rightMin
            && fwdAmt <= -behindMin
            && upAmt >= heightMin
            && offset.magnitude <= maxDistance;
    }

    void UpdateChargeSound(bool active)
    {
        if (chargeSource == null) return;

        bool chargeStart = active && isJavelinInZone && !isCharged;
        bool inZone = active && isJavelinInZone;


        if (chargeStart && !wasChargeStart)
            chargeSource.Play();


        if (!inZone && wasInZone)
            chargeSource.Stop();

        wasChargeStart = chargeStart;
        wasInZone = inZone;
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
        ResetAccumulation();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!drawDebug || head == null) return;

        Vector3 fwd = OrientationRef.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, fwd);

        // 頭部位置
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(head.position, 0.08f);

        // 藍線指向前方（球的「後半邊」才是可蓄力方向參考）
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(head.position, head.position + fwd * 0.5f);

        // 橘色點雲 = 真正的可蓄力區域（右+後+高度，且在 maxDistance 內）
        // 與 IsJavelinInThrowPose 的條件完全一致，前方不會被包含
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        const float step = 0.2f;
        for (float r = rightMin; r <= maxDistance; r += step)
            for (float b = behindMin; b <= maxDistance; b += step)
                for (float u = heightMin; u <= maxDistance; u += step)
                {
                    Vector3 local = right * r - fwd * b + Vector3.up * u;
                    if (local.magnitude > maxDistance) continue;
                    Gizmos.DrawCube(head.position + local, Vector3.one * 0.04f);
                }

        if (javelinThrow != null)
        {
            Gizmos.color = IsJavelinInThrowPose() ? Color.green : Color.red;
            Gizmos.DrawWireSphere(javelinThrow.transform.position, 0.06f);
        }
    }
#endif
}
