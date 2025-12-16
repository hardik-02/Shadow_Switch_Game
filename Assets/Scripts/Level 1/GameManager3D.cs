using UnityEngine;
using System.Collections;
using UnityEngine.UI; // if you use UI

public class GameManager3D : MonoBehaviour
{
    public static GameManager3D Instance { get; private set; }
    public Transform playerSpawn;
    public GameObject playerPrefab;
    public float respawnDelay = 0.7f;

    // Add a Win UI panel in the scene and assign it here
    public GameObject winPanel;

    GameObject currentPlayer;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    void Start()
    {
        SpawnPlayer();
        if (winPanel) winPanel.SetActive(false);
    }

    public void SpawnPlayer()
    {
        if (playerPrefab != null && playerSpawn != null)
        {
            if (currentPlayer != null) Destroy(currentPlayer);
            currentPlayer = Instantiate(playerPrefab, playerSpawn.position, Quaternion.identity);
        }
    }

    public void PlayerHit()
    {
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        if (currentPlayer != null)
        {
            var controller = currentPlayer.GetComponent<PlayerMovement3D>();
            if (controller) controller.enabled = false;
        }

        yield return new WaitForSeconds(respawnDelay);

        if (currentPlayer == null && playerPrefab != null)
        {
            SpawnPlayer();
            yield break;
        }

        if (currentPlayer != null && playerSpawn != null)
        {
            var rb = currentPlayer.GetComponent<Rigidbody>();
            currentPlayer.transform.position = playerSpawn.position;
            currentPlayer.transform.rotation = Quaternion.identity;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            var controller = currentPlayer.GetComponent<PlayerMovement3D>();
            if (controller) controller.enabled = true;
        }
    }

    // Call this when player enters the portal and the level is considered complete
    public void LevelComplete()
    {
        // disable player controls so the player can't move during win screen
        if (currentPlayer != null)
        {
            var controller = currentPlayer.GetComponent<PlayerMovement3D>();
            if (controller) controller.enabled = false;
        }

        // show win UI if assigned
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        else
        {
            // fallback: reload scene after short wait
            StartCoroutine(DelayedReload(1.2f));
        }
    }

    IEnumerator DelayedReload(float delay)
    {
        yield return new WaitForSeconds(delay);
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
