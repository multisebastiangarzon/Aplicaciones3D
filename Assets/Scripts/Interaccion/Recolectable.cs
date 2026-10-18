using UnityEngine;
using UnityEngine.InputSystem;

public class Recolectable : MonoBehaviour
{
    [Tooltip("Nombre que aparece en los mensajes de la consola.")]
    public string nombre = "Objeto sagrado";

    [Tooltip("Distancia (en metros) a la que el jugador puede recogerlo.")]
    public float radio = 2.5f;

    public static int TotalRecogidos { get; private set; }

    private Transform jugador;
    private bool jugadorCerca;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ReiniciarContador()
    {
        TotalRecogidos = 0;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            jugador = p.transform;
        else
            Debug.LogWarning("Recolectable '" + nombre + "': no se encontró ningún objeto con el tag Player.");
    }

    void Update()
    {
        if (jugador == null) return;

        Vector3 d = jugador.position - transform.position;
        d.y = 0f;
        bool ahoraCerca = d.magnitude <= radio;

        if (ahoraCerca != jugadorCerca)
        {
            jugadorCerca = ahoraCerca;
            if (jugadorCerca) Debug.Log("Al alcance: " + nombre + " presiona E para recoger");
            else Debug.Log("Fuera del alcance: " + nombre);
        }

        if (jugadorCerca && Keyboard.current != null &&
            (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
        {
            Recoger();
        }
    }

    void Recoger()
    {
        TotalRecogidos++;
        Debug.Log("Objeto recogido: " + nombre + " (" + TotalRecogidos + "/2)");

        if (TotalRecogidos >= 2)
            Debug.Log("Ambos objetos sagrados recogidos: ya se puede hacer el ritual en el altar.");

        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}