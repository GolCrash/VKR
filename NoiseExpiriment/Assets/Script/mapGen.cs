using Assets.Scripts;
using UnityEngine;

public class mapGen : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        FalloffMap,
        VoronoiMap,
        TestCombainMap
    };

    #region Параметры генерации
    [Header("Общие настройки")]
    public DrawMode drawMode;
    public bool autoUpdate;
    public int width;
    public int height;
    public int seed;

    [Header("Настройки шума")]
    public float noiseScale;
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public Vector2 offset;

    [Header("Настройки гассовского распределения")]
    public bool useFalloff = false;
    public bool ellipticalIsland = false;
    [Range(0.1f, 1f)]
    public float gaussianSigmaX = 0.3f;
    [Range(0.1f, 1f)]
    public float gaussianSigmaY = 0.3f;
    [Range(0.5f, 2f)]
    public float gaussianAmplitude = 1f;
    [Range(0f, 90f)]
    public float islandRotation = 0f;

    [Header("Настройки диаграммы Вороного")]
    public bool useVoronoiDiagramm = false;
    public int gridSizeX = 3;
    public int gridSizeY = 2;
    #endregion
    
    public TerrainType[] regions;
    private float[,] falloffMap;
    private Texture2D test;

    private void Awake()
    {
        if (ellipticalIsland)
            falloffMap = FalloffMap.GenerateFalloffMap(
                width, height, gaussianSigmaX, gaussianSigmaY, islandRotation, gaussianAmplitude);
        else
            falloffMap = FalloffMap.GenerateFalloffMap(
                width, height, gaussianSigmaX, gaussianSigmaY, gaussianAmplitude);

    }

    public void GenerateMap()
    {
        int[] regionMap = ClassicVoronoi.RegionCoord(width, height, seed, gridSizeX, gridSizeY);

        float[,] noiseMap = NoiseExp.GenerateNoiseMap(width, height, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colorMap = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (useFalloff)
                {
                    // Гауссов falloff гарантирует, что в центре значение близко к 0
                    // Поэтому вычитаем его из высоты
                    float falloffValue = falloffMap[x, y];
                    float adjustedHeight = noiseMap[x, y] - falloffValue;

                    // Дополнительно усиливаем центр, если нужно
                    float centerBoost = CalculateCenterBoost(x, y, width, height);
                    adjustedHeight += centerBoost * 0.1f; // Небольшое усиление

                    noiseMap[x, y] = Mathf.Clamp01(adjustedHeight);
                }

                float currentHeight = noiseMap[x, y];

                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorMap[y * width + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        mapDis display = FindObjectOfType<mapDis>();
        test = textGen.TextureFromColor(colorMap, width, height);
        if (drawMode == DrawMode.NoiseMap)
            display.DrawTexture(textGen.TextureFromHight(noiseMap));
        else if (drawMode == DrawMode.ColorMap)
            display.DrawTexture(textGen.TextureFromColor(colorMap, width, height));
        
        else if (drawMode == DrawMode.FalloffMap)
            display.DrawTexture(textGen.TextureFromHight(FalloffMap.GenerateFalloffMap(width, height, gaussianSigmaX, gaussianSigmaY, islandRotation, gaussianAmplitude)));
        else if (drawMode == DrawMode.VoronoiMap)
            display.DrawTexture(ClassicVoronoi.GenerateVoronoiMap(width, height, seed, gridSizeX, gridSizeY));
        else if (drawMode == DrawMode.TestCombainMap)
            display.DrawTexture(CombainMaps.CombainMap(regionMap, test, width, height));
        
    }

    public void CombainMapTest()
    {
        int[] regionMap = ClassicVoronoi.RegionCoord(width, height, seed, gridSizeX, gridSizeY);

        //int[] CellFromVoronoi = CombainMaps.CellRegion(regionMap);



        

        float[,] noiseMap = NoiseExp.GenerateNoiseMap(width, height, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colorMap = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (useFalloff)
                {
                    // Гауссов falloff гарантирует, что в центре значение близко к 0
                    // Поэтому вычитаем его из высоты
                    float falloffValue = falloffMap[x, y];
                    float adjustedHeight = noiseMap[x, y] - falloffValue;

                    // Дополнительно усиливаем центр, если нужно
                    float centerBoost = CalculateCenterBoost(x, y, width, height);
                    adjustedHeight += centerBoost * 0.1f; // Небольшое усиление

                    noiseMap[x, y] = Mathf.Clamp01(adjustedHeight);
                }

                float currentHeight = noiseMap[x, y];

                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorMap[y * width + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        mapDis display = FindObjectOfType<mapDis>();
        test = textGen.TextureFromColor(colorMap, width, height);
        if (drawMode == DrawMode.NoiseMap)
            display.DrawTexture(textGen.TextureFromHight(noiseMap));
        else if (drawMode == DrawMode.ColorMap)
            display.DrawTexture(textGen.TextureFromColor(colorMap, width, height));

        else if (drawMode == DrawMode.FalloffMap)
            display.DrawTexture(textGen.TextureFromHight(FalloffMap.GenerateFalloffMap(width, height, gaussianSigmaX, gaussianSigmaY, islandRotation, gaussianAmplitude)));
        else if (drawMode == DrawMode.VoronoiMap)
            display.DrawTexture(ClassicVoronoi.GenerateVoronoiMap(width, height, seed, gridSizeX, gridSizeY));
        else if (drawMode == DrawMode.TestCombainMap)
            display.DrawTexture(CombainMaps.CombainMap(regionMap, test, width, height));

    }

    private float CalculateCenterBoost(int x, int y, int width, int height)
    {
        Vector2 center = new Vector2(width / 2f, height / 2f);
        float maxDist = Vector2.Distance(Vector2.zero, center);
        float dist = Vector2.Distance(new Vector2(x, y), center);

        return Mathf.Exp(-(dist * dist) / (2 * (maxDist * 0.3f) * (maxDist * 0.3f)));
    }

    private void OnValidate()
    {
        if (width < 1)
            width = 1;
        if (height < 1)
            height = 1;
        if (lacunarity < 1)
            lacunarity = 1;
        if (octaves < 0)
            octaves = 0;


        if (ellipticalIsland)
            falloffMap = FalloffMap.GenerateFalloffMap(
                width, height, gaussianSigmaX, gaussianSigmaY, islandRotation, gaussianAmplitude);
        else
            falloffMap = FalloffMap.GenerateFalloffMap(
                width, height, gaussianSigmaX, gaussianSigmaY, gaussianAmplitude);

    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}