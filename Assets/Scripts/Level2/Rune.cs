using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RunePickup : MonoBehaviour
{
    public string playerTag = "Player";
    public ParticleSystem collectEffect;

    void Reset()
    {
        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        Level2Manager.Instance?.AddRune();
        if (collectEffect != null) collectEffect.Play();
        var mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 1.1f);
    }
}
