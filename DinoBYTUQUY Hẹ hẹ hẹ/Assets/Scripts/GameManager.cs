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
        pauseMenu.SetActive(true);
    }
    
    public void ConfirmMainMenu()
    {
        // shouldSaveScore removed
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void CancelMainMenu()
    {
        HideMainMenuConfirmationDialog();
    }
    
    public void ShowLeaderboard()
    {
        leaderboardPanel.SetActive(true);
        gameStartMess.SetActive(false);
        //resetButton.SetActive(false);
        exitButton.SetActive(false);
        leaderboardCloseButton.SetActive(true);
        //muteButton.SetActive(false);
        DisplayLeaderboard();
    }
    
    public void HideLeaderboard()
    {
        leaderboardPanel.SetActive(false);
        gameStartMess.SetActive(true);
        //resetButton.SetActive(true);
        exitButton.SetActive(true);
        leaderboardCloseButton.SetActive(false);
        //muteButton.SetActive(true);
        
        // Clean up leaderboard entries immediately
        if (leaderboardContent != null)
        {
            Debug.Log($"Cleaning up {leaderboardContent.transform.childCount} leaderboard entries");
            
            // Destroy all child objects immediately
            while (leaderboardContent.transform.childCount > 0)
            {
                Transform child = leaderboardContent.transform.GetChild(0);
                DestroyImmediate(child.gameObject);
            }
            
            Debug.Log($"Cleanup complete. Remaining children: {leaderboardContent.transform.childCount}");
        }
        else
        {
            Debug.LogError("LeaderboardContent is null during cleanup!");
        }
    }
    
    private void LoadLeaderboard()
    {
        string leaderboardData = PlayerPrefs.GetString("Leaderboard", "");
        if (string.IsNullOrEmpty(leaderboardData))
        {
            leaderboard = new List<LeaderboardEntry>();
        }
        else
        {
            try
            {
                leaderboard = JsonUtility.FromJson<List<LeaderboardEntry>>(leaderboardData);
                if (leaderboard == null)
                {
                    leaderboard = new List<LeaderboardEntry>();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to load leaderboard: " + e.Message);
                leaderboard = new List<LeaderboardEntry>();
            }
        }
    }
    
    private void SaveLeaderboard()
    {
        string leaderboardData = JsonUtility.ToJson(leaderboard);
        PlayerPrefs.SetString("Leaderboard", leaderboardData);
        PlayerPrefs.Save();
    }
    
    private void AddToLeaderboard(float scoreValue)
    {
        string currentDate = System.DateTime.Now.ToString("MM/dd/yyyy");
        LeaderboardEntry newEntry = new LeaderboardEntry
        {
            score = Mathf.FloorToInt(scoreValue).ToString(),
            date = currentDate
        };
        
        leaderboard.Add(newEntry);
        leaderboard.Sort((a, b) => int.Parse(b.score).CompareTo(int.Parse(a.score)));
        
        if (leaderboard.Count > MAX_LEADERBOARD_ENTRIES)
        {
            leaderboard = leaderboard.GetRange(0, MAX_LEADERBOARD_ENTRIES);
        }
        
        SaveLeaderboard();
    }
    
    private void DisplayLeaderboard()
    {
        if (leaderboardContent == null)
        {
            Debug.LogError("LeaderboardContent is not assigned in Inspector!");
            return;
        }
        
        if (leaderboardEntryPrefab == null)
        {
            Debug.LogError("LeaderboardEntryPrefab is not assigned in Inspector!");
            return;
        }
        
        // Check prefab structure
        TextMeshProUGUI[] prefabTexts = leaderboardEntryPrefab.GetComponentsInChildren<TextMeshProUGUI>();
        Debug.Log($"LeaderboardEntryPrefab has {prefabTexts.Length} TextMeshProUGUI components");
        
        // Clean up existing entries
        foreach (Transform child in leaderboardContent.transform)
        {
            Destroy(child.gameObject);
        }
        
        // Reset content size
        RectTransform contentRect = leaderboardContent.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            contentRect.anchoredPosition = Vector2.zero;
        }
        
        for (int i = 0; i < leaderboard.Count; i++)
        {
            if (leaderboard[i] == null)
            {
                Debug.LogError($"Leaderboard entry at index {i} is null!");
                continue;
            }
            
            GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardContent.transform);
            if (entry == null)
            {
                Debug.LogError($"Failed to instantiate leaderboard entry prefab at index {i}!");
                continue;
            }
            
            // Position the entry properly within the content
            RectTransform entryRect = entry.GetComponent<RectTransform>();
            if (entryRect != null)
            {
                entryRect.anchoredPosition = new Vector2(0, -i * 50); // Position each entry 50 units down
                entryRect.anchorMin = new Vector2(0, 1); // Anchor to top-left
                entryRect.anchorMax = new Vector2(1, 1);
                entryRect.pivot = new Vector2(0, 1);
                entryRect.sizeDelta = new Vector2(0, 40); // Height of 40, width fills content
            }
            
            TextMeshProUGUI[] texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
            Debug.Log($"Entry {i} has {texts.Length} TextMeshProUGUI components");
            
            if (texts.Length >= 2)
            {
                if (texts[0] != null)
                {
                    texts[0].text = (i + 1).ToString() + ".";
                    Debug.Log($"Set rank text: {(i + 1).ToString() + "."}");
                }
                if (texts[1] != null)
                {
                    texts[1].text = leaderboard[i].score + " - " + leaderboard[i].date;
                    Debug.Log($"Set score+date text: {leaderboard[i].score + " - " + leaderboard[i].date}");
                }
            }
            else
            {
                Debug.LogError($"Leaderboard entry prefab needs at least 2 TextMeshProUGUI components! Found: {texts.Length}");
                
                // Try to find any text components and show what we found
                if (texts.Length == 1)
                {
                    Debug.LogError("Only 1 TextMeshProUGUI found. Need 2: one for rank, one for score+date");
                }
                else
                {
                    Debug.LogError("No TextMeshProUGUI components found on prefab!");
                }
            }
        }
        
        // Adjust content height based on number of entries
        if (contentRect != null)
        {
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, leaderboard.Count * 50);
            }
    }
    
    private void InitializeBackgrounds()
    {
        // Reset background switching variables
        lastBackgroundSwitchScore = 0;
        currentBackgroundIndex = 0;
        
        // Show Background1, hide Background2 at start
        if (background1 != null && background2 != null)
        {
            background1.SetActive(true);
            background2.SetActive(false);
        }
        else
        {
            Debug.LogError("Background objects not assigned in Inspector!");
        }
    }
    
    private void SwitchBackground()
    {
        if (background1 == null || background2 == null)
        {
            Debug.LogError("Background objects not assigned!");
            return;
        }
        
        // Toggle between backgrounds
        if (currentBackgroundIndex == 0)
        {
            // Switch to Background2
            background1.SetActive(false);
            background2.SetActive(true);
            currentBackgroundIndex = 1;
            Debug.Log("Switched to Background2 at score: " + Mathf.FloorToInt(score));
        }
        else
        {
            // Switch to Background1
            background1.SetActive(true);
            background2.SetActive(false);
            currentBackgroundIndex = 0;
            Debug.Log("Switched to Background1 at score: " + Mathf.FloorToInt(score));
        }
    }
    
    /// <summary>
    /// Sets a custom score for testing background switching
    /// Call this method to test different score values and background changes
    /// </summary>
    /// <param name="customScore">The score value to set (e.g., 500, 1000, 1500)</param>
    public void SetCustomScore(int customScore)
    {
        // Update score display
        score = customScore;
        speedText.text = "Score: " + Mathf.FloorToInt(score).ToString();
        
        // Calculate which background should be active based on 500-point intervals
        int scoreThreshold = 500;
        int expectedBackgroundIndex = (customScore / scoreThreshold) % 2;
        
        // Switch background if the expected background is different from current
        if (expectedBackgroundIndex != currentBackgroundIndex)
        {
            SwitchBackground();
            lastBackgroundSwitchScore = (customScore / scoreThreshold) * scoreThreshold;
        }
        
        // Log the change for debugging
        string backgroundName = expectedBackgroundIndex == 0 ? "Background1" : "Background2";
        Debug.Log($"Custom score set to: {customScore}, Background: {backgroundName}");
    }
    
    // Gửi điểm lên Firebase và load lại scene
    public void BamNutGuiDiem()
    {
        // Gửi điểm lên Firebase
        FindAnyObjectByType<FirebaseManager>().GameOver(Mathf.FloorToInt(score));
        // Load lại Scene để chơi ván mới
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    // Mở bảng xếp hạng để tải dữ liệu từ Firebase
    public void OpenLeaderboard()
    {
        leaderboardPanel.SetActive(true);
        if (leaderboardText != null)
        {
            leaderboardText.text = "Đang tải dữ liệu...";
        }
        StartCoroutine(FetchLeaderboard());
    }
    
    // Đóng bảng xếp hạng
    public void CloseLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }
    
    // Tải dữ liệu bảng xếp hạng từ Firebase
    private IEnumerator FetchLeaderboard()
    {
        // Thay URL này bằng Firebase URL của bạn
        string firebaseUrl = "https://dinorun-463a4-default-rtdb.asia-southeast1.firebasedatabase.app/leaderboard.json";
        
        using (UnityWebRequest request = UnityWebRequest.Get(firebaseUrl))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log("Firebase Response: " + json);
                
                try
                {
                    // Tạo list để lưu các entry (tên - điểm)
                    List<KeyValuePair<string, int>> scores = new List<KeyValuePair<string, int>>();
                    
                    // Sử dụng Regex để quét tất cả "name" và "score" trong JSON
                    // Pattern này tìm: "name": "...", "score": ...
                    string pattern = @"""name""\s*:\s*""([^""]*)""\s*,\s*""score""\s*:\s*(\d+)";
                    
                    MatchCollection matches = Regex.Matches(json, pattern);
                    
                    Debug.Log("Số lượng entry tìm được: " + matches.Count);
                    
                    // Duyệt qua tất cả match và lưu vào list
                    foreach (Match match in matches)
                    {
                        if (match.Groups.Count >= 3)
                        {
                            string name = match.Groups[1].Value;
                            string scoreStr = match.Groups[2].Value;
                            
                            if (int.TryParse(scoreStr, out int scoreValue))
                            {
                                scores.Add(new KeyValuePair<string, int>(name, scoreValue));
                                Debug.Log("Tìm được: " + name + " - " + scoreValue);
                            }
                        }
                    }
                    
                    // Sắp xếp từ cao xuống thấp
                    scores.Sort((a, b) => b.Value.CompareTo(a.Value));
                    
                    // Lấy tối đa 10 người
                    int topCount = Mathf.Min(scores.Count, 10);
                    string leaderboardDisplay = "<size=120%><b>TOP CAO THỦ</b></size>\n\n";
                    
                    Debug.Log("TopCount: " + topCount);

                    if (topCount == 0)
                    {
                        leaderboardDisplay = "Chưa có dữ liệu!";
                    }
                    else
                    {
                        // Tìm độ dài tên dài nhất để căn phải điểm số
                        int maxNameLength = 0;
                        for (int i = 0; i < topCount; i++)
                        {
                            if (scores[i].Key.Length > maxNameLength)
                                maxNameLength = scores[i].Key.Length;
                        }
                        int padLength = maxNameLength + 2; // +2 cho đẹp
                        
                        // Cộng dồn tất cả các dòng INTO leaderboardDisplay
                        for (int i = 0; i < topCount; i++)
                        {
                            string stt = (i + 1).ToString("D2");
                            string name = scores[i].Key.ToUpper();
                            // Căn phải điểm số bằng dấu chấm
                            string line = stt + ". " + name.PadRight(padLength, '.') + " " + scores[i].Value;
                            leaderboardDisplay += line + "\n";
                        }
                    }
                    
                    // Gán text CHỈ 1 LẦN duy nhất SAU khi vòng lặp hoàn thành
                    if (leaderboardText != null)
                    {
                        leaderboardText.text = leaderboardDisplay;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Lỗi parse JSON: " + e.Message);
                    if (leaderboardText != null)
                    {
                        leaderboardText.text = "Lỗi tải dữ liệu!";
                    }
                }
            }
            else
            {
                Debug.LogError("Lỗi kết nối Firebase: " + request.error);
                if (leaderboardText != null)
                {
                    leaderboardText.text = "Lỗi kết nối!";
                }
            }
        }
    }
}
