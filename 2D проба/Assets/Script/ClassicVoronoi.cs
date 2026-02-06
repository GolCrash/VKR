using Assets.Scripts;
using UnityEngine;

/// <summary>
/// Класс для создания диаграммы Вороного
/// </summary>
/// <remarks>
/// Класс требует более умной переделки, так как мои знания математики не позволяют полностью самостоятельно пытаться эту хуйню изменить
/// </remarks>
public static class ClassicVoronoi
{
    //Изначальный класс для генерации диаграммы Вороного, сейчас здесь как референс, по факту уже не нужен
    /* public static Texture2D GenerateVoronoiMap(int mapWidth, int mapHeight, int seed, int gridSizeX, int gridSizeY)
     {
         Texture2D voronoiMap = new Texture2D(mapWidth, mapHeight);
         Color[] pixels = new Color[mapWidth * mapHeight];

         int[] regionMap = new int[mapWidth * mapHeight];
         Color[] colorRegion = new Color[] {Color.cyan, Color.gray, Color.yellow, Color.magenta, Color.red, Color.green };
         Tectonic[]  tectonics = new Tectonic[gridSizeX * gridSizeY];

         System.Random random = new System.Random(seed);

         int gridCellX = mapWidth / gridSizeX;
         int gridCellY = mapHeight / gridSizeY;

         int index = 0;
         for (int gx = 0; gx < gridSizeX; gx++)
         {
             for (int gy = 0; gy < gridSizeY; gy++)
             {
                 int startX = gx * gridCellX;
                 int startY = gy * gridCellY;

                 int marginY = gridCellY / 4;

                 int px = startX + random.Next(1, gridCellX - 1);
                 int py = startY + random.Next(marginY, gridCellY - marginY);

                 px = (px + mapWidth) % mapWidth;

                 tectonics[index++] = new Tectonic(px, py, random);
             }
         }

         for (int y = 0; y < mapHeight; y++)
         {
             //float latitude = (float)y / (mapHeight - 1);
             //float compression = GetLatitudeCompression(latitude);

             for (int x = 0; x < mapWidth; x++)
             {
                 float minDist = float.MaxValue;
                 int nearest = 0;

                 for (int i = 0; i < tectonics.Length; i++)
                 {
                     Vector2Int c = tectonics[i].center;

                     //float centerLat = (float)c.y / (mapHeight - 1);
                     //float centerCompression = GetLatitudeCompression(centerLat);

                     float dx = Mathf.Abs(x - c.x);
                     dx = Mathf.Min(dx, mapWidth - dx);

                     float dy = Mathf.Abs(y - c.y);

                     //dx *= Mathf.Lerp(compression, centerCompression, 0.5f);

                     float d = dx * dx + dy * dy;

                     if (d < minDist)
                     {
                         minDist = d;
                         nearest = i;
                     }
                 }

                 regionMap[y * mapWidth + x] = nearest;
                 pixels[y * mapWidth + x] = colorRegion[nearest];
             }
         }

         for (int y = 0; y < mapHeight; y++)
         {
             for (int x = 0; x < mapWidth; x++)
             {
                 int i = y * mapWidth + x;
                 int r = regionMap[i];

                 bool isBorder = false;
                 int other = r;

                 int xl = (x - 1 + mapWidth) % mapWidth;
                 int xr = (x + 1) % mapWidth;

                 if (regionMap[y * mapWidth + xl] != r)
                 {
                     isBorder = true;
                     other = regionMap[y * mapWidth + xl];
                 }
                 else if (regionMap[y * mapWidth + xr] != r)
                 {
                     isBorder = true;
                     other = regionMap[y * mapWidth + xr];
                 }

                 if (!isBorder && y > 0 && regionMap[(y - 1) * mapWidth + x] != r)
                 {
                     isBorder = true;
                     other = regionMap[(y - 1) * mapWidth + x];
                 }
                 else if (!isBorder && y < mapHeight - 1 && regionMap[(y + 1) * mapWidth + x] != r)
                 {
                     isBorder = true;
                     other = regionMap[(y + 1) * mapWidth + x];
                 }

                 if (isBorder)
                     DrawBorderTectonic(tectonics, pixels, i, r, other);
             }
         }

         voronoiMap.filterMode = FilterMode.Point;
         voronoiMap.wrapMode = TextureWrapMode.Repeat;
         voronoiMap.SetPixels(pixels);
         voronoiMap.Apply();
         return voronoiMap;
     }*/

