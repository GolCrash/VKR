using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SaveScript : MonoBehaviour
{
    public Button saveButton;

    void Start()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveTexture);
    }

    public void SaveTexture()
    {
        // Находим Plane в сцене
        GameObject plane = GameObject.Find("Plane"); // или ваше имя

        if (plane == null)
        {
            Debug.LogError("Plane не найден!");
            return;
        }

        // Получаем текстуру из материала
        Renderer renderer = plane.GetComponent<Renderer>();
        if (renderer == null || renderer.sharedMaterial == null)
        {
            Debug.LogError("У Plane нет материала!");
            return;
        }

        Texture2D texture = renderer.sharedMaterial.mainTexture as Texture2D;

        if (texture == null)
        {
            Debug.LogError("На материале нет Texture2D!");
            return;
        }

        // Сохраняем
        byte[] bytes = texture.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, "plane_texture.png");
        File.WriteAllBytes(path, bytes);

        Debug.Log($"Сохранено: {path}");

#if UNITY_EDITOR
        UnityEditor.EditorUtility.RevealInFinder(path);
#endif
    }
}