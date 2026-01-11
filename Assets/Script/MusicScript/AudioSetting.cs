using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSetting : MonoBehaviour
{
    [Header("Sliders")]
    public Slider sliderMusikLatar;
    public Slider sliderEfek;
    public Slider sliderAtmosfer;

    [Header("Text Angka Kiri (Opsional)")]
    public TextMeshProUGUI textMusikLatarKiri;
    public TextMeshProUGUI textEfekKiri;
    public TextMeshProUGUI textAtmosferKiri;

    [Header("Text Angka Kanan (Opsional)")]
    public TextMeshProUGUI textMusikLatarKanan;
    public TextMeshProUGUI textEfekKanan;
    public TextMeshProUGUI textAtmosferKanan;

    void Start()
    {
        // Load pengaturan tersimpan
        float musikLatar = PlayerPrefs.GetFloat("VolMusikLatar", 5f);
        float efek = PlayerPrefs.GetFloat("VolEfek", 5f);
        float atmosfer = PlayerPrefs.GetFloat("VolAtmosfer", 5f);

        // Set nilai slider
        sliderMusikLatar.value = musikLatar;
        sliderEfek.value = efek;
        sliderAtmosfer.value = atmosfer;

        // Setup slider listeners
        sliderMusikLatar.onValueChanged.AddListener(SetMusikLatar);
        sliderEfek.onValueChanged.AddListener(SetEfek);
        sliderAtmosfer.onValueChanged.AddListener(SetAtmosfer);

        // Update text angka
        UpdateTextAngka();
    }

    public void SetMusikLatar(float value)
    {
        // Convert dari skala 1-10 ke 0-1
        float volume = (value - 1f) / 9f;

        // Set ke AudioManager
        if (AudioManager.instance != null)
            AudioManager.instance.SetVolumeBGM(volume);

        // Update text
        if (textMusikLatarKanan != null)
            textMusikLatarKanan.text = value.ToString("F0");

        // Simpan
        PlayerPrefs.SetFloat("VolMusikLatar", value);
        Debug.Log("Musik Latar volume set to: " + value);
    }

    public void SetEfek(float value)
    {
        float volume = (value - 1f) / 9f;

        if (AudioManager.instance != null)
            AudioManager.instance.SetVolumeSFX(volume);

        if (textEfekKanan != null)
            textEfekKanan.text = value.ToString("F0");

        PlayerPrefs.SetFloat("VolEfek", value);
    }

    public void SetAtmosfer(float value)
    {
        float volume = (value - 1f) / 9f;

        if (AudioManager.instance != null)
            AudioManager.instance.SetVolumeAtmosfer(volume);

        if (textAtmosferKanan != null)
            textAtmosferKanan.text = value.ToString("F0");

        PlayerPrefs.SetFloat("VolAtmosfer", value);
    }

    void UpdateTextAngka()
    {
        // Text kiri (angka 1)
        if (textMusikLatarKiri != null) textMusikLatarKiri.text = "1";
        if (textEfekKiri != null) textEfekKiri.text = "1";
        if (textAtmosferKiri != null) textAtmosferKiri.text = "1";

        // Text kanan (angka 10)
        if (textMusikLatarKanan != null) textMusikLatarKanan.text = "10";
        if (textEfekKanan != null) textEfekKanan.text = "10";
        if (textAtmosferKanan != null) textAtmosferKanan.text = "10";
    }

    public void Kembali()
    {
        PlayerPrefs.Save();

        // Play SFX button
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(AudioManager.instance.sfxButton);

        // Tutup panel atau kembali ke menu
        gameObject.SetActive(false);
    }
}