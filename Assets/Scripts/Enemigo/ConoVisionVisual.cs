using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ConoVisionVisual : MonoBehaviour
{
    private float angulo = 60f;
    private float alcance = 10f;
    public int segmentos = 20;

    private Mesh malla;

    void Awake()
    {
        malla = new Mesh();
        GetComponent<MeshFilter>().mesh = malla;
    }

    public void Configurar(float nuevoAngulo, float nuevoAlcance)
    {
        angulo = nuevoAngulo;
        alcance = nuevoAlcance;
        ConstruirCono();
    }

    void ConstruirCono()
    {
        Vector3[] vertices = new Vector3[segmentos + 2];
        int[] triangulos = new int[segmentos * 3];

        vertices[0] = Vector3.zero;

        float anguloInicial = -angulo * 0.5f;
        float paso = angulo / segmentos;

        for (int i = 0; i <= segmentos; i++)
        {
            float anguloActual = (anguloInicial + paso * i) * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Sin(anguloActual), 0f, Mathf.Cos(anguloActual)) * alcance;
        }

        for (int i = 0; i < segmentos; i++)
        {
            triangulos[i * 3] = 0;
            triangulos[i * 3 + 1] = i + 2;
            triangulos[i * 3 + 2] = i + 1;
        }

        malla.Clear();
        malla.vertices = vertices;
        malla.triangles = triangulos;
        malla.RecalculateNormals();
    }
}