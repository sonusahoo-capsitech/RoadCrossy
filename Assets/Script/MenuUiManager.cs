using System.ComponentModel.Design.Serialization;
using Gamewise.crossyroad;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUiManager : MonoBehaviour
{
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }


    public GameState CurrentState { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Image play;
    public Image Background;
    public GameObject levelSelectPanel;

    public GameObject gameOverPanel;
    public GameObject winPanel;
    public GameObject background;
    public GameObject pausePanel;
    public GameObject pauseButton;

    public static MenuUiManager Instance { get; private set; }


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    void Start()
    {
        background.SetActive(true);
        play.gameObject.SetActive(true);
        levelSelectPanel.gameObject.SetActive(false);
        winPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        Instance = this;
        SetState(GameState.Menu);
        DontDestroyOnLoad(this.gameObject);
        pausePanel .SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPlayButtonPressed()
    {
        play.gameObject.SetActive(false);
        levelSelectPanel.gameObject.SetActive(false);
        if (Background != null) Background.gameObject.SetActive(false);
        if (background != null) background.SetActive(false);
        StartGame();
        SceneManager.LoadScene("GamePlay");
        pauseButton.SetActive(true);
    }


    // ------------------------ IGNORE -----------------------


    void SetState(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.Menu:
                Time.timeScale = 1f;
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                break;
        }

        Debug.Log("Game State Changed to: " + newState);
    }

    // ===================== PUBLIC API =====================

    public void StartGame()
    {
        SetState(GameState.Playing);
    }
    public void openPausepanel()
    {
        pausePanel .SetActive (true);
        pauseButton.SetActive(false);
        PauseGame();
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
       pausePanel.SetActive(false);
        pauseButton.SetActive(true);
        SetState(GameState.Playing);
    }

    public void EndGame()
    {
        gameOverPanel.SetActive(true);
        pauseButton.SetActive(false);
        PlayerManager.Instance.updownButton.SetActive(false);
        SetState(GameState.GameOver);

    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false); 
        background.SetActive(true);
        play.gameObject.SetActive(true);
        SceneManager.LoadScene(0);
        SetState(GameState.Menu);
        pausePanel.SetActive(false);
        pauseButton.SetActive(false);

    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
        SetState(GameState.Playing);
        pausePanel.SetActive(false);
        pauseButton.SetActive(true);

    }

    public void LoadLevel(int levelIndex)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(levelIndex);
        SetState(GameState.Playing);
    }
    public void WinGame()
    {
        winPanel.SetActive(true);
        PlayerManager.Instance.updownButton.SetActive(false);
        // SetState(GameState.GameOver);
        Debug.Log("Win Game State Reached");
        SetState(GameState.Paused);
    }

    public bool IsPlaying()
    {
        return CurrentState == GameState.Playing;
    }





}
