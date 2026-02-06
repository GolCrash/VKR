using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Основной класс-генератор карт для процедурной генерации миров
/// Управляет созданием карт высот, диаграмм Вороного и их визуализацией
/// </summary>
/// <remarks>
/// Этот класс объединяет все компоненты генерации:
/// - Шум Перлина для создания высот
/// - Диаграмму Вороного для разделения на тектонические плиты
/// - Карты затухания для формирования островов/континентов
/// - Систему биомов для раскраски по высоте
/// Поддерживает несколько режимов отображения для отладки и демонстрации
/// </remarks>
public class mapGen : MonoBehaviour
{
    /// <summary>
    /// Режимы отображения доступные в генераторе карт
    /// </summary>
    public enum DrawMode
    {
        /// <summary>Отображает черно-белую карту высот</summary>
        NoiseMap,
        /// <summary>Отображает цветную карту высот, и использоланием биомов</summary>
        ColorMap,
        /// <summary>Отображает черно-белую карту затухания</summary>
        FalloffMap,
        /// <summary>Отображает карту регионов диаграммы Вороного</summary>
        VoronoiMap,
        /// <summary>Отображает комбинированную карту высот и диаграммы Вороного</summary>
        TestCombainMap
    };

    #region Параметры генерации
    [Header("Общие настройки")]
    /// <summary>Текущий режим отображения карты</summary>
    public DrawMode drawMode;
    /// <summary>Режим автообновления карты при изменения параметров в редакторе</summary>
    public bool autoUpdate;
    /// <summary>Ширина карты</summary>
    public int width;
    /// <summary>Высота карты</summary>
    public int height;
    /// <summary>Зерно для генерации случайных значений</summary>
    public int seed;

    [Header("Настройки шума")]
    /// <summary>Масштаб шума Перлина, меньшие значения = более детализированный шум</summary>
    public float noiseScale;
    /// <summary>Количество октав фрактального шума, каждая октава добавляет детализация</summary>
    public int octaves;
    /// <summary>Коэффициент стойкости, определяет влияние каждой следующей октавы</summary>
    [Range(0, 1)]
    public float persistance;
    /// <summary>Коэффициент лакунарности, определяет увеличение частоты каждой следующей октавы</summary>
    public float lacunarity;
    /// <summary>Смещение координат карты шума</summary>
    public Vector2 offset;

    [Header("Настройки гассовского распределения")]
    /// <summary>Режим использования карты затухания</summary>
    public bool useFalloff = false;
    /// <summary>Режим эллиптической карты затухания</summary>
    public bool ellipticalIsland = false;
    /// <summary>Стандартное отклонение гауссова распрделения по оси X</summary>
    [Range(0.1f, 1f)]
    public float gaussianSigmaX = 0.3f;
    /// <summary>Стандартное отклонение гауссова распрделения по оси Y</summary>
    [Range(0.1f, 1f)]
    public float gaussianSigmaY = 0.3f;
    /// <summary>Амплитуда гауссова распределения, влияющая на интенсивность затухания</summary>
    [Range(0.5f, 2f)]
    public float gaussianAmplitude = 1f;
    /// <summary>Угол поворота эллиптической карты в градусах</summary>
    [Range(0f, 90f)]
    public float islandRotation = 0f;

    [Header("Настройки диаграммы Вороного")]
    /// <summary>Режим использования карты диграммы Ворного</summary>
    public bool useVoronoiDiagramm = false;
    /// <summary>Размер сетки по X</summary>
    public int gridSizeX = 3;
    /// <summary>Размер сетки по Y</summary>
    public int gridSizeY = 2;
    #endregion

    #region UI компоненты

    //Пока не используются, только начинаю разбираться

    #endregion

    /// <summary>Массив типов биомов, определяющихся по высоте пикселя</summary>
    public TerrainType[] regions;
    /// <summary>Матрица значений карты затухания</summary>
    private float[,] falloffMap;
    /// <summary>Тестовая карта (не используется)</summary>
    private Texture2D test;
    /// <summary>Массив координат центров регионов Вороного</summary>
    private Vector2Int[] centersVoronoi;
    /// <summary>Список тектонических плит, соответствующих регионам Вороного</summary>
    private List<Tectonic> tectonicsPlate = new List<Tectonic>(100);

