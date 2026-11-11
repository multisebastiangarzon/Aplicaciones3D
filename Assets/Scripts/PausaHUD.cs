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

    void Start()
    {
        Time.timeScale = 1f;
        fondoPausa.SetActive(false);
        panelPausa.SetActive(false);
        AsegurarEventSystem();
    }

    void Update()
    {
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
        if (pausado) return;
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
        Time.timeScale = 1f;
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