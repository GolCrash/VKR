using UnityEngine;

/// <summary>
/// Класс для создания и настройки текстур.
/// </summary>
/// <remarks>
/// Класс создан исключительно для создания текстур, хотя ещё некоторые текстуры я генерирую где-то внутри других классов
/// Надо будет полностью перенести сюда все подобные задачи с работой с Texture2D
/// </remarks>
public static class textGen
{
    /// <summary>
    /// Функция для создания цветной версии карты высот
    /// </summary>
    /// <param name="colorMap">Массив писклей с их значением цвета</param>
    /// <param name="width">Ширина создаваемой текстуры</param>
    /// <param name="height">Высота создаваемой текстуры</param>
    /// <returns>Цветную текстуру карты</returns>
    /// /// <remarks>
    /// Этот метод является также финальным для всех остальных методов в этом классе.
    /// </remarks>
    public static Texture2D TextureFromColor(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);

        texture.filterMode = FilterMode.Point;      //Устанавливаем режим фильтрации на Point, что предотвращает размытие пикселей при масштабировании
        texture.wrapMode = TextureWrapMode.Clamp;   //Устанавливаем режим обтекания текстуры, чтоб при обращения к координатам вне диапозона использовался крайний цвет текстуры
        texture.SetPixels(colorMap);                //Заполняем текстуры цветами
        texture.Apply();                            //Применяем все изменения к текстуре
        return texture;                     
    }

    /// <summary>
    /// Функция для создания чёрнобелой версии карты высот
    /// </summary>
    /// <param name="heightMap">Массив писклей с их значением высоты</param>
    /// <returns>Чёрнобелая карта высот</returns>
    public static Texture2D TextureFromHight(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);         //Возвращаем ширину матрицы
        int height = heightMap.GetLength(1);        //Возвращаем высоту матрицы

        Color[] colorMap = new Color[width * height];

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                //Выдаём оттенок серого, зависящий от значения высоты пикселя
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);

        return TextureFromColor(colorMap, width, height);
    }

    /// <summary>
    /// Функция для создания карты диаграммы Вороного
    /// </summary>
    /// <param name="regionMap">Массив писклей с их принадлежностью к определённым ячейкам Вороного</param>
    /// <returns>Цветную текстуру карты диаграммы Вороного</returns>
    public static Texture2D TextureFromVoronoi(int[,] regionMap)
    {
        int width = regionMap.GetLength(0);     
        int height = regionMap.GetLength(1);    

        Color[] colorMap = new Color[width * height];
        //Этот массив надо вообще убрать к чёрту, но мне лень, буду пока работать с 6 плитами для отладки
        Color[] colorRegion = new Color[] { Color.cyan, Color.gray, Color.yellow, Color.magenta, Color.red, Color.green };

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int i = 0; i < colorRegion.Length; i++)        
                    if (regionMap[x,y] == i)                        
                        colorMap[y * width + x] = colorRegion[i];

        return TextureFromColor(colorMap, width, height);
    }
}