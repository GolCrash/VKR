using UnityEngine;

/// <summary>
/// Класс для создания карты затухания на основе нормального (Гауссовского) распределения
/// </summary>
/// <remarks>
/// Основная суть - сделать так, чтоб в цетре карты была земля, а по краям вода
/// Можно использвать иные функции распределения, но результат лучше всего выходит именно у нормального распределения
/// </remarks>
public static class FalloffMap
{
    /// <summary>
    /// Функция для просчёта карты затухания
    /// </summary>
    /// <param name="width">Ширина генерируемой карты</param>
    /// <param name="height">Высота генерируемой карты</param>
    /// <param name="sigmaMajor">Стандартное отклонение гауссова распределения по главной оси (ось X) (по умолчанию 0.3f)</param>
    /// <param name="sigmaMinor">Стандартное отклонение гауссова распределения по второстепенной оси (ось Y) (по умолчанию 0.15f)</param>
    /// <param name="angleDegrees">Угол поворота эллиптического распределения в градусах (по умолчанию 0f)</param>
    /// <param name="amplitude">Амплитуда (максимальное значение) гауссова распределения (по умолчанию 1f)</param>
    /// <returns>Карта нормального распределения</returns>
    public static float[,] GenerateFalloffMap(int width, int height, float sigmaMajor = 0.3f, float sigmaMinor = 0.15f, float angleDegrees = 0f, float amplitude = 1f)
    {
        float[,] map = new float[width, height];

        Vector2 center = new Vector2(width / 2, height / 2);                    //Вычисляем цетр карты для нормализации высот
        float maxRadius = Mathf.Sqrt(width * width + height * height) / 2f;     //Максимальный радиус нужен для того, чтоб карта была независима от абсолютных размеров

        //Производим расчёты поворота распределения сразу
        float angle = angleDegrees * Mathf.Deg2Rad;
        float cosAngle = Mathf.Cos(angle);
        float sinAngle = Mathf.Sin(angle);

        for (int i = 0; i < height; i++) 
        {
            for (int j = 0; j < width; j++) 
            {
                //Нормализуем координаты
                float x = (j - center.x) / maxRadius;
                float y = (i - center.y) / maxRadius;

                //Применяем поворот координат на заданный угол
                float xRot = x * cosAngle - y * sinAngle;
                float yRot = x * sinAngle + y * cosAngle;

                //Вычисляем значение экспоненты для двумерного гауссова распрделения, формулы есть в интернете
                float exponent = (xRot * xRot) / (2 * sigmaMajor * sigmaMajor)
                               + (yRot * yRot) / (2 * sigmaMinor * sigmaMinor);

                //Вычисляем значение самой функции
                float gaussianValue = amplitude * Mathf.Exp(-exponent);

                map[j, i] = 1 - gaussianValue;  //Преобразуем гауссово распределение в карту, инвертируя её для удобства
            }           
        }

        return map;
    }
}