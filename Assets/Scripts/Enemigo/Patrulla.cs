using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Patrulla : MonoBehaviour
{
    [Header("Patrulla")]
    public Transform[] puntos;
    public float velocidadPatrulla = 2f;
    public float velocidadPersecucion = 4.5f;

    [Header("Cono de vision (deteccion)")]
    public Transform jugador;
    public float angulo = 60f;
    public float alcance = 10f;
    public LayerMask capaObstaculos;
    public float alturaOjos = 0.5f;
    public float tiempoParaDetectar = 0.75f;
    public float tiempoParaPerder = 3f;
    public ConoVisionVisual vistaCono;

    [Header("Persecucion rapida")]
    public float velocidadMaximaPersecucion = 9f;
    public float rampaPersecucion = 1.5f;
    public float aceleracionPersecucion = 30f;
    public float giroPersecucion = 360f;
    [Tooltip("Cada cuantos segundos se recalcula el destino")]
    public float intervaloDestino = 0.1f;

    [Header("Persecucion: memoria y sentidos")]
    public float anguloPersecucion = 140f;
    public float alcancePersecucion = 14f;
    public float radioOlfato = 3.5f;
    public float tiempoParaPerderCerca = 6f;
    public float alturaSaltoTolerancia = 3f;
    [Tooltip("Ajusta el cono visual al perseguir")]
    public bool cambiarConoVisualAlPerseguir = true;

    [Header("Ataque / Captura")]
    public float distanciaCaptura = 1.2f;
    [Tooltip("Nombre exacto del estado de ataque en el Animator")]
    public string estadoAtaque = "ATTACK";
    [Tooltip("Pon aqui el Length del clip ATTACK; el tope es el doble")]
    public float duracionAtaque = 1f;
    [Range(0.1f, 1f)] public float puntoGolpe = 0.9f;

    [Header("Animator")]
    public string paramVelocidad = "Speed";
    public string paramPersiguiendo = "Chasing";
    public string paramAtaque = "IsAttacking";

    [Header("Audio")]
    [Tooltip("Si se deja vacio, lo busca en este objeto")]
    public SerpienteAudio audioSerpiente;

    [Header("Dificultad (multiplicadores)")]
    [Tooltip("Multiplica velocidades. >1 = mas rapida")]
    public float velocidadFacil = 0.8f,  velocidadNormal = 1f, velocidadDificil = 1.2f;
    [Tooltip("Multiplica angulo y alcance de vision. >1 = ve mas")]
    public float visionFacil = 0.8f,     visionNormal = 1f,    visionDificil = 1.2f;
    [Tooltip("Multiplica el tiempo para detectarte. <1 = te detecta antes")]
    public float deteccionFacil = 1.4f,  deteccionNormal = 1f, deteccionDificil = 0.7f;
    [Tooltip("Multiplica el tiempo que tarda en perderte. >1 = te recuerda mas")]
    public float memoriaFacil = 0.7f,    memoriaNormal = 1f,   memoriaDificil = 1.3f;

    NavMeshAgent agente;
    Animator anim;
    Movimiento jugadorScript;

    int hashSpeed, hashChasing, hashAtaque;
    bool tieneSpeed, tieneChasing, tieneAtaque;

    int indice;
    bool persiguiendo, atacando;
    float tiempoVisible, tiempoOculto, proximoDestino;
    Vector3 ultimaPosVista;
    float aceleracionPatrulla, giroPatrulla;
    float cosMedioAngulo, cosMedioAnguloPersecucion;

    // Valores base (los del Inspector) sobre los que se aplica la dificultad
    float baseVelPatrulla, baseVelPersecucion, baseVelMax;
    float baseAngulo, baseAlcance, baseAnguloPers, baseAlcancePers;
    float baseTiempoDetectar, baseTiempoPerder, baseTiempoPerderCerca;

    void Start()
    {
        // 1) Guardar los valores base del Inspector
        baseVelPatrulla = velocidadPatrulla;
        baseVelPersecucion = velocidadPersecucion;
        baseVelMax = velocidadMaximaPersecucion;
        baseAngulo = angulo;
        baseAlcance = alcance;
        baseAnguloPers = anguloPersecucion;
        baseAlcancePers = alcancePersecucion;
        baseTiempoDetectar = tiempoParaDetectar;
        baseTiempoPerder = tiempoParaPerder;
        baseTiempoPerderCerca = tiempoParaPerderCerca;

        // 2) Escalar segun la dificultad guardada (ANTES de usar los valores)
        AplicarDificultad();

        agente = GetComponent<NavMeshAgent>();
        agente.speed = velocidadPatrulla;
        aceleracionPatrulla = agente.acceleration;
        giroPatrulla = agente.angularSpeed;

        if (jugador != null) jugadorScript = jugador.GetComponent<Movimiento>();
        if (vistaCono != null)
        {
            vistaCono.alturaDeteccion = alturaOjos;   // mismo alto de rayo que la deteccion
            vistaCono.Configurar(angulo, alcance, capaObstaculos);
        }

        if (audioSerpiente == null) audioSerpiente = GetComponent<SerpienteAudio>();

        anim = GetComponentInChildren<Animator>();
        hashSpeed = Animator.StringToHash(paramVelocidad);
        hashChasing = Animator.StringToHash(paramPersiguiendo);
        hashAtaque = Animator.StringToHash(paramAtaque);
        tieneSpeed = TieneParametro(paramVelocidad, AnimatorControllerParameterType.Float);
        tieneChasing = TieneParametro(paramPersiguiendo, AnimatorControllerParameterType.Bool);
        tieneAtaque = TieneParametro(paramAtaque, AnimatorControllerParameterType.Bool);

        RecalcularCosenos();

        if (puntos.Length > 0) agente.SetDestination(puntos[0].position);
    }

    // ----------------------------------------------------------------
    // DIFICULTAD
    // ----------------------------------------------------------------
    struct Mods
    {
        public float velocidad, vision, deteccion, memoria;
    }

    Mods ObtenerMods()
    {
        switch (Dificultad.Actual)
        {
            case NivelDificultad.Facil:
                return new Mods { velocidad = velocidadFacil, vision = visionFacil, deteccion = deteccionFacil, memoria = memoriaFacil };
            case NivelDificultad.Dificil:
                return new Mods { velocidad = velocidadDificil, vision = visionDificil, deteccion = deteccionDificil, memoria = memoriaDificil };
            default:
                return new Mods { velocidad = velocidadNormal, vision = visionNormal, deteccion = deteccionNormal, memoria = memoriaNormal };
        }
    }

    void AplicarDificultad()
    {
        var m = ObtenerMods();

        velocidadPatrulla = baseVelPatrulla * m.velocidad;
        velocidadPersecucion = baseVelPersecucion * m.velocidad;
        velocidadMaximaPersecucion = baseVelMax * m.velocidad;

        angulo = baseAngulo * m.vision;
        alcance = baseAlcance * m.vision;
        anguloPersecucion = baseAnguloPers * m.vision;
        alcancePersecucion = baseAlcancePers * m.vision;

        tiempoParaDetectar = baseTiempoDetectar * m.deteccion;
        tiempoParaPerder = baseTiempoPerder * m.memoria;
        tiempoParaPerderCerca = baseTiempoPerderCerca * m.memoria;

        Debug.Log($"[{name}] Dificultad {Dificultad.Actual}: velMax={velocidadMaximaPersecucion}, alcance={alcance}, detectar={tiempoParaDetectar}");
    }

    void RecalcularCosenos()
    {
        cosMedioAngulo = Mathf.Cos(angulo * 0.5f * Mathf.Deg2Rad);
        cosMedioAnguloPersecucion = Mathf.Cos(anguloPersecucion * 0.5f * Mathf.Deg2Rad);
    }

    // ----------------------------------------------------------------
    // LOGICA ORIGINAL
    // ----------------------------------------------------------------
    void Update()
    {
        if (atacando) { ActualizarAnimator(); return; }

        if (jugador == null)
        {
            Patrullar();
            ActualizarAnimator();
            return;
        }

        bool ve = EstaEnCono();

        if (ve)
        {
            ultimaPosVista = jugador.position;
            tiempoVisible += Time.deltaTime;
            tiempoOculto = 0f;
            if (!persiguiendo && tiempoVisible >= tiempoParaDetectar)
                ActivarPersecucion();
        }
        else
        {
            tiempoVisible = 0f;
            if (persiguiendo)
            {
                Vector3 dd = jugador.position - transform.position;
                dd.y = 0;
                float r = radioOlfato * 2f;
                bool cerca = dd.sqrMagnitude <= r * r;
                float limite = cerca ? tiempoParaPerderCerca : tiempoParaPerder;

                tiempoOculto += Time.deltaTime;
                if (tiempoOculto >= limite) DesactivarPersecucion();
            }
        }

        if (persiguiendo) Perseguir(ve);
        else Patrullar();

        ActualizarAnimator();
    }

    void Perseguir(bool ve)
    {
        agente.speed = Mathf.MoveTowards(agente.speed, velocidadMaximaPersecucion,
                                         rampaPersecucion * Time.deltaTime);

        Vector3 d = jugador.position - transform.position;
        d.y = 0;
        float sqr = d.sqrMagnitude;

        if (Time.time >= proximoDestino)
        {
            proximoDestino = Time.time + intervaloDestino;

            Vector3 destino = ve ? jugador.position : ultimaPosVista;
            if (!ve && sqr <= radioOlfato * radioOlfato)
                destino = jugador.position;   // lo huele: sigue su posicion real

            agente.SetDestination(destino);
        }

        bool puedeCapturar = jugadorScript == null || !jugadorScript.EstaEscondido;
        if (puedeCapturar && sqr <= distanciaCaptura * distanciaCaptura)
            StartCoroutine(Atacar());
    }

    IEnumerator Atacar()
    {
        atacando = true;
        agente.isStopped = true;
        agente.velocity = Vector3.zero;

        Vector3 dir = jugador.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);

        if (anim != null && tieneAtaque) anim.SetBool(hashAtaque, true);

        if (audioSerpiente != null) audioSerpiente.ReproducirAtaque();

        float t = 0f;
        while (anim != null && !anim.GetCurrentAnimatorStateInfo(0).IsName(estadoAtaque) && t < 0.5f)
        {
            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;
        float tope = Mathf.Max(0.5f, duracionAtaque * 2f);
        while (anim != null && t < tope)
        {
            var info = anim.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(estadoAtaque) && info.normalizedTime >= puntoGolpe) break;
            t += Time.deltaTime;
            yield return null;
        }

        jugadorScript?.ReiniciarEnCheckpoint();
        TerminarAtaque();
    }

    void TerminarAtaque()
    {
        if (anim != null && tieneAtaque) anim.SetBool(hashAtaque, false);

        DesactivarPersecucion();
        tiempoVisible = tiempoOculto = 0f;
        atacando = false;

        if (agente != null && agente.isOnNavMesh)
        {
            agente.isStopped = false;
            if (puntos.Length > 0) agente.SetDestination(puntos[indice].position);
        }
    }

    void OnDisable()
    {
        if (audioSerpiente != null) audioSerpiente.Detener();

        if (atacando)
        {
            atacando = false;
            if (anim != null && tieneAtaque) anim.SetBool(hashAtaque, false);
            persiguiendo = false;
        }
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

    // Origen y frente de la vision: los mismos que usa el dibujo del cono.
    // Si no hay cono activo, usa el propio transform de la serpiente.
    void ObtenerOjos(out Vector3 origen, out Vector3 frente)
    {
        if (vistaCono != null && vistaCono.isActiveAndEnabled)
        {
            origen = vistaCono.OrigenMundo;
            frente = vistaCono.FrenteMundo;
            return;
        }

        origen = transform.position;
        frente = transform.forward;
        frente.y = 0f;
        frente = frente.sqrMagnitude > 0.0001f ? frente.normalized : Vector3.forward;
    }

    bool EstaEnCono()
    {
        if (jugadorScript != null && jugadorScript.EstaEscondido) return false;

        ObtenerOjos(out Vector3 origenCono, out Vector3 frente);

        Vector3 d = jugador.position - origenCono;
        if (Mathf.Abs(d.y) > alturaSaltoTolerancia) return false;
        d.y = 0;
        float sqr = d.sqrMagnitude;

        float alc = persiguiendo ? alcancePersecucion : alcance;
        float cosA = persiguiendo ? cosMedioAnguloPersecucion : cosMedioAngulo;

        bool olfato = persiguiendo && sqr <= radioOlfato * radioOlfato;

        if (!olfato)
        {
            if (sqr > alc * alc) return false;
            if (sqr > 0.0001f && Vector3.Dot(frente, d) < cosA * Mathf.Sqrt(sqr))
                return false;
        }

        // Raycast al centro del cuerpo (no a los pies), asi el salto no lo esconde.
        // Sale del mismo punto y a la misma altura que los rayos del cono dibujado.
        Vector3 origen = origenCono + Vector3.up * alturaOjos;
        Vector3 objetivo = jugador.position + Vector3.up * alturaOjos;
        Vector3 hacia = objetivo - origen;
        return !Physics.Raycast(origen, hacia.normalized, hacia.magnitude,
                                capaObstaculos, QueryTriggerInteraction.Ignore);
    }

    public void IniciarPersecucion()
    {
        if (jugador == null || persiguiendo || atacando) return;
        ActivarPersecucion();
        ultimaPosVista = jugador.position;
        tiempoOculto = 0f;
    }

    void ActivarPersecucion()
    {
        persiguiendo = true;
        agente.speed = velocidadPersecucion;
        agente.acceleration = aceleracionPersecucion;
        agente.angularSpeed = giroPersecucion;

        if (audioSerpiente != null) audioSerpiente.ReproducirAlerta();

        if (cambiarConoVisualAlPerseguir && vistaCono != null)
            vistaCono.Configurar(anguloPersecucion, alcancePersecucion, capaObstaculos);
    }

    void DesactivarPersecucion()
    {
        persiguiendo = false;
        if (agente == null) return;
        agente.speed = velocidadPatrulla;
        agente.acceleration = aceleracionPatrulla;
        agente.angularSpeed = giroPatrulla;

        if (cambiarConoVisualAlPerseguir && vistaCono != null)
            vistaCono.Configurar(angulo, alcance, capaObstaculos);
    }

    void ActualizarAnimator()
    {
        if (anim != null)
        {
            if (tieneSpeed)
                anim.SetFloat(hashSpeed, agente.velocity.magnitude / velocidadMaximaPersecucion, 0.1f, Time.deltaTime);
            if (tieneChasing) anim.SetBool(hashChasing, persiguiendo);
        }

        if (audioSerpiente != null)
            audioSerpiente.ActualizarMovimiento(agente.velocity.magnitude, velocidadMaximaPersecucion, atacando);
    }

    bool TieneParametro(string nombre, AnimatorControllerParameterType tipo)
    {
        if (anim == null || string.IsNullOrEmpty(nombre)) return false;
        foreach (var p in anim.parameters)
            if (p.name == nombre && p.type == tipo) return true;
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 izq = Quaternion.Euler(0, -angulo * 0.5f, 0) * transform.forward * alcance;
        Vector3 der = Quaternion.Euler(0, angulo * 0.5f, 0) * transform.forward * alcance;
        Gizmos.DrawLine(transform.position, transform.position + izq);
        Gizmos.DrawLine(transform.position, transform.position + der);
        Gizmos.DrawWireSphere(transform.position, alcance);

        Gizmos.color = new Color(1f, 0.4f, 0f);
        Gizmos.DrawWireSphere(transform.position, radioOlfato);
    }
}