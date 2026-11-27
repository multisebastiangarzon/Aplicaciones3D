using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public enum TipoObjeto { Otro, Disco, Esmeralda }

public class Recolectable : MonoBehaviour
{
    [Tooltip("Nombre que aparece en los mensajes de la consola.")]
    public string nombre = "Objeto sagrado";

    [Tooltip("Qué objeto es. Sirve para diferenciarlos (íconos, objetivo siguiente).")]
    public TipoObjeto tipo = TipoObjeto.Otro;

    [Tooltip("Distancia (en metros) a la que el jugador puede recogerlo.")]
    public float radio = 2.5f;

    [Tooltip("Aviso en pantalla mientras el jugador está cerca. Vacío = no muestra nada.")]
    public string mensajeAviso = "[E] Recoger";

    [Header("Audio")]
    [Tooltip("Sonido que se reproduce al recoger el objeto.")]
    public AudioClip sonidoRecoger;
    [Range(0f, 1f)] public float volumenRecoger = 0.8f;

    [Header("Eventos")]
    [Tooltip("Se dispara al recoger este objeto (mensaje en el HUD, etc.).")]
    public UnityEvent alRecogido;

    public static int TotalRecogidos { get; private set; }
    public static bool DiscoRecogido { get; private set; }
    public static bool EsmeraldaRecogida { get; private set; }

    // Envía el total de objetos recogidos hasta ahora.
    public static event Action<int> AlRecoger;
    // Envía el tipo del objeto recién recogido.
    public static event Action<TipoObjeto> AlRecogerTipo;

    private Transform jugador;
    private bool jugadorCerca;
    private bool mostrandoAviso;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ReiniciarContador()
    {
        TotalRecogidos = 0;
        DiscoRecogido = false;
        EsmeraldaRecogida = false;
        AlRecoger = null;
        AlRecogerTipo = null;
    }

    // Para empezar una partida nueva (no toca los suscriptores).
    public static void Reiniciar()
    {
        TotalRecogidos = 0;
        DiscoRecogido = false;
        EsmeraldaRecogida = false;
    }

    public static bool Tiene(TipoObjeto t)
    {
        switch (t)
        {
            case TipoObjeto.Disco: return DiscoRecogido;
            case TipoObjeto.Esmeralda: return EsmeraldaRecogida;
            default: return false;
        }
    }

    void OnDisable()
    {
        OcultarAviso();
    }

    void Update()
    {
        // Sin recoger nada con el juego en pausa, en diálogo o con el acertijo abierto
        if (Time.timeScale == 0f || AcertijoCultural.Activo || DialogoInteractuable.Hablando)
        {
            OcultarAviso();
            return;
        }

        if (jugador == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) return;
            jugador = p.transform;
        }

        Vector3 d = jugador.position - transform.position;
        d.y = 0f;
        bool ahoraCerca = d.magnitude <= radio;

        if (ahoraCerca != jugadorCerca)
        {
            jugadorCerca = ahoraCerca;
            if (jugadorCerca) Debug.Log("Al alcance: " + nombre + " (presiona E para recoger)");
            else Debug.Log("Fuera del alcance: " + nombre);
        }

        if (jugadorCerca) MostrarAviso();
        else OcultarAviso();

        if (jugadorCerca && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Recoger();
        }
    }

    void Recoger()
    {
        TotalRecogidos++;
        if (tipo == TipoObjeto.Disco) DiscoRecogido = true;
        else if (tipo == TipoObjeto.Esmeralda) EsmeraldaRecogida = true;

        Debug.Log("Objeto recogido: " + nombre + " (total: " + TotalRecogidos + ")");
        OcultarAviso();

        if (sonidoRecoger != null)
            AudioSource.PlayClipAtPoint(sonidoRecoger, transform.position, volumenRecoger);

        alRecogido?.Invoke();
        AlRecoger?.Invoke(TotalRecogidos);
        AlRecogerTipo?.Invoke(tipo);

        gameObject.SetActive(false);
    }

    void MostrarAviso()
    {
        if (mostrandoAviso || string.IsNullOrEmpty(mensajeAviso)) return;
        if (GestorAvisos.Instancia == null) return;
        GestorAvisos.Instancia.MostrarAviso(this, mensajeAviso);
        mostrandoAviso = true;
    }

    void OcultarAviso()
    {
        if (!mostrandoAviso) return;
        if (GestorAvisos.Instancia != null) GestorAvisos.Instancia.OcultarAviso(this);
        mostrandoAviso = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}