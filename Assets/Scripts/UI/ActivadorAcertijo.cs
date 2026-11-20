using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ActivadorAcertijo : MonoBehaviour
{
    [Header("Acertijo a abrir")]
    public AcertijoCultural acertijo;

    [Header("Opciones")]
    [Tooltip("Si está marcado, la zona se desactiva cuando el acertijo se resuelve.")]
    public bool soloUnaVez = true;

    private bool resuelto;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    // Llámalo desde AcertijoCultural al acertar
    public void MarcarResuelto()
    {
        if (soloUnaVez) resuelto = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (resuelto || acertijo == null) return;
        if (!other.CompareTag("Player")) return;

        acertijo.Abrir();
    }
}