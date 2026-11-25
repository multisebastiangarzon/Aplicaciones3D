using UnityEngine;

/// <summary>
/// Maneja los sonidos de la serpiente: caminar (loop), alerta (siseo) y ataque (opcional).
/// Si no asignas los AudioSource, los crea automaticamente.
/// </summary>
public class SerpienteAudio : MonoBehaviour
{
    [Header("Fuentes (opcional, se crean solas si estan vacias)")]
    public AudioSource audioMovimiento;
    public AudioSource audioEfectos;

    [Header("Clips")]
    public AudioClip sonidoCaminar;
    public AudioClip sonidoAlerta;
    [Tooltip("Opcional: sonido al morder")]
    public AudioClip sonidoAtaque;

    [Header("Volumen")]
    [Range(0f, 1f)] public float volumenCaminar = 0.5f;
    [Range(0f, 1f)] public float volumenAlerta = 0.9f;
    [Range(0f, 1f)] public float volumenAtaque = 1f;

    [Header("Movimiento")]
    public float velocidadMinimaSonido = 0.2f;
    public float pitchLento = 0.9f;
    public float pitchRapido = 1.4f;

    [Header("Sonido 3D")]
    public float distanciaMinima = 2f;
    public float distanciaMaxima = 18f;

    void Awake()
    {
        if (audioMovimiento == null) audioMovimiento = gameObject.AddComponent<AudioSource>();
        if (audioEfectos == null) audioEfectos = gameObject.AddComponent<AudioSource>();

        ConfigurarFuente(audioMovimiento, true);
        ConfigurarFuente(audioEfectos, false);

        audioMovimiento.clip = sonidoCaminar;
    }

    void ConfigurarFuente(AudioSource fuente, bool loop)
    {
        fuente.playOnAwake = false;
        fuente.loop = loop;
        fuente.spatialBlend = 1f;               // 3D
        fuente.rolloffMode = AudioRolloffMode.Linear;
        fuente.minDistance = distanciaMinima;
        fuente.maxDistance = distanciaMaxima;
    }

    /// <summary>Llamar cada frame. Controla el loop de caminar segun la velocidad.</summary>
    public void ActualizarMovimiento(float velocidad, float velocidadMaxima, bool silenciar)
    {
        if (sonidoCaminar == null) return;

        bool moviendo = velocidad > velocidadMinimaSonido && !silenciar;

        if (moviendo)
        {
            if (audioMovimiento.clip != sonidoCaminar) audioMovimiento.clip = sonidoCaminar;
            if (!audioMovimiento.isPlaying) audioMovimiento.Play();

            // Mas rapido = mas fuerte y mas agudo
            float t = Mathf.Clamp01(velocidad / Mathf.Max(0.01f, velocidadMaxima));
            audioMovimiento.volume = Mathf.Lerp(0.3f, 1f, t) * volumenCaminar;
            audioMovimiento.pitch = Mathf.Lerp(pitchLento, pitchRapido, t);
        }
        else if (audioMovimiento.isPlaying)
        {
            audioMovimiento.Stop();
        }
    }

    /// <summary>Siseo cuando detecta al jugador.</summary>
    public void ReproducirAlerta()
    {
        if (sonidoAlerta == null) return;
        audioEfectos.pitch = 1f;
        audioEfectos.PlayOneShot(sonidoAlerta, volumenAlerta);
    }

    /// <summary>Sonido del mordisco (opcional).</summary>
    public void ReproducirAtaque()
    {
        if (sonidoAtaque == null) return;
        audioEfectos.pitch = 1f;
        audioEfectos.PlayOneShot(sonidoAtaque, volumenAtaque);
    }

    public void Detener()
    {
        if (audioMovimiento != null) audioMovimiento.Stop();
        if (audioEfectos != null) audioEfectos.Stop();
    }
}