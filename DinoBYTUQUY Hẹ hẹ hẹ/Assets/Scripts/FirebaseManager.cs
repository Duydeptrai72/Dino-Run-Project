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