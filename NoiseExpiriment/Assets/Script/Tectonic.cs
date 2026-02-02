using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Tectonic
    {
        public int id { get; set; }

        public int cellWidth;
        public int cellHeight;

        public int[] localCoord { get; set; }
        public int[] globalCoord { get; set; }

        public int speedTectonic { get; set; }
        public bool isOcean { get; set; }
        public Vector2 directionTectonic { get; set; }
        public Vector2Int center { get; set; }


        public Tectonic(int pointX, int pointY, System.Random random) 
        {
            speedTectonic = random.Next(1,11);
            directionTectonic = new Vector2(
                (float)random.NextDouble() * 2f - 1f,
                (float)random.NextDouble() * 2f - 1f

            ).normalized;

            center = new Vector2Int(pointX, pointY);
        }

        public static int InteractionTectonic(Tectonic firstPlate, Tectonic secondPlate)
        {
            float ScalarVector = Vector2.Dot(firstPlate.directionTectonic, secondPlate.directionTectonic);

            return (int)(ScalarVector* firstPlate.speedTectonic * secondPlate.speedTectonic)/10;
        }

        private int CellWidht(int[] region, int width, int height, int id)
        {
            int maxValueX = int.MinValue;
            int minValueX = int.MaxValue;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (region[y * width + x] == id)
                    {
                        if (x > maxValueX)
                            maxValueX = x;
                        if (x < minValueX)
                            minValueX = x;
                    }
                }
            }

            cellWidth = maxValueX - minValueX + 1;

            return maxValueX - minValueX + 1;
        }

        private static int CellHeiht(int[] region, int width, int height, int id)
        {
            int maxValueY = int.MinValue;
            int minValueY = int.MaxValue;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (region[y * width + x] == id)
                    {
                        if (y > maxValueY)
                            maxValueY = y;
                        if (y < minValueY)
                            minValueY = y;
                    }
                }
            }

            return maxValueY - minValueY + 1;
        }
    }
}