    /// <summary>
    /// Инициализация при создании объекта
    /// Генерирует карту затухания на основе текущих параметров
    /// </summary>
    private void Awake()
    {
        GenerateFalloffMap();
    }

    /// <summary>
    /// Основной метод генерации карты в соответствии с выбранным режимом отображения
    /// </summary>
    /// <remarks>
    /// Последовательность генерации:
    /// 1. Создание диаграммы Вороного и центров регионов
    /// 2. Инициализация тектонических плит
    /// 3. Генерация базовой карты шума
    /// 4. Отображение результата в выбранном режиме
    /// </remarks>
    public void GenerateMap()
    {
        centersVoronoi = new Vector2Int[gridSizeX * gridSizeY];     //Инициализация массива центров регионов Вороного
        
        //Генерацияы диаграммы Вороного и получение матрицы регионов
        int[,] regionMap = ClassicVoronoi.RegionCoord(width, height, seed, gridSizeX, gridSizeY, centersVoronoi);

        //Очистка и создание тектонических плит для каждого региона
        tectonicsPlate.Clear();
        for (int i = 0; i < gridSizeX * gridSizeY; i++)
            tectonicsPlate.Add(new Tectonic(i, regionMap, seed, centersVoronoi[i]));

        //Получение компонента отображения карты
        //mapDis display = FindObjectOfType<mapDis>();
        mapDis display = FindFirstObjectByType<mapDis>();

        //Генерация базовой карты шума
        float[,] noiseMap = NoiseExp.GenerateNoiseMap(
                width,
                height,
                seed,
                noiseScale,
                octaves,
                persistance,
                lacunarity,
                offset
            );

        if (drawMode == DrawMode.NoiseMap)
        {
            //Отображение карты высот
            display.DrawTexture(textGen.TextureFromHight(noiseMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            //Отображение раскрашенной карты высот
            Color[] colorMap = GenerateColorMap(noiseMap);
            display.DrawTexture(textGen.TextureFromColor(colorMap, width, height));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            //отображение карты затухания
            display.DrawTexture(textGen.TextureFromHight(FalloffMap.GenerateFalloffMap(
                width, height, gaussianSigmaX, gaussianSigmaY, islandRotation, gaussianAmplitude)));
        }
        else if (drawMode == DrawMode.VoronoiMap)
        {
            //Отображение диаграммы Вороного
            display.DrawTexture(textGen.TextureFromVoronoi(regionMap));
        }
        else if (drawMode == DrawMode.TestCombainMap)
        {
            //Отображение комбинированной карты с масками Вороного
            Texture2D combinedMap = CombainMaps.CombineWithVoronoiMasks(
                regionMap, tectonicsPlate, width, height, seed, noiseScale,
                octaves, persistance, lacunarity, offset, regions, useFalloff, falloffMap);

            display.DrawTexture(combinedMap);
        }
    }

    /// <summary>
    /// Функция генерации цветной карты для определённой плиты на основе карте шума
    /// </summary>
    /// <param name="noiseMap">Карту шума высот</param>
    /// <param name="plate">Тектоническая плита</param>
    /// <returns>
    /// Массив цветов для карты определённой плиты
    /// </returns>
    /// <remarks>
    /// В данной реализации не используется, нужна была лишь для дебага
    /// </remarks>
    private Color[] GenerateColorMapForPlate(float[,] noiseMap, Tectonic plate)
    {
        int width = plate.cellWidth;
        int height = plate.cellHeight;
        Color[] colorMap = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (useFalloff)
                {
                    float falloffValue = falloffMap[x, y];
                    float adjustedHeight = noiseMap[x, y] - falloffValue;
                    float centerBoost = CalculateCenterBoost(x, y, width, height);
                    adjustedHeight += centerBoost * 0.1f;
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

        return colorMap;
    }

    /// <summary>
    /// Функция генерации цветной карты для всей карты, не зависимо от тектонических плит
    /// </summary>
    /// <param name="noiseMap">Карту шума высот</param>
    /// <returns>
    /// Массив цветов для всей карты
    /// </returns>
    private Color[] GenerateColorMap(float[,] noiseMap)
    {
        Color[] colorMap = new Color[width * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (useFalloff)
                {
                    float falloffValue = falloffMap[x, y];                          //Получаем значение затухания из глобальной карты затухания
                    float adjustedHeight = noiseMap[x, y] - falloffValue;           //Вычитаем затухание
                    float centerBoost = CalculateCenterBoost(x, y, width, height);  //Добавляем усилиение к центру плиты, чтоб полностью быть уверенным в том, что в центре карты будет суша
                    adjustedHeight += centerBoost * 0.1f;
                    noiseMap[x, y] = Mathf.Clamp01(adjustedHeight);                  //Ограничиваем значения диапозоном [0, 1]
                }

                float currentHeight = noiseMap[x, y];

                //Определение цвета по высоте на основе биомов
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        Color regionColor = regions[i].color;
                        regionColor.a = 1f;
                        colorMap[y * width + x] = regionColor;
                        break;
                    }
                }
                /*
                if (0.37f < currentHeight && currentHeight <= 1f)
                {
                    colorMap[y * width + x] = new Color(0.4f, 0.8f, 0.4f); // Зелёный
                }
                else if (currentHeight <= 0.37f)
                {
                    colorMap[y * width + x] = new Color(0f, 0.4f, 0.8f); // Синий
                }*/
            }
        }

        return colorMap;
    }

