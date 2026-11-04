using UnityEngine;

public class GestorPaneles : MonoBehaviour
{
    public GameObject fondoOscuro;
    public GameObject[] paneles;

    void Start()
    {
        CerrarActual();
    }

    public void Abrir(GameObject panel)
    {
        foreach (GameObject p in paneles)
        {
            p.SetActive(false);
        }
        fondoOscuro.SetActive(true);
        panel.SetActive(true);
    }

    public void CerrarActual()
    {
        foreach (GameObject p in paneles)
        {
            p.SetActive(false);
        }
        fondoOscuro.SetActive(false);
    }
}