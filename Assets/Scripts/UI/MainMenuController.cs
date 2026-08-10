using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "Level";

    public void StartGame()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogWarning("[MainMenuController] Falta asignar la escena de gameplay.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
