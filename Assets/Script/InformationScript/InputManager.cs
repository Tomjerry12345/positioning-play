using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    public TMP_InputField inputName;
    public TMP_InputField inputPosition;
    public string nextSceneName = "MainScene";

    public void Submit()
    {
        string playerName = inputName.text;
        string playerPosition = inputPosition.text;
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.SetString("PlayerPosition", playerPosition);
        PlayerPrefs.Save();
        Debug.Log($"Saved Name: {playerName}, Position: {playerPosition}");

        //AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);

        AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);

        SceneManager.LoadScene(nextSceneName);
    }
}
