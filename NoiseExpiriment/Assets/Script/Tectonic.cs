using System;
using UnityEngine;

/// <summary>
/// Класс для описания тектонических плит.
/// </summary>
/// <remarks>
/// Плиты создаются основываясь на уже созданной диаграмме Вороного
/// Берётся одна определённая ячейка, на её основе находим прямоугольник, в который полностью помещается ячейка
/// Внутри этого прямоугольника генерируется новая карта высот, располагается на глобальной карте в месте ячейки Вороного, а после обрезается по её форме.
/// Есть проблемы в динамичном изменении размеров прямоугольника, вмещающего ячейку, а также изменения значений центра. 
/// </remarks>
namespace Assets.Scripts
{
    public class Tectonic
    {
        public int id { get; set; }

        public int cellWidth;                           //Ширина ячейки
        public int cellHeight;                          //Длина ячейки

        public bool isOcean { get; set; }               //Пока не реализовал. По идеи даже бесполезный, я ориентируюсь на реальную карту тектонических плит, и там, если мы не считаем микро плиты, всего одна океаническая плита
                                                        //Всего 7 плит (15 с маленькими, которые я посчитал незначительными) из них 6 плит - с материками. Лишь одна плита (тихоокеанская) покрыта водой на 100%
        public Vector2 directionTectonic { get; set; }  //Вектор для определения направления движения плиты и её "скорости"
        public Vector2Int center { get; set; }          //Координаты центра плиты на глобальной карте

        public int minX;                                //Максимальные и минимальные координаты плит на глобальной карте, нужно будет перевести в Vector2
        public int minY;
        public int maxX;
        public int maxY;

        /// <summary>
        /// Конструктор инициализации класса
        /// </summary>
        /// <param name="i">Идентификатор тектонической плиты</param>
        /// <param name="region">Матрица, определяющая принадлежность определённой координаты к определённой ячейке Вороного</param>
        /// <param name="seed">Зерно генерации</param>
        /// <param name="center">Координата центра плиты на глобальной карте</param>
        public Tectonic(int i, int[,] region, int seed, Vector2Int center)
        {
            System.Random random = new System.Random(seed);

            id = i;
            this.center = center;
            directionTectonic = RandomUnitVector(random) * random.Next(1, 11);

            CellSize(region);
        }

        /// <summary>
        /// Функция определения вектора направления плиты и её скорости
        /// </summary>
        /// <param name="seed">Зерно генерации</param>
        /// <returns>Вектор направления тектонической плиты</returns>
        public static Vector2 RandomUnitVector(System.Random random)
        {
            float angle = (float)(random.NextDouble() * 2 * Math.PI);

            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);

            return new Vector2(x, y);
        }

        /// <summary>
        /// Функция расчёта взаимодействия плит
        /// </summary>
        /// <param name="firstPlate">Первая взаимодействующая плита</param>
        /// <param name="secondPlate">Вторая взаимодействующая плита</param>
        /// <returns>Значения взаимодействия двух тектонических плит. (>0 - плиты сталкиваются, =0 - плиты нейтральны, <0 - плиты расходятся)</returns>
        /// <remarks>Обычное скалярное произведение векторов. Над коэффицентами надо ещё подумать.</remarks>
        public static int InteractionTectonic(Tectonic firstPlate, Tectonic secondPlate)
        {
            float ScalarVector = Vector2.Dot(firstPlate.directionTectonic, secondPlate.directionTectonic);

            return (int)(ScalarVector* firstPlate.directionTectonic.magnitude * secondPlate.directionTectonic.magnitude) /10;
        }

        /// <summary>
        /// Функция поиска границ прямоугольника для определённой ячейки Вороного
        /// </summary>
        /// <param name="region">Матрица, определяющая принадлежность определённой координаты к определённой ячейке Вороного</param>
        private void CellSize(int[,] region)
        {
            int maxValueX = int.MinValue;
            int minValueX = int.MaxValue;
            int maxValueY = int.MinValue;
            int minValueY = int.MaxValue;

            for (int y = 0; y < region.GetLength(1); y++)
            {
                for (int x = 0; x < region.GetLength(0); x++)
                {
                    if (region[x, y] == id)
                    {
                        if (x > maxValueX) maxValueX = x;
                        if (x < minValueX) minValueX = x;
                        if (y > maxValueY) maxValueY = y;
                        if (y < minValueY) minValueY = y;
                    }
                }
            }

            minX = minValueX;
            minY = minValueY;
            maxX = maxValueX;
            maxY = maxValueY;

            cellWidth = maxX - minX + 1;
            cellHeight = maxY - minY + 1;
        }
    }
}