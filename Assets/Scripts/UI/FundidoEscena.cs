using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FundidoEscena : MonoBehaviour
{
    public CanvasGroup velo;
    public float duracion = 0.8f;

    void Start()
    {
        velo.blocksRaycasts = true;
        StartCoroutine(Fundir(1f, 0f, false));
    }

    public void CargarEscena(string nombre)
    {
        StartCoroutine(SalirA(nombre));
    }

    IEnumerator SalirA(string nombre)
    {
        velo.blocksRaycasts = true;
        yield return Fundir(0f, 1f, true);
        SceneManager.LoadScene(nombre);
    }

    IEnumerator Fundir(float desde, float hasta, bool bloquear)
    {
        float t = 0f;
        velo.alpha = desde;
        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            velo.alpha = Mathf.Lerp(desde, hasta, t / duracion);
            yield return null;
        }
        velo.alpha = hasta;
        velo.blocksRaycasts = bloquear;
    }
}