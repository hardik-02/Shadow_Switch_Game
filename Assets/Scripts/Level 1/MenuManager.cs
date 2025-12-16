using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject levelSelectPanel;

    [Header("Scene Names")]
    public string level1SceneName = "Level1";  // change to your exact scene names
    public string level2SceneName = "Level2";
    public string level3SceneName = "Level3";
    void Start()
    {
        ShowMain();
    }

    // === Main buttons ===
    public void OnPlay()
    {
        LoadLevel(level1SceneName);
    }

    public void OnLevels()
    {
        if (mainPanel) mainPanel.SetActive(false);
        if (levelSelectPanel) levelSelectPanel.SetActive(true);
    }

    public void OnExit()
    {
        #if UNITY_EDITOR
        // Stop play mode in the editor
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // === Level select ===
    public void OnSelectLevel1() => LoadLevel(level1SceneName);
    public void OnSelectLevel2() => LoadLevel(level2SceneName);
    public void OnSelectLevel3() => LoadLevel(level3SceneName);

    public void OnBack()
    {
        ShowMain();
    }

    // === Helpers ===
    void ShowMain()
    {
        if (mainPanel) mainPanel.SetActive(true);
        if (levelSelectPanel) levelSelectPanel.SetActive(false);
    }

    void LoadLevel(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name not set on MenuManager.");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }
}
