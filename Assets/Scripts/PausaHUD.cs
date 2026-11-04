using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PausaHUD : MonoBehaviour
{
    public GameObject fondoPausa;
    public GameObject panelPausa;
    public string escenaMenu;

    bool pausado;

    void Start()
    {
        Time.timeScale = 1f;
        fondoPausa.SetActive(false);
        panelPausa.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (pausado) Reanudar();
            else Pausar();
        }
    }

    public void Pausar()
    {
        pausado = true;
        Time.timeScale = 0f;
        fondoPausa.SetActive(true);
        panelPausa.SetActive(true);
    }

    public void Reanudar()
    {
        pausado = false;
        Time.timeScale = 1f;
        fondoPausa.SetActive(false);
        panelPausa.SetActive(false);
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaMenu);
    }
}