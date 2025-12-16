using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("If true, checkpoint activates only once.")]
    public bool oneTime = false;

    public AudioClip activateClip; // optional SFX

    bool activated = false;

    void Reset()
    {
        // ensure collider is a trigger when added via inspector
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (activated && oneTime) return;
        if (!other.CompareTag("Player")) return;

        GameManager3D gm = GameManager3D.Instance;
        if (gm != null)
        {
            // set spawn position slightly above this object to avoid clipping
            gm.playerSpawn.position = transform.position + Vector3.up * 1.2f;
            if (activateClip) AudioSource.PlayClipAtPoint(activateClip, transform.position);
        }

        activated = true;

        // visual feedback: change color if renderer exists
        var rend = GetComponentInParent<Renderer>();
        if (rend) rend.material.color = Color.green;
    }
}
