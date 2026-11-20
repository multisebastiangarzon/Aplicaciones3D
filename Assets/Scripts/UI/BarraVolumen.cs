using UnityEngine;
using UnityEngine.UI;

public class BarraVolumen : MonoBehaviour
{
    public enum Tipo { General, Musica }

    [Header("Qué controla")]
    public Tipo tipo = Tipo.General;

    [Header("UI")]
    [Tooltip("Image Type = Filled, Fill Method = Horizontal.")]
    public Image relleno;
    public Button botonSubir;
    public Button botonBajar;

    [Header("Niveles")]
    [Min(1)] public int niveles = 6;
    public int nivelInicial = 3;

    [Header("Solo si tipo = Musica")]
    public AudioSource musica;

    string Clave => "Nivel_" + tipo;
    int nivel;

    void Awake()
    {
        nivel = Mathf.Clamp(PlayerPrefs.GetInt(Clave, nivelInicial), 0, niveles);
        botonSubir.onClick.AddListener(() => Cambiar(+1));
        botonBajar.onClick.AddListener(() => Cambiar(-1));
        Aplicar();
    }

    void Cambiar(int delta)
    {
        nivel = Mathf.Clamp(nivel + delta, 0, niveles);
        PlayerPrefs.SetInt(Clave, nivel);
        Aplicar();
    }

    void Aplicar()
    {
        float v = (float)nivel / niveles;

        if (relleno != null) relleno.fillAmount = v;

        if (tipo == Tipo.General) AudioListener.volume = v;
        else if (musica != null) musica.volume = v;
    }
}