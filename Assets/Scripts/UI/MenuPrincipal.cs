using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    [Tooltip("Botón Salir (opcional). Se oculta automáticamente en WebGL.")]
    public GameObject botonSalir;

    void Start()
    {
        Time.timeScale = 1f;                      
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

#if UNITY_WEBGL
        if (botonSalir != null) botonSalir.SetActive(false);
#endif
    }

    public void Jugar(string nombreEscena)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscena);
    }

    public void Salir()
    {
#if UNITY_WEBGL
        return;                                    // en navegador no se puede cerrar
#elif UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}