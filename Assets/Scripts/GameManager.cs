using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int Score { get; set; } = 0;

    [Header("점수 주기")] [SerializeField] private int scorePerSecond = 1;
    
    // 게임 상태
    public enum GameState
    {
        Playing,
        Paused,
        GameOver
    }

    public GameState CurrentGameState { get; set; } = GameState.Playing;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않도록 설정
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(IE_AddScore());
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CurrentGameState == GameState.Playing)
            {
                CurrentGameState = GameState.Paused;
                Time.timeScale = 0; // 게임 일시 정지
            }
            else if (CurrentGameState == GameState.Paused)
            {
                CurrentGameState = GameState.Playing;
                Time.timeScale = 1;
            }
        }
    }

    private IEnumerator IE_AddScore()
    {
        while (true)
        {
            Score += scorePerSecond;
#if UNITY_EDITOR
            Debug.Log("Score: " + Score);
#endif
            yield return new WaitForSeconds(1f);
        }
    }

    public void GameOver()
    {
        CurrentGameState = GameState.GameOver;
        // TODO: 게임 오버 처리
    }
}