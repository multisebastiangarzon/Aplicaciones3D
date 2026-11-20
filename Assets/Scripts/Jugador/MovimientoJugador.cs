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
    [Tooltip("Distancia bajo los pies a la que todavía se considera suelo.")]
    public float distanciaSuelo = 0.2f;
    [Tooltip("Todo lo que cuenta como suelo. Quita Personajes y Decoracion.")]
    public LayerMask capasSuelo = ~0;

    [Header("Audio")]
    public AudioClip sonidoSalto;
    [Range(0f, 1f)] public float volumenSalto = 0.8f;
    [Tooltip("Rango aleatorio de tono para que no suene idéntico cada vez. 1 y 1 = sin variación.")]
    public Vector2 variacionTono = new Vector2(0.95f, 1.05f);

    [Header("Pendientes")]
    [Tooltip("Pendiente máxima que se puede subir caminando, en grados. " +
             "En caras más empinadas no se trepa ni se puede saltar.")]
    public float anguloMaxPendiente = 50f;

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
    static readonly Collider[] bufferSuelo = new Collider[8];

    Rigidbody rb;
    Collider col;
    Animator anim;
    AudioSource audioSrc;
    InputAction moveAction;

    Vector3 checkpointActual;
    Vector3 normalEmpinada;
    Vector2 inputMovimiento;
    float velocidadHorizontal;
    int zonasEscondite;
    bool agachado, enSuelo;
    bool pendienteEmpinada, contactoLlano, bloqueaSalto;
    float ultimoTiempoSuelo = SinTiempo;
    float ultimoSaltoPresionado = SinTiempo;
    float ultimoSalto = SinTiempo;

    int hashVelocidad, hashAgachado, hashEnSuelo, hashSalto;
    bool tieneVelocidad, tieneAgachado, tieneEnSuelo, tieneSalto;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null)
        {
            audioSrc = gameObject.AddComponent<AudioSource>();
            audioSrc.spatialBlend = 0f;   // 2D: se oye igual desde cualquier cámara
        }
        audioSrc.playOnAwake = false;
        audioSrc.loop = false;

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
        Vector3 dir = AcertijoCultural.Activo ? Vector3.zero : DireccionDeMovimiento();

        bool corriendo = !agachado && dir.sqrMagnitude > 0.001f && TeclaMantenida(teclaCorrer, RightTriggerMando());
        Vector3 horizontal = dir * (corriendo ? velocidadCorrer : speed);

        if (pendienteEmpinada)
        {
            Vector3 fuera = new Vector3(normalEmpinada.x, 0f, normalEmpinada.z).normalized;
            float dentro = Vector3.Dot(horizontal, fuera);
            if (dentro < 0f) horizontal -= fuera * dentro;
        }
        bloqueaSalto = pendienteEmpinada && !contactoLlano;   // sin suelo normal bajo los pies no se salta
        pendienteEmpinada = contactoLlano = false;            // se vuelven a marcar en el siguiente paso

        rb.linearVelocity = new Vector3(horizontal.x, CalcularVelocidadVertical(), horizontal.z);

        Rotar(dir);
        velocidadHorizontal = horizontal.magnitude;
    }

    void OnCollisionStay(Collision c)
    {
        float cosMax = Mathf.Cos(anguloMaxPendiente * Mathf.Deg2Rad);

        for (int i = 0; i < c.contactCount; i++)
        {
            Vector3 n = c.GetContact(i).normal;

            if (n.y >= cosMax) contactoLlano = true;
            else if (n.y > 0.05f) { pendienteEmpinada = true; normalEmpinada = n; }
        }
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

        // Justo tras saltar no cuenta como suelo, para no "tocar suelo" al despegar
        enSuelo = Time.time - ultimoSalto >= 0.15f && DetectarSuelo();
        if (enSuelo) ultimoTiempoSuelo = Time.time;

        bool puedeSaltar = !agachado && !bloqueaSalto && !DialogoInteractuable.Hablando && !AcertijoCultural.Activo;
        bool saltoPedido = Time.time - ultimoSaltoPresionado <= bufferSalto;
        bool apoyadoRecien = Time.time - ultimoTiempoSuelo <= tiempoCoyote;

        if (puedeSaltar && saltoPedido && apoyadoRecien)
        {
            ultimoSaltoPresionado = ultimoTiempoSuelo = SinTiempo;
            ultimoSalto = Time.time;
            enSuelo = false;             // IsGrounded pasa a falso en el mismo instante
            if (tieneSalto) anim.SetTrigger(hashSalto);

            ReproducirSalto();

            return Mathf.Sqrt(2f * -Physics.gravity.y * alturaSalto);
        }

        if (vy < 0f)
            vy += Physics.gravity.y * (multiplicadorCaida - 1f) * Time.fixedDeltaTime;

        return vy;
    }

    void ReproducirSalto()
    {
        if (sonidoSalto == null || audioSrc == null) return;

        audioSrc.pitch = Random.Range(variacionTono.x, variacionTono.y);
        audioSrc.PlayOneShot(sonidoSalto, volumenSalto);
    }

    // Esfera bajo los pies: cubre aristas, escalones y cuestas (un solo rayo fallaba ahí)
    Vector3 CentroEsferaSuelo(out float radio)
    {
        Bounds b = col.bounds;
        radio = Mathf.Min(b.extents.x, b.extents.z) * 0.9f;
        return new Vector3(b.center.x, b.min.y + radio - distanciaSuelo, b.center.z);
    }

    bool DetectarSuelo()
    {
        if (col == null) return false;

        Vector3 centro = CentroEsferaSuelo(out float radio);
        int n = Physics.OverlapSphereNonAlloc(centro, radio, bufferSuelo, capasSuelo, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
            if (bufferSuelo[i] != col && !bufferSuelo[i].transform.IsChildOf(transform)) return true;

        return false;
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
        Physics.SyncTransforms();

        agachado = false;
        zonasEscondite = 0;
        pendienteEmpinada = bloqueaSalto = false;
        ultimoSaltoPresionado = ultimoSalto = SinTiempo;
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

}