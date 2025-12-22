using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

[System.Serializable]
public class ScoreData
{
    public List<LogData> scores = new List<LogData>();
}
public class ScoringManager : MonoBehaviour
{
    [Header("Profile")]
    public TMP_Text nameText;
    public TMP_Text roleText;
    public TMP_Text poinText;

    [Header("Positioning")]
    public TMP_Text timeInRoleArea;
    public TMP_Text timeOutsideRoleArea;
    public TMP_Text scorePositioning;

    [Header("Passing")]
    public TMP_Text passingSuccess;
    public TMP_Text scorePassing;

    [Header("UI Elements")]
    public GameObject gameOverPanel;

    int totalScore;
    string role;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CanvasGroup gameOverRootCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
        if (gameOverRootCanvasGroup == null)
            gameOverRootCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();

        gameOverRootCanvasGroup.alpha = 0f;   // sembunyikan GameOver

        int durasiGame = 120; // dalam detik

        string playerName = PlayerPrefs.GetString("PlayerName", "Guest");
        role = PlayerPrefs.GetString("Role");

        float inTime = PlayerPrefs.GetFloat("TimeInRoleArea");
        float outTime = PlayerPrefs.GetFloat("TimeOutsideRoleArea");


        double totalScorePositioning = (((inTime * 1) / durasiGame) + ((outTime * 0.2) / durasiGame)) * 100;

        int totalPasses = PlayerPrefs.GetInt("TotalPasses");
        int totalSuccessfulPasses = totalPasses * 2; // misal setiap passing sukses dapat 2 poin

        Debug.Log($"inTime : {inTime}");
        Debug.Log($"outTime : {outTime}");
        Debug.Log($"totalPasses : {totalPasses}");

        totalScore = Mathf.RoundToInt((float)totalScorePositioning) + totalSuccessfulPasses;

        timeInRoleArea.text = $"{inTime:F1} Detik";
        timeOutsideRoleArea.text = $"{outTime:F1} Detik";
        scorePositioning.text = $"{totalScorePositioning:F1}";

        passingSuccess.text = $"{totalPasses}x";
        scorePassing.text = $"{totalSuccessfulPasses}";

        nameText.text = playerName;
        roleText.text = role;
        poinText.text = totalScore.ToString();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Clear()
    {
        PlayerPrefs.DeleteKey("TimeInRoleArea");
        PlayerPrefs.DeleteKey("TimeOutsideRoleArea");
        PlayerPrefs.DeleteKey("TotalPasses");

        Time.timeScale = 1f; // pastikan waktu game normal lagi
        StopAllCoroutines(); // hentikan coroutine yang mungkin masih jalan

        AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);
    }

    public void Retry()
    {
        Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Exit()
    {
        SaveScore(totalScore); // simpan ke array

        Clear();

        PlayerPrefs.SetString("StateScene", "MenuScene");
        PlayerPrefs.SetString("ResetBGM", "true");

        SceneManager.LoadScene("MenuScene");
    }

    void SaveScore(int score)
    {
        string json = PlayerPrefs.GetString("ScoreHistory", "");
        ScoreData data;

        if (string.IsNullOrEmpty(json))
        {
            data = new ScoreData();
        }
        else
        {
            data = JsonUtility.FromJson<ScoreData>(json);
        }

        data.scores.Add(new LogData() { positionName = role, points = score });

        string newJson = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("ScoreHistory", newJson);
        PlayerPrefs.Save();

        Debug.Log("Poin disimpan: " + score);
    }
}
