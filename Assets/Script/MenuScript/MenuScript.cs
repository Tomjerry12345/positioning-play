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
    public Animator popupAudio;

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

    void Clicked(Animator anim, string triggerName)
    {
        AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);
        anim.SetTrigger(triggerName);
    }

    public void ShowAudio()
    {
        Clicked(popupAudio, "ShowAudio");
    }

    public void HideAudio()
    {
        Clicked(popupAudio, "HideAudio");
    }

    public void ShowTutorial()
    {
        Clicked(popupTutorial, "ShowTutorial");
    }

    public void HideTutorial()
    {
        Clicked(popupTutorial, "HideTutorial");
    }

    public void ShowMulai()
    {
        Clicked(popupAnimator, "ShowPopup");
    }

    public void HideMulai()
    {
        Clicked(popupAnimator, "HidePopup");
    }

    public void ShowLog()
    {
        Clicked(popupLog, "ShowLog");
    }

    public void HideLog()
    {
        Clicked(popupLog, "HideLog");
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

            //Debug.Log($"Role: {log.positionName} — Poin: {log.points}");
        }


        PlayerPrefs.SetInt("TotalScore", totalPoints);

        poinText.text = $"{totalPoints}";
    }

}
