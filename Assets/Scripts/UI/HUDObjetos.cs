using UnityEngine;

public class HUDObjetos : MonoBehaviour
{
    [Tooltip("Orden: el 1.º objeto recogido enciende el slot 0, el 2.º el slot 1...")]
    public SlotObjetoHUD[] slots;

    void OnEnable()
    {
        Recolectable.AlRecoger += Refrescar;
        // Si ya había objetos recogidos, se muestran sin animación
        Sincronizar(Recolectable.TotalRecogidos, true);
    }

    void OnDisable()
    {
        Recolectable.AlRecoger -= Refrescar;
    }

    void Refrescar(int total)
    {
        Sincronizar(total, false);
    }

    void Sincronizar(int total, bool instantaneo)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;

            if (i < total) slots[i].Obtener(instantaneo);
            else if (slots[i].Obtenido) slots[i].Vaciar();
        }
    }
}