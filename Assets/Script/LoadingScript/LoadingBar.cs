using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingBar : MonoBehaviour
{
    [Header("UI Elements")]
    public Image fillBar;             // Drag LoadingBarFill ke sini

    [Header("Scene Settings")]
    public string informationScene = "InformationScene";
    public string menuScene = "MenuScene";
    public string mainScene = "MainScene";

    [Header("Animation")]
    public float fillSpeed = 0.5f;    // Kecepatan bar loading

    void Start()
    {
        AudioManager.instance.PlaySFX(AudioManager.instance.sfxLoading);
        StartCoroutine(LoadAsyncOperation());
    }

    IEnumerator LoadAsyncOperation()
    {
        // 🔍 Cek apakah player sudah isi data sebelumnya
        string playerName = PlayerPrefs.GetString("PlayerName", "");
        string playerPosition = PlayerPrefs.GetString("PlayerPosition", "");

        string stateScene = PlayerPrefs.GetString("StateScene", "MenuScene");

        string nextSceneName = informationScene;

        if (stateScene == "MenuScene")
        {
            PlayerPrefs.SetString("ResetBGM", "true");
            nextSceneName = string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(playerPosition)
            ? informationScene
            : menuScene;
        } else
        {
            PlayerPrefs.SetString("IsPlayingBgm", "non active");
            nextSceneName = mainScene;
            PlayerPrefs.SetString("StateScene", nextSceneName);
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        asyncLoad.allowSceneActivation = false;

        float fakeProgress = 0;

        while (!asyncLoad.isDone)
        {
            float realProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            fakeProgress = Mathf.MoveTowards(fakeProgress, realProgress, Time.deltaTime * fillSpeed);
            fillBar.fillAmount = fakeProgress;

            if (fakeProgress >= 1f)
            {
                yield return new WaitForSeconds(0.5f);
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
