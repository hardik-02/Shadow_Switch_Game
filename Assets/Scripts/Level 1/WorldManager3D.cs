using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// WorldManager3D
/// - Toggle between real (day) and shadow (night) worlds
/// - Switches RenderSettings.skybox, ambient color/intensity, fog color, and a directional "sun" Light
/// - Safe singleton and optional key toggle (default = E)
/// - Fires OnWorldSwitch(bool) so other systems can subscribe
/// </summary>
public class WorldManager3D : MonoBehaviour
{
    public static WorldManager3D Instance { get; private set; }
    public static event Action<bool> OnWorldSwitch; // true => shadow (night) active

    [Header("World parents (optional containers)")]
    [Tooltip("Parent GameObject containing objects for the real (day) world.")]
    public GameObject realWorld;
    [Tooltip("Parent GameObject containing objects for the shadow (night) world.")]
    public GameObject shadowWorld;

    [Header("Skyboxes")]
    [Tooltip("Skybox material used for Day / Real world (procedural or cubemap)")]
    public Material daySkybox;
    [Tooltip("Skybox material used for Night / Shadow world")]
    public Material nightSkybox;

    [Header("Directional (Sun) Light - assign your main directional light here")]
    public Light sunLight;

    [Header("Day lighting settings")]
    public Color dayLightColor = Color.white;
    public float dayLightIntensity = 1.0f;
    public Color dayAmbientColor = new Color(0.7f, 0.7f, 0.75f);
    public Color dayFogColor = new Color(0.7f, 0.8f, 1f);

    [Header("Night / Shadow lighting settings")]
    public Color nightLightColor = new Color(0.6f, 0.65f, 1f);
    public float nightLightIntensity = 0.15f;
    public Color nightAmbientColor = new Color(0.05f, 0.05f, 0.08f);
    public Color nightFogColor = new Color(0.02f, 0.02f, 0.05f);

    [Header("Audio (optional)")]
    public AudioClip switchClip;
    [Range(0f,1f)] public float switchVolume = 1f;
    AudioSource audioSrc;

    [Header("Input (toggle key)")]
    public bool enableKeyToggle = true;
    [Tooltip("Key to toggle the world (avoid LeftShift if used by sprint). Default: E")]
    public KeyCode toggleKey = KeyCode.E;

    [HideInInspector] public bool isShadowActive = false;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        // apply initial visuals and notify listeners
        ApplyWorldState();
        try { OnWorldSwitch?.Invoke(isShadowActive); } catch (Exception ex) { Debug.LogWarning(ex); }
    }

    void Update()
    {
        if (!enableKeyToggle) return;

        bool pressed = false;

        // Legacy input
        if (Input.GetKeyDown(toggleKey)) pressed = true;

        // New Input System fallback for common keys (optional)
        #if ENABLE_INPUT_SYSTEM
        if (!pressed && Keyboard.current != null)
        {
            if (toggleKey == KeyCode.E && Keyboard.current.eKey.wasPressedThisFrame) pressed = true;
            if (toggleKey == KeyCode.LeftShift && Keyboard.current.leftShiftKey.wasPressedThisFrame) pressed = true;
            if (toggleKey == KeyCode.Space && Keyboard.current.spaceKey.wasPressedThisFrame) pressed = true;
        }
        #endif

        if (pressed) ToggleWorld();
    }

    /// <summary>
    /// Toggle between real (day) and shadow (night) worlds.
    /// </summary>
    public void ToggleWorld()
    {
        isShadowActive = !isShadowActive;
        ApplyWorldState();

        if (switchClip != null && audioSrc != null) audioSrc.PlayOneShot(switchClip, switchVolume);

        try { OnWorldSwitch?.Invoke(isShadowActive); } catch (Exception ex) { Debug.LogWarning(ex); }
        Debug.Log("WorldManager3D: switched to " + (isShadowActive ? "Shadow (Night)" : "Real (Day)") + " world.");
    }

    /// <summary>
    /// Force set world state. notify triggers event if notify==true.
    /// </summary>
    public void SetWorld(bool shadowActive, bool notify = true)
    {
        if (isShadowActive == shadowActive) return;
        isShadowActive = shadowActive;
        ApplyWorldState();
        if (notify) OnWorldSwitch?.Invoke(isShadowActive);
    }

    void ApplyWorldState()
    {
        // Enable / disable world parents if assigned
        if (realWorld != null) realWorld.SetActive(!isShadowActive);
        if (shadowWorld != null) shadowWorld.SetActive(isShadowActive);

        // Skybox switch
        if (!isShadowActive && daySkybox != null)
            RenderSettings.skybox = daySkybox;
        else if (isShadowActive && nightSkybox != null)
            RenderSettings.skybox = nightSkybox;

        // Lighting: ambient, fog, and directional sun
        if (!isShadowActive)
        {
            RenderSettings.ambientLight = dayAmbientColor;
            RenderSettings.fogColor = dayFogColor;
            if (sunLight != null)
            {
                sunLight.color = dayLightColor;
                sunLight.intensity = dayLightIntensity;
            }
        }
        else
        {
            RenderSettings.ambientLight = nightAmbientColor;
            RenderSettings.fogColor = nightFogColor;
            if (sunLight != null)
            {
                sunLight.color = nightLightColor;
                sunLight.intensity = nightLightIntensity;
            }
        }

        // Make sure skybox / GI update (immediate)
        // DynamicGI may be necessary in Built-in pipeline; in URP/HDRP behavior varies.
        try { DynamicGI.UpdateEnvironment(); } catch { /* ignore if unavailable */ }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    #if UNITY_EDITOR
    [ContextMenu("Toggle World (Editor)")]
    void EditorToggle() => ToggleWorld();
    #endif
}