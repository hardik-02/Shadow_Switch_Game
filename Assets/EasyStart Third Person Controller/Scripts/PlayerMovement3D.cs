using System.Collections;
using UnityEngine;

/// <summary>
/// ThirdPersonShadowController
/// - CharacterController movement (camera-relative WASD)
/// - Sprint, Crouch, Jump (physics-correct initial jump velocity)
/// - Robust ground detection (cc.isGrounded + sphere fallback)
/// - Animator parameter updates (Speed, run, sprint, air, crouch, Jump trigger)
/// - Player color changes when WorldManager3D.isShadowActive toggles
/// - Does NOT use StarterAssets
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovement3D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintAddition = 3f;
    public float crouchSlowPercent = 0.5f;
    public float rotationSmooth = 12f; // larger = faster rotation (used in Slerp speed)

    [Header("Jump / Gravity")]
    public float jumpHeight = 1.2f; // meters
    public float gravity = -18f;   // negative value
    public float jumpDurationMax = 0.85f; // optional controlled ascent smoothing

    [Header("Ground Check")]
    public Transform groundCheck; // if null, created automatically
    public float groundCheckRadius = 0.28f;
    public LayerMask groundMask;

    [Header("Animation")]
    public Animator animator; // assign Animator (optional but recommended)
    [Tooltip("If your Animator uses a 'Speed' float, it'll be set to current horizontal speed.")]
    public string paramSpeed = "Speed";
    public string paramRun = "run";
    public string paramSprint = "sprint";
    public string paramAir = "air";
    public string paramCrouch = "crouch";
    public string paramJumpTrigger = "Jump"; // optional trigger

    [Header("World Colors")]
    public Renderer[] renderersToColor; // assign SkinnedMeshRenderer(s) or leave empty to auto-find
    public Color realColor = Color.cyan;
    public Color shadowColor = new Color(0.4f, 0.1f, 0.6f);
    public float colorLerpDuration = 0.35f;

    [Header("Audio")]
    public AudioClip jumpClip;
    [Range(0f,1f)] public float jumpVol = 0.9f;

    // runtime
    CharacterController cc;
    AudioSource audioSrc;

    Vector2 rawInput;
    Vector3 verticalVel = Vector3.zero;
    bool isGrounded = false;
    bool isCrouching = false;
    bool lastShadowState = false;

    // cached animator hashes (faster than strings)
    int hashSpeed, hashRun, hashSprint, hashAir, hashCrouch, hashJumpTrigger;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        audioSrc = GetComponent<AudioSource>();
        if (audioSrc != null) audioSrc.playOnAwake = false;

        // auto find animator if not assigned
        if (animator == null) animator = GetComponentInChildren<Animator>();

        // compute hashes
        hashSpeed = Animator.StringToHash(paramSpeed);
        hashRun = Animator.StringToHash(paramRun);
        hashSprint = Animator.StringToHash(paramSprint);
        hashAir = Animator.StringToHash(paramAir);
        hashCrouch = Animator.StringToHash(paramCrouch);
        hashJumpTrigger = Animator.StringToHash(paramJumpTrigger);

        // create groundCheck if missing (placed near bottom of controller)
        if (groundCheck == null)
        {
            GameObject g = new GameObject("GroundCheck");
            g.transform.SetParent(transform);
            // place slightly above bottom of character controller
            float y = -Mathf.Max(0.01f, cc.height * 0.5f - cc.skinWidth - 0.05f);
            g.transform.localPosition = new Vector3(0f, y, 0f);
            groundCheck = g.transform;
        }

        // auto find renderers if none set
        if (renderersToColor == null || renderersToColor.Length == 0)
        {
            Renderer r = GetComponentInChildren<Renderer>();
            if (r != null) renderersToColor = new Renderer[] { r };
        }
    }

    void Start()
    {
        // initialize color from WorldManager if available
        if (WorldManager3D.Instance != null)
        {
            lastShadowState = WorldManager3D.Instance.isShadowActive;
            ApplyColorInstant(lastShadowState ? shadowColor : realColor);
        }
    }

    void Update()
    {
        ReadInput();
        HandleJumpInput();
        PollWorldStateAndColor();
        // small debug: if Animator missing warn once
        // (developer can ignore)
    }

    void FixedUpdate()
    {
        UpdateGroundedState();
        MoveCharacter();
        ApplyVerticalMotion();
        UpdateAnimatorParameters();
    }

    // ===== INPUT =====
    void ReadInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        rawInput = new Vector2(h, v);

        if (Input.GetKeyDown(KeyCode.LeftControl))
            isCrouching = !isCrouching;
    }

    // ===== GROUND & JUMP =====
    void UpdateGroundedState()
    {
        // prefer cc.isGrounded (handles slopes), but also do a sphere check to be robust
        bool sphere = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);
        isGrounded = cc.isGrounded || sphere;

        // when grounded, reset small downward velocity to keep contact
        if (isGrounded && verticalVel.y < 0f)
            verticalVel.y = -2f;
    }

    void HandleJumpInput()
    {
        // Jump start
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump")) && isGrounded)
        {
            // initial upward velocity to reach jumpHeight:
            float initialVy = Mathf.Sqrt(2f * Mathf.Abs(gravity) * jumpHeight);
            verticalVel.y = initialVy;
            // optional smoothing time not used to override physics; we still allow a small controlled ascent
            if (audioSrc != null && jumpClip != null) audioSrc.PlayOneShot(jumpClip, jumpVol);

            // animator trigger if exists
            if (animator != null)
            {
                if (animator.HasParameterOfType(paramJumpTrigger, AnimatorControllerParameterType.Trigger))
                    animator.SetTrigger(hashJumpTrigger);
                else
                    animator.SetBool(hashAir, true);
            }
        }
    }

    // ===== MOVEMENT =====
    void MoveCharacter()
    {
        // compute camera-relative move
        Vector3 camF = Camera.main != null ? Camera.main.transform.forward : Vector3.forward;
        Vector3 camR = Camera.main != null ? Camera.main.transform.right : Vector3.right;
        camF.y = camR.y = 0f;
        camF.Normalize();
        camR.Normalize();

        Vector3 move = camF * rawInput.y + camR * rawInput.x;
        if (move.magnitude > 1f) move.Normalize();

        // speed adjustments
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && rawInput.magnitude > 0.1f;
        float speed = moveSpeed + (isSprinting ? sprintAddition : 0f);
        if (isCrouching) speed *= (1f - crouchSlowPercent);

        // rotate towards movement
        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmooth * Time.fixedDeltaTime);
        }

        // horizontal move
        Vector3 horiz = move * speed * Time.fixedDeltaTime;
        cc.Move(horiz);
    }

    void ApplyVerticalMotion()
    {
        // gravity affects verticalVel each physics frame
        verticalVel.y += gravity * Time.fixedDeltaTime;
        cc.Move(verticalVel * Time.fixedDeltaTime);
    }

    // ===== ANIMATOR =====
    void UpdateAnimatorParameters()
    {
        if (animator == null) return;

        float horizontalSpeed = new Vector3(cc.velocity.x, 0f, cc.velocity.z).magnitude;
        animator.SetFloat(hashSpeed, horizontalSpeed);

        animator.SetBool(hashRun, horizontalSpeed > 0.5f);
        animator.SetBool(hashSprint, Input.GetKey(KeyCode.LeftShift) && horizontalSpeed > 0.5f);
        animator.SetBool(hashAir, !isGrounded);
        animator.SetBool(hashCrouch, isCrouching);
    }

    // ===== WORLD COLOR =====
    void PollWorldStateAndColor()
    {
        var wm = WorldManager3D.Instance;
        if (wm == null) return;

        bool current = wm.isShadowActive;
        if (current != lastShadowState)
        {
            lastShadowState = current;
            StopAllCoroutines();
            StartCoroutine(LerpColor(current ? shadowColor : realColor, colorLerpDuration));
        }
    }

    IEnumerator LerpColor(Color target, float duration)
    {
        if (renderersToColor == null || renderersToColor.Length == 0) yield break;

        Color[] starts = new Color[renderersToColor.Length];
        for (int i = 0; i < renderersToColor.Length; i++)
            starts[i] = renderersToColor[i] != null ? renderersToColor[i].material.color : Color.white;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float f = Mathf.Clamp01(t / duration);
            for (int i = 0; i < renderersToColor.Length; i++)
            {
                if (renderersToColor[i] == null) continue;
                renderersToColor[i].material.color = Color.Lerp(starts[i], target, f);
            }
            yield return null;
        }
        for (int i = 0; i < renderersToColor.Length; i++)
            if (renderersToColor[i] != null) renderersToColor[i].material.color = target;
    }

    void ApplyColorInstant(Color c)
    {
        if (renderersToColor == null) return;
        foreach (var r in renderersToColor) if (r != null) r.material.color = c;
    }

    // ===== UTILITIES =====
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}

// ----- helper extension used above to check Animator parameter existence -----
static class AnimatorExtensions
{
    public static bool HasParameterOfType(this Animator animator, string name, AnimatorControllerParameterType type)
    {
        if (animator == null) return false;
        foreach (var p in animator.parameters)
            if (p.type == type && p.name == name) return true;
        return false;
    }
}
