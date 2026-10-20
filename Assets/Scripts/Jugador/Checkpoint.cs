using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent<Movimiento>(out var jugador))
        {
            jugador.EstablecerCheckpoint(transform.position);
            Debug.Log("Checkpoint actualizado en: " + transform.position);
        }
    }
}