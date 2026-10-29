using UnityEngine;
using UnityEngine.AI;

public class Patrulla : MonoBehaviour
{
    [Header("Patrulla")]
    public Transform[] puntos;
    public float velocidadPatrulla = 2f;
    public float velocidadPersecucion = 4.5f;

    [Header("Cono de visión")]
    public Transform jugador;
    public float angulo = 60f;
    public float alcance = 10f;
    public LayerMask capaObstaculos;
    public float tiempoParaDetectar = 0.75f;
    public float tiempoParaPerder = 3f;
    public ConoVisionVisual vistaCono;

    [Header("Sensación de persecución rápida")]
    public float velocidadMaximaPersecucion = 9f;
    public float rampaPersecucion = 1.5f;
    public float aceleracionPersecucion = 30f;
    public float giroPersecucion = 360f;

    [Header("Captura")]
    public float distanciaCaptura = 1.2f;

    [Header("Animator (parámetros opcionales)")]
    public string paramVelocidad = "Speed";       
    public string paramPersiguiendo = "Chasing";   

    private NavMeshAgent agente;
    private Animator anim;
    private int indice;
    private bool persiguiendo;
    private float tiempoVisible;
    private float tiempoOculto;
    private Vector3 ultimaPosVista;
    private Movimiento jugadorScript;
    private float aceleracionPatrulla;
    private float giroPatrulla;
    private bool tieneSpeed;
    private bool tieneChasing;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.speed = velocidadPatrulla;
        aceleracionPatrulla = agente.acceleration;
        giroPatrulla = agente.angularSpeed;

        if (puntos.Length > 0)
            agente.SetDestination(puntos[0].position);

        if (jugador != null)
            jugadorScript = jugador.GetComponent<Movimiento>();

        if (vistaCono != null)
            vistaCono.Configurar(angulo, alcance, capaObstaculos);

        anim = GetComponentInChildren<Animator>();
        tieneSpeed = TieneParametro(paramVelocidad, AnimatorControllerParameterType.Float);
        tieneChasing = TieneParametro(paramPersiguiendo, AnimatorControllerParameterType.Bool);
    }

    void Update()
    {
        bool detectaAhora = EstaEnCono();

        if (detectaAhora)
        {
            ultimaPosVista = jugador.position;
            tiempoVisible += Time.deltaTime;
            tiempoOculto = 0f;

            if (!persiguiendo && tiempoVisible >= tiempoParaDetectar)
            {
                ActivarPersecucion();
                Debug.Log("¡Serpiente detectó al jugador!");
            }
        }
        else
        {
            tiempoVisible = 0f;

            if (persiguiendo)
            {
                tiempoOculto += Time.deltaTime;
                if (tiempoOculto >= tiempoParaPerder)
                {
                    DesactivarPersecucion();
                    Debug.Log("La serpiente perdió al jugador.");
                }
            }
        }

        if (persiguiendo)
        {

            agente.speed = Mathf.MoveTowards(agente.speed, velocidadMaximaPersecucion,
                                             rampaPersecucion * Time.deltaTime);

            agente.SetDestination(detectaAhora ? jugador.position : ultimaPosVista);

            bool puedeCapturar = jugadorScript == null || !jugadorScript.EstaEscondido;

            if (puedeCapturar && Vector3.Distance(transform.position, jugador.position) <= distanciaCaptura)
            {
                Debug.Log("¡Atrapado! Regresando al checkpoint.");
                jugadorScript?.ReiniciarEnCheckpoint();
                DesactivarPersecucion();
                tiempoVisible = 0f;
                tiempoOculto = 0f;

                if (puntos.Length > 0)
                    agente.SetDestination(puntos[indice].position);
            }
        }
        else
        {
            Patrullar();
        }

        ActualizarAnimator();
    }

    void Patrullar()
    {
        if (puntos.Length == 0) return;

        if (!agente.pathPending && agente.remainingDistance <= agente.stoppingDistance + 0.3f)
        {
            indice = (indice + 1) % puntos.Length;
            agente.SetDestination(puntos[indice].position);
        }
    }

    bool EstaEnCono()
    {
        if (jugador == null) return false;

        if (jugadorScript != null && jugadorScript.EstaEscondido) return false;

        Vector3 direccionAlJugador = jugador.position - transform.position;
        direccionAlJugador.y = 0;
        float distancia = direccionAlJugador.magnitude;

        if (distancia > alcance) return false;

        float anguloEntre = Vector3.Angle(transform.forward, direccionAlJugador);
        if (anguloEntre > angulo * 0.5f) return false;

        Vector3 origen = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(origen, direccionAlJugador.normalized, distancia, capaObstaculos,
                            QueryTriggerInteraction.Ignore))
            return false;

        return true;
    }

    public void IniciarPersecucion()
    {
        if (jugador == null || persiguiendo) return;

        ActivarPersecucion();
        ultimaPosVista = jugador.position;
        tiempoOculto = 0f;
        Debug.Log("La serpiente inicia la persecución.");
    }

    void ActivarPersecucion()
    {
        persiguiendo = true;
        agente.speed = velocidadPersecucion;
        agente.acceleration = aceleracionPersecucion;
        agente.angularSpeed = giroPersecucion;
    }

    void DesactivarPersecucion()
    {
        persiguiendo = false;
        agente.speed = velocidadPatrulla;
        agente.acceleration = aceleracionPatrulla;
        agente.angularSpeed = giroPatrulla;
    }

    void ActualizarAnimator()
    {
        if (anim == null) return;
        if (tieneSpeed) anim.SetFloat(paramVelocidad, agente.velocity.magnitude);
        if (tieneChasing) anim.SetBool(paramPersiguiendo, persiguiendo);
    }

    bool TieneParametro(string nombre, AnimatorControllerParameterType tipo)
    {
        if (anim == null || string.IsNullOrEmpty(nombre)) return false;
        foreach (AnimatorControllerParameter p in anim.parameters)
            if (p.name == nombre && p.type == tipo) return true;
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 izquierda = Quaternion.Euler(0, -angulo * 0.5f, 0) * transform.forward * alcance;
        Vector3 derecha = Quaternion.Euler(0, angulo * 0.5f, 0) * transform.forward * alcance;
        Gizmos.DrawLine(transform.position, transform.position + izquierda);
        Gizmos.DrawLine(transform.position, transform.position + derecha);
        Gizmos.DrawWireSphere(transform.position, alcance);
    }
}