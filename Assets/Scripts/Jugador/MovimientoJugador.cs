using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Movimiento : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;
    public float velocidadCorrer = 8f;
    public Key teclaCorrer = Key.LeftShift;
    public float velocidadGiro = 12f;
    public Transform cameraTransform;

    [Header("Salto")]
    public Key teclaSaltar = Key.Space;
    public float alturaSalto = 1.2f;
    public float multiplicadorCaida = 2f;
    public float tiempoCoyote = 0.1f;
    public float bufferSalto = 0.1f;
    public float distanciaSuelo = 0.2f;
    public LayerMask capasSuelo = ~0;

    [Header("Agacharse / Esconderse")]
    public Key teclaEsconder = Key.C;
    [Range(0f, 1f)] public float velocidadAgachado = 0f;

    [Header("Animator (parámetros)")]
    public float suavizadoAnimacion = 0.1f;
    public string paramVelocidad = "Speed";       
    public string paramAgachado = "IsCrouching";  
    public string paramEnSuelo = "IsGrounded";    
    public string paramSalto = "Jump";            

    public bool EstaAgachado => agachado;
    public bool EstaEscondido => agachado && zonasEscondite > 0;
    public bool PuedeEsconderse => zonasEscondite > 0;
    public bool EstaEnSuelo => enSuelo;

    const float SinTiempo = -10f;

    Rigidbody rb;
    Collider col;
    Animator anim;
    InputAction moveAction;

    Vector3 checkpointActual;
    Vector2 inputMovimiento;
    float velocidadHorizontal;   
    int zonasEscondite;
    bool agachado;
    bool enSuelo;
    float ultimoTiempoSuelo = SinTiempo;
    float ultimoSaltoPresionado = SinTiempo;

    int hashVelocidad, hashAgachado, hashEnSuelo, hashSalto;
    bool tieneVelocidad, tieneAgachado, tieneEnSuelo, tieneSalto;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        CrearAccionMovimiento();
    }

    void Start()
    {
        ConfigurarAnimator();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        checkpointActual = transform.position;
    }

    void OnEnable() { moveAction.Enable(); }
    void OnDisable() { moveAction.Disable(); }
    void OnDestroy() { moveAction?.Dispose(); }

    void Update()
    {
        LeerInput();
        ActualizarAnimator();
    }

    void FixedUpdate()
    {
        Vector3 dir = DireccionDeMovimiento();

        bool corriendo = !agachado && dir.sqrMagnitude > 0.001f && TeclaMantenida(teclaCorrer, RightTriggerMando());
        Vector3 horizontal = dir * (corriendo ? velocidadCorrer : speed);

        rb.linearVelocity = new Vector3(horizontal.x, CalcularVelocidadVertical(), horizontal.z);

        Rotar(dir);
        velocidadHorizontal = horizontal.magnitude;
    }


    void CrearAccionMovimiento()
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

    void LeerInput()
    {
        inputMovimiento = moveAction.ReadValue<Vector2>();

        if (TeclaPulsada(teclaEsconder) || (Gamepad.current?.buttonEast.wasPressedThisFrame ?? false))
            agachado = !agachado;

        if (TeclaPulsada(teclaSaltar) || (Gamepad.current?.buttonSouth.wasPressedThisFrame ?? false))
            ultimoSaltoPresionado = Time.time;
    }

    static bool TeclaPulsada(Key tecla)
        => Keyboard.current != null && Keyboard.current[tecla].wasPressedThisFrame;

    static bool TeclaMantenida(Key tecla, bool botonMando)
        => botonMando || (Keyboard.current != null && Keyboard.current[tecla].isPressed);

    static bool RightTriggerMando()
        => Gamepad.current != null && Gamepad.current.rightTrigger.isPressed;


    Vector3 DireccionDeMovimiento()
    {
        Vector2 input = agachado ? inputMovimiento * velocidadAgachado : inputMovimiento;

        if (cameraTransform == null || input.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        Vector3 adelante = cameraTransform.forward;
        Vector3 derecha = cameraTransform.right;
        adelante.y = 0f;
        derecha.y = 0f;
        adelante.Normalize();
        derecha.Normalize();

        return Vector3.ClampMagnitude(adelante * input.y + derecha * input.x, 1f);
    }

    void Rotar(Vector3 dir)
    {
        if (dir.sqrMagnitude <= 0.001f) return;

        Quaternion objetivo = Quaternion.LookRotation(dir);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, objetivo, velocidadGiro * Time.fixedDeltaTime));
    }

    float CalcularVelocidadVertical()
    {
        float vy = rb.linearVelocity.y;
        enSuelo = vy <= 0.1f && DetectarSuelo();
        if (enSuelo) ultimoTiempoSuelo = Time.time;

        bool puedeSaltar = !agachado && !DialogoInteractuable.Hablando;
        bool saltoPedido = Time.time - ultimoSaltoPresionado <= bufferSalto;
        bool apoyadoRecien = Time.time - ultimoTiempoSuelo <= tiempoCoyote;

        if (puedeSaltar && saltoPedido && apoyadoRecien)
        {
            ultimoSaltoPresionado = SinTiempo;  
            ultimoTiempoSuelo = SinTiempo;      
            if (tieneSalto) anim.SetTrigger(hashSalto);

            return Mathf.Sqrt(2f * -Physics.gravity.y * alturaSalto);
        }

        if (vy < 0f)
            vy += Physics.gravity.y * (multiplicadorCaida - 1f) * Time.fixedDeltaTime;

        return vy;
    }

    Vector3 OrigenDetectorSuelo()
    {
        if (col == null) return transform.position + Vector3.up * 0.1f;

        Bounds b = col.bounds;
        return new Vector3(b.center.x, b.min.y + 0.1f, b.center.z);
    }

    bool DetectarSuelo()
    {
        return Physics.Raycast(OrigenDetectorSuelo(), Vector3.down, 0.1f + distanciaSuelo,
                               capasSuelo, QueryTriggerInteraction.Ignore);
    }

    void ConfigurarAnimator()
    {
        anim = GetComponentInChildren<Animator>();
        if (anim == null) return;

        AnimatorControllerParameter[] parametros = anim.parameters;

        hashVelocidad = Animator.StringToHash(paramVelocidad);
        hashAgachado = Animator.StringToHash(paramAgachado);
        hashEnSuelo = Animator.StringToHash(paramEnSuelo);
        hashSalto = Animator.StringToHash(paramSalto);

        tieneVelocidad = ExisteParametro(parametros, paramVelocidad, AnimatorControllerParameterType.Float);
        tieneAgachado = ExisteParametro(parametros, paramAgachado, AnimatorControllerParameterType.Bool);
        tieneEnSuelo = ExisteParametro(parametros, paramEnSuelo, AnimatorControllerParameterType.Bool);
        tieneSalto = ExisteParametro(parametros, paramSalto, AnimatorControllerParameterType.Trigger);
    }

    static bool ExisteParametro(AnimatorControllerParameter[] parametros, string nombre, AnimatorControllerParameterType tipo)
    {
        if (!string.IsNullOrEmpty(nombre))
            foreach (AnimatorControllerParameter p in parametros)
                if (p.name == nombre && p.type == tipo) return true;

        Debug.LogWarning("Movimiento: el Animator no tiene el parámetro '" + nombre + "' de tipo " + tipo + ".");
        return false;
    }

    void ActualizarAnimator()
    {
        if (anim == null) return;

        if (tieneVelocidad) anim.SetFloat(hashVelocidad, velocidadHorizontal, suavizadoAnimacion, Time.deltaTime);
        if (tieneAgachado) anim.SetBool(hashAgachado, agachado);
        if (tieneEnSuelo) anim.SetBool(hashEnSuelo, enSuelo);
    }

    public void EstablecerCheckpoint(Vector3 posicion)
    {
        checkpointActual = posicion;
    }

    public void ReiniciarEnCheckpoint()
    {
        rb.linearVelocity = Vector3.zero;
        transform.position = checkpointActual;
        agachado = false;
        zonasEscondite = 0;
        ultimoSaltoPresionado = SinTiempo;
        Debug.Log("Jugador regresado al Checkpoint");
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Escondite")) zonasEscondite++;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Escondite")) zonasEscondite = Mathf.Max(0, zonasEscondite - 1);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (col == null) col = GetComponent<Collider>();
        Vector3 origen = OrigenDetectorSuelo();
        Gizmos.DrawLine(origen, origen + Vector3.down * (0.1f + distanciaSuelo));
    }
}