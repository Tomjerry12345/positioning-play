using TMPro;
using UnityEngine;

public class MenuScript : MonoBehaviour
{

    public TMP_Text playerNameText;
    public TMP_Text playerPositionText;
    public TMP_Text poinText;

    public Animator popupAnimator;
    public Animator popupTutorial;
    public Animator popupLog;

    void Start()
    {
        string resetBgm = PlayerPrefs.GetString("ResetBGM");

        if (resetBgm == "true")
        {
            AudioManager.instance.PlayBGM(AudioManager.instance.bgmMain);
        }

        string playerName = PlayerPrefs.GetString("PlayerName", "Guest");
        string playerPosition = PlayerPrefs.GetString("PlayerPosition", "Not Set Position");

        playerNameText.text = playerName;
        playerPositionText.text = playerPosition;

        getAllHistory();
    }

    public void ShowTutorial()
    {
        AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);
        Debug.Log("Tutorial button clicked");
        popupTutorial.SetTrigger("ShowTutorial");
    }

    public void HideTutorial()
    {
        AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);
        Debug.Log("HideTutorial button clicked");
        popupTutorial.SetTrigger("HideTutorial");
    }

    public void ShowMulai()
    {
        AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);
        Debug.Log("Mulai button clicked");
        popupAnimator.SetTrigger("ShowPopup");
    }

    public void HideMulai()
    {
        AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);
        Debug.Log("HideMulai button clicked");
        popupAnimator.SetTrigger("HidePopup");
    }

    public void ShowLog()
    {
        AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);
        Debug.Log("ShowLog button clicked");
        popupLog.SetTrigger("ShowLog");
    }

    public void HideLog()
    {
        AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);
        Debug.Log("HideLog button clicked");
        popupLog.SetTrigger("HideLog");
    }

    void getAllHistory()
    {
        int totalPoints = 0;

        string json = PlayerPrefs.GetString("ScoreHistory", "");

        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("Belum ada skor!");
            poinText.text = "0";
            return;
        }

        ScoreData data = JsonUtility.FromJson<ScoreData>(json);

        foreach (LogData log in data.scores)
        {
         
            totalPoints += log.points;

            Debug.Log($"Role: {log.positionName} — Poin: {log.points}");
        }


        PlayerPrefs.SetInt("TotalScore", totalPoints);

        poinText.text = $"{totalPoints}";
    }

}
