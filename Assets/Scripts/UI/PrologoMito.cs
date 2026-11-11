using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PrologoMito : MonoBehaviour
{
    [System.Serializable]
    public class Momento
    {
        public CanvasGroup carta;
        [TextArea(2, 4)] public string texto;
        public AudioClip voz;
    }

    public Momento[] momentos;
    public TextMeshProUGUI textoUI;
    public CanvasGroup pistaContinuar;
    public AudioSource audioSource;
    public FundidoEscena fundido;
    public float retrasoInicial = 1.5f;
    public float velocidadTexto = 30f;
    public float duracionFade = 0.8f;
    public float velocidadPulso = 3f;
    public string escenaSiguiente;

    int indice = -1;
    bool iniciado;
    bool escribiendo;
    bool mostrarPista;
    bool saliendo;
    Coroutine escritura;

    IEnumerator Start()
    {
        foreach (Momento m in momentos)
        {
            if (m.carta != null) m.carta.alpha = 0;
        }
        textoUI.text = "";
        if (pistaContinuar != null) pistaContinuar.alpha = 0;

        yield return new WaitForSeconds(retrasoInicial);
        iniciado = true;
        Avanzar();
    }

    void Update()
    {
        if (pistaContinuar == null) return;

        if (mostrarPista)
        {
            float onda = (Mathf.Sin(Time.time * velocidadPulso) + 1f) / 2f;
            pistaContinuar.alpha = Mathf.Lerp(0.3f, 1f, onda);
        }
        else
        {
            pistaContinuar.alpha = 0f;
        }
    }

    public void Avanzar()
    {
        if (!iniciado || saliendo) return;

        if (escribiendo)
        {
            StopCoroutine(escritura);
            textoUI.maxVisibleCharacters = int.MaxValue;
            escribiendo = false;
            mostrarPista = true;
            return;
        }

        indice++;
        if (indice >= momentos.Length)
        {
            Saltar();
            return;
        }

        Momento m = momentos[indice];
        if (m.carta != null) StartCoroutine(FadeIn(m.carta));
        if (m.voz != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = m.voz;
            audioSource.Play();
        }
        escritura = StartCoroutine(Escribir(m.texto));
    }

    public void Saltar()
    {
        if (saliendo) return;
        saliendo = true;
        mostrarPista = false;

        if (fundido != null) fundido.CargarEscena(escenaSiguiente);
        else SceneManager.LoadScene(escenaSiguiente);
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
            acumulado += Time.deltaTime * velocidadTexto;
            textoUI.maxVisibleCharacters = Mathf.FloorToInt(acumulado);
            yield return null;
        }
        textoUI.maxVisibleCharacters = int.MaxValue;
        escribiendo = false;
        mostrarPista = true;
    }

    IEnumerator FadeIn(CanvasGroup cg)
    {
        float t = 0f;
        while (t < duracionFade)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t / duracionFade);
            yield return null;
        }
        cg.alpha = 1f;
    }
}