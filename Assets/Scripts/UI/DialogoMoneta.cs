using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using TMPro;

public class DialogoMoneta : MonoBehaviour
{
    [Header("Contenido")]
    [TextArea(2, 4)] public string[] lineas;
    public TextMeshProUGUI textoUI;
    public CanvasGroup pistaContinuar;
    public GameObject botonAvanzar;
    [FormerlySerializedAs("panelOpciones")]
    public GameObject panelRespuestas;
    public GameObject raizDialogo;

    [Header("Animación de entrada")]
    public CanvasGroup oscurecedor;
    public RectTransform retrato;
    public CanvasGroup burbuja;
    public RectTransform[] botonesRespuesta;
    public float retrasoInicial = 1f;
    public float duracionRetrato = 0.7f;
    public float duracionBurbuja = 0.4f;
    public float duracionRespuestas = 0.5f;
    public float desplazamientoRetrato = 900f;
    public float desplazamientoRespuestas = 400f;
    public float escalonado = 0.15f;

    [Header("Ajustes")]
    public float velocidadTexto = 30f;
    public float velocidadPulso = 3f;
    public bool pausarJuego = false;
    public UnityEvent alTerminar;

    int indice = -1;
    bool escribiendo;
    bool mostrarPista;
    Coroutine escritura;
    Vector2[] botonesFinal;

    IEnumerator Start()
    {
        if (pausarJuego) Time.timeScale = 0f;

        // Guardar posiciones finales
        Vector2 retratoFinal = retrato != null ? retrato.anchoredPosition : Vector2.zero;
        botonesFinal = new Vector2[botonesRespuesta.Length];
        for (int i = 0; i < botonesRespuesta.Length; i++)
        {
            botonesFinal[i] = botonesRespuesta[i].anchoredPosition;
        }

        // Estado inicial: todo escondido
        panelRespuestas.SetActive(false);
        botonAvanzar.SetActive(false);
        textoUI.text = "";
        if (pistaContinuar != null) pistaContinuar.alpha = 0;
        if (oscurecedor != null) oscurecedor.alpha = 0;
        if (burbuja != null)
        {
            burbuja.alpha = 0;
            burbuja.transform.localScale = Vector3.one * 0.85f;
        }
        if (retrato != null)
        {
            retrato.anchoredPosition = retratoFinal + Vector2.down * desplazamientoRetrato;
        }

        yield return new WaitForSecondsRealtime(retrasoInicial);

        // 1. Oscurecido y Monetá
        if (oscurecedor != null) StartCoroutine(FadeGrupo(oscurecedor, duracionRetrato));
        if (retrato != null)
        {
            yield return Mover(retrato, retratoFinal, duracionRetrato, 0f);
        }

        // 2. Burbuja
        if (burbuja != null)
        {
            yield return AparecerBurbuja();
        }

        // 3. Texto
        botonAvanzar.SetActive(true);
        Avanzar();
    }

    void Update()
    {
        if (pistaContinuar == null) return;

        if (mostrarPista)
        {
            float onda = (Mathf.Sin(Time.unscaledTime * velocidadPulso) + 1f) / 2f;
            pistaContinuar.alpha = Mathf.Lerp(0.3f, 1f, onda);
        }
        else
        {
            pistaContinuar.alpha = 0f;
        }
    }

    public void Avanzar()
    {
        if (escribiendo)
        {
            StopCoroutine(escritura);
            textoUI.maxVisibleCharacters = int.MaxValue;
            FinLinea();
            return;
        }

        indice++;
        if (indice >= lineas.Length) return;
        escritura = StartCoroutine(Escribir(lineas[indice]));
    }

    public void Elegir()
    {
        if (pausarJuego) Time.timeScale = 1f;
        alTerminar.Invoke();
        raizDialogo.SetActive(false);
    }

    void FinLinea()
    {
        escribiendo = false;
        if (indice >= lineas.Length - 1)
        {
            mostrarPista = false;
            botonAvanzar.SetActive(false);
            StartCoroutine(MostrarRespuestas());
        }
        else
        {
            mostrarPista = true;
        }
    }

    IEnumerator MostrarRespuestas()
    {
        panelRespuestas.SetActive(true);

        for (int i = 0; i < botonesRespuesta.Length; i++)
        {
            botonesRespuesta[i].anchoredPosition = botonesFinal[i] + Vector2.down * desplazamientoRespuestas;
        }
        for (int i = 0; i < botonesRespuesta.Length; i++)
        {
            StartCoroutine(Mover(botonesRespuesta[i], botonesFinal[i], duracionRespuestas, i * escalonado));
        }
        yield return null;
    }

    IEnumerator Escribir(string t)
    {
        escribiendo = true;
        mostrarPista = false;
        textoUI.text = t;
        textoUI.maxVisibleCharacters = 0;
        textoUI.ForceMeshUpdate();
        int total = textoUI.textInfo.characterCount;
        float acumulado = 0f;

        while (textoUI.maxVisibleCharacters < total)
        {
            acumulado += Time.unscaledDeltaTime * velocidadTexto;
            textoUI.maxVisibleCharacters = Mathf.FloorToInt(acumulado);
            yield return null;
        }
        textoUI.maxVisibleCharacters = int.MaxValue;
        FinLinea();
    }

    IEnumerator Mover(RectTransform rt, Vector2 destino, float duracion, float espera)
    {
        if (espera > 0f) yield return new WaitForSecondsRealtime(espera);

        Vector2 origen = rt.anchoredPosition;
        float t = 0f;
        duracion = Mathf.Max(duracion, 0.01f);

        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            rt.anchoredPosition = Vector2.LerpUnclamped(origen, destino, Suave(Mathf.Clamp01(t / duracion)));
            yield return null;
        }
        rt.anchoredPosition = destino;
    }

    IEnumerator AparecerBurbuja()
    {
        float t = 0f;
        float duracion = Mathf.Max(duracionBurbuja, 0.01f);

        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            float k = Suave(Mathf.Clamp01(t / duracion));
            burbuja.alpha = k;
            burbuja.transform.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, k);
            yield return null;
        }
        burbuja.alpha = 1f;
        burbuja.transform.localScale = Vector3.one;
    }

    IEnumerator FadeGrupo(CanvasGroup cg, float duracion)
    {
        float t = 0f;
        duracion = Mathf.Max(duracion, 0.01f);
        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(t / duracion);
            yield return null;
        }
        cg.alpha = 1f;
    }

    float Suave(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}