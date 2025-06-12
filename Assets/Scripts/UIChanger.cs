using UnityEngine;
using UnityEngine.SceneManagement;

public class UIChanger : MonoBehaviour
{
    public void OnClickStart()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OnClickQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
