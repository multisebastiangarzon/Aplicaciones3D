using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class Respuesta
{
    public string texto;
    [Tooltip("Índice de la línea a la que lleva. -1 = cerrar el diálogo.")]
    public int siguiente = -1;
}

[Serializable]
public class LineaDialogo
{
    [TextArea(2, 5)] public string texto;
    [Tooltip("Vacío = con E pasa a la siguiente línea.")]
    public Respuesta[] respuestas;
}

public class DialogoInteractuable : MonoBehaviour
{
    public static bool Hablando { get; private set; }

    [Header("Diálogo")]
    public LineaDialogo[] lineas;
    public float radio = 2.5f;

    [Header("UI")]
    public GameObject panelDialogo;
    public TMP_Text textoDialogo;
    public GameObject avisoE;
    public Button[] botones;              

    private Transform jugador;
    private int indice;
    private bool abierto;
    private bool cerca;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jugador = p.transform;
        else Debug.LogWarning("DialogoInteractuable: no hay objeto con tag Player.");

        
        for (int i = 0; i < botones.Length; i++)
        {
            int n = i;
            botones[i].onClick.AddListener(() => Elegir(n));
        }
    }

    void OnDisable()
    {
        if (abierto) { abierto = false; Hablando = false; }
    }

    void Update()
    {
        if (jugador == null) return;

        Vector3 d = jugador.position - transform.position;
        d.y = 0f;
        bool ahoraCerca = d.magnitude <= radio;

        if (ahoraCerca != cerca)
        {
            cerca = ahoraCerca;
            if (avisoE != null) avisoE.SetActive(cerca && !abierto);
            if (!cerca && abierto) Cerrar();
        }

        var k = Keyboard.current;
        if (k == null) return;

        if (cerca && !abierto && k.eKey.wasPressedThisFrame)
        {
            Abrir();
            return;
        }

        if (!abierto) return;

        LineaDialogo actual = lineas[indice];
        bool hayRespuestas = actual.respuestas != null && actual.respuestas.Length > 0;

        if (hayRespuestas)
        {
            if (k.digit1Key.wasPressedThisFrame) Elegir(0);
            else if (k.digit2Key.wasPressedThisFrame) Elegir(1);
            else if (k.digit3Key.wasPressedThisFrame) Elegir(2);
        }
        else if (k.eKey.wasPressedThisFrame)
        {
            Ir(indice + 1);   
        }
    }

    void Abrir()
    {
        if (lineas.Length == 0) return;
        abierto = true;
        Hablando = true;
        if (avisoE != null) avisoE.SetActive(false);
        panelDialogo.SetActive(true);
        Ir(0);
    }

    void Elegir(int n)
    {
        if (!abierto) return;
        var resp = lineas[indice].respuestas;
        if (resp == null || n >= resp.Length) return;
        Ir(resp[n].siguiente);
    }

    void Ir(int nuevo)
    {
        if (nuevo < 0 || nuevo >= lineas.Length) { Cerrar(); return; }

        indice = nuevo;
        LineaDialogo l = lineas[indice];
        textoDialogo.text = l.texto;

        for (int i = 0; i < botones.Length; i++)
        {
            bool usar = l.respuestas != null && i < l.respuestas.Length;
            botones[i].gameObject.SetActive(usar);
            if (usar)
                botones[i].GetComponentInChildren<TMP_Text>().text = (i + 1) + ". " + l.respuestas[i].texto;
        }
    }

    void Cerrar()
    {
        abierto = false;
        Hablando = false;
        panelDialogo.SetActive(false);
        if (avisoE != null) avisoE.SetActive(cerca);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}