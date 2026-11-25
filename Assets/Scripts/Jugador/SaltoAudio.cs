using UnityEngine;

/// <summary>
/// Reproduce el sonido de salto detectando el impulso vertical del Rigidbody.
/// No depende del script de movimiento: solo necesita Rigidbody.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SaltoAudio : MonoBehaviour
{
    [Tooltip("Si se deja vacio, usa el clip del AudioSource que ya tenga el jugador.")]
    public AudioClip sonidoSalto;
    [Range(0f, 1f)] public float volumen = 0.5f;
    public Vector2 variacionTono = new Vector2(0.95f, 1.05f);

    [Header("Deteccion")]
    [Tooltip("Velocidad vertical (hacia arriba) que cuenta como salto. " +
             "Con alturaSalto 1 el impulso es ~4.4, asi que 3 funciona.")]
    public float umbralSalto = 3f;
    [Tooltip("Tiempo minimo entre sonidos de salto")]
    public float tiempoEntreSaltos = 0.25f;

    Rigidbody rb;
    AudioSource fuente;
    float vyAnterior;
    float ultimoSonido = -10f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        fuente = GetComponent<AudioSource>();
        if (fuente == null) fuente = gameObject.AddComponent<AudioSource>();

        // Si no asignas clip, usa el que ya tenga el AudioSource
        if (sonidoSalto == null) sonidoSalto = fuente.clip;

        fuente.playOnAwake = false;
        fuente.loop = false;
        fuente.spatialBlend = 0f;   // 2D: se oye igual desde cualquier camara
    }

    void FixedUpdate()
    {
        float vy = rb.linearVelocity.y;

        // Salto = la velocidad vertical pasa de baja a alta de golpe
        bool despego = vy > umbralSalto && vyAnterior <= umbralSalto;
        if (despego && Time.time - ultimoSonido >= tiempoEntreSaltos)
        {
            ultimoSonido = Time.time;
            Reproducir();
        }

        vyAnterior = vy;
    }

    void Reproducir()
    {
        if (sonidoSalto == null) return;

        fuente.pitch = Random.Range(variacionTono.x, variacionTono.y);
        fuente.PlayOneShot(sonidoSalto, volumen);
    }
}