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

    // 2. TẢI VÀ SẮP XẾP BẢNG XẾP HẠNG
    IEnumerator GetLeaderboard()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(firebaseUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string rawJson = www.downloadHandler.text;
                
                // Phép thuật Regex lọc lấy đúng Tên và Điểm từ đống lộn xộn
                MatchCollection matches = Regex.Matches(rawJson, @"\""name\"":\""(.*?)\"".*?\""score\"":(\d+)");

                List<PlayerScore> scoreList = new List<PlayerScore>();

                foreach (Match match in matches)
                {
                    string n = match.Groups[1].Value;
                    int s = int.Parse(match.Groups[2].Value);
                    scoreList.Add(new PlayerScore { name = n, score = s });
                }

                // Sắp xếp điểm từ Cao xuống Thấp
                scoreList = scoreList.OrderByDescending(x => x.score).ToList();

                // Viết ra màn hình
                string finalText = "🏆 TOP THỢ SĂN DINO 🏆\n\n";
                for (int i = 0; i < scoreList.Count && i < 10; i++)
                {
                    finalText += (i + 1) + ". " + scoreList[i].name + " - " + scoreList[i].score + "\n";
                }

                if (leaderboardText != null)
                {
                    leaderboardText.text = finalText;
                }
            }
        }
    }
}