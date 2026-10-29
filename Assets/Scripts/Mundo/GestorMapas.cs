
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GestorMapas : MonoBehaviour
{
    public static GestorMapas Instancia;

    [Header("Referencias")]
    [SerializeField] CanvasGroup panel;
    [SerializeField] Slider barra;
    [SerializeField] Transform jugador;
    [SerializeField] GameObject mapaA;
    [SerializeField] GameObject mapaB;

    [Header("Ajustes")]
    [SerializeField] float duracionFade = 0.5f;
    [SerializeField] float tiempoCarga = 1.5f;

    GameObject mapaActual;
    bool cargando;

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        mapaA.SetActive(true);
        mapaB.SetActive(false);

        mapaActual = mapaA;

        panel.alpha = 0f;
        panel.blocksRaycasts = false;
        panel.interactable = false;

        barra.value = 0f;
    }

    public void CambiarMapa(GameObject destino, string idSpawn)
    {
        if (cargando || destino == mapaActual)
            return;

        StartCoroutine(Rutina(destino, idSpawn));
    }

    IEnumerator Rutina(GameObject destino, string idSpawn)
    {
        cargando = true;

        panel.blocksRaycasts = true;
        yield return Fade(1f);

        barra.value = 0f;

        mapaActual.SetActive(false);
        destino.SetActive(true);
        mapaActual = destino;

        ColocarJugador(destino, idSpawn);

        float t = 0f;

        while (t < tiempoCarga)
        {
            t += Time.unscaledDeltaTime;

            barra.value = Mathf.Clamp01(
                t / tiempoCarga
            );

            yield return null;
        }

        barra.value = 1f;

        yield return Fade(0f);

        panel.blocksRaycasts = false;
        panel.interactable = false;

        cargando = false;
    }

    IEnumerator Fade(float alphaObjetivo)
    {
        float alphaInicial = panel.alpha;
        float tiempo = 0f;

        if (duracionFade <= 0f)
        {
            panel.alpha = alphaObjetivo;
            yield break;
        }

        while (tiempo < duracionFade)
        {
            tiempo += Time.unscaledDeltaTime;

            float progreso = Mathf.Clamp01(
                tiempo / duracionFade
            );

            panel.alpha = Mathf.Lerp(
                alphaInicial,
                alphaObjetivo,
                progreso
            );

            yield return null;
        }

        panel.alpha = alphaObjetivo;
    }

    void ColocarJugador(GameObject mapa, string idSpawn)
    {
        foreach (var p in mapa.GetComponentsInChildren<PuntoSpawn>())
        {
            if (p.id != idSpawn)
                continue;

            var cc = jugador.GetComponent<CharacterController>();

            if (cc)
                cc.enabled = false;

            jugador.SetPositionAndRotation(
                p.transform.position,
                p.transform.rotation
            );

            if (cc)
                cc.enabled = true;

            return;
        }

        Debug.LogWarning(
            $"No se encontró el spawn '{idSpawn}'"
        );
    }
}
