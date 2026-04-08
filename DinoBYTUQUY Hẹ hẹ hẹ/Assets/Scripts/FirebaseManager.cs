using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq; // Thư viện dùng để sắp xếp điểm

public class FirebaseManager : MonoBehaviour
{
    [Header("Giao diện UI")]
    public TMP_InputField nameInput;
    public TextMeshProUGUI leaderboardText;

    // Link Firebase của Vinh
    public string firebaseUrl = "https://dinorun-463a4-default-rtdb.asia-southeast1.firebasedatabase.app/leaderboard.json";

    // Tạo cái khuôn để chứa Tên và Điểm
    private class PlayerScore
    {   
        public string name;
        public int score;
    }
    
    // 1. GỌI KHI GAME OVER
    public void GameOver(int score)
    {
        string playerName = "Khách Không Tên";
        if (nameInput != null && !string.IsNullOrEmpty(nameInput.text))
        {
            playerName = nameInput.text;
        }

        StartCoroutine(SendScore(playerName, score));
    }

    IEnumerator SendScore(string name, int score)
    {
        string json = "{\"name\":\"" + name + "\",\"score\":" + score + "}";

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(firebaseUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Đã bắn điểm lên mạng! Đang lấy bảng xếp hạng về...");
                StartCoroutine(GetLeaderboard());
            }
        }
    }