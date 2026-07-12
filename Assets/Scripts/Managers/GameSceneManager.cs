using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName;

    public void RestartGameplay()
    {
        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogError("Gameplay scene name is not assigned.");
            return;
        }

        SceneManager.LoadScene(gameplaySceneName);
    }
}