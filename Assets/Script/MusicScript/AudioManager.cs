using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource atmosferSource;
    public AudioClip bgmMain;
    public AudioClip sfxPass;
    public AudioClip sfxDribble;
    public AudioClip sfxButton;
    public AudioClip sfxPenonton;
    public AudioClip sfxLoading;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            //LoadVolumeSettings(); // Load pengaturan volume saat start
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadVolumeSettings();
    }

    // FUNGSI BARU - Load volume settings
    void LoadVolumeSettings()
    {
        // Load dari PlayerPrefs, default value 5 (tengah-tengah)
        float musikLatar = PlayerPrefs.GetFloat("VolMusikLatar", 5f);
        float efek = PlayerPrefs.GetFloat("VolEfek", 5f);
        float atmosfer = PlayerPrefs.GetFloat("VolAtmosfer", 5f);

        // Convert dari skala 1-10 ke 0-1
        SetVolumeBGM((musikLatar - 1f) / 9f);
        SetVolumeSFX((efek - 1f) / 9f);
        SetVolumeAtmosfer((atmosfer - 1f) / 9f);
    }

    // FUNGSI BARU - Set volume BGM
    public void SetVolumeBGM(float volume)
    {
        bgmSource.volume = volume;
    }

    // FUNGSI BARU - Set volume SFX
    public void SetVolumeSFX(float volume)
    {
        sfxSource.volume = volume;
    }

    // FUNGSI BARU - Set volume Atmosfer
    public void SetVolumeAtmosfer(float volume)
    {
        if (atmosferSource != null)
            atmosferSource.volume = volume;
    }

    public void PlayBGM(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    // FUNGSI BARU - Play atmosfer (suara penonton loop)
    public void PlayAtmosfer(AudioClip clip)
    {
        if (atmosferSource != null)
        {
            atmosferSource.clip = clip;
            atmosferSource.loop = true;
            atmosferSource.Play();
        }
    }

    public void StopAtmosfer()
    {
        if (atmosferSource != null)
            atmosferSource.Stop();
    }
}