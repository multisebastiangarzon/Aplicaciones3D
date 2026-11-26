using UnityEngine;
using UnityEngine.UI;

public class SelectorDificultad : MonoBehaviour
{
    [Header("Botones (Image de cada uno)")]
    [SerializeField] private Image botonFacil;
    [SerializeField] private Image botonNormal;
    [SerializeField] private Image botonDificil;

    [Header("Colores")]
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorSeleccionado = new Color(0.4f, 0.4f, 0.4f, 1f);

    private void OnEnable()
    {
        ActualizarVisual();
    }

    public void Facil()
    {
        Dificultad.Actual = NivelDificultad.Facil;
        ActualizarVisual();
    }

    public void Normal()
    {
        Dificultad.Actual = NivelDificultad.Normal;
        ActualizarVisual();
    }

    public void Dificil()
    {
        Dificultad.Actual = NivelDificultad.Dificil;
        ActualizarVisual();
    }

    private void ActualizarVisual()
    {
        botonFacil.color   = Dificultad.Actual == NivelDificultad.Facil   ? colorSeleccionado : colorNormal;
        botonNormal.color  = Dificultad.Actual == NivelDificultad.Normal  ? colorSeleccionado : colorNormal;
        botonDificil.color = Dificultad.Actual == NivelDificultad.Dificil ? colorSeleccionado : colorNormal;
    }
}