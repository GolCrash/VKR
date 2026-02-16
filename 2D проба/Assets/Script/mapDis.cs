using UnityEngine;

/// <summary>
/// Класс для отображения сгенерированных текстур карт.
/// </summary>
public class mapDis : MonoBehaviour
{
    public Renderer textureRender;  //Объект, который отображает текстуру

    public void DrawTexture(Texture2D texture)
    {
        // Применяем текстуру к материалу рендера
        textureRender.sharedMaterial.mainTexture = texture;

        // Масштабируем объект в соответствии с размерами текстуры
        //textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height); // X = ширина текстуры, Z = высота текстуры, Y = 1 (сохраняем толщину)
    }
}