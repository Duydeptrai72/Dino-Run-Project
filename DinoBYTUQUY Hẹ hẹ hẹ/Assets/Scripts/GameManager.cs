using UnityEngine;
using TMPro;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private float gameSpeed = 5f;
    [SerializeField]
    private float speedIncrease = 0.15f;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    private float score = 0f;
    private float highScore = 0f;
    [SerializeField] private GameObject scoreTextobject;
    [SerializeField] private GameObject gameOverMess;
    [SerializeField] private GameObject gameStartMess;
    [SerializeField] private GameObject titleGame;
    [SerializeField] private GameObject resetButton;
    [SerializeField] private GameObject confirmationDialog;
    [SerializeField] private GameObject yesButton;
    [SerializeField] private GameObject noButton;
    [SerializeField] private GameObject exitButton;
    [SerializeField] private GameObject exitConfirmationDialog;
    [SerializeField] private GameObject exitYesButton;
    [SerializeField] private GameObject exitNoButton;
    [SerializeField] private GameObject muteButton;
    [SerializeField] private TextMeshProUGUI muteButtonText;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject resumeButton;
    [SerializeField] private GameObject mainMenuButton;
    [SerializeField] private GameObject mainMenuConfirmationDialog;
    [SerializeField] private GameObject mainMenuYesButton;
    [SerializeField] private GameObject mainMenuNoButton;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private GameObject leaderboardContent;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private GameObject leaderboardCloseButton;
    [SerializeField] private TextMeshProUGUI leaderboardText;
    [SerializeField] private GameObject creditsTable;
    public GameObject leaderboardButtonObj;
    [SerializeField] private GameObject background1;
    [SerializeField] private GameObject background2;
    [SerializeField] private GameObject nameInputUI;
    private bool isGameOver = false;
    private bool isMuted = false;
    private bool isPaused = false;
    private int lastBackgroundSwitchScore = 0;
    private int currentBackgroundIndex = 0;
    
    [System.Serializable]
    public class LeaderboardEntry
    {
        public string score;
        public string date;
    }
    
    private List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();
    private const int MAX_LEADERBOARD_ENTRIES = 10;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public float GetGameSpeed()
    {
        return gameSpeed;
    }
    void Start()
    {
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        HandleStartGameInput();
        if (UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetHighScore();
        }
        if (!isGameOver)
        {
            UpdateGameSpeed();
            UpdateScore();
        }
    }
    private void UpdateGameSpeed()
    {
        gameSpeed += Time.deltaTime * speedIncrease;
    }
    private void UpdateScore()
    {
        score += Time.deltaTime * gameSpeed;
        speedText.text = "Score: " + Mathf.FloorToInt(score).ToString();
        
        // Check for background switching every 500 points
        int currentScore = Mathf.FloorToInt(score);
        int scoreThreshold = 250;
        
        if (currentScore >= lastBackgroundSwitchScore + scoreThreshold)
        {
            SwitchBackground();
            lastBackgroundSwitchScore = currentScore;
        }
        
        highScoreText.text = "Highscore: " + Mathf.FloorToInt(highScore).ToString();
    }
    private void StartGame()
    {
        LoadHighScore();
        LoadMuteState();
        LoadLeaderboard();
        // shouldSaveScore removed
        Time.timeScale = 0f;
        scoreTextobject.SetActive(false);
        gameStartMess.SetActive(true);
        gameOverMess.SetActive(false);
        //resetButton.SetActive(true);
        creditsTable.SetActive(true);
        leaderboardCloseButton.SetActive(false);
        titleGame.SetActive(true);
        exitButton.SetActive(true);
        //muteButton.SetActive(true);
        confirmationDialog.SetActive(false);
        exitConfirmationDialog.SetActive(false);
        pauseMenu.SetActive(false);
        mainMenuConfirmationDialog.SetActive(false);
        leaderboardPanel.SetActive(false);
        highScoreText.text = "Highscore: " + Mathf.FloorToInt(highScore).ToString();
        UpdateMuteButton();
        
        // Initialize backgrounds
        InitializeBackgrounds();
    }
    private void HandleStartGameInput()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !isPaused)
        {
            Time.timeScale = 1f;
            scoreTextobject.SetActive(true);
            titleGame.SetActive(false);
            gameStartMess.SetActive(false);
            //resetButton.SetActive(false);
            creditsTable.SetActive(false);
            exitButton.SetActive(false);
            leaderboardButtonObj.SetActive(false);
            //muteButton.SetActive(false);
            confirmationDialog.SetActive(false);
            exitConfirmationDialog.SetActive(false);
            pauseMenu.SetActive(false);
            mainMenuConfirmationDialog.SetActive(false);
            leaderboardPanel.SetActive(false);
            leaderboardCloseButton.SetActive(false);
            
            // Clean up any leaderboard entries that might be visible
            if (leaderboardContent != null)
            {
                while (leaderboardContent.transform.childCount > 0)
                {
                    Transform child = leaderboardContent.transform.GetChild(0);
                    DestroyImmediate(child.gameObject);
                }
            }
        }
        
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Time.timeScale == 1f && !isGameOver)
            {
                PauseGame();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
        }
    }
    public void GameOver()
    {
        Debug.Log("GameOver called!");
        Time.timeScale = 0f; // Dừng game ngay lập tức
        
        // Set game over state
        isGameOver = true;
        isPaused = false;
        
        // Hide unnecessary UI
        gameOverMess.SetActive(true);
        exitButton.SetActive(false);
        confirmationDialog.SetActive(false);
        exitConfirmationDialog.SetActive(false);
        pauseMenu.SetActive(false);
        mainMenuConfirmationDialog.SetActive(false);
        leaderboardButtonObj.SetActive(true);
        leaderboardPanel.SetActive(false);
        leaderboardCloseButton.SetActive(false);
        
        // Clean up leaderboard entries
        if (leaderboardContent != null)
        {
            while (leaderboardContent.transform.childCount > 0)
            {
                Transform child = leaderboardContent.transform.GetChild(0);
                DestroyImmediate(child.gameObject);
            }
        }
        
        // Get clean score values
        int currentScore = Mathf.FloorToInt(score);
        int currentHigh = Mathf.FloorToInt(highScore);
        
        Debug.Log("SO SÁNH ĐIỂM: Score = " + currentScore + " | HighScore = " + currentHigh);
        
        // Compare and decide action
        if (currentScore > currentHigh)
        {
            Debug.Log("Phá kỷ lục! Đang chờ nhập tên...");
            highScore = score; // Update highScore
            SaveHighScore();
            AddToLeaderboard(score);
            nameInputUI.SetActive(true); // Hiện bảng
            // TUYỆT ĐỐI KHÔNG GỌI LỆNH AUTO-RELOAD Ở ĐÂY
        }
        else
        {
            Debug.Log("Không phá kỷ lục. Auto-reloading...");
            AddToLeaderboard(score);
            StartCoroutine(ReloadScene());
        }
    }
    private IEnumerator ReloadScene()
    {
        yield return new WaitForSecondsRealtime(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetFloat("HighScore", 0f);
    }
    
    private void SaveHighScore()
    {
        PlayerPrefs.SetFloat("HighScore", highScore);
        PlayerPrefs.Save();
    }
    
    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey("HighScore");
        highScore = 0f;
        highScoreText.text = "Highscore: 0";
    }
    
    public void ShowConfirmationDialog()
    {
        confirmationDialog.SetActive(true);
        //resetButton.SetActive(false);
        exitButton.SetActive(false);
        //muteButton.SetActive(false);
    }
    
    public void OnResetButtonClick()
    {
        ShowConfirmationDialog();
    }
    
    public void OnExitButtonClick()
    {
        ShowExitConfirmationDialog();
    }
    
    public void HideConfirmationDialog()
    {
        confirmationDialog.SetActive(false);
        //resetButton.SetActive(true);
        exitButton.SetActive(true);
        //muteButton.SetActive(true);
    }
    
    public void ConfirmResetHighScore()
    {
        ResetHighScore();
        HideConfirmationDialog();
    }
    
    public void CancelResetHighScore()
    {
        HideConfirmationDialog();
    }
    
    public void ShowExitConfirmationDialog()
    {
        exitConfirmationDialog.SetActive(true);
        //resetButton.SetActive(false);
        exitButton.SetActive(false);
        //muteButton.SetActive(false);
    }
    
    public void HideExitConfirmationDialog()
    {
        exitConfirmationDialog.SetActive(false);
        //resetButton.SetActive(true);
        exitButton.SetActive(true);
        //muteButton.SetActive(true);
    }
    
    public void ConfirmExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    public void CancelExitGame()
    {
        HideExitConfirmationDialog();
    }
    
    public void OnMuteButtonClick()
    {
        ToggleMute();
    }
    
    private void ToggleMute()
    {
        isMuted = !isMuted;
        AudioListener.volume = isMuted ? 0f : 1f;
        SaveMuteState();
        UpdateMuteButton();
    }
    
    private void UpdateMuteButton()
    {
        if (muteButtonText != null)
        {
            muteButtonText.text = isMuted ? "Unmute" : "Mute";
        }
    }
    
    private void LoadMuteState()
    {
        isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
        AudioListener.volume = isMuted ? 0f : 1f;
    }
    
    private void SaveMuteState()
    {
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        titleGame.SetActive(true);
        //muteButton.SetActive(true);
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        titleGame.SetActive(false);
        //muteButton.SetActive(false);
    }
    
    public void OnResumeButtonClick()
    {
        ResumeGame();
    }
    
    public void OnMainMenuButtonClick()
    {
        ShowMainMenuConfirmationDialog();
    }
    
    public void ShowMainMenuConfirmationDialog()
    {
        mainMenuConfirmationDialog.SetActive(true);
        pauseMenu.SetActive(false);
    }
    
    public void HideMainMenuConfirmationDialog()
    {
        mainMenuConfirmationDialog.SetActive(false);