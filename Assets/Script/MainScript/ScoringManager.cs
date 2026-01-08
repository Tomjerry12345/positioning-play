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
    [Header("Feedback")]
    public List<string> feedback;

    [Header("Profile")]
    public TMP_Text nameText;
    public TMP_Text roleText;
    public TMP_Text poinText;
    public TMP_Text feedbacktext;

    [Header("Positioning")]
    public TMP_Text timeInAreaIdeal;
    public TMP_Text timeInAreaNetral;
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

        float idealTime = PlayerPrefs.GetFloat("IdealTime");
        float netralTime = PlayerPrefs.GetFloat("NetralTime");
        float scorePositioningTotal = PlayerPrefs.GetFloat("ScorePositioning");

        int passingIdeal = PlayerPrefs.GetInt("PassingIdeal");
        int scorePassingTotal = PlayerPrefs.GetInt("ScorePassing"); // misal setiap passing sukses dapat 2 poin

        int totalScore = (int)scorePositioningTotal + scorePassingTotal;

        Debug.Log($"idealTime : {idealTime} | netralTime : {netralTime} | scorePositioning : {scorePositioningTotal}");
        Debug.Log($"passingIdeal : {passingIdeal} | scorePassing : {scorePassingTotal}");

        int indexFeedback = PlayerPrefs.GetInt("IndexFeedback");
        Debug.Log($"Index Feedback : {indexFeedback}");


        timeInAreaIdeal.text = $"{idealTime:F1} Detik";
        timeInAreaNetral.text = $"{netralTime:F1} Detik";
        scorePositioning.text = $"{scorePositioningTotal:F1}";

        passingSuccess.text = $"{passingIdeal}x";
        scorePassing.text = $"{scorePassingTotal}";

        nameText.text = playerName;
        roleText.text = role;
        poinText.text = totalScore.ToString();

        feedbacktext.text = feedback[indexFeedback];

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Clear()
    {
        PlayerPrefs.DeleteKey("IdealTime");
        PlayerPrefs.DeleteKey("NetralTime");
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
