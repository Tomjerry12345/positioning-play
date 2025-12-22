using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScript : MonoBehaviour
{
    public TMP_InputField inputRole;
    public TMP_InputField inputDurationSimulation;
    public TMP_InputField inputCountdownMove;

    public string[] roleOptions = { "Gelandang Serang", "Gelandang Tengah", "Gelandang Bertahan"};
    private string[] roleShort = { "AM", "CM", "DM" };
    public string[] timeOptionsSimulation = { "30 detik", "1 menit", "2 menit", "5 menit" };
    public string[] timeOptionsCountdownMove = { "3 detik", "6 detik", "9 detik" };

    private int currenRoleIndex = 0;
    private int currentSimulationIndex = 0;
    private int currentCountdownIndex = 0;

    public string nextSceneName = "LoadingScene";


    private void Start()
    {
        inputRole.text = roleOptions[currenRoleIndex];
        inputDurationSimulation.text = timeOptionsSimulation[currentSimulationIndex];
        inputCountdownMove.text = timeOptionsCountdownMove[currentCountdownIndex];
    }

    public void NextChangeRole()
    {
        // Naik satu langkah
        AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);
        currenRoleIndex = (currenRoleIndex + 1) % roleOptions.Length;
        inputRole.text = roleOptions[currenRoleIndex];
    }

    public void PreviousChangeRole()
    {
        // Turun satu langkah
        AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);
        currenRoleIndex = (currenRoleIndex - 1 + roleOptions.Length) % roleOptions.Length;
        inputRole.text = roleOptions[currenRoleIndex];
    }

    public void NextChangeTimeSimulation()
    {
        // Naik satu langkah
        AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);
        currentSimulationIndex = (currentSimulationIndex + 1) % timeOptionsSimulation.Length;
        inputDurationSimulation.text = timeOptionsSimulation[currentSimulationIndex];
    }

    public void PreviousChangeTimeSimulation()
    {
        // Turun satu langkah
        AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);
        currentSimulationIndex = (currentSimulationIndex - 1 + timeOptionsSimulation.Length) % timeOptionsSimulation.Length;
        inputDurationSimulation.text = timeOptionsSimulation[currentSimulationIndex];
    }

    public void NextChangeCountdownMove()
    {
        // Naik satu langkah
        AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);
        currentCountdownIndex = (currentCountdownIndex + 1) % timeOptionsCountdownMove.Length;
        inputCountdownMove.text = timeOptionsCountdownMove[currentCountdownIndex];
    }

    public void PreviousChangeCountdownMove()
    {
        // Turun satu langkah
        AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);
        currentCountdownIndex = (currentCountdownIndex - 1 + timeOptionsCountdownMove.Length) % timeOptionsCountdownMove.Length;
        inputCountdownMove.text = timeOptionsCountdownMove[currentCountdownIndex];
    }

    public void PlayStart()
    {
        AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);
        AudioManager.instance.StopBGM();

        string roleText = roleOptions[currenRoleIndex];
        string roleS = roleShort[currenRoleIndex];
        string simText = timeOptionsSimulation[currentSimulationIndex];
        string moveText = timeOptionsCountdownMove[currentCountdownIndex];

        int simSeconds = ConvertToSeconds(simText);
        int moveSeconds = ConvertToSeconds(moveText);

        PlayerPrefs.SetString("Role", roleText);
        PlayerPrefs.SetString("RoleShort", roleS);
        PlayerPrefs.SetInt("SimulationDuration", simSeconds);
        PlayerPrefs.SetInt("CountdownMove", moveSeconds);
        PlayerPrefs.SetString("StateScene", "MainScene");

        PlayerPrefs.Save();

        SceneManager.LoadScene(nextSceneName);
    }

    private int ConvertToSeconds(string input)
    {
        input = input.ToLower();

        if (input.Contains("detik"))
        {
            return int.Parse(input.Replace("detik", "").Trim());
        }
        else if (input.Contains("menit"))
        {
            int menit = int.Parse(input.Replace("menit", "").Trim());
            return menit * 60;
        }

        return 0;
    }

}
