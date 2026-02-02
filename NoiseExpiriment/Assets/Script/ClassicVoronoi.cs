using Assets.Scripts;
using UnityEngine;

public static class ClassicVoronoi
{
    public static Texture2D GenerateVoronoiMap(int mapWidth, int mapHeight, int seed, int gridSizeX, int gridSizeY)
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
            float latitude = (float)y / (mapHeight - 1);
            float compression = GetLatitudeCompression(latitude);

            for (int x = 0; x < mapWidth; x++)
            {
                float minDist = float.MaxValue;
                int nearest = 0;

                for (int i = 0; i < tectonics.Length; i++)
                {
                    Vector2Int c = tectonics[i].center;

                    float centerLat = (float)c.y / (mapHeight - 1);
                    float centerCompression = GetLatitudeCompression(centerLat);

                    float dx = Mathf.Abs(x - c.x);
                    dx = Mathf.Min(dx, mapWidth - dx);

                    float dy = Mathf.Abs(y - c.y);

                    dx *= Mathf.Lerp(compression, centerCompression, 0.5f);

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

                //if (isBorder)
                    //DrawBorderTectonic(tectonics, pixels, i, r, other);
            }
        }

        voronoiMap.filterMode = FilterMode.Point;
        voronoiMap.wrapMode = TextureWrapMode.Repeat;
        voronoiMap.SetPixels(pixels);
        voronoiMap.Apply();
        return voronoiMap;
    }

    public static int[] RegionCoord(int mapWidth, int mapHeight, int seed, int gridSizeX, int gridSizeY)
    {
        int[] regionMap = new int[mapWidth * mapHeight];
        Vector2Int[] centers = new Vector2Int[gridSizeX * gridSizeY];

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

                centers[index++] = new Vector2Int(px, py);
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            float latitude = (float)y / (mapHeight - 1);
            float compression = GetLatitudeCompression(latitude);

            for (int x = 0; x < mapWidth; x++)
            {
                float minDist = float.MaxValue;
                int nearest = 0;

                for (int i = 0; i < centers.Length; i++)
                {
                    Vector2Int c = centers[i];

                    float centerLat = (float)c.y / (mapHeight - 1);
                    float centerCompression = GetLatitudeCompression(centerLat);

                    float dx = Mathf.Abs(x - c.x);
                    dx = Mathf.Min(dx, mapWidth - dx);

                    float dy = Mathf.Abs(y - c.y);

                    dx *= Mathf.Lerp(compression, centerCompression, 0.5f);

                    float d = dx * dx + dy * dy;

                    if (d < minDist)
                    {
                        minDist = d;
                        nearest = i;
                    }
                }

                regionMap[y * mapWidth + x] = nearest;
            }
        }

        return regionMap;
    }

    private static float GetLatitudeCompression(float latitude)
    {
        float lat = (latitude - 0.5f) * 2f;
        float c = Mathf.Cos(Mathf.Abs(lat) * Mathf.PI * 0.5f);
        return Mathf.Pow(c, 0.7f);
    }

    private static void DrawBorderTectonic(Tectonic[] tectonics, Color[] pixels, int pixelIndex, int nearestIndex, int secondNearestIndex)
    {
        int powerTectonic = Tectonic.InteractionTectonic(tectonics[nearestIndex], tectonics[secondNearestIndex]);

        if (powerTectonic > 0)
            pixels[pixelIndex] = Color.red;

        else if (powerTectonic == 0)
            pixels[pixelIndex] = Color.green;

        else
            pixels[pixelIndex] = Color.blue;
    }
}