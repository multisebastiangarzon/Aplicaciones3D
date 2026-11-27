using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlotObjetoHUD : MonoBehaviour
{
    [Header("Referencias")]
    public Image icono;
    public Image destello;          // opcional

    [Header("Estados")]
    public Color colorVacio = new Color(0f, 0f, 0f, 0.45f);  // silueta oscura
    public Color colorLleno = Color.white;

    [Header("Animación")]
    public float duracion = 0.6f;
    public float escalaMax = 1.6f;

    public bool Obtenido { get; private set; }

    Vector3 escalaBase;
    Coroutine anim;

    void Awake()
    {
        if (icono == null) icono = GetComponent<Image>();
        escalaBase = transform.localScale;
        Vaciar();
    }

    public void Vaciar()
    {
        if (anim != null) StopCoroutine(anim);
        Obtenido = false;
        icono.color = colorVacio;
        transform.localScale = escalaBase;
        if (destello != null) SetAlpha(destello, 0f);
    }

    public void Obtener(bool instantaneo = false)
    {
        if (Obtenido) return;
        Obtenido = true;

        if (instantaneo)
        {
            icono.color = colorLleno;
            return;
        }

        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(Animar());
    }

    IEnumerator Animar()
    {
        float t = 0f;
        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;           // funciona aunque el juego esté en pausa
            float k = Mathf.Clamp01(t / duracion);

            // Escala: sube rápido y vuelve con un pequeño rebote
            float pulso = Mathf.Sin(k * Mathf.PI);
            transform.localScale = escalaBase * (1f + (escalaMax - 1f) * pulso);

            // Pasa de silueta a color completo
            icono.color = Color.Lerp(colorVacio, colorLleno, Mathf.Clamp01(k * 2f));

            // Destello que se apaga
            if (destello != null) SetAlpha(destello, (1f - k) * 0.9f);

            yield return null;
        }

        transform.localScale = escalaBase;
        icono.color = colorLleno;
        if (destello != null) SetAlpha(destello, 0f);
    }

    static void SetAlpha(Image img, float a)
    {
        var c = img.color;
        c.a = a;
        img.color = c;
    }
}