using UnityEngine;
using UnityEngine.AI;

public class SerpienteTest : MonoBehaviour
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

    [Header("Captura")]
    public float distanciaCaptura = 1.2f;

    private NavMeshAgent agente;
    private int indice;
    private bool persiguiendo;
    private float tiempoVisible;
    private float tiempoOculto;
    private Movimiento jugadorScript;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.speed = velocidadPatrulla;

        if (puntos.Length > 0)
            agente.SetDestination(puntos[0].position);

        if (jugador != null)
            jugadorScript = jugador.GetComponent<Movimiento>();

        if (vistaCono != null)
            vistaCono.Configurar(angulo, alcance);
    }

    void Update()
    {
        bool detectaAhora = EstaEnCono();

        if (detectaAhora)
        {
            tiempoVisible += Time.deltaTime;
            tiempoOculto = 0f;

            if (!persiguiendo && tiempoVisible >= tiempoParaDetectar)
            {
                persiguiendo = true;
                agente.speed = velocidadPersecucion;
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
                    persiguiendo = false;
                    agente.speed = velocidadPatrulla;
                    Debug.Log("La serpiente perdió al jugador.");
                }
            }
        }

        if (persiguiendo)
        {
            agente.SetDestination(jugador.position);

            if (Vector3.Distance(transform.position, jugador.position) <= distanciaCaptura)
            {
                Debug.Log("¡Atrapado! Regresando al checkpoint.");
                jugadorScript?.ReiniciarEnCheckpoint();
                persiguiendo = false;
                agente.speed = velocidadPatrulla;
                tiempoVisible = 0f;
            }
        }
        else
        {
            Patrullar();
        }
    }

    void Patrullar()
    {
        if (puntos.Length == 0) return;

        if (!agente.pathPending && agente.remainingDistance < 0.3f)
        {
            indice = (indice + 1) % puntos.Length;
            agente.SetDestination(puntos[indice].position);
        }
    }

    bool EstaEnCono()
    {
        if (jugador == null) return false;

        Vector3 direccionAlJugador = jugador.position - transform.position;
        direccionAlJugador.y = 0;
        float distancia = direccionAlJugador.magnitude;

        if (distancia > alcance) return false;

        float anguloEntre = Vector3.Angle(transform.forward, direccionAlJugador);
        if (anguloEntre > angulo * 0.5f) return false;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, direccionAlJugador.normalized, out RaycastHit hit, distancia, capaObstaculos))
            return false;

        return true;
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