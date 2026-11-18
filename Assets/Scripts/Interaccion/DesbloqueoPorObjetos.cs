using UnityEngine;
using UnityEngine.Events;

public class DesbloqueoPorObjetos : MonoBehaviour
{
    [Header("Requisito")]
    public int objetosNecesarios = 2;

    [Header("Al cumplirse")]
    [Tooltip("Se desactivan (por ejemplo, la roca o barrera que bloquea el camino).")]
    public GameObject[] desactivar;
    [Tooltip("Se activan (por ejemplo, una luz, un portal o un nuevo trigger).")]
    public GameObject[] activar;
    public UnityEvent alCumplir;

    private bool cumplido;

    void OnEnable()
    {
        Recolectable.AlRecoger += Comprobar;
        // Por si ya se habían recogido antes de activarse este objeto
        Comprobar(Recolectable.TotalRecogidos);
    }

    void OnDisable()
    {
        Recolectable.AlRecoger -= Comprobar;
    }

    void Comprobar(int total)
    {
        if (cumplido || total < objetosNecesarios) return;
        cumplido = true;

        foreach (var g in desactivar) if (g != null) g.SetActive(false);
        foreach (var g in activar) if (g != null) g.SetActive(true);
        alCumplir?.Invoke();
    }
}