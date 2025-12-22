using UnityEngine;
using TMPro;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    [Header("Simulation Time")]
    public float simulationDuration = 60f; // total durasi simulasi (detik)
    private float simulationTimer;
    public TMP_Text simTimeText;

    [Header("Movement Time")]
    public float movementDuration = 5f; // waktu giliran player/AI (detik)
    private float movementTimer = 3f;
    public TMP_Text moveTimeText;

    public bool isSimulationRunning = false;
    public bool isMovementRunning = false;

    public delegate void TimeUp();
    public event TimeUp OnSimulationEnd;
    public event TimeUp OnMovementEnd;

    public GameOverManager gameOverManager;

    void Start()
    {
        simulationTimer = PlayerPrefs.GetInt("SimulationDuration");
        movementTimer = PlayerPrefs.GetInt("CountdownMove");

        UpdateSimUI();
        UpdateMoveUI();
    }

    void Update()
    {
        if (isSimulationRunning)
        {
            simulationTimer -= Time.deltaTime;
            if (simulationTimer <= 0f)
            {
                simulationTimer = 0f;
                isSimulationRunning = false;
                gameOverManager.ShowGameOver();
                //OnSimulationEnd?.Invoke();
            }
            UpdateSimUI();
        }

        if (isMovementRunning)
        {
            movementTimer -= Time.deltaTime;
            if (movementTimer <= 0f)
            {
                movementTimer = 0f;
                isMovementRunning = false;
                gameOverManager.ShowGameOver();
                //OnMovementEnd?.Invoke();
            }
            UpdateMoveUI();
        }
    }

    public void StartMovement()
    {
        movementTimer = PlayerPrefs.GetInt("CountdownMove"); ;

        isSimulationRunning = true;
        isMovementRunning = true;
    }

    public void StopMovement()
    {
        isSimulationRunning = false;
        isMovementRunning = false;
    }

    public void ResetMovement()
    {
        movementTimer = PlayerPrefs.GetInt("CountdownMove");

        isMovementRunning = false; // pastikan timer mulai lagi
    }

    private void UpdateSimUI()
    {
        //Debug.Log("Updating Simulation Time UI");
        if (simTimeText != null)
            simTimeText.text = $"{Mathf.CeilToInt(simulationTimer)}";
    }

    private void UpdateMoveUI()
    {
        //Debug.Log("Updating Movement Time UI");
        if (moveTimeText != null)
            moveTimeText.text = $"{Mathf.CeilToInt(movementTimer)}";
    }
}
