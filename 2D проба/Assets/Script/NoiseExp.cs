using UnityEngine;

/// <summary>
/// Класс для создания шумовой карты высот.
/// </summary>
public static class NoiseExp
{
    /// <summary>
    /// Функция для создания цветной версии карты высот
    /// </summary>
    /// <param name="mapWidth">Ширина карты</param>
    /// <param name="mapHeight">Высота карты</param>
    /// <param name="seed">Зерно для случайной генерации</param>
    /// <param name="scale">Масштаб шума, чем меньше значение, тем больше деталей.</param>
    /// <param name="octaves">Количество октав шума. Говорят, что большее количество октав = детализацией, но у меня после 4 октав ничего не меняется</param>
    /// <param name="persistance">Коэффицент устойчивости, влияет на амплитуду каждой октавы (уменьшает детализацию)</param>
    /// <param name="lacunarity">Коэффицент лакунарности, влияет на частоту каждой октавы (тоже увеличивает детализацию)</param>
    /// <param name="offset">Смещение координат</param>
    /// <returns>Матрица со значением высоты для каждого пикселя</returns>
    /// <remarks>
    /// Класс полностью переписанный с видоса на ютубе https://www.youtube.com/watch?v=wbpMiKiSKm8&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3
    /// Были использованы серии с 1 по 4, так как дальше он стал переводить плоскость в 3D, что мне не надо было
    /// Также была использована 11 серия с картой падающих значений, но формула в его видео была так себе, и была выбрана Гауссовское распределение для этих целей
    /// По поводу октав, после 4 идёт изменение двух-трёх пикселей, может зависеть от масштаба и размера, но критически важного ничего не происходит, после ~25+ октав картинка ломается
    /// </remarks>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random random = new System.Random(seed);

        //Чтоб увеличить уникальность картинки, для каждой октавы выбирается случайным образом своё смещение
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            //Для каждой октавы генерируется случайное смещенеие, смещение больше чем 100000 может привести к артефактам
            float offsetX = random.Next(-100000, 100000) + offset.x;
            float offsetY = random.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        //Защита от деления на 0
        if (scale <= 0)
            scale = 0.0001f;

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;        //Амплитуда для первой октавы
                float frequency = 1;        //Частота для первой октавы
                float noiseHeight = 0;      //Накопленное значение шума для текущего пикселя

                for (int i = 0; i < octaves; i++)
                {
                    //Вычисляем и нормализуем координаты для выборки шума Перлина
                    float sampleX = (float)x / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (float)y / scale * frequency + octaveOffsets[i].y;

                    //Получаем значения шума Перлина, и переводим их к виду от -1 до 1
                    float perlinNoise = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinNoise * amplitude; //Добавляем вклад текущей октавы к общему значению шума

                    amplitude *= persistance;               //Уменьшаем амплитуду для слудющей октавы
                    frequency *= lacunarity;                //Увеличиваем частоту для следующей октавы
                }

                //Сохраняем максимальное и минимальное значение пикселя, чтоб позже нормализовать значения к диапозону от 0 до 1
                if (noiseHeight > maxNoiseHeight)
                    maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight)
                    minNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;
            }
        }

        //Нормализуем значение к диапозону от 0 до 1
        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);

        return noiseMap;
    }
}