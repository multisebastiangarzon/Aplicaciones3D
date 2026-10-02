using UnityEngine;

public class CoberturaTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Debug.Log("Jugador escondido ");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            Debug.Log("Jugador salió de la cobertura");
    }
}