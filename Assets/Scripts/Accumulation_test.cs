using UnityEngine;

public class Accumulation_test : MonoBehaviour
{
    [Header("References")]
    public OVRInput.Controller controllerHand = OVRInput.Controller.LTouch;
    public Renderer sphereRenderer;

    private bool isInZone = false;
    private bool isCharged = false;
    private float accumulateTimer = 0f;

    void Update()
    {
        bool grabHeld = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controllerHand);

        // 在蓄力區內 + 按著 Grab + 還沒蓄滿
        if (isInZone && grabHeld && !isCharged)
        {
            accumulateTimer += Time.deltaTime;
            UpdateSphereColor();

            if (accumulateTimer >= 3f)
            {
                isCharged = true;
                sphereRenderer.material.color = Color.green;
                Debug.Log("success");
            }
        }
        // 中途放開 Grab 就重置
        else if (!grabHeld && !isCharged)
        {
            ResetAccumulation();
        }
    }

    void UpdateSphereColor()
    {
        if (accumulateTimer < 1f)
            sphereRenderer.material.color = Color.red;
        else if (accumulateTimer < 2f)
            sphereRenderer.material.color = Color.yellow;
        // 2~3 秒保持黃色，滿 3 秒在 Update 裡變綠
    }

    void ResetAccumulation()
    {
        accumulateTimer = 0f;
        if (sphereRenderer != null)
            sphereRenderer.material.color = Color.white;
    }

    // 子物件呼叫
    public void EnterZone()
    {
        isInZone = true;
    }

    public void ExitZone()
    {
        isInZone = false;
        if (!isCharged) ResetAccumulation();
    }

    // JavelinThrow 呼叫
    public bool IsCharged() => isCharged;

    public void ConsumeCharge()
    {
        isCharged = false;
        ResetAccumulation();
    }
}