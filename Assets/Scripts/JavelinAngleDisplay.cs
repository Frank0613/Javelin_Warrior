using UnityEngine;
using TMPro;

public class JavelinAngleDisplay : MonoBehaviour
{
    [Header("References")]
    public Transform javelinTransform;
    public TMP_Text angleText;
    public Transform canvasTransform;

    [Header("Target Angle Range")]
    public float minAngle = 31f;
    public float maxAngle = 35f;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color targetColor = Color.green;

    private Transform mainCamera;

    void Start()
    {
        mainCamera = Camera.main.transform;
    }

    void LateUpdate()
    {
        Vector3 forward = javelinTransform.forward;
        float angle = Mathf.Asin(forward.y) * Mathf.Rad2Deg;

        angleText.text = angle.ToString("F1") + "°";

        if (angle >= minAngle && angle <= maxAngle)
            angleText.color = targetColor;
        else
            angleText.color = normalColor;

        if (mainCamera != null && canvasTransform != null)
        {
            canvasTransform.LookAt(mainCamera);
            canvasTransform.Rotate(0, 180, 0);
        }
    }
}