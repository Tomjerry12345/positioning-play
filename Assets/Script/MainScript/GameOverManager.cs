using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // jika pakai TextMeshPro

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;  // Panel hitam
    public TMP_Text gameOverText;     // Text "GAME OVER"
    public float fadeDuration = 0.5f; // animasi muncul
    private CanvasGroup canvasGroup;

    public GameObject scorePanel;
    private CanvasGroup scoreCanvasGroup;
    public float scoreFadeDuration = 0.5f;

    void Start()
    {
        if (gameOverPanel != null)
        {
            canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();

            gameOverPanel.SetActive(false);
            canvasGroup.alpha = 0f;
        }

        if (scorePanel != null)
        {
            scoreCanvasGroup = scorePanel.GetComponent<CanvasGroup>();
            if (scoreCanvasGroup == null)
                scoreCanvasGroup = scorePanel.AddComponent<CanvasGroup>();

            scorePanel.SetActive(false);
            scoreCanvasGroup.alpha = 0f;
        }
    }

    public void ShowGameOver(string message = "GAME OVER")
    {
        if (gameOverPanel == null || gameOverText == null) return;

        Time.timeScale = 0f;

        gameOverText.text = message;
        gameOverPanel.SetActive(true);

        StartCoroutine(FadeInPanel());
    }

    private IEnumerator FadeInPanel()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;

        // Tunggu 1 detik (real time)
        yield return new WaitForSecondsRealtime(1f);

        // Mulai Fade-In Score
        StartCoroutine(FadeInScore());
    }

    private IEnumerator FadeInScore()
    {
        if (scorePanel == null || scoreCanvasGroup == null)
            yield break;

        scorePanel.SetActive(true);

        float elapsed = 0f;
        while (elapsed < scoreFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            scoreCanvasGroup.alpha = Mathf.Clamp01(elapsed / scoreFadeDuration);
            yield return null;
        }

        scoreCanvasGroup.alpha = 1f;
        // Pastikan UI bisa diklik
        scoreCanvasGroup.interactable = true;
        scoreCanvasGroup.blocksRaycasts = true;
    }

}
