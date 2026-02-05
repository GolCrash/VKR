using System.Linq;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.IO;
using Object = UnityEngine.Object;
using System.Text;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

/// <summary>
/// Вспомогательный статический класс, содержащий полезные утилиты для всего проекта
/// </summary>
public static class TUH
{
    /// <summary>
    /// Находит усреднённую точку (центроид) среди всех точек в массиве Vector3.
    /// </summary>
    /// <param name="points">Массив точек типа Vector3.</param>
    /// <returns>Усреднённая точка. Если массив пустой или null, возвращается Vector3.zero.</returns>
    public static Vector3 GetAveragePoint(Vector3[] points)
    {
        if (points == null || points.Length == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;

        foreach (Vector3 point in points)
        {
            sum += point;
        }

        return sum / points.Length;
    }

    /// <summary>
    /// Проверяет, находятся ли указанные координаты внутри границ матрицы
    /// </summary>
    /// <typeparam name="T">Тип элементов матрицы</typeparam>
    /// <param name="matrix">Двумерная матрица</param>
    /// <param name="x">Координата X (столбец)</param>
    /// <param name="y">Координата Y (строка)</param>
    /// <returns>True, если координаты находятся внутри матрицы, иначе false</returns>
    public static bool AreCoordinatesValid<T>(T[,] matrix, int x, int y)
    {
        // Проверяем, что матрица не null
        if (matrix == null)
            return false;

        // Проверяем границы координат
        return x >= 0 && x < matrix.GetLength(1) &&  // ширина (столбцы)
               y >= 0 && y < matrix.GetLength(0);    // высота (строки)
    }

    /// <summary>
    /// Загружает текстовый файл из папки Resources и возвращает его содержимое.
    /// </summary>
    /// <param name="filePath">Путь к файлу относительно папки Resources (без расширения .txt)</param>
    /// <returns>Содержимое файла в виде строки, или null, если файл не найден</returns>
    public static string LoadTextFile(string filePath)
    {
        // Загружаем текстовый файл как TextAsset
        TextAsset textAsset = Resources.Load<TextAsset>(filePath);

        // Проверяем, удалось ли загрузить файл
        if (textAsset != null)
        {
            return textAsset.text;
        }
        else
        {
            Debug.LogError($"Файл не найден: {filePath}");
            return null;
        }
    }

    /// <summary>
    /// Возвращает массив случайных точек на одной случайной грани меша с нормальным распределением.
    /// </summary>
    /// <param name="mesh">Меш, на котором нужно найти точки.</param>
    /// <param name="pointCount">Количество точек для генерации.</param>
    /// <param name="distributionFactor">Фактор нормального распределения (чем больше значение, тем точки ближе к центру треугольника). Рекомендуемое значение: 2-10.</param>
    /// <returns>Массив случайных точек на поверхности одной грани меша относительно его центра.</returns>
    public static Vector3[] GetRandomPointsOnSingleMeshFace(Mesh mesh, int pointCount, float distributionFactor = 5f)
    {
        if (mesh == null)
        {
            Debug.LogError("Mesh is null!");
            return new Vector3[0];
        }

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        if (triangles.Length < 3)
        {
            Debug.LogError("Mesh has less than 3 indices (not enough for a triangle).");
            return new Vector3[0];
        }

        // Выбираем случайный треугольник
        int randomTriangleIndex = Random.Range(0, triangles.Length / 3) * 3;

        Vector3 v1 = vertices[triangles[randomTriangleIndex]];
        Vector3 v2 = vertices[triangles[randomTriangleIndex + 1]];
        Vector3 v3 = vertices[triangles[randomTriangleIndex + 2]];

        // Вычисляем центр треугольника
        Vector3 triangleCenter = (v1 + v2 + v3) / 3f;

        // Вычисляем центр меша
        Vector3 meshCenter = Vector3.zero;
        foreach (Vector3 v in vertices)
        {
            meshCenter += v;
        }
        meshCenter /= vertices.Length;

        Vector3[] points = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            // Генерируем точки с нормальным распределением вокруг центра треугольника
            Vector3 pointOnTriangle = GetPointWithNormalDistribution(v1, v2, v3, triangleCenter, distributionFactor);

            // Возвращаем точку относительно центра меша
            points[i] = pointOnTriangle - meshCenter;
        }

        return points;
    }

    private static Vector3 GetPointWithNormalDistribution(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 center, float distributionFactor)
    {
        // Генерируем случайные значения с нормальным распределением
        // Используем Box-Muller transform для генерации нормально распределенных значений
        float u1 = Random.Range(0f, 1f);
        float u2 = Random.Range(0f, 1f);

        // Избегаем логарифма от нуля
        if (u1 == 0f) u1 = 0.0001f;

        float randNormal = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);

        // Нормализуем значение в диапазон [0, 1]
        float normalizedRand = Mathf.Clamp01(0.5f + randNormal / distributionFactor);

        // Интерполируем между центром и случайной точкой на треугольнике
        float r1 = Random.Range(0f, 1f);
        float r2 = Random.Range(0f, 1f);

        if (r1 + r2 > 1)
        {
            r1 = 1 - r1;
            r2 = 1 - r2;
        }

        float r3 = 1 - r1 - r2;

        // Случайная точка на треугольнике
        Vector3 randomPoint = r1 * v1 + r2 * v2 + r3 * v3;

