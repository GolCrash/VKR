using UnityEngine;

public static class textGen
{
    public static Texture2D TextureFromColor(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHight(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Color[] colorMap = new Color[width * height];

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);

        return TextureFromColor(colorMap, width, height);
    }

    public static Texture2D TextureFromVoronoi(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Color[] colorMap = new Color[width * height];

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);

        return TextureFromColor(colorMap, width, height);
    }
}