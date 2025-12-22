using UnityEngine;
using UnityEngine.SceneManagement;



public class MenuTutorialScript : MonoBehaviour
{

    public string nextSceneName = "TutorialScene";

    void MoveScene(string position)
    {
        AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);
        Debug.Log(position);
        PlayerPrefs.SetString("Position Clicked", position);
        PlayerPrefs.SetString("ResetBGM", "false");
        SceneManager.LoadScene(nextSceneName);
    }

    public void ClickGelandangSerang()
    {
        MoveScene("Gelandang Serang");
    }

    public void ClickGelandangBertahan()
    {
        MoveScene("Gelandang Bertahan");
    }

    public void ClickGelandangTengah()
    {
        MoveScene("Gelandang Tengah");
    }

}
