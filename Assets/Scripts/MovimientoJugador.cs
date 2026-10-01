using UnityEngine;
using UnityEngine.InputSystem;

public class Movimiento : MonoBehaviour
{
    public float speed = 5f;
    public float velocidadGiro = 12f;             
    public Transform cameraTransform;
    private Rigidbody rb;
    private Vector3 checkpointActual;
    private InputAction moveAction;

    void Awake()
    {
        moveAction = new InputAction("Move", InputActionType.Value);

        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        moveAction.AddBinding("<Gamepad>/leftStick");
    }

    void OnEnable() { moveAction.Enable(); }
    void OnDisable() { moveAction.Disable(); }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        checkpointActual = transform.position;
    }

    void FixedUpdate()
    {
        Seguimiento();
    }

    void Seguimiento()
    {
        if (cameraTransform == null) return;

        Vector2 input = moveAction.ReadValue<Vector2>();

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 dir = Vector3.ClampMagnitude(camForward * input.y + camRight * input.x, 1f);
        Vector3 movement = dir * speed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion objetivo = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, objetivo, velocidadGiro * Time.fixedDeltaTime));
        }
    }

    public void EstablecerCheckpoint(Vector3 posicion)
    {
        checkpointActual = posicion;
    }

    public void ReiniciarEnCheckpoint()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;   
        transform.position = checkpointActual;
        Debug.Log("Jugador regresado al Checkpoint");
    }
}