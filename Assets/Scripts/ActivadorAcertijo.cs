using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ActivadorAcertijo : MonoBehaviour
{
    [Header("Acertijo a abrir")]
    public AcertijoCultural acertijo;

    [Header("Opciones")]
    [Tooltip("Si está marcado, la zona se desactiva tras usarse una vez.")]
    public bool soloUnaVez = true;

    private bool usado;

    void Reset()
    {
       
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (usado || acertijo == null) return;
        if (!other.CompareTag("Player")) return;

        acertijo.Abrir();
        if (soloUnaVez) usado = true;
    }
}