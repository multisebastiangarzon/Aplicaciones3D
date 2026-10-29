using UnityEngine;

public class PuertaMapa : MonoBehaviour
{
    [SerializeField] GameObject mapaDestino;
    [SerializeField] string spawnDestino;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GestorMapas.Instancia.CambiarMapa(mapaDestino, spawnDestino);
    }
}