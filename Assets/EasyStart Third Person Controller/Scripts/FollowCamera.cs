using UnityEngine;

/// <summary>
/// Third-person follow camera with right-mouse orbit and scroll zoom.
/// Toggle FPP/TPP with C (optional if you also supply firstPersonAnchor).
/// </summary>
public class FollowCamera : MonoBehaviour
{
    public Transform target; // assign player root
    public Transform firstPersonAnchor; // optional (child at head height)
    public Vector3 offset = new Vector3(0f, 1.6f, -3f);
    public float distance = 3f;
    public float minDistance = 1.2f, maxDistance = 6f;
    public float mouseSensitivity = 4f;
    public float pitchMin = -20f, pitchMax = 60f;
    public float positionSmoothTime = 0.08f;
    public float rotationSmoothTime = 0.06f;
    public LayerMask collisionMask = ~0;

    public KeyCode toggleKey = KeyCode.C;
    public bool startInFirstPerson = false;
    public float fppSmooth = 0.06f;

    float yaw = 0f;
    float pitch = 10f;
    float currentDistance;
    Vector3 vel;
    bool isFpp = false;

    void Start()
    {
        if (target == null) Debug.LogWarning("FollowCamera: target not assigned.");
        currentDistance = distance;
        isFpp = startInFirstPerson;
        Vector3 e = transform.eulerAngles;
        yaw = e.y; pitch = e.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (Input.GetKeyDown(toggleKey)) isFpp = !isFpp;

        if (isFpp) UpdateFirstPerson();
        else UpdateThirdPerson();
    }

    void UpdateThirdPerson()
    {
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch += -Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f) currentDistance = Mathf.Clamp(currentDistance - scroll * 4f, minDistance, maxDistance);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desired = target.position + rot * new Vector3(offset.x, offset.y, -currentDistance);

        // collision avoid
        Vector3 anchor = target.position + Vector3.up * (offset.y * 0.5f);
        Vector3 dir = (desired - anchor).normalized;
        float maxCheck = Vector3.Distance(anchor, desired);
        if (Physics.SphereCast(anchor, 0.25f, dir, out RaycastHit hit, maxCheck, collisionMask, QueryTriggerInteraction.Ignore))
        {
            float hitDist = Mathf.Max(hit.distance - 0.15f, minDistance);
            desired = anchor + dir * hitDist;
        }

        transform.position = Vector3.SmoothDamp(transform.position, desired, ref vel, positionSmoothTime);
        Quaternion look = Quaternion.LookRotation((target.position + Vector3.up * (offset.y * 0.5f)) - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, 1f - Mathf.Exp(-rotationSmoothTime * 60f * Time.deltaTime));
    }

    void UpdateFirstPerson()
    {
        Vector3 desired = (firstPersonAnchor != null) ? firstPersonAnchor.position : target.position + Vector3.up * offset.y;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref vel, fppSmooth);
        Quaternion desiredRot = (firstPersonAnchor != null) ? firstPersonAnchor.rotation : target.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, 1f - Mathf.Exp(-fppSmooth * 60f * Time.deltaTime));
    }
}