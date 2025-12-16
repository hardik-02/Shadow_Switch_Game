using UnityEngine;

public class Level2Manager : MonoBehaviour
{
    public static Level2Manager Instance { get; private set; }

    [Header("Rune / Gate")]
    public int runesNeeded = 3;
    public int runesCollected = 0;

    [Tooltip("Assign the gate GameObject (it will move up when unlocked)")]
    public GameObject gate;
    public float gateOpenHeight = 5f;
    public float gateOpenSpeed = 2f;

    bool gateOpening = false;
    Vector3 gateClosedPos;
    Vector3 gateOpenPos;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    void Start()
    {
        if (gate != null)
        {
            gateClosedPos = gate.transform.position;
            gateOpenPos = gateClosedPos + Vector3.up * gateOpenHeight;
        }
    }

    public void AddRune()
    {
        runesCollected++;
        Debug.Log($"Level2Manager: Rune collected {runesCollected}/{runesNeeded}");
        if (runesCollected >= runesNeeded)
        {
            OpenGate();
        }
    }

    void OpenGate()
    {
        gateOpening = true;
        Debug.Log("Level2Manager: All runes collected — opening gate.");
    }

    void Update()
    {
        if (gateOpening && gate != null)
        {
            gate.transform.position = Vector3.MoveTowards(gate.transform.position, gateOpenPos, gateOpenSpeed * Time.deltaTime);
        }
    }
}