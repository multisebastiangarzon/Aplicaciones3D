using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ConoVisionVisual : MonoBehaviour
{
    [Header("Forma (Patrulla los configura al iniciar)")]
    [SerializeField] float angulo = 60f;
    [SerializeField] float alcance = 10f;
    [Range(8, 120)] public int segmentos = 40;
    public LayerMask obstaculos;
    public float alturaDeteccion = 0.45f;

    [Header("Volumen (metros sobre el suelo)")]
    public float alturaCabeza = 1.2f;
    public float alturaFondo = 1.0f;
    public bool ajustarAlSuelo = true;

    [Header("Apariencia")]
    public Color colorCono = new Color(1f, 0.85f, 0.2f);
    [Range(0f, 1f)] public float opacidadRelleno = 0.30f;
    [Range(0f, 1f)] public float opacidadBorde = 0.90f;

    const int Punta = 0;

    Mesh malla;
    MeshRenderer mr;
    Material materialPropio;
    Vector3[] vertices;
    Color[] colores;
    int n;           
    int anillo;      
    int bloque;     

    int IndiceAbajo(int i) { return 1 + i; }
    int IndiceTecho(int i) { return 1 + anillo + i; }


    public void Configurar(float nuevoAngulo, float nuevoAlcance)
    {
        angulo = nuevoAngulo;
        alcance = nuevoAlcance;
        Construir();
    }

    public void Configurar(float nuevoAngulo, float nuevoAlcance, LayerMask nuevosObstaculos)
    {
        obstaculos = nuevosObstaculos;
        Configurar(nuevoAngulo, nuevoAlcance);
    }

    void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        PrepararRenderer();

        malla = new Mesh { name = "ConoVision" };
        malla.MarkDynamic();
        GetComponent<MeshFilter>().sharedMesh = malla;

        Construir();
    }

    void LateUpdate()
    {
        ActualizarForma();
    }

    void OnDestroy()
    {
        if (malla != null) Destroy(malla);
        if (materialPropio != null) Destroy(materialPropio);
    }

    void PrepararRenderer()
    {

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            materialPropio = new Material(shader);
            mr.sharedMaterials = new Material[] { materialPropio, materialPropio };
        }
        else
        {
            Debug.LogWarning("ConoVisionVisual: no se encontró el shader Sprites/Default.");
        }

        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
    }

    void Construir()
    {
        n = Mathf.Clamp(segmentos, 8, 120);
        anillo = n + 1;
        bloque = 1 + 2 * anillo;

        vertices = new Vector3[2 * bloque];
        colores = new Color[2 * bloque];

        int[] tri = new int[6 * n + 6];
        int t = 0;

        for (int i = 0; i < n; i++)
        {
            tri[t++] = Punta; tri[t++] = IndiceAbajo(i); tri[t++] = IndiceAbajo(i + 1);  
            tri[t++] = Punta; tri[t++] = IndiceTecho(i); tri[t++] = IndiceTecho(i + 1);   
        }

        tri[t++] = Punta; tri[t++] = IndiceTecho(0); tri[t++] = IndiceAbajo(0);
        tri[t++] = Punta; tri[t++] = IndiceTecho(n); tri[t++] = IndiceAbajo(n);

        int[] lin = new int[2 * (6 + 2 * n)];
        int l = 0;

        void Linea(int a, int b)
        {
            lin[l++] = bloque + a;
            lin[l++] = bloque + b;
        }

        Linea(Punta, IndiceTecho(0));
        Linea(Punta, IndiceTecho(n));
        Linea(Punta, IndiceAbajo(0));
        Linea(Punta, IndiceAbajo(n));
        Linea(IndiceAbajo(0), IndiceTecho(0));
        Linea(IndiceAbajo(n), IndiceTecho(n));

        for (int i = 0; i < n; i++)
        {
            Linea(IndiceAbajo(i), IndiceAbajo(i + 1));  
            Linea(IndiceTecho(i), IndiceTecho(i + 1));   
        }

        malla.Clear();
        malla.vertices = vertices;
        malla.subMeshCount = 2;
        malla.SetTriangles(tri, 0);
        malla.SetIndices(lin, MeshTopology.Lines, 1);

        PintarColores();
        ActualizarForma();
    }

    void ActualizarForma()
    {
        if (malla == null || vertices == null) return;

        float escala = Mathf.Max(0.0001f, transform.lossyScale.x);   
        float cabeza = alturaCabeza / escala;
        float fondo = alturaFondo / escala;

        float sueloLocal = 0f;
        if (ajustarAlSuelo && NavMesh.SamplePosition(transform.position, out NavMeshHit nav, 3f, NavMesh.AllAreas))
            sueloLocal = (nav.position.y - transform.position.y) / escala;

        Vector3 origenRayo = transform.position + Vector3.up * alturaDeteccion;
        bool hayObstaculos = obstaculos.value != 0;

        float inicio = -angulo * 0.5f * Mathf.Deg2Rad;
        float paso = angulo * Mathf.Deg2Rad / n;

        vertices[Punta] = new Vector3(0f, sueloLocal + cabeza, 0f);

        for (int i = 0; i <= n; i++)
        {
            float rad = inicio + paso * i;
            Vector3 dirLocal = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));

            float distancia = alcance;
            if (hayObstaculos)
            {
                Vector3 dirMundo = transform.TransformDirection(dirLocal);
                if (Physics.Raycast(origenRayo, dirMundo, out RaycastHit hit, alcance, obstaculos,
                                    QueryTriggerInteraction.Ignore))
                    distancia = hit.distance;
            }

            float proporcion = alcance > 0f ? distancia / alcance : 0f;
            Vector3 horizontal = dirLocal * (distancia / escala);

            float alturaAbajo = cabeza * (1f - proporcion);
            float alturaTecho = Mathf.Lerp(cabeza, fondo, proporcion);

            vertices[IndiceAbajo(i)] = new Vector3(horizontal.x, sueloLocal + alturaAbajo, horizontal.z);
            vertices[IndiceTecho(i)] = new Vector3(horizontal.x, sueloLocal + alturaTecho, horizontal.z);
        }

        System.Array.Copy(vertices, 0, vertices, bloque, bloque);

        malla.vertices = vertices;
        malla.RecalculateBounds();
    }

    void PintarColores()
    {
        Color cerca = ConAlfa(colorCono, opacidadRelleno);
        Color lejos = ConAlfa(colorCono, opacidadRelleno * 0.6f);

        colores[Punta] = cerca;
        for (int i = 0; i <= n; i++)
        {
            colores[IndiceAbajo(i)] = lejos;
            colores[IndiceTecho(i)] = lejos;
        }

        Color borde = ConAlfa(colorCono, opacidadBorde);
        for (int i = bloque; i < colores.Length; i++)
            colores[i] = borde;

        malla.colors = colores;
    }

    static Color ConAlfa(Color c, float alfa)
    {
        c.a = alfa;
        return c;
    }

    void OnValidate()
    {
        if (Application.isPlaying && malla != null && colores != null)
            PintarColores();
    }
}