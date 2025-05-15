using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiSceneLoader : MonoBehaviour
{
    void Awake()
    {
        // BackgroundScene이 아직 로드되지 않았다면 Additive로 로드
        if (!SceneManager.GetSceneByName("BackgroundScene").isLoaded)
        {
            SceneManager.LoadSceneAsync("BackgroundScene", LoadSceneMode.Additive);
        }
    }
}
