using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс для генерации карты высот для каждого региона диаграммы Вороного и объединение этих карт в одну
/// </summary>
public static class CombainMaps
{
    /// <summary>
    /// Функция построения диаграммы Вороного, разделяя карту на регионы и вычисляя их цетры
    /// </summary>
    /// <param name="region">Матрица принадлежности координат к региону Вороного</param>
    /// <param name="mapTexture">Исходная карта высот (уже цветная)</param>
    /// <param name="width">Ширина карты</param>
    /// <param name="height">Высота карты</param>
    /// <returns>Текстуру "вырезанной" в определённом участке, используя регион Воронога в качестве маски</returns>
    /// <remarks>
    /// Пока он ничего не делает, и возможно ничего делать не будет
    /// Создавался для тестов объединения
    /// Все его обязанности выполняет функция CombineWithVoronoiMasks
    /// </remarks>
    public static Texture2D CombainMap(int[,] region, Texture2D mapTexture, int width, int height)
    {
        Texture2D newMap = new Texture2D(width, height);
        Color[] newMapColor = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (region[x, y] == 2)
                    newMapColor[y * width + x] = mapTexture.GetPixel(x, y);
                else
                    newMapColor[y * width + x] = Color.white;
            }
        }

        newMap.filterMode = FilterMode.Point;
        newMap.wrapMode = TextureWrapMode.Repeat;
        newMap.SetPixels(newMapColor);
        newMap.Apply();
        return newMap;
    }

    /// <summary>
    /// Функция для объединения карт с масками Вороного
    /// </summary>
    /// <param name="regionMap">Матрица принадлежности каждого пикселя к определённому региону Вороного</param>
    /// <param name="tectonics">Список тектонических плит</param>
    /// <param name="width">Ширина глобальной карты</param>
    /// <param name="height">Высота глобальной карты</param>
    /// <param name="seed">Зерно для случайной генерации</param>
    /// <param name="noiseScale">Масштаб шума, чем меньше значение, тем больше деталей.</param>
    /// <param name="octaves">Количество октав шума. Говорят, что большее количество октав = детализацией, но у меня после 4 октав ничего не меняется</param>
    /// <param name="persistance">Коэффицент устойчивости, влияет на амплитуду каждой октавы (уменьшает детализацию)</param>
    /// <param name="lacunarity">Коэффицент лакунарности, влияет на частоту каждой октавы (тоже увеличивает детализацию)</param>
    /// <param name="offset">Смещение координат</param>
    /// <param name="regions">Биомы карты, зависящие от значения "высоты" пикселя</param>
    /// <param name="useFalloff">Использования карты падений значений</param>
    /// <param name="falloffMap">Матрица с картой падения значений</param>
    /// <returns>Конечную текстуру карты</returns>
    /// <remarks>
    /// Может быть позже переделаю, но пока работает
    /// Единственное, что меня здесь беспокоит - это то, что надо будет для каждой отдельной ячейки передавать свои параметры, в данном случаем у всех плит одно и тоже
    /// Нужно выделить общие параметры (пока могу выделить лишь размеры карты и зерно) и локальные, такие как настройки детализации шума
    /// </remarks>
    public static Texture2D CombineWithVoronoiMasks(int[,] regionMap, List<Tectonic> tectonics,
        int width, int height, int seed, float noiseScale, int octaves,
        float persistance, float lacunarity, Vector2 offset,
        TerrainType[] regions, bool useFalloff, float[,] falloffMap)
    {
        Texture2D result = new Texture2D(width, height);
        Color[] colors = new Color[width * height];

        //Проходим по всем плитам
        foreach (Tectonic plate in tectonics)
        {
            //Генерируем уникальный шум для каждой плиты
            float[,] plateNoise = NoiseExp.GenerateNoiseMap(
                plate.cellWidth,
                plate.cellHeight,
                seed + plate.id,        //Уникальный seed для каждой плиты
                noiseScale,
                octaves,
                persistance,
                lacunarity,
                offset
            );

            //Применяем falloff к шуму плиты
            if (useFalloff)
            {
                for (int y = 0; y < plate.cellHeight; y++)
                {
                    for (int x = 0; x < plate.cellWidth; x++)
                    {
                        if (x < width && y < height)
                        {
                            float falloffValue = falloffMap[x, y];                                              //Получаем значение затухания из глобальной карты затухания
                            float adjustedHeight = plateNoise[x, y] - falloffValue;                             //Вычитаем затухание
                            float centerBoost = CalculateCenterBoost(x, y, plate.cellWidth, plate.cellHeight);  //Добавляем усилиение к центру плиты, чтоб полностью быть уверенным в том, что в центре карты будет суша
                            adjustedHeight += centerBoost * 0.1f;
                            plateNoise[x, y] = Mathf.Clamp01(adjustedHeight);                                   //Ограничиваем значения диапозоном [0, 1]
                        }
                    }
                }
            }

            //Заполняем цвета для этой плиты
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (regionMap[x, y] == plate.id)
                    {
                        //Находим координаты в локальной системе плиты
                        int localX = x - plate.minX;
                        int localY = y - plate.minY;

                        if (localX >= 0 && localX < plate.cellWidth &&
                            localY >= 0 && localY < plate.cellHeight)
                        {
                            float heightValue = plateNoise[localX, localY];

                            //Определяем цвет по высоте
                            for (int i = 0; i < regions.Length; i++)
                            {
                                if (heightValue <= regions[i].height)
                                {
                                    colors[y * width + x] = regions[i].color;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        result.filterMode = FilterMode.Point;
        result.wrapMode = TextureWrapMode.Repeat;
        result.SetPixels(colors);
        result.Apply();
        return result;
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
    private static float CalculateCenterBoost(int x, int y, int width, int height)
    {
        Vector2 center = new Vector2(width / 2f, height / 2f);                          //Вычисляем центр области
        float maxDist = Vector2.Distance(Vector2.zero, center);                         //Вычисляем максимальное расстояние от центра до угла
        float dist = Vector2.Distance(new Vector2(x, y), center);                       //Вычисляем расстояние от текущей точки до центра
        return Mathf.Exp(-(dist * dist) / (2 * (maxDist * 0.3f) * (maxDist * 0.3f)));   //Вычисляем гауссово усиления по формуле
    }
}