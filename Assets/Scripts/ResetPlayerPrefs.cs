using UnityEditor;
using UnityEngine;

public class ResetPlayerPrefs : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Window/PlayerPrefs 초기화")]
    private static void ResetPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs has been reset.");
    }
#endif
}