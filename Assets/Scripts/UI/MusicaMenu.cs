using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class AjustesAudio : MonoBehaviour
{
    [SerializeField] private AudioClip musica;

    [SerializeField] private Button botonSubirVolumen;
    [SerializeField] private Button botonBajarVolumen;
    [SerializeField] private Button botonSubirMusica;
    [SerializeField] private Button botonBajarMusica;
    [SerializeField] private Button botonSubirGraficos;
    [SerializeField] private Button botonBajarGraficos;

    [SerializeField] private Image barraVolumen;
    [SerializeField] private Image barraMusica;
    [SerializeField] private Image barraGraficos;

    [SerializeField] private int pasosAudio = 6;

    private AudioSource audioSource;
    private int nivelVolumen;
    private int nivelMusica;
    private int nivelGraficos;

    private const string KEY_VOLUMEN = "NivelVolumen";
    private const string KEY_MUSICA = "NivelMusica";
    private const string KEY_GRAFICOS = "NivelGraficos";

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (musica != null) audioSource.clip = musica;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        nivelVolumen = PlayerPrefs.GetInt(KEY_VOLUMEN, pasosAudio);
        nivelMusica = PlayerPrefs.GetInt(KEY_MUSICA, pasosAudio / 2);
        nivelGraficos = PlayerPrefs.GetInt(KEY_GRAFICOS, QualitySettings.GetQualityLevel());

        botonSubirVolumen.onClick.AddListener(() => CambiarVolumen(1));
        botonBajarVolumen.onClick.AddListener(() => CambiarVolumen(-1));
        botonSubirMusica.onClick.AddListener(() => CambiarMusica(1));
        botonBajarMusica.onClick.AddListener(() => CambiarMusica(-1));
        botonSubirGraficos.onClick.AddListener(() => CambiarGraficos(1));
        botonBajarGraficos.onClick.AddListener(() => CambiarGraficos(-1));

        AplicarTodo();
        audioSource.Play();
    }

    void CambiarVolumen(int delta)
    {
        nivelVolumen = Mathf.Clamp(nivelVolumen + delta, 0, pasosAudio);
        AplicarVolumen();
    }

    void CambiarMusica(int delta)
    {
        nivelMusica = Mathf.Clamp(nivelMusica + delta, 0, pasosAudio);
        AplicarMusica();
    }

    void CambiarGraficos(int delta)
    {
        nivelGraficos = Mathf.Clamp(nivelGraficos + delta, 0, QualitySettings.names.Length - 1);
        AplicarGraficos();
    }

    void AplicarTodo()
    {
        AplicarVolumen();
        AplicarMusica();
        AplicarGraficos();
    }

    void AplicarVolumen()
    {
        float v = (float)nivelVolumen / pasosAudio;
        AudioListener.volume = v;
        if (barraVolumen != null) barraVolumen.fillAmount = v;
        PlayerPrefs.SetInt(KEY_VOLUMEN, nivelVolumen);
    }

    void AplicarMusica()
    {
        float v = (float)nivelMusica / pasosAudio;
        audioSource.volume = v;
        if (barraMusica != null) barraMusica.fillAmount = v;
        PlayerPrefs.SetInt(KEY_MUSICA, nivelMusica);
    }

    void AplicarGraficos()
    {
        QualitySettings.SetQualityLevel(nivelGraficos, true);
        if (barraGraficos != null) barraGraficos.fillAmount = (nivelGraficos + 1f) / QualitySettings.names.Length;
        PlayerPrefs.SetInt(KEY_GRAFICOS, nivelGraficos);
    }

    void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}