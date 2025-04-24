using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneLoader : MonoBehaviour
{
    public string sceneName;  // Assign the scene name in Inspector

    public void LoadNextScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
