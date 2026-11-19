using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ConoVisionVisual : MonoBehaviour
{
    [Header("Forma (Patrulla los configura al iniciar)")]
    [SerializeField] float angulo = 60f;
    [SerializeField] float alcance = 10f;
    [Range(8, 120)] public int segmentos = 40;
    [Tooltip("Solo lo que bloquea la vista (rocas, paredes). NO incluyas aquí el terreno.")]
    public LayerMask obstaculos;
    public float alturaDeteccion = 0.45f;

    [Header("Origen y orientación")]
    [Tooltip("Objeto vacío situado entre los ojos. Si está vacío se usa este objeto.")]
    public Transform ojos;
    [Tooltip("Capas del terreno, para medir la altura del suelo. Opcional: si está vacío se usa el NavMesh.")]
    public LayerMask suelo;
    [Tooltip("Mayor = el cono gira más rápido. 0 = sin suavizado.")]
    public float suavizadoDireccion = 8f;
    [Tooltip("Mayor = la altura del suelo se actualiza más rápido. 0 = sin suavizado.")]
    public float suavizadoAltura = 6f;

    [Header("Volumen (metros sobre el suelo)")]
    public float alturaCabeza = 1.2f;
    public float alturaFondo = 1.0f;
    public bool ajustarAlSuelo = true;

    [Header("Apariencia")]
    public Color colorCono = new Color(1f, 0.85f, 0.2f);
    [Range(0f, 1f)] public float opacidadRelleno = 0.30f;
    [Range(0f, 1f)] public float opacidadBorde = 0.90f;

    // Para que la detección real (Patrulla) use el mismo origen y dirección que el dibujo
    public Vector3 OrigenMundo { get; private set; }
    public Vector3 FrenteMundo { get; private set; } = Vector3.forward;

    const int Punta = 0;

    Mesh malla;
    MeshRenderer mr;
    Material materialPropio;
    Vector3[] vertices;
    Color[] colores;
    int n;
    int anillo;
    int bloque;

    Vector3 frenteSuave = Vector3.forward;
    float sueloSuave;
    bool inicializado;

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

    // Altura del suelo bajo el origen: primero el terreno real, si no el NavMesh
    float AlturaSuelo(Vector3 origen)
    {
        if (suelo.value != 0 &&
            Physics.Raycast(origen + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 8f, suelo,
                            QueryTriggerInteraction.Ignore))
            return hit.point.y;

        if (ajustarAlSuelo && NavMesh.SamplePosition(origen, out NavMeshHit nav, 3f, NavMesh.AllAreas))
            return nav.position.y;

        return origen.y;
    }

    void ActualizarForma()
    {
        if (malla == null || vertices == null) return;

        // 1. Origen: los ojos (o este objeto si no se asignaron)
        Transform fuente = ojos != null ? ojos : transform;
        Vector3 origen = fuente.position;

        // 2. Dirección: solo horizontal, ignora cualquier inclinación del cuerpo
        Vector3 objetivo = fuente.forward;
        objetivo.y = 0f;
        if (objetivo.sqrMagnitude < 0.0001f) objetivo = frenteSuave;
        objetivo.Normalize();

        float dt = Time.deltaTime;

        if (!inicializado || suavizadoDireccion <= 0f)
        {
            frenteSuave = objetivo;
        }
        else
        {
            float k = 1f - Mathf.Exp(-suavizadoDireccion * dt);
            frenteSuave = Vector3.Slerp(frenteSuave, objetivo, k);
            frenteSuave.y = 0f;
            if (frenteSuave.sqrMagnitude < 0.0001f) frenteSuave = objetivo;
            frenteSuave.Normalize();
        }

        // 3. Altura del suelo, suavizada para que el terreno irregular no la haga saltar
        float sueloObjetivo = AlturaSuelo(origen);
        if (!inicializado || suavizadoAltura <= 0f)
            sueloSuave = sueloObjetivo;
        else
            sueloSuave = Mathf.Lerp(sueloSuave, sueloObjetivo, 1f - Mathf.Exp(-suavizadoAltura * dt));

        inicializado = true;

        // 4. Geometría del cono, calculada en el mundo
        Vector3 centro = new Vector3(origen.x, sueloSuave, origen.z);
        Vector3 origenRayo = centro + Vector3.up * alturaDeteccion;
        bool hayObstaculos = obstaculos.value != 0;

        float inicio = -angulo * 0.5f;   // grados
        float paso = angulo / n;

        vertices[Punta] = transform.InverseTransformPoint(centro + Vector3.up * alturaCabeza);

        for (int i = 0; i <= n; i++)
        {
            Vector3 dir = Quaternion.AngleAxis(inicio + paso * i, Vector3.up) * frenteSuave;

            float distancia = alcance;
            if (hayObstaculos &&
                Physics.Raycast(origenRayo, dir, out RaycastHit hit, alcance, obstaculos,
                                QueryTriggerInteraction.Ignore))
                distancia = hit.distance;

            float proporcion = alcance > 0f ? distancia / alcance : 0f;
            Vector3 punto = centro + dir * distancia;

            float alturaAbajo = alturaCabeza * (1f - proporcion);
            float alturaTecho = Mathf.Lerp(alturaCabeza, alturaFondo, proporcion);

            vertices[IndiceAbajo(i)] = transform.InverseTransformPoint(punto + Vector3.up * alturaAbajo);
            vertices[IndiceTecho(i)] = transform.InverseTransformPoint(punto + Vector3.up * alturaTecho);
        }

        // El segundo bloque (las líneas del borde) copia al primero
        System.Array.Copy(vertices, 0, vertices, bloque, bloque);

        malla.vertices = vertices;
        malla.RecalculateBounds();

        OrigenMundo = centro;
        FrenteMundo = frenteSuave;
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