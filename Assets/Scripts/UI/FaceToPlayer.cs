using UnityEngine;

// Keeps this object's Y rotation pointed at the player's head (billboard on Y only),
// so a dialogue object always faces the player without tilting up or down.
public class FaceToPlayer : MonoBehaviour
{
    [Header("Target")]
    public Transform cameraRig;             // the VR rig root (assign this); the head camera is found under it
    public bool faceAway = false;           // true if the object's forward points backwards

    // The actual head/eye camera. In VR the rig root doesn't move when you only turn
    // your head, so we must follow the HMD-tracked camera under the rig instead.
    private Transform head;

    void Start()
    {
        ResolveHead();
    }

    void ResolveHead()
    {
        // Prefer the tracked camera under the assigned rig; fall back to the rig itself,
        // then to the main camera if nothing was assigned.
        if (cameraRig != null)
        {
            Camera cam = cameraRig.GetComponentInChildren<Camera>();
            head = cam != null ? cam.transform : cameraRig;
        }
        else if (Camera.main != null)
        {
            head = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (head == null) { ResolveHead(); if (head == null) return; }

        // Direction to the head, flattened to the horizontal plane (Y rotation only).
        Vector3 dir = head.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        if (faceAway) dir = -dir;
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
