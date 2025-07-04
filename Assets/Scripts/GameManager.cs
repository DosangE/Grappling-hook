using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro 사용 시

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameState { Playing, Paused, GameOver }
    public GameState CurrentGameState { get; private set; } = GameState.Playing;

    public int Score { get; private set; } = 0;
    [SerializeField] private int scorePerSecond = 1;

    [Header("UI Panels")]
    public GameObject gameUI;
    public GameObject pausePopupUI;
    public GameObject gameOverUI;

    [Header("UI Texts")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI endScoreText;

    public TextMeshProUGUI healthText;

    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        StartGame(); // 게임 초기화    
    }

    private void Update()
    {
        if (CurrentGameState == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
        {
            if (SoundManager.instance != null)
                SoundManager.instance.PlayClickSound();
            PauseGame();
        }
        else if (CurrentGameState == GameState.Paused && Input.GetKeyDown(KeyCode.Escape))
        {
            if (SoundManager.instance != null)
                SoundManager.instance.PlayClickSound();
            ResumeGame();
        }
    }

    // 점수 코루틴
    private IEnumerator IE_AddScore()
    {
        while (true)
        {
            if (CurrentGameState == GameState.Playing)
            {
                Score += scorePerSecond;
                if (scoreText != null)
                    scoreText.text = "Score: " + Score;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    // UI & 상태 변경 메서드

    public void StartGame()
    {
        Score = 0;
        scoreText.text = "Score: 0";
        CurrentGameState = GameState.Playing;
        gameUI.SetActive(true);
        pausePopupUI.SetActive(false);
        gameOverUI.SetActive(false);
        Time.timeScale = 1;
        if (SoundManager.instance != null)
            SoundManager.instance.PlayBGM(SoundManager.instance.mainBGM);

        StartCoroutine(IE_AddScore());
    }

    public void PauseGame()
    {
        CurrentGameState = GameState.Paused;
        pausePopupUI.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        CurrentGameState = GameState.Playing;
        pausePopupUI.SetActive(false);
        Time.timeScale = 1;
    }

    public void GameOver()
    {
        CurrentGameState = GameState.GameOver;
        endScoreText.text = "Your Score is : " + Score;
        gameOverUI.SetActive(true);
        Time.timeScale = 0;
    }

    public void ReturnToLobby()
    {
        if (SoundManager.instance != null)
            SoundManager.instance.PlayClickSound();
        SceneManager.LoadScene("LobbyScene");
    }

    public void QuitGame()
    {
        if (SoundManager.instance != null)
            SoundManager.instance.PlayClickSound();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
