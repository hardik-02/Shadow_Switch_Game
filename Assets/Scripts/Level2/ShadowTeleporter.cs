using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShadowTeleporter : MonoBehaviour
{
    public Transform teleportTarget;
    public string playerTag = "Player";
    public bool requireShadowWorld = true;

    void Reset()
    {
        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (requireShadowWorld && WorldManager3D.Instance != null && !WorldManager3D.Instance.isShadowActive) return;
        if (teleportTarget != null)
        {
            var cc = other.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                other.transform.position = teleportTarget.position;
                cc.enabled = true;
            }
            else
            {
                other.transform.position = teleportTarget.position;
            }
        }
    }
}
