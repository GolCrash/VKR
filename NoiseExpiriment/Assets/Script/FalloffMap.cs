using UnityEngine;

public static class FalloffMap
{
    public static float[,] GenerateFalloffMap(int width, int height, float sigmaMajor = 0.3f,
                                                              float sigmaMinor = 0.15f,
                                                              float angleDegrees = 0f,
                                                              float amplitude = 1f)
    {
        float[,] map = new float[width, height];

        Vector2 center = new Vector2(width / 2, height / 2);
        float maxRadius = Mathf.Sqrt(width * width + height * height) / 2f;

        float angle = angleDegrees * Mathf.Deg2Rad;
        float cosAngle = Mathf.Cos(angle);
        float sinAngle = Mathf.Sin(angle);

        for (int i = 0; i < height; i++) 
        {
            for (int j = 0; j < width; j++) 
            {
                float x = (j - center.x) / maxRadius;
                float y = (i - center.y) / maxRadius;

                float xRot = x * cosAngle - y * sinAngle;
                float yRot = x * sinAngle + y * cosAngle;

                float exponent = (xRot * xRot) / (2 * sigmaMajor * sigmaMajor)
                               + (yRot * yRot) / (2 * sigmaMinor * sigmaMinor);

                float gaussianValue = amplitude * Mathf.Exp(-exponent);

                map[j, i] = 1 - gaussianValue;

                //float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                //map[j, i] = Evaluate(value);
            }           
        }

        return map;
    }

    static float Evaluate(float value)
    {
        float a = 3f;
        float b = 2.2f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}