    /// <summary>
    /// Функция построения диаграммы Вороного, разделяя карту на регионы и вычисляя их цетры
    /// </summary>
    /// <param name="mapWidth">Ширина карты</param>
    /// <param name="mapHeight">Высота карты</param>
    /// <param name="seed">Зерно для случайной генерации</param>
    /// <param name="gridSizeX">Размер сетки по оси X</param>
    /// <param name="gridSizeY">Размер сетки по оси Y</param>
    /// <param name="centers">Массив с координатами центра на глобальной карте. Нужен лишь для того, чтоб передать эти значения в класс Tectonic</param>
    /// <returns>Матрица принадлежности каждого пикселя к ячейки Вороного</returns>
    /// <remarks>
    /// По поводу сетки, нужна для равномерного распределения точек центра, есть проблемы:
    /// Сложно выбрать определённое количество материков: если кол-во плит = простому числу, то мы не сможем это реализовать
    /// Нужно всегда вводить два числа для этой сетки
    /// Нужно будет добавить или метод определения этой сетки по одному числу (что я выбирал именно количество плит, а не определял вручную размер сетки)
    /// </remarks>
    public static int[,] RegionCoord(int mapWidth, int mapHeight, int seed, int gridSizeX, int gridSizeY, Vector2Int[] centers)
    {
        int[,] regionMap = new int[mapWidth, mapHeight];

        System.Random random = new System.Random(seed);

        //Вычисляем размеры сетки в пикселях
        int gridCellX = mapWidth / gridSizeX;
        int gridCellY = mapHeight / gridSizeY;

        int index = 0;  //Индекс для заполнения массива центров

        //Проходим по всем ячейкам сетки
        for (int gx = 0; gx < gridSizeX; gx++)
        {
            for (int gy = 0; gy < gridSizeY; gy++)
            {
                //Вычисяем начальные координаты текущей ячейки сетки
                int startX = gx * gridCellX;
                int startY = gy * gridCellY;

                //Создаём небольшой отступ по вертикали, чтоб избежать слишком близкого расположения центров от границ ячеек
                //Надо бы поизменять коэффиценты и посмотреть как будет изменятся диаграмма
                int marginY = gridCellY / 4;

                //Генерируем случайные координаты для центра внутри ячейки сетки
                int px = startX + random.Next(1, gridCellX - 1);
                int py = startY + random.Next(marginY, gridCellY - marginY);

                //Так как делаем карту мира, нужно добиться торичности (зацикливания) карты по горизонтали (уйти на запад - прийти с востока и наоборот)
                px = (px + mapWidth) % mapWidth;

                //Сохраняем цетры региона в массив
                centers[index++] = new Vector2Int(px, py);
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            //Закоментированный код для широтного сжатия (нужно для создания искажения подобного как на обычных картах (где гренладния соразмерна африки и т.п.))
            //Также задумывалось, что это расстянет вверхние и нижнии плиты до полюсов, что позволит избежать проблем не состыковок границ на полюсах
            //float latitude = (float)y / (mapHeight - 1);
            //float compression = GetLatitudeCompression(latitude);

            for (int x = 0; x < mapWidth; x++)
            {
                float minDist = float.MaxValue;
                int nearest = 0;

                for (int i = 0; i < centers.Length; i++)
                {
                    Vector2Int c = centers[i];

                    //Относиться к коду для широтного сжатия
                    //float centerLat = (float)c.y / (mapHeight - 1);
                    //float centerCompression = GetLatitudeCompression(centerLat);

                    //Карта у нас зацикленная, поэтому вычисляем расстояние по X с учётом торичности карты
                    float dx = Mathf.Abs(x - c.x);
                    dx = Mathf.Min(dx, mapWidth - dx);

                    //Вычисляем расстояние по Y
                    float dy = Mathf.Abs(y - c.y);

                    //Относиться к коду для широтного сжатия, применяем широтное сжатие
                    //dx *= Mathf.Lerp(compression, centerCompression, 0.5f);

                    //Вычислеям квадрат расстояния
                    float d = dx * dx + dy * dy;

                    //Поиск более близкого центра
                    if (d < minDist)
                    {
                        minDist = d;
                        nearest = i;
                    }
                }

                //Присваем пикселю индекс ближайшего региона
                regionMap[x, y] = nearest;
            }
        }

        return regionMap;
    }

    /// <summary>
    /// Функция вычисляет коэффицент сжатия для широт
    /// </summary>
    /// <param name="latitude">Широта в диапозона от 0 до 1, где 0 - южный, а 1 - северный полюса</param>
    /// <returns>Коэффицент сжатия. На экваторе равен 1, по полюсам приближено к 0</returns>
    private static float GetLatitudeCompression(float latitude)
    {
        //Нормализуем широту из [0, 1] в [-1, 1]
        float lat = (latitude - 0.5f) * 2f;
        float c = Mathf.Cos(Mathf.Abs(lat) * Mathf.PI * 0.5f); //Формула по которой и получается на экваторе 1, а на полюсах 0
        return Mathf.Pow(c, 0.7f);  //Степень в 0.7f можно изменять для получения другого результата сжатия
    }

    /// <summary>
    /// Функция определения границ и тип взаимодействия двух плит (устарел)
    /// </summary>
    /// <param name="tectonics">Массив плит (устарел)</param>
    /// <param name="pixels">Массив цветов пикселей</param>
    /// <param name="pixelIndex">Индекс текущего пикселя</param>
    /// <param name="nearestIndex">Индекс первой ближайшей плиты</param>
    /// <param name="secondNearestIndex">Индекс второй ближайшей плиты</param>
    /// <returns>Значение цвета для граничещего пикселя</returns>
    /// <remarks>
    /// Вызывался сугубо для пикселей расположенних на границе
    /// Требует калибровки, и может даже переноса в шейдеры
    /// Пока не используется и устарел на данный момент
    /// </remarks>
    private static void DrawBorderTectonic(Tectonic[] tectonics, Color[] pixels, int pixelIndex, int nearestIndex, int secondNearestIndex)
    {
        //Вызываем функцию класса Tectonic для определения силы взаимодействия двух граничащих плит
        int powerTectonic = Tectonic.InteractionTectonic(tectonics[nearestIndex], tectonics[secondNearestIndex]);

        if (powerTectonic > 0)
            pixels[pixelIndex] = Color.red;     //Конвергенция плит (схождение плит друг на друга)

        else if (powerTectonic == 0)
            pixels[pixelIndex] = Color.green;   //Трансформ плит (плиты перемащаются параллельно друг другу или их взаимодействия крайне мало)

        else
            pixels[pixelIndex] = Color.blue;    //Дивергенция плит (плиты расходятся друг от друга)
    }
}