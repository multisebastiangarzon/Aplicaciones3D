using UnityEngine;

public enum NivelDificultad { Facil = 0, Normal = 1, Dificil = 2 }

public static class Dificultad
{
    const string Clave = "dificultad";

    public static NivelDificultad Actual
    {
        get => (NivelDificultad)Mathf.Clamp(PlayerPrefs.GetInt(Clave, (int)NivelDificultad.Normal), 0, 2);
        set { PlayerPrefs.SetInt(Clave, (int)value); PlayerPrefs.Save(); }
    }
}