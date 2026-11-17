using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class AcertijoCultural : MonoBehaviour
{
    public static bool Activo { get; private set; }

    [Header("Contenido")]
    public string titulo = "Acertijo";
    [TextArea(2, 5)] public string pregunta = "";
    public string[] opciones = { "", "", "" };
    public string[] prefijos = { "A) ", "B) ", "C) " };
    [Tooltip("Índice de la respuesta correcta: 0, 1 o 2.")]
    public int correcta = 0;
    public string mensajeAcierto = "";
    public string mensajeError = "¡Te oyó!";

    [Header("Activación")]
    public float radio = 3f;
    public GameObject avisoE;

    [Header("UI")]
    public GameObject panel;
    public GameObject marco;
    public TMP_Text textoTitulo;
    public TMP_Text textoPregunta;
    public TMP_Text textoFeedback;
    public Button[] botones;

    [Header("Colores")]
    public Color colorBotonAcierto = new Color(0.55f, 1f, 0.55f);
    public Color colorBotonError = new Color(1f, 0.45f, 0.45f);
    public Color colorTextoAcierto = new Color(0.3f, 0.85f, 0.3f);
    public Color colorTextoError = new Color(0.9f, 0.2f, 0.2f);
    [Tooltip("Apaga el tinte de hover/presionado de estos botones.")]
    public bool sinTintBotones = true;

    [Header("Tiempos (segundos)")]
    public float tiempoBoton = 0.6f;
    public float tiempoMensaje = 2f;

    [Header("Comportamiento")]
    public bool pausarJuego = true;
    public bool liberarCursor = true;
    public bool permitirTeclas = true;

    [Header("Eventos")]
    public UnityEvent alAcertar;
    public UnityEvent alFallar;

    Transform jugador;
    Image[] imagenes;
    Color[] colorOriginal;
    bool abierto;
    bool respondiendo;
    bool resuelto;
    bool cerca;
    float escalaPrevia = 1f;
    CursorLockMode bloqueoPrevio;
    bool visiblePrevio;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jugador = p.transform;
        else Debug.LogWarning("AcertijoCultural: no hay objeto con tag Player.");

        imagenes = new Image[botones.Length];
        colorOriginal = new Color[botones.Length];

        for (int i = 0; i < botones.Length; i++)
        {
            int n = i;
            botones[i].onClick.AddListener(() => Responder(n));

            imagenes[i] = botones[i].image;
            if (imagenes[i] != null) colorOriginal[i] = imagenes[i].color;
            if (sinTintBotones) botones[i].transition = Selectable.Transition.None;
        }

        panel.SetActive(false);
        if (avisoE != null) avisoE.SetActive(false);
    }

    void Update()
    {
        if (resuelto) return;

        var k = Keyboard.current;
        if (k == null) return;

        if (!abierto)
        {
            if (Time.timeScale == 0f) return;
            if (jugador == null) return;

            Vector3 d = jugador.position - transform.position;
            d.y = 0f;
            bool ahoraCerca = d.magnitude <= radio;

            if (ahoraCerca != cerca)
            {
                cerca = ahoraCerca;
                if (avisoE != null) avisoE.SetActive(cerca);
            }

            if (cerca && k.eKey.wasPressedThisFrame) Abrir();
            return;
        }

        if (respondiendo || !permitirTeclas) return;

        if (k.digit1Key.wasPressedThisFrame) Responder(0);
        else if (k.digit2Key.wasPressedThisFrame) Responder(1);
        else if (k.digit3Key.wasPressedThisFrame) Responder(2);
    }

    void LateUpdate()
    {
        if (abierto && liberarCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnDisable()
    {
        if (abierto)
        {
            abierto = false;
            Activo = false;
            if (pausarJuego) Time.timeScale = escalaPrevia;
            RestaurarCursor();
        }
    }

    void Abrir()
    {
        abierto = true;
        Activo = true;
        if (avisoE != null) avisoE.SetActive(false);

        escalaPrevia = Time.timeScale;
        if (pausarJuego) Time.timeScale = 0f;

        bloqueoPrevio = Cursor.lockState;
        visiblePrevio = Cursor.visible;

        if (textoTitulo != null) textoTitulo.text = titulo;
        if (textoPregunta != null) textoPregunta.text = pregunta;
        if (textoFeedback != null) textoFeedback.text = "";
        if (marco != null) marco.SetActive(true);

        for (int i = 0; i < botones.Length; i++)
        {
            bool usar = i < opciones.Length;
            botones[i].gameObject.SetActive(usar);
            if (imagenes[i] != null) imagenes[i].color = colorOriginal[i];
            if (usar)
            {
                TMP_Text t = botones[i].GetComponentInChildren<TMP_Text>();
                if (t != null)
                {
                    string pref = (prefijos != null && i < prefijos.Length) ? prefijos[i] : (i + 1) + ". ";
                    t.text = pref + opciones[i];
                }
            }
        }

        panel.SetActive(true);
    }

    void Responder(int n)
    {
        if (!abierto || respondiendo) return;
        if (n < 0 || n >= opciones.Length) return;
        StartCoroutine(Resolver(n));
    }

    IEnumerator Resolver(int n)
    {
        respondiendo = true;
        bool acierto = n == correcta;

        if (imagenes[n] != null)
            imagenes[n].color = acierto ? colorBotonAcierto : colorBotonError;

        yield return new WaitForSecondsRealtime(tiempoBoton);

        if (marco != null) marco.SetActive(false);
        if (textoFeedback != null)
        {
            textoFeedback.text = acierto ? mensajeAcierto : mensajeError;
            textoFeedback.color = acierto ? colorTextoAcierto : colorTextoError;
        }

        yield return new WaitForSecondsRealtime(tiempoMensaje);

        Cerrar();
        resuelto = true;

        if (acierto) alAcertar.Invoke();
        else alFallar.Invoke();
    }

    void Cerrar()
    {
        abierto = false;
        respondiendo = false;
        Activo = false;
        panel.SetActive(false);
        if (pausarJuego) Time.timeScale = escalaPrevia;
        RestaurarCursor();
    }

    void RestaurarCursor()
    {
        if (!liberarCursor) return;
        Cursor.lockState = bloqueoPrevio;
        Cursor.visible = visiblePrevio;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}