        // Интерполируем между центром и случайной точкой в зависимости от нормального распределения
        return Vector3.Lerp(randomPoint, center, normalizedRand);
    }

    /// <summary>
    /// Возвращает случайную точку на поверхности меша, относительно центра меша.
    /// </summary>
    /// <param name="mesh">Меш, на котором нужно найти точку.</param>
    /// <returns>Случайная точка на поверхности меша относительно его центра.</returns>
    public static Vector3 GetRandomPointOnMeshSurface(Mesh mesh)
    {
        if (mesh == null)
        {
            Debug.LogError("Mesh is null!");
            return Vector3.zero;
        }

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        if (triangles.Length < 3)
        {
            Debug.LogError("Mesh has less than 3 indices (not enough for a triangle).");
            return Vector3.zero;
        }

        // Выбираем случайный треугольник (индекс в массиве индексов, кратный 3)
        int randomTriangleIndex = Random.Range(0, triangles.Length / 3) * 3;

        Vector3 v1 = vertices[triangles[randomTriangleIndex]];
        Vector3 v2 = vertices[triangles[randomTriangleIndex + 1]];
        Vector3 v3 = vertices[triangles[randomTriangleIndex + 2]];

        // Генерируем случайные барицентрические координаты
        float r1 = Random.Range(0f, 1f);
        float r2 = Random.Range(0f, 1f);

        if (r1 + r2 > 1)
        {
            r1 = 1 - r1;
            r2 = 1 - r2;
        }

        float r3 = 1 - r1 - r2;

        // Вычисляем точку на треугольнике
        Vector3 pointOnTriangle = r1 * v1 + r2 * v2 + r3 * v3;

        // Центр меша можно приближенно взять как среднее арифметическое всех вершин
        Vector3 meshCenter = Vector3.zero;
        foreach (Vector3 v in vertices)
        {
            meshCenter += v;
        }
        meshCenter /= vertices.Length;

        // Возвращаем точку относительно центра меша
        return pointOnTriangle - meshCenter;
    }

    /// <summary>
    /// Загружает спрайты из SpriteSheet, расположенного в Resources.
    /// </summary>
    /// <param name="spriteSheetPath">Путь к текстуре в папке Resources (без расширения файла).</param>
    /// <returns>Массив спрайтов из SpriteSheet.</returns>
    public static Sprite[] LoadSpritesFromSheet(string spriteSheetPath)
    {
        // Загружаем текстуру из Resources
        Texture2D texture = Resources.Load<Texture2D>(spriteSheetPath);

        if (texture == null)
        {
            Debug.LogError($"Не удалось загрузить текстуру по пути: {spriteSheetPath}");
            return new Sprite[0];
        }

        // Используем Sprite.Create для извлечения всех спрайтов
        // Unity не предоставляет прямого способа автоматического разбиения,
        // поэтому если у вас SpriteSheet с фиксированными размерами спрайтов —
        // можно использовать этот метод с указанием rect'ов.
        // Но если вы уже подготовили SpriteSheet и он содержит метаданные (например, через Sprite Editor),
        // то можно использовать Resources.LoadAll<Sprite>().

        // Если вы подготовили SpriteSheet в редакторе и указали тип текстуры как Sprite (2D and UI),
        // и разбили на спрайты, то можно использовать следующее:

        Sprite[] sprites = Resources.LoadAll<Sprite>(spriteSheetPath);

        if (sprites.Length == 0)
        {
            Debug.LogWarning($"Спрайты не найдены в: {spriteSheetPath}. Убедитесь, что текстура настроена как Sprite и разбита на спрайты.");
        }

        return sprites;
    }

    /// <summary>
    /// Загружает Aseprite файл и возвращает массив спрайтов
    /// </summary>
    /// <param name="assetPath">Путь к файлу в папке Resources (без расширения .aseprite)</param>
    /// <returns>Массив спрайтов из Aseprite файла</returns>
    public static Sprite[] LoadAsepriteAsSprites(string assetPath)
    {
        // Загружаем текстурный атлас, созданный Aseprite Importer'ом
        // Обычно Aseprite Importer создает файл с суффиксом "_Atlas" или подобным
        Texture2D atlas = Resources.Load<Texture2D>(assetPath);

        if (atlas == null)
        {
            Debug.LogError($"Не удалось найти атлас текстур по пути: {assetPath}");
            return new Sprite[0];
        }

        // Альтернативный способ - загрузка самого ассета как Sprite
        Sprite[] sprites = Resources.LoadAll<Sprite>(assetPath);

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError($"Не удалось загрузить спрайты по пути: {assetPath}");
            return new Sprite[0];
        }

        return sprites;
    }

    /// <summary>
    /// Выводит в консоль иерархию дочерних элементов указанного GameObject.
    /// </summary>
    /// <param name="root">Корневой GameObject, с которого начинается вывод иерархии.</param>
    public static void PrintHierarchy(GameObject root)
    {
        if (root == null)
        {
            Debug.LogError("Переданный GameObject равен null.");
            return;
        }

        StringBuilder hierarchyBuilder = new StringBuilder();
        PrintHierarchyRecursive(root, 0, hierarchyBuilder);
        Debug.Log(hierarchyBuilder.ToString());
    }

    /// <summary>
    /// Рекурсивно формирует строку с иерархией объектов.
    /// </summary>
    /// <param name="obj">Текущий GameObject.</param>
    /// <param name="level">Уровень вложенности (глубина).</param>
    /// <param name="builder">StringBuilder для накопления результата.</param>
    private static void PrintHierarchyRecursive(GameObject obj, int level, StringBuilder builder)
    {
        // Создаем отступ в зависимости от уровня вложенности
        string indent = new string(' ', level * 2); // 2 пробела на уровень

        // Добавляем имя объекта с отступом
        builder.AppendLine($"{indent}- {obj.name}");

        // Рекурсивно обрабатываем всех детей
        foreach (Transform child in obj.transform)
        {
            PrintHierarchyRecursive(child.gameObject, level + 1, builder);
        }
    }

    /// <summary>
    /// Рекурсивно устанавливает глобальную координату Z для всех объектов в иерархии.
    /// Родительский объект получает Z = 0, его дочерние объекты — Z = -1, внуки — Z = -2 и так далее.
    /// </summary>
    /// <param name="transform">Трансформ объекта, с которого начинается обход (обычно корневой объект).</param>
    /// <param name="depth">Текущая глубина вложенности. По умолчанию 0 (используется при рекурсивных вызовах).</param>
    public static void SetHierarchyZ(Transform transform, int depth = 0)
    {
        // Вычисляем значение Z на основе глубины
        Vector3 position = transform.position;
        position.z = -depth;
        transform.position = position;

        // Рекурсивно обрабатываем всех детей
        foreach (Transform child in transform)
        {
            SetHierarchyZ(child, depth + 1);
        }
    }

    /// <summary>
    /// Находит идентификатор ссылки, которая окружает символ по указанному индексу в строке.
    /// Индекс считается без учета тегов (как в визуальном тексте).
    /// </summary>
    /// <param name="input">Строка, содержащая текст и ссылки в формате <link="id">текст</link></param>
    /// <param name="index">Индекс символа в строке без учета тегов</param>
    /// <returns>Идентификатор ссылки, если символ находится внутри ссылки, иначе null</returns>
    public static string GetLinkAtStrippedPosition(string input, int index)
    {
        if (string.IsNullOrEmpty(input) || index < 0)
            return null;

        int visibleCharCount = 0;
        int i = 0;

        while (i < input.Length)
        {
            // Проверяем начало тега ссылки
            if (i <= input.Length - 6 && i + 5 < input.Length &&
                input.Substring(i, Math.Min(6, input.Length - i)).Equals("<link=", StringComparison.OrdinalIgnoreCase))
            {
                // Находим конец открывающего тега
                int openTagEnd = input.IndexOf('>', i);
                if (openTagEnd == -1)
                {
                    i++;
                    continue;
                }

                // Извлекаем ID ссылки
                string tagContent = input.Substring(i, openTagEnd - i + 1);
                int firstQuote = tagContent.IndexOf('"');
                string linkId = null;
                if (firstQuote != -1)
                {
                    int secondQuote = tagContent.IndexOf('"', firstQuote + 1);
                    if (secondQuote != -1)
                    {
                        linkId = tagContent.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
                    }
                }

                // Находим конец ссылки
                int closeTagStart = input.IndexOf("</link>", openTagEnd, StringComparison.OrdinalIgnoreCase);
                if (closeTagStart == -1) closeTagStart = input.Length;

                // Подсчитываем символы внутри ссылки
                int charsInLink = 0;

                // Сначала подсчитываем общее количество символов в ссылке
                for (int j = openTagEnd + 1; j < closeTagStart; j++)
                {
                    if (input[j] == '<')
                    {
                        // Пропускаем вложенные теги
                        int endOfNestedTag = input.IndexOf('>', j);
                        if (endOfNestedTag != -1)
                            j = endOfNestedTag;
                    }
                    else
                    {
                        charsInLink++;
                    }
                }

                // Проверяем, попадает ли искомый индекс в диапазон ссылки
                if (index >= visibleCharCount && index < visibleCharCount + charsInLink)
                {
                    // Теперь находим конкретный символ
                    int currentCharInLink = 0;
                    for (int j = openTagEnd + 1; j < closeTagStart; j++)
                    {
                        if (input[j] == '<')
                        {
                            // Пропускаем вложенные теги
                            int endOfNestedTag = input.IndexOf('>', j);
                            if (endOfNestedTag != -1)
                                j = endOfNestedTag;
                        }
                        else
                        {
                            if (visibleCharCount + currentCharInLink == index)
                            {
                                return linkId;
                            }
                            currentCharInLink++;
                        }
                    }
                }

                visibleCharCount += charsInLink;
                i = closeTagStart + 7; // Длина "</link>"
            }
            else if (input[i] == '<')
            {
                // Пропускаем другие теги
                int tagEnd = input.IndexOf('>', i);
                if (tagEnd != -1)
                    i = tagEnd + 1;
                else
                    i++;
            }
            else
            {
                // Обычный символ
                if (visibleCharCount == index)
                {
                    return null; // Символ не в ссылке
                }
                visibleCharCount++;
                i++;
            }
        }

        return null;
    }

    /// <summary>
    /// Преобразует индекс символа в оригинальной строке с тегами 
    /// в соответствующий индекс в строке без тегов (внутри угловых скобок).
    /// </summary>
    /// <param name="input">Оригинальная строка, содержащая текст и теги в формате <...></param>
    /// <param name="originalIndex">Индекс символа в оригинальной строке с тегами</param>
    /// <returns>Индекс символа в строке без тегов. Возвращает -1, если индекс некорректен или указывает на тег.</returns>
    public static int GetStrippedIndexFromOriginalIndex(string input, int originalIndex)
    {
        if (string.IsNullOrEmpty(input) || originalIndex < 0 || originalIndex >= input.Length)
            return -1;

        int strippedIndex = 0;

        for (int i = 0; i <= originalIndex; i++)
        {
            // Если символ находится внутри тега, возвращаем -1
            if (input[i] == '<')
            {
                // Проверяем, не находится ли искомый индекс внутри этого тега
                int tagStart = i;
                while (i < input.Length && input[i] != '>')
                    i++;

                // Если искомый индекс находится внутри тега
                if (originalIndex <= i && originalIndex >= tagStart)
                    return -1;

                if (i < input.Length && input[i] == '>')
                    i--; // Корректируем индекс для продолжения цикла
                continue;
            }

            // Если мы дошли до искомого символа
            if (i == originalIndex)
            {
                return strippedIndex;
            }

            strippedIndex++;
        }

        return -1;
    }

    /// <summary>
    /// Преобразует индекс символа в строке без тегов (внутри угловых скобок) 
    /// в соответствующий индекс в оригинальной строке с тегами.
    /// </summary>
    /// <param name="input">Оригинальная строка, содержащая текст и теги в формате <...></param>
    /// <param name="indexWithoutTags">Индекс символа в строке без тегов</param>
    /// <returns>Индекс символа в оригинальной строке с тегами. Возвращает -1, если индекс некорректен.</returns>
    public static int GetOriginalIndexFromTagStrippedIndex(string input, int indexWithoutTags)
    {
        if (string.IsNullOrEmpty(input) || indexWithoutTags < 0)
            return -1;

        int originalIndex = 0;
        int strippedIndex = 0;

        for (int i = 0; i < input.Length; i++)
        {
            // Пропускаем символы внутри тегов <...>
            if (input[i] == '<')
            {
                // Пропускаем всё до закрывающей скобки
                while (i < input.Length && input[i] != '>')
                    i++;
                if (i < input.Length) // Убедимся, что мы не вышли за пределы строки
                    originalIndex = i; // Включаем символ '>'
                continue;
            }

            // Если дошли до нужного символа в "очищенной" строке
            if (strippedIndex == indexWithoutTags)
            {
                return i;
            }

            strippedIndex++;
            originalIndex = i;
        }

        // Если индекс выходит за пределы строки
        return -1;
    }

    /// <summary>
    /// Возвращает индекс символа в TMP_Text, который находится под заданной позицией.
    /// </summary>
    /// <param name="text">Ссылка на TMP_Text компонент.</param>
    /// <param name="position">Позиция в мировых или экранных координатах (в зависимости от контекста).</param>
    /// <param name="camera">Камера, используемая для перевода экранных координат в мировые. Может быть null при использовании Screen Space - Overlay.</param>
    /// <returns>Индекс символа или -1, если символ не найден.</returns>
    public static int FindIntersectingCharacter(TMP_Text text, Vector2 position, Camera camera)
    {
        RectTransform rectTransform = text.rectTransform;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, position, camera, out Vector2 localPoint))
            return -1;

        Vector3 worldPoint = rectTransform.TransformPoint(localPoint);

        int characterCount = text.textInfo.characterCount;

        if (characterCount == 0)
            return -1;

        for (int i = 0; i < characterCount; i++)
        {
            TMP_CharacterInfo charInfo = text.textInfo.characterInfo[i];

            // Пропускаем невидимые символы
            if (!charInfo.isVisible)
                continue;

            // Получаем границы символа в локальных координатах
            Vector3 bl = rectTransform.TransformPoint(new Vector3(charInfo.bottomLeft.x, charInfo.descender, 0));
            Vector3 tl = rectTransform.TransformPoint(new Vector3(charInfo.bottomLeft.x, charInfo.ascender, 0));
            Vector3 tr = rectTransform.TransformPoint(new Vector3(charInfo.topRight.x, charInfo.ascender, 0));
            Vector3 br = rectTransform.TransformPoint(new Vector3(charInfo.topRight.x, charInfo.descender, 0));

            // Проверяем пересечение точки с прямоугольником символа
            if (PointIntersectRectangle(worldPoint, bl, tl, tr, br))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Сп#здел метод
    /// </summary>
    /// <param name="m"></param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="d"></param>
    /// <returns></returns>
    private static bool PointIntersectRectangle(Vector3 m, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        // A zero area rectangle is not valid for this method.
        Vector3 normal = Vector3.Cross(b - a, d - a);
        if (normal == Vector3.zero)
            return false;

        Vector3 ab = b - a;
        Vector3 am = m - a;
        Vector3 bc = c - b;
        Vector3 bm = m - b;

        float abamDot = Vector3.Dot(ab, am);
        float bcbmDot = Vector3.Dot(bc, bm);

        return 0 <= abamDot && abamDot <= Vector3.Dot(ab, ab) && 0 <= bcbmDot && bcbmDot <= Vector3.Dot(bc, bc);
    }


    /// <summary>
    /// Проверяет, находится ли точка внутри прямоугольника TextMeshProUGUI элемента
    /// </summary>
    /// <param name="point">Позиция проверяемой точки</param>
    /// <param name="textMeshPro">TextMeshProUGUI элемент для проверки</param>
    /// <returns>True, если точка находится внутри прямоугольника текста, иначе false</returns>
    public static bool IsPointInsideRect(Vector2 point, TMPro.TextMeshProUGUI textMeshPro)
    {
        if (textMeshPro == null || textMeshPro.rectTransform == null)
            return false;

        return IsPointInsideRect(
            point,
            textMeshPro.rectTransform.position,
            textMeshPro.rectTransform.rect.size,
            textMeshPro.rectTransform.pivot
        );
    }

    /// <summary>
    /// Проверяет, находится ли точка внутри прямоугольника с учетом pivot-точки
    /// </summary>
    /// <param name="point">Позиция проверяемой точки</param>
    /// <param name="rectPosition">Позиция центра прямоугольника (координаты центральной точки)</param>
    /// <param name="rectSize">Размеры прямоугольника (ширина и высота)</param>
    /// <param name="pivot">Точка отсчета (pivot) прямоугольника. 
    /// (0,0) - левый нижний угол, (0.5,0.5) - центр, (1,1) - правый верхний угол</param>
    /// <returns>True, если точка находится внутри прямоугольника, иначе false</returns>
    public static bool IsPointInsideRect(Vector2 point, Vector2 rectPosition, Vector2 rectSize, Vector2 pivot)
    {
        // Вычисляем смещение прямоугольника относительно его позиции на основе pivot
        Vector2 offset = new Vector2(
            rectSize.x * pivot.x,
            rectSize.y * pivot.y
        );

        // Вычисляем фактические границы прямоугольника
        float left = rectPosition.x - offset.x;
        float right = rectPosition.x - offset.x + rectSize.x;
        float bottom = rectPosition.y - offset.y;
        float top = rectPosition.y - offset.y + rectSize.y;

        // Проверяем, находится ли точка внутри границ
        return point.x >= left && point.x <= right && point.y >= bottom && point.y <= top;
    }

    /// <summary>
    /// Удаляет из первого массива все элементы, которые присутствуют во втором массиве
    /// </summary>
    /// <typeparam name="T">Тип элементов массивов</typeparam>
    /// <param name="firstArray">Массив, из которого удаляются элементы</param>
    /// <param name="secondArray">Массив, содержащий элементы для удаления</param>
    /// <returns>Новый массив без элементов из второго массива</returns>
    public static T[] RemoveElements<T>(T[] firstArray, T[] secondArray)
    {
        if (firstArray == null || secondArray == null)
            return firstArray ?? new T[0];

        // Преобразуем второй массив в HashSet для быстрого поиска
        var elementsToRemove = new HashSet<T>(secondArray);

        // Фильтруем первый массив, оставляя только элементы, которых нет во втором
        return System.Array.FindAll(firstArray, element => !elementsToRemove.Contains(element));
    }

    /// <summary>
    /// Находит GameObject среди предков, которые содержат компонент типа T.
    /// </summary>
    /// <typeparam name="T">Тип компонента, который нужно найти (должен наследоваться от Component).</typeparam>
    /// <param name="childObject">GameObject, предков которого нужно проверить.</param>
    /// <returns>GameObject, который содержит компонент типа T.</returns>
    public static T FindParentWithComponent<T>(GameObject childObject) where T : Component
    {
        Transform current = childObject.transform.parent;
        while (current != null)
        {
            T component = current.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
            current = current.parent;
        }
        return null;
    }

    /// <summary>
    /// Возвращает размеры прямоугольника (в пикселях), необходимого для отображения текста,
    /// с учетом шрифта, размера, переносов, выравнивания и других параметров текстового компонента.
    /// </summary>
    /// <param name="textComponent">Текстовый компонент (Text), для которого необходимо вычислить размер.</param>
    /// <returns>Вектор, содержащий ширину и высоту прямоугольника, необходимого для отображения текста.</returns>
    public static Vector2 GetTextSize(Text textComponent)
    {
        if (textComponent == null)
            return Vector2.zero;

        // Используем встроенные свойства текстового компонента
        return new Vector2(textComponent.preferredWidth, textComponent.preferredHeight);
    }

    /// <summary>
    /// Возвращает размеры прямоугольника (в пикселях), необходимого для отображения текста,
    /// с учетом шрифта, размера, переносов, выравнивания и других параметров текстового компонента TextMeshProUGUI.
    /// </summary>
    /// <param name="textComponent">Текстовый компонент TextMeshProUGUI, для которого необходимо вычислить размер.</param>
    /// <returns>Вектор, содержащий ширину и высоту прямоугольника, необходимого для отображения текста.</returns>
    public static Vector2 GetTextSize(TextMeshProUGUI textComponent)
    {
        if (textComponent == null)
            return Vector2.zero;

        // Принудительно обновляем текстовый компонент для получения актуальных размеров
        textComponent.ForceMeshUpdate();

        // Используем встроенные свойства TextMeshProUGUI
        return new Vector2(textComponent.preferredWidth, textComponent.preferredHeight);
    }

    /// <summary>
    /// Загружает обычный шрифт из папки Resources по указанному пути и преобразует его в TMP_FontAsset.
    /// </summary>
    /// <param name="path">Путь к шрифту в папке Resources (без расширения файла и без "Resources/")</param>
    /// <returns>Созданный TMP_FontAsset или null, если шрифт не найден</returns>
    public static TMPro.TMP_FontAsset LoadTMPFont(string path)
    {
        Font regularFont = LoadFont(path);

        TMPro.TMP_FontAsset tmpFont = TMPro.TMP_FontAsset.CreateFontAsset(regularFont);

        if (tmpFont == null)
        {
            Debug.LogError($"Не удалось создать TMP_FontAsset из шрифта: {path}");
        }

        return tmpFont;
    }

    /// <summary>
    /// Загружает шрифт из папки Resources по указанному пути.
    /// </summary>
    /// <param name="path">Путь к шрифту в папке Resources (без расширения файла и без "Resources/")</param>
    /// <returns>Загруженный шрифт или null, если не найден</returns>
    public static Font LoadFont(string path)
    {
        Font font = Resources.Load<Font>(path);
        if (font == null)
        {
            Debug.LogError($"Шрифт не найден по пути: {path}");
        }
        return font;
    }

    /// <summary>
    /// Проверяет, находится ли точка внутри прямоугольника RectTransform.
    /// </summary>
    /// <param name="rectTransform">RectTransform, задающий прямоугольник.</param>
    /// <param name="worldPoint">Точка в мировых координатах.</param>
    /// <returns>True, если точка внутри прямоугольника, иначе false.</returns>
    public static bool ContainsPoint(this RectTransform rectTransform, Vector2 worldPoint)
    {
        // Преобразуем мировую точку в локальные координаты относительно RectTransform
        Vector2 localPoint = rectTransform.InverseTransformPoint(worldPoint);

        // Получаем прямоугольник в локальных координатах
        Rect rect = rectTransform.rect;

        // Проверяем, попадает ли точка в границы прямоугольника
        return rect.Contains(localPoint);
    }

    /// <summary>
    /// Находит все GameObject, которые содержат компонент типа T.
    /// Если массив объектов не указан, ищет среди всех активных объектов сцены.
    /// </summary>
    /// <typeparam name="T">Тип компонента, который нужно найти (должен наследоваться от Component).</typeparam>
    /// <param name="objectsToCheck">Массив GameObject для поиска. Если null — ищет в активных объектах сцены.</param>
    /// <returns>Массив GameObject, которые содержат компонент типа T.</returns>
    public static GameObject[] FindGameObjectsWithComponent<T>(GameObject[] objectsToCheck = null) where T : Component
    {
        List<GameObject> results = new List<GameObject>();

        if (objectsToCheck == null || objectsToCheck.Length == 0)
        {
            // Используем новый метод без сортировки для повышения производительности
            T[] allComponents = Object.FindObjectsByType<T>(FindObjectsSortMode.InstanceID);
            foreach (T component in allComponents)
            {
                results.Add(component.gameObject);
            }
        }
        else
        {
            // Поиск только среди переданных объектов
            foreach (GameObject obj in objectsToCheck)
            {
                if (obj != null && obj.TryGetComponent<T>(out _))
                {
                    results.Add(obj);
                }
            }
        }

        return results.ToArray();
    }
    
    /// <summary>
    /// Преобразует массив Transform[] в массив GameObject[].
    /// Каждый элемент массива Transform будет преобразован в соответствующий GameObject.
    /// </summary>
    /// <param name="transforms">Массив компонентов Transform</param>
    /// <returns>Массив объектов GameObject</returns>
    public static GameObject[] TransformsToGameObjects(Transform[] transforms)
    {
        if (transforms == null) return null;

        GameObject[] gameObjects = new GameObject[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            gameObjects[i] = transforms[i].gameObject;
        }
        return gameObjects;
    }

    /// <summary>
    /// Преобразует массив GameObject[] в массив Transform[].
    /// Каждый элемент массива GameObject будет преобразован в соответствующий Transform.
    /// </summary>
    /// <param name="gameObjects">Массив игровых объектов GameObject</param>
    /// <returns>Массив компонентов Transform</returns>
    public static Transform[] GameObjectsToTransforms(GameObject[] gameObjects)
    {
        if (gameObjects == null) return null;

        Transform[] transforms = new Transform[gameObjects.Length];
        for (int i = 0; i < gameObjects.Length; i++)
        {
            transforms[i] = gameObjects[i].transform;
        }
        return transforms;
    }

    /// <summary>
    /// Рекурсивно получает все дочерние элементы (включая вложенные) указанного родительского объекта.
    /// </summary>
    /// <param name="parent">Родительский Transform, чьи дочерние элементы нужно собрать.</param>
    /// <returns>Массив всех дочерних Transform (все уровни вложенности).</returns>
    public static Transform[] GetAllChildren(Transform parent)
    {
        List<Transform> results = new List<Transform>();
        CollectAllChildren(parent, results);
        return results.ToArray();
    }

    private static void CollectAllChildren(Transform parent, List<Transform> results)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            results.Add(child);
            CollectAllChildren(child, results); // Рекурсивный вызов для вложенных элементов
        }
    }

    /// <summary>
    /// Загружает текстуру из папки Resources по указанному пути.
    /// </summary>
    /// <param name="resourcePath">Путь к файлу без расширения (например: "Textures/Character" для Resources/Textures/Character.png)</param>
    /// <returns>Загруженная Texture2D или null, если текстура не найдена</returns>
    public static Texture2D LoadTexture(string resourcePath)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);

        return texture;
    }

    /// <summary>
    /// Возвращает угол (в градусах) между прямой, соединяющей две точки, и осью X.
    /// Угол измеряется от оси X против часовой стрелки.
    /// </summary>
    /// <param name="point1">Первая точка (Vector2).</param>
    /// <param name="point2">Вторая точка (Vector2).</param>
    /// <returns>Угол в градусах.</returns>
    public static float GetAngleBetweenPoints(Vector2 point1, Vector2 point2)
    {
        // Вычисляем вектор между точками
        Vector2 vector = point2 - point1;

        // Если точки совпадают, вектор нулевой — угол не определён
        if (vector == Vector2.zero)
        {
            Debug.LogWarning("Точки совпадают, вектор нулевой. Угол не может быть определён.");
            return 0f;
        }

        // Вычисляем угол в радианах
        float angleRad = Mathf.Atan2(vector.y, vector.x);

        // Переводим радианы в градусы
        float angleDeg = angleRad * Mathf.Rad2Deg;

        return angleDeg;
    }

    /// <summary>
    /// Находит все GameObject с компонентом типа T и возвращает их в виде объектов типа T.
    /// Если массив объектов не указан, ищет среди всех активных объектов сцены.
    /// </summary>
    /// <typeparam name="T">Тип компонента, который нужно найти (должен наследоваться от Component).</typeparam>
    /// <param name="objects">Массив GameObject для поиска. Если null — ищет в активных объектах сцены.</param>
    /// <returns>Массив объектов типа T</returns>
    public static T[] FindObjectsWithComponent<T>(GameObject[] objectsToCheck = null) where T : Component
    {
        List<T> results = new List<T>();

        if (objectsToCheck == null || objectsToCheck.Length == 0)
        {
            // Используем новый метод без сортировки для повышения производительности
            T[] allComponents = Object.FindObjectsByType<T>(FindObjectsSortMode.InstanceID);
            results.AddRange(allComponents);
        }
        else
        {
            // Поиск только среди переданных объектов
            foreach (GameObject obj in objectsToCheck)
            {
                T component = obj.GetComponent<T>();
                if (component != null)
                {
                    results.Add(component);
                }
            }
        }

        return results.ToArray();
    }

    /// <summary>
    /// Ищет GameObject с заданным именем в указанном массиве или среди всех активных объектов сцены.
    /// </summary>
    /// <param name="objects">Массив объектов для поиска. Если null — поиск среди всех активных объектов.</param>
    /// <param name="name">Имя искомого объекта.</param>
    /// <returns>Первый найденный объект с совпадающим именем, или null, если не найден.</returns>
    public static GameObject FindGameObjectByName(GameObject[] objects, string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        if (objects != null)
        {
            foreach (GameObject obj in objects)
            {
                if (obj != null && obj.name == name)
                    return obj;
            }
        }
        else
        {
            // Используем новый метод FindObjectsByType вместо устаревшего FindObjectsOfType
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(
                FindObjectsSortMode.InstanceID); // Без сортировки для лучшей производительности
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.name == name)
                    return obj;
            }
        }

        return null;
    }

    /// <summary>
    /// Перегрузка: ищет GameObject с заданным именем среди всех активных объектов сцены.
    /// </summary>
    public static GameObject FindGameObjectByName(string name)
    {
        return FindGameObjectByName(null, name);
    }

    /// <summary>
    /// Возвращает локализованную строку из указанного файла
    /// </summary>
    /// <param name="variable">Имя переменной (ключ)</param>
    /// <param name="fileName">Имя файла или путь к файлу без расширения</param>
    /// <param name="languageCode">Код локализации (например, RUS)</param>
    /// <returns>Значение переменной или исходное имя переменной, если не найдено</returns>
    public static string GetLocalizedString(string variable, string fileName, string languageCode)
    {
        string path = $"Localization/{languageCode}/{fileName}";
        TextAsset textAsset = Resources.Load<TextAsset>(path);

        if (textAsset == null)
        {
            Debug.LogError($"Файл не найден: {path}.txt");
            return variable;
        }

        string content = Encoding.UTF8.GetString(textAsset.bytes);

        string[] lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            int separatorIndex = line.IndexOf('=');
            if (separatorIndex < 0)
                continue;

            string key = line.Substring(0, separatorIndex).Trim();
            string value = line.Substring(separatorIndex + 1).Trim();

            if (key == variable)
                return value;
        }

        Debug.LogWarning($"Переменная '{variable}' не найдена в {path}.txt");
        return variable;
    }

    public static Object LoadInputActions(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return null;

        string relativePath = filePath.Replace("Assets/", "");
        if (!relativePath.StartsWith("Resources/"))
            return null;

        relativePath = relativePath.Replace("Resources/", "");
        int lastDotIndex = relativePath.LastIndexOf('.');
        if (lastDotIndex != -1)
            relativePath = relativePath.Substring(0, lastDotIndex);

        relativePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');

        return Resources.Load(relativePath);
    }

    /// <summary>
    /// Рекурсивно ищет GameObject с заданным именем 
    /// в указанном объекте и всех его дочерних объектах
    /// </summary>
    /// <param name="parent">Объект, с которого начинается поиск</param>
    /// <param name="name">Имя, которое нужно найти</param>
    /// <returns>Найденный GameObject или null, если не найден</returns>
    public static GameObject FindChildRecursive(GameObject parent, string name)
    {
        // Проверка на null
        if (parent == null)
            return null;

        // Проверяем имя текущего объекта
        if (parent.name == name)
            return parent;

        // Рекурсивный поиск среди всех дочерних объектов
        foreach (Transform child in parent.transform)
        {
            // Рекурсивно вызываем метод для каждого дочернего объекта
            GameObject found = FindChildRecursive(child.gameObject, name);

            // Если объект найден, возвращаем его
            if (found != null)
                return found;
        }

        // Объект не найден
        return null;
    }

    public static void SetStaticRecursively(GameObject obj)
    {
        obj.isStatic = true;

        foreach (Transform child in obj.transform)
        {
            SetStaticRecursively(child.gameObject);
        }
    }

    /// <summary>
    /// Загружает GameObject из папки Resources.
    /// </summary>
    /// <param name="path">Путь внутри папки Resources (без "Resources/").</param>
    /// <returns>Возвращает GameObject или null, если не найден.</returns>
    public static GameObject LoadGameObjectFromResources(string path)
    {
        // Проверяем, что путь не пустой
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Путь к ресурсу не указан.");
            return null;
        }

        // Загружаем объект из Resources
        GameObject loadedObject = Resources.Load<GameObject>(path);

        if (loadedObject == null)
        {
            Debug.LogError($"Не удалось загрузить GameObject по пути: {path}");
        }

        return loadedObject;
    }

    public static T[] AddElement<T>(T[] array, T element)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        T[] newArray = new T[array.Length + 1];
        Array.Copy(array, newArray, array.Length);
        newArray[newArray.Length - 1] = element;
        return newArray;
    }

    public static T[] RemoveElements<T>(T[] array, T value)
    {
        if (array == null) return null;

        return array.Where(item =>
            !object.Equals(item, value)
        ).ToArray();
    }

    /// <summary>
    /// Делает из элементов коллекции строчку с разделительным пробелом.
    /// </summary>
    /// <typeparam name="T">Тип элементов коллекции.</typeparam>
    /// <param name="collection">Коллекция.</param>
    public static string CollectionToString<T>(IEnumerable<T> collection)
    {
        if (collection == null)
        {
            return "";
        }

        string result = "";
        foreach (var item in collection)
        {
            string itemStr = "null";
            if (item != null)
            {
                try
                {
                    itemStr = item.ToString();
                }
                catch
                {
                    itemStr = $"[{item.GetType().Name}]";
                }
            }
            else
            {
                itemStr = "null";
            }

            result += itemStr + " ";
        }

        return result.Trim();
    }

    /// <summary>
    /// Логирует элементы коллекции в консоль Unity через пробел.
    /// Если коллекция null или пустая — выводит соответствующее сообщение.
    /// </summary>
    /// <typeparam name="T">Тип элементов коллекции.</typeparam>
    /// <param name="collection">Коллекция, элементы которой нужно вывести.</param>
    public static void LogCollection<T>(IEnumerable<T> collection)
    {
        if (collection == null)
        {
            Debug.Log("Коллекция равна null.");
            return;
        }

        string result = "";
        foreach (var item in collection)
        {
            string itemStr = "null";
            if (item != null)
            {
                try
                {
                    itemStr = item.ToString();
                }
                catch
                {
                    itemStr = $"[{item.GetType().Name}]";
                }
            }
            else
            {
                itemStr = "null";
            }

            result += itemStr + " ";
        }

        Debug.Log(result.Trim());
    }

    /// <summary>
    /// Возвращает случайный элемент из массива с учётом заданных весов.
    /// </summary>
    /// <typeparam name="T">Тип элементов массива.</typeparam>
    /// <param name="items">Массив элементов.</param>
    /// <param name="probabilities">Массив вероятностей (весов), соответствующих каждому элементу.</param>
    /// <returns>Элемент из массива items, выбранный на основе вероятностей.</returns>
    /// <exception cref="ArgumentNullException">Если один из массивов null или длина массивов не совпадает.</exception>
    /// <exception cref="InvalidOperationException">Если нет элементов с положительной вероятностью.</exception>
    public static T GetWeightedRandomItem<T>(T[] items, float[] probabilities)
    {
        if (items == null || probabilities == null)
            throw new ArgumentNullException("Массивы не могут быть null.");

        if (items.Length != probabilities.Length)
            throw new ArgumentException("Массивы должны иметь одинаковую длину.");

        if (items.Length == 0)
            throw new ArgumentNullException("Массивы пусты");

        float totalProbability = 0f;
        for (int i = 0; i < probabilities.Length; i++)
        {
            if (probabilities[i] > 0)
                totalProbability += probabilities[i];
        }

        if (totalProbability <= 0)
            throw new InvalidOperationException("Нет подходящих элементов с положительной вероятностью.");

        float randomValue = Random.Range(0f, totalProbability);

        for (int i = 0; i < items.Length; i++)
        {
            if (probabilities[i] <= 0)
                continue;

            if (randomValue < probabilities[i])
                return items[i];

            randomValue -= probabilities[i];
        }

        for (int i = probabilities.Length - 1; i >= 0; i--)
        {
            if (probabilities[i] > 0)
                return items[i];
        }

        throw new InvalidOperationException("Неожиданное состояние: не найден ни один элемент.");
    }

    /// <summary>
    /// Преобразует двумерную матрицу в одномерный массив, объединяя все элементы по порядку.
    /// </summary>
    /// <typeparam name="T">Тип элементов матрицы.</typeparam>
    /// <param name="matrix">Двумерная матрица, которую нужно преобразовать.</param>
    /// <returns>Одномерный массив, содержащий все элементы матрицы.</returns>
    /// <exception cref="ArgumentNullException">Если matrix равен null.</exception>
    public static T[] FlattenMatrix<T>(T[,] matrix)
    {
        if (matrix == null)
            throw new ArgumentNullException(nameof(matrix));

        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        T[] result = new T[rows * cols];
        int index = 0;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result[index++] = matrix[i, j];
            }
        }

        return result;
    }

    /// <summary>
    /// Возвращает случайные элементы из массива.
    /// Можно выбрать как уникальные элементы, так и с возможными повторениями.
    /// </summary>
    /// <typeparam name="T">Тип элементов массива.</typeparam>
    /// <param name="array">Исходный массив.</param>
    /// <param name="count">Количество запрашиваемых элементов.</param>
    /// <param name="allowDuplicates">Разрешены ли повторения. По умолчанию false.</param>
    /// <returns>Массив случайных элементов.</returns>
    public static T[] GetRandomElements<T>(this T[] array, int count, bool allowDuplicates = false)
    {
        if (array == null || array.Length == 0 || count <= 0)
            return new T[0];

        // Если разрешены дубликаты — выбираем с возвратом
        if (allowDuplicates)
        {
            T[] result = new T[count];
            for (int i = 0; i < count; i++)
            {
                int index = Random.Range(0, array.Length);
                result[i] = array[index];
            }
            return result;
        }

        // Иначе — без повторений
        List<T> availableItems = new List<T>(array);
        T[] output = new T[Mathf.Min(count, array.Length)];

        for (int i = 0; i < output.Length; i++)
        {
            int index = Random.Range(0, availableItems.Count);
            output[i] = availableItems[index];
            availableItems.RemoveAt(index);
        }

        return output;
    }

    /// <summary>
    /// Загружает Material из папки Resources по указанному имени.
    /// Путь к материалу: "Materials/" + materialName.
    /// </summary>
    /// <param name="materialName">Имя материала (без расширения).</param>
    /// <returns>Загруженный Material или null, если материал не найден.</returns>
    public static Material LoadMaterialByName(string materialName)
    {
        string path = "Materials/" + materialName;

        Material material = Resources.Load<Material>(path);

        if (material == null)
        {
            Debug.LogWarning($"Материал не найден по пути: {path}");
        }

        return material;
    }

    /// <summary>
    /// Объединяет два массива в один.
    /// </summary>
    /// <typeparam name="T">Тип элементов массивов.</typeparam>
    /// <param name="firstArray">Первый массив.</param>
    /// <param name="secondArray">Второй массив.</param>
    /// <returns>Новый массив, содержащий элементы firstArray и secondArray подряд.</returns>
    /// <exception cref="ArgumentNullException">Если любой из массивов null.</exception>
    public static T[] ConcatArrays<T>(T[] firstArray, T[] secondArray)
    {
        if (firstArray == null || secondArray == null)
        {
            throw new ArgumentNullException("Массивы не могут быть null.");
        }

        T[] result = new T[firstArray.Length + secondArray.Length];

        Array.Copy(firstArray, result, firstArray.Length);
        Array.Copy(secondArray, 0, result, firstArray.Length, secondArray.Length);

        return result;
    }
}