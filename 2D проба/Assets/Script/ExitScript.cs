using UnityEngine;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    [Header("Настройки кнопки")]
    [Tooltip("Кнопка для закрытия приложения. Если не назначена, будет использоваться кнопка на этом объекте.")]
    public Button quitButton;

    private void Start()
    {
        // Если кнопка не назначена в инспекторе, пробуем найти её на этом объекте
        if (quitButton == null)
        {
            quitButton = GetComponent<Button>();
        }

        // Если кнопка найдена, подписываемся на событие клика
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitApplication);
        }
        else
        {
            Debug.LogError("QuitButton: Не удалось найти компонент Button!");
        }
    }

    /// <summary>
    /// Метод для закрытия приложения
    /// </summary>
    public void QuitApplication()
    {
        Debug.Log("Приложение закрывается...");

#if UNITY_EDITOR
        // В редакторе Unity останавливаем Play Mode
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // В собранном приложении закрываем его
            Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        // Важно отписаться от события при уничтожении объекта
        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitApplication);
        }
    }
}