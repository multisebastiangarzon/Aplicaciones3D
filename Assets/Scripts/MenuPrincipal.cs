using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public void Jugar(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }

    public void Salir()
    {
        Debug.Log("Salir");
        Application.Quit();
    }
}