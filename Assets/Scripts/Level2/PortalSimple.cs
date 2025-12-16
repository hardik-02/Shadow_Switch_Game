using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class PortalSimple : MonoBehaviour
{
    public string nextSceneName = "Level3";
    public string playerTag = "Player";
    public float delay = 0.6f;

    void Reset()
    {
        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        StartCoroutine(DoTeleport());
    }

    IEnumerator DoTeleport()
    {
        yield return new WaitForSeconds(delay);
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("PortalSimple: nextSceneName empty - set it in inspector or add Level3 to Build Settings.");
    }
}
