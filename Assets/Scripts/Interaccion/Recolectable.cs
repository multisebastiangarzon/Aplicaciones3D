using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Recolectable : MonoBehaviour
{
    [Tooltip("Nombre que aparece en los mensajes de la consola.")]
    public string nombre = "Objeto sagrado";

    [Tooltip("Distancia (en metros) a la que el jugador puede recogerlo.")]
    public float radio = 2.5f;

    [Header("Audio")]
    [Tooltip("Sonido que se reproduce al recoger el objeto.")]
    public AudioClip sonidoRecoger;
    [Range(0f, 1f)] public float volumenRecoger = 0.8f;

    [Header("Eventos")]
    [Tooltip("Se dispara al recoger este objeto (mensaje en el HUD, etc.).")]
    public UnityEvent alRecogido;

    public static int TotalRecogidos { get; private set; }

    // Los demás scripts se suscriben a esto para enterarse de cada recogida.
    // Envía el total de objetos recogidos hasta ahora.
    public static event Action<int> AlRecoger;

    private Transform jugador;
    private bool jugadorCerca;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ReiniciarContador()
    {
        TotalRecogidos = 0;
        AlRecoger = null;
    }

    void Update()
    {
        // No se puede recoger nada mientras el acertijo está abierto
        if (AcertijoCultural.Activo) return;

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

        if (jugadorCerca && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Recoger();
        }
    }

    void Recoger()
    {
        TotalRecogidos++;
        Debug.Log("Objeto recogido: " + nombre + " (total: " + TotalRecogidos + ")");

        // PlayClipAtPoint crea una fuente temporal: el sonido termina aunque
        // el objeto se desactive justo después.
        if (sonidoRecoger != null)
            AudioSource.PlayClipAtPoint(sonidoRecoger, transform.position, volumenRecoger);

        alRecogido?.Invoke();
        AlRecoger?.Invoke(TotalRecogidos);

        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}