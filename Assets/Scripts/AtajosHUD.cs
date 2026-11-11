using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class AtajosHUD : MonoBehaviour
{
    public GestorPaneles gestorPaneles;
    public GameObject panelInfo;

    public Image iconoSonido;
    public Sprite spriteSonidoActivo;
    public Sprite spriteSonidoSilencio;

    bool silenciado;

    void Start()
    {
        silenciado = AudioListener.volume <= 0f;
        ActualizarIcono();
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.iKey.wasPressedThisFrame) AlternarInfo();
        if (Keyboard.current.mKey.wasPressedThisFrame) AlternarSonido();
    }

    public void AlternarInfo()
    {
        if (panelInfo.activeSelf) gestorPaneles.CerrarActual();
        else gestorPaneles.Abrir(panelInfo);
    }

    public void AlternarSonido()
    {
        silenciado = !silenciado;
        AudioListener.volume = silenciado ? 0f : 1f;
        ActualizarIcono();
    }

    void ActualizarIcono()
    {
        if (iconoSonido == null) return;
        if (spriteSonidoActivo == null || spriteSonidoSilencio == null) return;
        iconoSonido.sprite = silenciado ? spriteSonidoSilencio : spriteSonidoActivo;
    }
}