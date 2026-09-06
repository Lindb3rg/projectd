using UnityEngine;
using UnityEngine.Events;

public enum GravityDirection { Down, Up, Left, Right, None }

public class GravityController : MonoBehaviour
{
    public static GravityController Instance { get; private set; }

    [SerializeField] private float gravityStrength = 9.81f;
    [SerializeField] private GravityDirection initialDirection = GravityDirection.Down;

    [SerializeField] private GravityDirection debugDirection;
    private GravityDirection lastDirection;

    // This public getter allows other scripts to read the current enum value directly
    public GravityDirection CurrentDirection { get; private set; }
    public Vector2 GravityVector { get; private set; }

    public UnityEvent<GravityDirection> OnGravityChanged = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        SetGravity(initialDirection);
        debugDirection = initialDirection;
        lastDirection = initialDirection;
    }

    void Update()
    {
        if (debugDirection != lastDirection)
        {
            SetGravity(debugDirection);
            lastDirection = debugDirection;
        }
    }

    public void SetGravity(GravityDirection dir)
    {
        CurrentDirection = dir;
        GravityVector = dir switch
        {
            GravityDirection.Up => Vector2.up * gravityStrength,
            GravityDirection.Left => Vector2.left * gravityStrength,
            GravityDirection.Right => Vector2.right * gravityStrength,
            GravityDirection.None => Vector2.zero,
            _ => Vector2.down * gravityStrength,
        };
        Physics.gravity = GravityVector;
        OnGravityChanged.Invoke(CurrentDirection);
    }
}
