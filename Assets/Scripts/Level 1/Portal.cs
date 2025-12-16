using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class Portal : MonoBehaviour
{
    [Tooltip("Name of the scene to load when player enters portal (must be in Build Settings).")]
    public string nextSceneName = "Level 2";

    [Tooltip("Optional: tag to identify player")]
    public string playerTag = "Player";

    [Tooltip("Delay before loading scene (seconds) to let sound/particle play)")]
    public float delay = 0.6f;

    void Reset()
    {
        // ensure collider is trigger when added via inspector
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        // start teleport coroutine (disable further triggers)
        StartCoroutine(DoTeleport());
    }

    System.Collections.IEnumerator DoTeleport()
    {
        // optional: play a sound/particle here

        // small delay so player sees the effect
        yield return new WaitForSeconds(delay);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            // make sure the scene is added to Build Settings (File -> Build Settings)
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("PortalSimple: nextSceneName is empty. Add scene to Build Settings or set name in inspector.");
        }
    }
}
