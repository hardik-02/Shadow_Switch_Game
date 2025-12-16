using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class TorchDamage : MonoBehaviour
{
    public string playerTag = "Player";

    void Reset()
    {
        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (WorldManager3D.Instance != null && !WorldManager3D.Instance.isShadowActive)
        {
            Debug.Log("TorchDamage: player hit torch in Real world. Restarting level.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
