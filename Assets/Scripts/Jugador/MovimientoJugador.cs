using UnityEngine;
using UnityEngine.InputSystem;

public class Movimiento : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;
    public float velocidadCorrer = 8f;
    public Key teclaCorrer = Key.LeftShift;
    public float velocidadGiro = 12f;
    public Transform cameraTransform;

    [Header("Agacharse / Esconderse")]
    public Key teclaEsconder = Key.C;
    [Range(0f, 1f)] public float velocidadAgachado = 0f;

    [Header("Animator (parámetros)")]
    public float suavizadoAnimacion = 0.1f;
    public string paramVelocidad = "Speed";   
    public string paramEscondido = "Hidden";  

    private Rigidbody rb;
    private Animator anim;
    private Vector3 checkpointActual;
    private InputAction moveAction;
    private int zonasEscondite;   
    private bool agachado;

    public bool EstaAgachado => agachado;

    public bool EstaEscondido => agachado && zonasEscondite > 0;


    public bool PuedeEsconderse => zonasEscondite > 0;

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
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        anim = GetComponentInChildren<Animator>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        checkpointActual = transform.position;
    }

    void Update()
    {
        if (PresionoEsconder())
        {
            agachado = !agachado;  
        }
    }

    bool PresionoEsconder()
    {
        bool teclado = Keyboard.current != null && Keyboard.current[teclaEsconder].wasPressedThisFrame;
        bool mando = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;
        return teclado || mando;
    }

    bool PresionoCorrer()
    {
        bool teclado = Keyboard.current != null && Keyboard.current[teclaCorrer].isPressed;
        bool mando = Gamepad.current != null && Gamepad.current.rightTrigger.isPressed;
        return teclado || mando;
    }

    void FixedUpdate()
    {
        Seguimiento();
    }

    void Seguimiento()
    {
        if (cameraTransform == null) return;

        Vector2 input = moveAction.ReadValue<Vector2>();
        if (agachado) input *= velocidadAgachado;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 dir = Vector3.ClampMagnitude(camForward * input.y + camRight * input.x, 1f);
        bool corriendo = !agachado && PresionoCorrer() && dir.sqrMagnitude > 0.001f;
        float velocidadActual = corriendo ? velocidadCorrer : speed;
        Vector3 movement = dir * velocidadActual;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

        rb.angularVelocity = Vector3.zero;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion objetivo = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, objetivo, velocidadGiro * Time.fixedDeltaTime));
        }


        if (anim != null)
        {
            anim.SetFloat(paramVelocidad, movement.magnitude, suavizadoAnimacion, Time.fixedDeltaTime);
            anim.SetBool(paramEscondido, agachado);
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
        agachado = false;
        zonasEscondite = 0;
        Debug.Log("Jugador regresado al Checkpoint");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Escondite")) zonasEscondite++;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Escondite")) return;

        zonasEscondite = Mathf.Max(0, zonasEscondite - 1);
    }
}