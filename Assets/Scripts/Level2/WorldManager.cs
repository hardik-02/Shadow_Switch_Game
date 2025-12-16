using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; }
    public static event Action<bool> OnWorldSwitch; // true => shadow active

    [Header("World parents (top-level containers)")]
    public GameObject realWorld;
    public GameObject shadowWorld;

    [Header("Skyboxes (optional)")]
    public Material daySkybox;
    public Material nightSkybox;

    [Header("Directional (Sun) Light")]
    public Light sunLight;
    public Color dayLightColor = Color.white;
    public float dayLightIntensity = 1.0f;
    public Color nightLightColor = new Color(0.6f,0.65f,1f);
    public float nightLightIntensity = 0.15f;

    [Header("Audio (optional)")]
    public AudioClip switchClip;
    [Range(0f,1f)] public float switchVolume = 1f;
    AudioSource audioSrc;

    [Header("Input (toggle key)")]
    public bool enableKeyToggle = true;
    public KeyCode toggleKey = KeyCode.E;

    [HideInInspector] public bool isShadowActive = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        ApplyWorldState();
        try { OnWorldSwitch?.Invoke(isShadowActive); } catch (Exception) { }
    }

    void Update()
    {
        if (!enableKeyToggle) return;
        bool pressed = false;
        if (Input.GetKeyDown(toggleKey)) pressed = true;
#if ENABLE_INPUT_SYSTEM
        if (!pressed && Keyboard.current != null)
        {
            if (toggleKey == KeyCode.E && Keyboard.current.eKey.wasPressedThisFrame) pressed = true;
            if (toggleKey == KeyCode.LeftShift && Keyboard.current.leftShiftKey.wasPressedThisFrame) pressed = true;
        }
#endif
        if (pressed) ToggleWorld();
    }

    public void ToggleWorld()
    {
        isShadowActive = !isShadowActive;
        ApplyWorldState();
        if (switchClip != null && audioSrc != null) audioSrc.PlayOneShot(switchClip, switchVolume);
        try { OnWorldSwitch?.Invoke(isShadowActive); } catch (Exception) { }
        Debug.Log("WorldManager3D: switched to " + (isShadowActive ? "Shadow" : "Real"));
    }

    public void SetWorld(bool shadowActive, bool notify = true)
    {
        if (isShadowActive == shadowActive) return;
        isShadowActive = shadowActive;
        ApplyWorldState();
        if (notify) OnWorldSwitch?.Invoke(isShadowActive);
    }

    void ApplyWorldState()
    {
        if (realWorld != null) realWorld.SetActive(!isShadowActive);
        if (shadowWorld != null) shadowWorld.SetActive(isShadowActive);

        if (!isShadowActive && daySkybox != null) RenderSettings.skybox = daySkybox;
        else if (isShadowActive && nightSkybox != null) RenderSettings.skybox = nightSkybox;

        if (!isShadowActive)
        {
            if (sunLight != null) { sunLight.color = dayLightColor; sunLight.intensity = dayLightIntensity; }
        }
        else
        {
            if (sunLight != null) { sunLight.color = nightLightColor; sunLight.intensity = nightLightIntensity; }
        }

        try { DynamicGI.UpdateEnvironment(); } catch { }
    }

    void OnDestroy() { if (Instance == this) Instance = null; }
}
