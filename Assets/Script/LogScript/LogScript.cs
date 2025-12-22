using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogScript : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text roleText;
    public TMP_Text poinText;

    public GameObject logPrefab;      // Prefab Log UI
    public Transform container;       // Parent: List Log

    private List<LogData> logs = new List<LogData>()
{
    new LogData() { positionName = "Gelandang Bertahan", points = 10 },
    new LogData() { positionName = "Gelandang Tengah", points = 10 },
    new LogData() { positionName = "Gelandang Serang", points = 20 }
};

    private void Start()
    {
        GenerateUI();
    }

    public void GenerateUI()
    {
        // Clear existing children
        foreach (Transform child in container)
            Destroy(child.gameObject);

        //// Instantiate new logs
        //foreach (var data in logs)
        //{
        //    GameObject obj = Instantiate(logPrefab, container);

        //    TMP_Text pos = obj.transform.Find("Text Position").GetComponent<TMP_Text>();
        //    TMP_Text poin = obj.transform.Find("Text Poin").GetComponent<TMP_Text>();

        //    pos.text = data.positionName;
        //    poin.text = data.points + " POIN";
        //}

        string playerName = PlayerPrefs.GetString("PlayerName", "Guest");
        string playerPosition = PlayerPrefs.GetString("PlayerPosition", "Not Set Position");

        nameText.text = playerName;
        roleText.text = playerPosition;

        getAllHistory();
    }

    void getAllHistory()
    {
        int totalPoints = PlayerPrefs.GetInt("TotalScore", 0);
        poinText.text = $"{totalPoints}";

        string json = PlayerPrefs.GetString("ScoreHistory", "");

        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("Belum ada skor!");
            return;
        }

        ScoreData data = JsonUtility.FromJson<ScoreData>(json);

        foreach (LogData log in data.scores)
        {
            GameObject obj = Instantiate(logPrefab, container);

            TMP_Text pos = obj.transform.Find("Text Position").GetComponent<TMP_Text>();
            TMP_Text poin = obj.transform.Find("Text Poin").GetComponent<TMP_Text>();

            pos.text = log.positionName;
            poin.text = log.points + " POIN";

            Debug.Log($"Role: {log.positionName} — Poin: {log.points}");
        }
        
    }

}

[System.Serializable]
public class LogData
{
    public string positionName;  // contoh: "Gelandang Bertahan"
    public int points;           // contoh: 10
}
