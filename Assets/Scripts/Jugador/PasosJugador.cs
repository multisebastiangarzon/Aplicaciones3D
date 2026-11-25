using UnityEngine;

[RequireComponent(typeof(Movimiento), typeof(Rigidbody))]
public class PasosJugador : MonoBehaviour
{
    public AudioClip[] pasos;
    [Range(0f, 1f)] public float volumen = 0.5f;
    public float intervaloCaminar = 0.5f;
    public float intervaloCorrer = 0.32f;
    [Tooltip("Velocidad desde la cual se considera que corre")]
    public float umbralCorrer = 6.5f;

    Movimiento mov;
    Rigidbody rb;
    AudioSource fuente;
    float proximo;

    void Awake()
    {
        mov = GetComponent<Movimiento>();
        rb = GetComponent<Rigidbody>();
        fuente = gameObject.AddComponent<AudioSource>();
        fuente.playOnAwake = false;
    }

    void Update()
    {
        if (pasos.Length == 0) return;

        Vector3 v = rb.linearVelocity;
        v.y = 0f;
        float vel = v.magnitude;

        if (!mov.EstaEnSuelo || mov.EstaAgachado || vel < 0.5f) { proximo = 0f; return; }
        if (Time.time < proximo) return;

        proximo = Time.time + (vel > umbralCorrer ? intervaloCorrer : intervaloCaminar);
        fuente.pitch = Random.Range(0.9f, 1.1f);
        fuente.PlayOneShot(pasos[Random.Range(0, pasos.Length)], volumen);
    }
}