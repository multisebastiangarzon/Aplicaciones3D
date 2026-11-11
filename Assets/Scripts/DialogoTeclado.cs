using UnityEngine;
using UnityEngine.InputSystem;

public class DialogoTeclado : MonoBehaviour
{
    public DialogoMoneta dialogo;

    CursorLockMode bloqueoPrevio;
    bool visiblePrevio;
    bool capturado;

    void Update()
    {
        if (Keyboard.current == null || dialogo == null) return;
        if (Time.timeScale == 0f) return;

        if (dialogo.botonAvanzar.activeSelf)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)
            {
                dialogo.Avanzar();
            }
        }
        else if (dialogo.panelRespuestas.activeSelf)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                dialogo.Elegir();
            }
        }
    }

    void LateUpdate()
    {
        if (!capturado)
        {
            bloqueoPrevio = Cursor.lockState;
            visiblePrevio = Cursor.visible;
            capturado = true;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDisable()
    {
        if (!capturado) return;
        Cursor.lockState = bloqueoPrevio;
        Cursor.visible = visiblePrevio;
    }
}