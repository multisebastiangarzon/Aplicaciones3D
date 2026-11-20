using UnityEngine;

public class SelectorDificultad : MonoBehaviour
{
    public void Facil()   { Dificultad.Actual = NivelDificultad.Facil; }
    public void Normal()  { Dificultad.Actual = NivelDificultad.Normal; }
    public void Dificil() { Dificultad.Actual = NivelDificultad.Dificil; }
}