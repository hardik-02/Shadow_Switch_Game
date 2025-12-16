using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ToggleCooldown : MonoBehaviour
{
    public float cooldownSeconds = 0.8f;
    float lastToggleTime = -999f;
    public KeyCode toggleKey = KeyCode.E;
    public bool useNewInputSystemFallback = true;

    void Update()
    {
        bool pressed = false;
        if (Input.GetKeyDown(toggleKey)) pressed = true;

#if ENABLE_INPUT_SYSTEM
        if (!pressed && useNewInputSystemFallback && Keyboard.current != null)
        {
            if (toggleKey == KeyCode.E && Keyboard.current.eKey.wasPressedThisFrame) pressed = true;
            if (toggleKey == KeyCode.LeftShift && Keyboard.current.leftShiftKey.wasPressedThisFrame) pressed = true;
        }
#endif

        if (pressed)
        {
            if (Time.time - lastToggleTime < cooldownSeconds) return;
            lastToggleTime = Time.time;
            WorldManager3D.Instance?.ToggleWorld();
        }
    }
}
