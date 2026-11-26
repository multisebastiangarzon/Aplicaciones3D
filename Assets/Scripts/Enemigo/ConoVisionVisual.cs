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
    [Tooltip("Patrulla lo iguala a su 'alturaOjos' al iniciar, para que el dibujo y la deteccion usen el mismo rayo.")]
    public float alturaDeteccion = 0.45f;

    [Header("Origen y orientación")]
    [Tooltip("Objeto vacío situado entre los ojos. Si está vacío se usa este objeto.")]
    public Transform ojos;
    [Tooltip("Capas del terreno, para medir la altura del suelo. Opcional: si está vacío se usa el NavMesh.")]
    public LayerMask suelo;
    [Tooltip("Mayor = el cono gira más rápido. 0 = sin suavizado (dibujo y deteccion giran exactamente igual que el cuerpo).")]
    public float suavizadoDireccion = 8f;
    [Tooltip("Mayor = la altura del suelo se actualiza más rápido. 0 = sin suavizado.")]
    public float suavizadoAltura = 6f;

    [Header("Volumen (metros sobre el suelo)")]
    [Tooltip("Solo se usa si NO hay objeto Ojos. Con Ojos, la altura sale de su posición.")]
    public float alturaCabeza = 1.2f;
    [Tooltip("Altura del borde superior al final del alcance. 0 = el cono termina tocando el suelo.")]
    public float alturaFondo = 0.1f;
    public bool ajustarAlSuelo = true;
    [Tooltip("El final del cono se apoya en el terreno real en cada rayo (cuestas, desniveles).")]
    public bool seguirTerreno = true;

    [Header("Apariencia")]
    public Color colorCono = new Color(1f, 0.85f, 0.2f);
    [Range(0f, 1f)] public float opacidadRelleno = 0.30f;
    [Range(0f, 1f)] public float opacidadBorde = 0.90f;

    // Patrulla usa estos valores para que la deteccion real coincida con el dibujo
    public Vector3 OrigenMundo { get; private set; }
    public Vector3 FrenteMundo { get; private set; } = Vector3.forward;

    const int Punta = 0;

    Mesh malla;
    MeshRenderer mr;
    Material materialPropio;
    Vector3[] vertices;
    Color[] colores;
    float[] cosTabla, sinTabla;
    int n, anillo, bloque;

    Vector3 frenteSuave = Vector3.forward;
    float sueloSuave;
    bool inicializado;

    int IndiceAbajo(int i) => 1 + i;
    int IndiceTecho(int i) => 1 + anillo + i;

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

    void LateUpdate() => ActualizarForma();

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
            mr.sharedMaterials = new[] { materialPropio, materialPropio };
        }
        else Debug.LogWarning("ConoVisionVisual: no se encontró el shader Sprites/Default.");

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

        // Seno/coseno de cada rayo respecto al frente, calculados una sola vez
        cosTabla = new float[n + 1];
        sinTabla = new float[n + 1];
        float inicio = -angulo * 0.5f;
        float paso = angulo / n;
        for (int i = 0; i <= n; i++)
        {
            float rad = (inicio + paso * i) * Mathf.Deg2Rad;
            cosTabla[i] = Mathf.Cos(rad);
            sinTabla[i] = Mathf.Sin(rad);
        }

        // Triángulos (relleno)
        int[] tri = new int[6 * n + 6];
        int t = 0;
        for (int i = 0; i < n; i++)
        {
            tri[t++] = Punta; tri[t++] = IndiceAbajo(i); tri[t++] = IndiceAbajo(i + 1);
            tri[t++] = Punta; tri[t++] = IndiceTecho(i); tri[t++] = IndiceTecho(i + 1);
        }
        tri[t++] = Punta; tri[t++] = IndiceTecho(0); tri[t++] = IndiceAbajo(0);
        tri[t++] = Punta; tri[t++] = IndiceTecho(n); tri[t++] = IndiceAbajo(n);

        // Líneas (borde), en el segundo bloque de vértices
        int[] lin = new int[2 * (6 + 2 * n)];
        int l = 0;
        void Linea(int a, int b) { lin[l++] = bloque + a; lin[l++] = bloque + b; }

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
        inicializado = false;
        ActualizarForma();
    }

    // Altura del suelo en un punto: primero el terreno real (capa 'suelo'), luego el NavMesh.
    // 'arriba' y 'abajo' definen cuanto se sube y se baja el rayo desde la altura de referencia.
    float BuscarSuelo(Vector3 punto, float yReferencia, float arriba, float abajo,
                      float radioNavMesh, float porDefecto)
    {
        if (suelo.value != 0)
        {
            Vector3 desde = new Vector3(punto.x, yReferencia + arriba, punto.z);
            if (Physics.Raycast(desde, Vector3.down, out RaycastHit hit, arriba + abajo, suelo,
                                QueryTriggerInteraction.Ignore))
                return hit.point.y;
        }

        if (ajustarAlSuelo)
        {
            Vector3 desde = new Vector3(punto.x, yReferencia, punto.z);
            if (NavMesh.SamplePosition(desde, out NavMeshHit nav, radioNavMesh, NavMesh.AllAreas))
                return nav.position.y;
        }

        return porDefecto;
    }

    void ActualizarForma()
    {
        if (malla == null || vertices == null) return;

        // 1. Origen: los ojos (o este objeto si no se asignaron)
        Transform fuente = ojos != null ? ojos : transform;
        Vector3 origen = fuente.position;

        // 2. Dirección horizontal, suavizada
        Vector3 objetivo = fuente.forward;
        objetivo.y = 0f;
        if (objetivo.sqrMagnitude < 0.0001f) objetivo = frenteSuave;
        objetivo.Normalize();

        float dt = Time.deltaTime;

        if (!inicializado || suavizadoDireccion <= 0f)
            frenteSuave = objetivo;
        else
        {
            frenteSuave = Vector3.Slerp(frenteSuave, objetivo, 1f - Mathf.Exp(-suavizadoDireccion * dt));
            frenteSuave.y = 0f;
            if (frenteSuave.sqrMagnitude < 0.0001f) frenteSuave = objetivo;
            frenteSuave.Normalize();
        }

        // 3. Altura del suelo bajo el origen, suavizada.
        //    Si no se encuentra suelo, con Ojos se usa la base de este objeto (no la altura de los ojos,
        //    que dejaria el cono aplastado a la altura de la cabeza).
        float respaldo = ojos != null ? transform.position.y : origen.y;
        float sueloObjetivo = BuscarSuelo(origen, origen.y, 2f, 6f, 3f, respaldo);
        sueloSuave = (!inicializado || suavizadoAltura <= 0f)
            ? sueloObjetivo
            : Mathf.Lerp(sueloSuave, sueloObjetivo, 1f - Mathf.Exp(-suavizadoAltura * dt));

        inicializado = true;

        Vector3 centro = new Vector3(origen.x, sueloSuave, origen.z);
        OrigenMundo = centro;
        FrenteMundo = frenteSuave;

        // Altura de la cabeza: con Ojos sale de su posición real, sin Ojos usa el campo
        float cabeza = ojos != null ? Mathf.Max(0.05f, origen.y - sueloSuave) : alturaCabeza;

        // Bounds manuales (más baratos que RecalculateBounds)
        Matrix4x4 aLocal = transform.worldToLocalMatrix;
        Vector3 esc = transform.lossyScale;
        float escMin = Mathf.Max(0.0001f, Mathf.Min(esc.x, Mathf.Min(esc.y, esc.z)));
        float lado = 2f * (alcance + Mathf.Max(cabeza, alturaFondo) + 1f) / escMin;
        malla.bounds = new Bounds(aLocal.MultiplyPoint3x4(centro), new Vector3(lado, lado, lado));

        // Si nadie ve el cono, no hace falta lanzar rayos ni tocar la malla
        if (!mr.isVisible) return;

        // 4. Geometría del cono
        Vector3 origenRayo = centro + Vector3.up * alturaDeteccion;
        bool hayObstaculos = obstaculos.value != 0;
        Vector3 der = new Vector3(frenteSuave.z, 0f, -frenteSuave.x); // equivale a Cross(up, frente)

        vertices[Punta] = aLocal.MultiplyPoint3x4(centro + Vector3.up * cabeza);

        for (int i = 0; i <= n; i++)
        {
            Vector3 dir = frenteSuave * cosTabla[i] + der * sinTabla[i];

            float distancia = alcance;
            if (hayObstaculos &&
                Physics.Raycast(origenRayo, dir, out RaycastHit hit, alcance, obstaculos,
                                QueryTriggerInteraction.Ignore))
                distancia = hit.distance;

            float proporcion = alcance > 0f ? distancia / alcance : 0f;
            Vector3 punto = centro + dir * distancia;

            // El final del cono se apoya en el terreno real de ESE punto, no en el del origen
            if (seguirTerreno)
                punto.y = BuscarSuelo(punto, sueloSuave, 1.5f, 3f, 1.5f, sueloSuave);

            // Borde inferior: baja de la cabeza al suelo (toca el suelo al final del alcance)
            // Borde superior: baja de la cabeza a 'alturaFondo'
            float alturaAbajo = cabeza * (1f - proporcion);
            float alturaTecho = Mathf.Lerp(cabeza, alturaFondo, proporcion);

            vertices[IndiceAbajo(i)] = aLocal.MultiplyPoint3x4(punto + Vector3.up * alturaAbajo);
            vertices[IndiceTecho(i)] = aLocal.MultiplyPoint3x4(punto + Vector3.up * alturaTecho);
        }

        // El segundo bloque (líneas del borde) copia al primero
        System.Array.Copy(vertices, 0, vertices, bloque, bloque);
        malla.vertices = vertices;
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

    static Color ConAlfa(Color c, float alfa) { c.a = alfa; return c; }

    void OnValidate()
    {
        // Reconstruye en Play para que ángulo, alcance y segmentos se apliquen al editar
        if (Application.isPlaying && malla != null) Construir();
    }
}