using UnityEngine;

public class JavelinTrajectory : MonoBehaviour
{
    [Header("References")]
    public OVRInput.Controller controllerHand = OVRInput.Controller.LTouch;
    public ChargingArea accumulationSystem;
    public LineRenderer lineRenderer;

    [Header("Settings")]
    public float velocityMultiplier = 6f;
    public float timeStep = 0.05f;
    public int maxSteps = 60;

    [Header("Physics Match")]
    public float projectileDrag = 0.1f;

    void Update()
    {
        if (accumulationSystem.IsCharged())
        {
            lineRenderer.enabled = true;
            DrawTrajectory();
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    void DrawTrajectory()
    {
        Vector3 startPos = transform.position;
        Vector3 throwDirection = transform.forward;

        Vector3 localVelocity = OVRInput.GetLocalControllerVelocity(controllerHand);
        Vector3 worldVelocity = transform.root.TransformDirection(localVelocity);
        float speed = worldVelocity.magnitude * velocityMultiplier;

        if (speed < 1f) speed = velocityMultiplier;

        Vector3 velocity = throwDirection * speed;
        Vector3 gravity = Physics.gravity;
        Vector3 pos = startPos;

        Vector3[] points = new Vector3[maxSteps];

        for (int i = 0; i < maxSteps; i++)
        {
            points[i] = pos;

            velocity += gravity * timeStep;
            velocity *= (1f - projectileDrag * timeStep);

            Vector3 nextPos = pos + velocity * timeStep;

            if (i > 0 && Physics.Raycast(pos, (nextPos - pos).normalized, out RaycastHit hit, (nextPos - pos).magnitude))
            {
                points[i] = hit.point;
                lineRenderer.positionCount = i + 1;
                lineRenderer.SetPositions(points);
                return;
            }

            pos = nextPos;
        }

        lineRenderer.positionCount = maxSteps;
        lineRenderer.SetPositions(points);
    }
}