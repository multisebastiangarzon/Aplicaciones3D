using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SonidoAcertijo : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip sonidoCorrecto;
    public AudioClip sonidoIncorrecto;

    [Header("Volumen de cada sonido")]
    [Range(0f, 1f)] public float volumenCorrecto = 0.6f;
    [Range(0f, 1f)] public float volumenIncorrecto = 0.3f;

    AudioSource fuente;

    void Awake()
    {
        fuente = GetComponent<AudioSource>();
        fuente.playOnAwake = false;
        fuente.loop = false;
        fuente.spatialBlend = 0f;
    }

    public void Correcto()   { Reproducir(sonidoCorrecto, volumenCorrecto); }
    public void Incorrecto() { Reproducir(sonidoIncorrecto, volumenIncorrecto); }

    void Reproducir(AudioClip clip, float volumen)
    {
        if (clip != null) fuente.PlayOneShot(clip, volumen);
    }
}