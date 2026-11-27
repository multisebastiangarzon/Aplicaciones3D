using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class AltarRitual : MonoBehaviour
{
    public static bool Cerrando { get; private set; }

    [Header("Requisitos")]
    [Tooltip("Cantidad de objetos recogidos para poder usar el altar.")]
    public int objetosRequeridos = 2;
    [Tooltip("Si está marcado, exige disco y esmeralda en concreto (no solo la cantidad).")]
    public bool exigirDiscoYEsmeralda = false;

    [Header("Interacción")]
    public float radio = 4f;
    public string mensajeAviso = "[E] Arrojar los objetos al altar";
    [Tooltip("Se muestra cerca del altar si aún no cumples. Vacío = no muestra nada.")]
    public string mensajeFalta = "";

    [Header("Cierre")]
    [Tooltip("Marcado: al pulsar E hace el fundido a la escena de cierre. Desmarcado: solo dispara el evento y otro script llama a IrACierre().")]
    public bool irDirectoACierre = true;
    public FundidoEscena fundido;
    public string escenaCierre = "Cierre";
    [Tooltip("Congela el juego al activar el altar, para que el jugador no se mueva durante el fundido.")]
    public bool congelarAlActivar = true;

    [Header("Eventos")]
    public UnityEvent alActivar;

    Transform jugador;
    bool usado;
    bool mostrando;
    string textoMostrado;

    void Start()
    {
        Cerrando = false;
    }

    void OnDisable()
    {
        OcultarAviso();
    }

    void Update()
    {
        if (usado) return;

        if (Time.timeScale == 0f || AcertijoCultural.Activo || DialogoInteractuable.Hablando)
        {
            OcultarAviso();
            return;
        }

        if (jugador == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) return;
            jugador = p.transform;
        }

        Vector3 d = jugador.position - transform.position;
        d.y = 0f;
        bool cerca = d.magnitude <= radio;

        bool cumple = Cumple();
        string texto = cumple ? mensajeAviso : mensajeFalta;

        if (cerca && !string.IsNullOrEmpty(texto)) MostrarAviso(texto);
        else OcultarAviso();

        if (cerca && cumple && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Activar();
        }
    }

    bool Cumple()
    {
        if (exigirDiscoYEsmeralda)
            return Recolectable.DiscoRecogido && Recolectable.EsmeraldaRecogida;
        return Recolectable.TotalRecogidos >= objetosRequeridos;
    }

    void Activar()
    {
        usado = true;
        Cerrando = true;
        OcultarAviso();

        if (congelarAlActivar) Time.timeScale = 0f;

        alActivar.Invoke();

        if (irDirectoACierre) IrACierre();
    }

    // Público para poder llamarlo desde otro evento (por ejemplo, al terminar un diálogo).
    public void IrACierre()
    {
        if (fundido != null) fundido.CargarEscena(escenaCierre);
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(escenaCierre);
        }
    }

    void MostrarAviso(string texto)
    {
        if (GestorAvisos.Instancia == null) return;
        if (mostrando && texto == textoMostrado) return;
        GestorAvisos.Instancia.MostrarAviso(this, texto);
        mostrando = true;
        textoMostrado = texto;
    }

    void OcultarAviso()
    {
        if (!mostrando) return;
        if (GestorAvisos.Instancia != null) GestorAvisos.Instancia.OcultarAviso(this);
        mostrando = false;
        textoMostrado = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}
