using UnityEngine;
using UnityEngine.UI;

public class BarraNiveles : MonoBehaviour
{
    public Image imagenBarra;
    public Sprite[] niveles;
    public int nivelActual = 3;

    void Start()
    {
        Actualizar();
    }

    public void Subir()
    {
        if (nivelActual < niveles.Length - 1) nivelActual++;
        Actualizar();
    }

    public void Bajar()
    {
        if (nivelActual > 0) nivelActual--;
        Actualizar();
    }

    void Actualizar()
    {
        imagenBarra.sprite = niveles[nivelActual];
    }
}