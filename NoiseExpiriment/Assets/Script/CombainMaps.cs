using UnityEngine;

public static class CombainMaps
{
    public static Texture2D CombainMap(int[] region, Texture2D mapTexture, int width, int height)
    {
        Texture2D newMap = new Texture2D(width, height);
        Color[] newMapColor = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (region[y * width + x] == 2)
                    newMapColor[y * width + x] = mapTexture.GetPixel(x,y);
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
    /*
    public static int[] CellRegion(int[] region, int width, int height, int numberCell = 2)
    {
        Vector2[] cell;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (region[y * width + x] == numberCell)
                    cell[y * width + x] = mapTexture.GetPixel(x, y);
                else
                    newMapColor[y * width + x] = Color.white;
            }
        }

        return cell;
    }
    */
    
}