    /// <summary>
    /// Вычисляет коэффициент усиления к центру области на основе гауссова распределения
    /// </summary>
    /// <param name="x">X-координата точки в локальной системе координат области</param>
    /// <param name="y">Y-координата точки в локальной системе координат области</param>
    /// <param name="width">Ширина области</param>
    /// <param name="height">Высота области</param>
    /// <returns>
    /// Значение от 0 до 1, где 1 - максимальное усиление в центре, экспоненциально убывающее к краям.
    /// </returns>
    /// <remarks>
    /// Используется, чтоб убедиться что в центре карты будет обязательно суша
    /// Формула: exp(-(distance²) / (2 * (0.3 * maxRadius)²))
    /// </remarks>
    private float CalculateCenterBoost(int x, int y, int width, int height)
    {
        Vector2 center = new Vector2(width / 2f, height / 2f);                          //Вычисляем центр области
        float maxDist = Vector2.Distance(Vector2.zero, center);                         //Вычисляем максимальное расстояние от центра до угла
        float dist = Vector2.Distance(new Vector2(x, y), center);                       //Вычисляем расстояние от текущей точки до центра
        return Mathf.Exp(-(dist * dist) / (2 * (maxDist * 0.3f) * (maxDist * 0.3f)));   //Вычисляем гауссово усиления по формуле
    }

    /// <summary>
    /// Генерирует карту затухания на основе текущих настроек
    /// </summary>
    /// <remarks>
    /// Вызывается при инициализации и при изменении параметров в редакторе.
    /// Вообще надо бы убрать переменную ellipticalIsland, так как её нужно включить по умолчанию всегда
    /// </remarks>
    private void GenerateFalloffMap()
    {
        if (ellipticalIsland)
            falloffMap = FalloffMap.GenerateFalloffMap(
                width, height, gaussianSigmaX, gaussianSigmaY, islandRotation, gaussianAmplitude);
        else
            falloffMap = FalloffMap.GenerateFalloffMap(
                width, height, gaussianSigmaX, gaussianSigmaY, gaussianAmplitude);
    }

    /// <summary>
    /// Метод валидации, вызываемый редактором Unity при изменении значений в инспекторе
    /// </summary>
    /// <remarks>
    /// Обеспечивает корректность параметров и перегенерирует карту затухания при необходимости
    /// </remarks>
    private void OnValidate()
    {
        //Валидация входных данных
        if (width < 1) width = 1;
        if (height < 1) height = 1;
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;

        GenerateFalloffMap();   //Перегенерация карты затухания при изменении параметров
    }
}

/// <summary>
/// Структура, описывающая тип местности (биом) для раскраски карты
/// </summary>
/// <remarks>
/// Определяет цвет для определенного диапазона высот
/// </remarks>
[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}