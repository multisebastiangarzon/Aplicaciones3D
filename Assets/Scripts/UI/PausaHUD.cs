using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class PausaHUD : MonoBehaviour
{
    public GameObject fondoPausa;
    public GameObject panelPausa;
    public GestorPaneles gestorPaneles;
    public string escenaMenu;

    bool pausado;
    CursorLockMode bloqueoPrevio;
    bool visiblePrevio;

    // No se puede pausar con el acertijo abierto ni durante el cierre del juego.
    bool Bloqueado
    {
        get { return AcertijoCultural.Activo || AltarRitual.Cerrando; }
    }

    void Start()
    {
        // Si Monetá ya congeló el juego, no lo descongeles.
        if (!DialogoMoneta.Congelando) Time.timeScale = 1f;

        fondoPausa.SetActive(false);
        panelPausa.SetActive(false);
        AsegurarEventSystem();
    }

    void Update()
    {
        if (Bloqueado) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (gestorPaneles != null && gestorPaneles.HayPanelAbierto())
            {
                gestorPaneles.CerrarActual();
            }
            else if (pausado)
            {
                Reanudar();
            }
            else
            {
                Pausar();
            }
        }
    }

    void LateUpdate()
    {
        if (pausado)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void Pausar()
    {
        if (pausado || Bloqueado) return;

        pausado = true;
        Time.timeScale = 0f;
        fondoPausa.SetActive(true);
        panelPausa.SetActive(true);

        bloqueoPrevio = Cursor.lockState;
        visiblePrevio = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Reanudar()
    {
        if (!pausado) return;
        pausado = false;

        // Si Monetá sigue abierta, el juego sigue congelado.
        Time.timeScale = DialogoMoneta.Congelando ? 0f : 1f;

        fondoPausa.SetActive(false);
        panelPausa.SetActive(false);

        Cursor.lockState = bloqueoPrevio;
        Cursor.visible = visiblePrevio;
    }

    public void IrAlMenu()
    {
        pausado = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(escenaMenu);
    }

    void AsegurarEventSystem()
    {
        EventSystem es = FindAnyObjectByType<EventSystem>();
        if (es == null)
        {
            es = new GameObject("EventSystem").AddComponent<EventSystem>();
        }

        if (es.GetComponent<InputSystemUIInputModule>() == null)
        {
            StandaloneInputModule viejo = es.GetComponent<StandaloneInputModule>();
            if (viejo != null) Destroy(viejo);
            es.gameObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
    }
}