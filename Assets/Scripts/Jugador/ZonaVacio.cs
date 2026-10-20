using UnityEngine;

public class ZonaVacio : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent<Movimiento>(out var jugador))
        {
            jugador.ReiniciarEnCheckpoint();
        }
    }
}