using System.Collections;
using UnityEngine;
using TMPro;

public class GestorAvisos : MonoBehaviour
{
    public static GestorAvisos Instancia { get; private set; }

    [Header("Aviso de interacción (abajo)")]
    public CanvasGroup grupoAviso;
    public TMP_Text textoAviso;
    public float velocidadPulso = 3f;

    [Header("Objetivo (arriba)")]
    public CanvasGroup grupoObjetivo;
    public TMP_Text textoObjetivo;
    public float duracionFade = 0.4f;
    public float duracionVisible = 3.5f;

    [Header("Textos (editables)")]
    public string objetivoDisco = "Objetivo: Encuentra el disco de oro.";
    public string objetivoEsmeralda = "Objetivo: Encuentra la esmeralda sagrada.";
    public string objetivoAltar = "Lleva los objetos al altar.";
    public string confirmaDisco = "Disco de oro obtenido";
    public string confirmaEsmeralda = "Esmeralda sagrada obtenida";

    object duenoAviso;
    bool avisoVisible;
    Coroutine rutinaObjetivo;

    void Awake()
    {
        Instancia = this;
        if (grupoAviso != null) grupoAviso.alpha = 0f;
        if (grupoObjetivo != null) grupoObjetivo.alpha = 0f;
    }

    void OnEnable()
    {
        Recolectable.AlRecogerTipo += AlRecogerObjeto;
    }

    void OnDisable()
    {
        Recolectable.AlRecogerTipo -= AlRecogerObjeto;
    }

    void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    void Update()
    {
        if (grupoAviso == null) return;

        if (avisoVisible)
        {
            float onda = (Mathf.Sin(Time.unscaledTime * velocidadPulso) + 1f) / 2f;
            grupoAviso.alpha = Mathf.Lerp(0.5f, 1f, onda);
        }
        else
        {
            grupoAviso.alpha = 0f;
        }
    }

    // ---------- Aviso de interacción ----------
    public void MostrarAviso(object dueno, string mensaje)
    {
        duenoAviso = dueno;
        if (textoAviso != null) textoAviso.text = mensaje;
        avisoVisible = true;
    }

    public void OcultarAviso(object dueno)
    {
        if (duenoAviso != dueno) return;
        avisoVisible = false;
        duenoAviso = null;
    }

    // ---------- Objetivo ----------
    void AlRecogerObjeto(TipoObjeto tipo)
    {
        string confirmacion = "";
        if (tipo == TipoObjeto.Disco) confirmacion = confirmaDisco;
        else if (tipo == TipoObjeto.Esmeralda) confirmacion = confirmaEsmeralda;

        string siguiente = TextoObjetivoPendiente();
        string mensaje = string.IsNullOrEmpty(confirmacion) ? siguiente : confirmacion + "\n" + siguiente;
        MostrarObjetivo(mensaje);
    }

    public string TextoObjetivoPendiente()
    {
        if (!Recolectable.DiscoRecogido) return objetivoDisco;
        if (!Recolectable.EsmeraldaRecogida) return objetivoEsmeralda;
        return objetivoAltar;
    }

    // Para conectar en el Inspector (por ejemplo, al terminar el diálogo de Monetá).
    public void MostrarObjetivoPendiente()
    {
        MostrarObjetivo(TextoObjetivoPendiente());
    }

    public void MostrarObjetivo(string mensaje)
    {
        if (textoObjetivo == null || grupoObjetivo == null) return;
        if (rutinaObjetivo != null) StopCoroutine(rutinaObjetivo);
        rutinaObjetivo = StartCoroutine(RutinaObjetivo(mensaje));
    }

    IEnumerator RutinaObjetivo(string mensaje)
    {
        textoObjetivo.text = mensaje;
        yield return Fade(grupoObjetivo.alpha, 1f);
        yield return new WaitForSecondsRealtime(duracionVisible);
        yield return Fade(1f, 0f);
        rutinaObjetivo = null;
    }

    IEnumerator Fade(float desde, float hasta)
    {
        float t = 0f;
        float dur = Mathf.Max(duracionFade, 0.01f);
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            grupoObjetivo.alpha = Mathf.Lerp(desde, hasta, t / dur);
            yield return null;
        }
        grupoObjetivo.alpha = hasta;
    }
}
