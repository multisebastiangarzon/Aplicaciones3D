using UnityEngine;
using UnityEditor;
using System.IO;

public class CrearTexturaBruma
{
    [MenuItem("Herramientas/Crear Textura Bruma")]
    static void Crear()
    {
        int tam = 256;
        Texture2D tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
        Vector2 centro = new Vector2(tam / 2f, tam / 2f);

        for (int y = 0; y < tam; y++)
        {
            for (int x = 0; x < tam; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), centro) / (tam / 2f);
                float alfa = Mathf.Clamp01(1f - d);
                alfa = alfa * alfa * (3f - 2f * alfa); // suavizado
                tex.SetPixel(x, y, new Color(1, 1, 1, alfa));
            }
        }
        tex.Apply();

        File.WriteAllBytes("Assets/TexturaBruma.png", tex.EncodeToPNG());
        AssetDatabase.Refresh();
    }
}