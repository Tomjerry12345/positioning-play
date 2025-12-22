//using System.Collections; // kalau pakai TextMeshPro
//using TMPro;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;

//public class TutorialScript : MonoBehaviour
//{
//    public string nextSceneName = "MenuScene";
//    private string keyClicked = "Position Clicked";

//    [Header("UI Elements")]
//    public TMP_Text typeDescText;
//    public TMP_Text roleText;
//    public TMP_Text positionText;
//    public TMP_Text descText;

//    [Header("Slides")]
//    [TextArea(3, 5)] public string[] descriptions;
//    string role;
//    string position;
//    private string[] typeDesc =
//    {
//        "Penjelasan Role",
//        "Grid Ideal",
//        "Tips Bermain"
//    };


//    [Header("Indicators (lingkaran)")]
//    public Image[] indicators; // <-- lingkaran (UI Image)
//    public Color activeColor = Color.white;
//    public Color inactiveColor = new Color(1, 1, 1, 0.3f);

//    [Header("TabAnimation")]
//    public Button[] tabs;
//    public Animator tabAnimator;
//    public GameObject textPanel;
//    public GameObject videoPanel;
//    public Color activeColorTab = new Color(0.8f, 0.8f, 0.8f);   // agak gelap
//    public Color inactiveColorTab = Color.white;

//    [Header("Animation")]
//    public Animator cardAnimator;
//    public float transitionDelay = 0.4f;

//    private int currentIndex = 0;
//    private int currentTabIndex = 0;


//    void Start()
//    {
//        position = PlayerPrefs.GetString(keyClicked);
//        Debug.Log("Position dari PlayerPrefs: '" + position + "'");
//        Debug.Log("Panjang string: " + position.Length);

//        if (position.Equals("Gelandang Serang", System.StringComparison.OrdinalIgnoreCase))
//        {
//            role = "AM";
//            descriptions = new string[]
//            {
//                "AM berfungsi sebagai kreator serangan. Tugas utamanya menciptakan peluang, masuk ke ruang antar lini, dan menghubungkan bola ke winger atau striker.",

//                "• Berada di zona tengah atas (depan CM).\n" +
//                "• Masuk ke ruang antarlini antara gelandang dan bek lawan.\n" +
//                "• Tidak turun terlalu rendah dan tidak melebar ke sisi sayap.",

//                "• Cari celah di belakang gelandang lawan untuk menerima bola.\n" +
//                "• Gunakan dribble pendek untuk membuka ruang.\n" +
//                "• Utamakan passing progresif ke sayap atau striker.",
//            };
//        } 
//        else if (position.Equals("Gelandang Bertahan", System.StringComparison.OrdinalIgnoreCase))
//        {
//            role = "DM";
//            descriptions = new string[]
//            {
//                "DM bertugas menjaga keseimbangan tim saat menyerang. Fokus utama adalah menjaga posisi di depan bek tengah, menjadi opsi passing aman, serta mengatur aliran bola dari belakang ke tengah.",

//                "• Berada di zona tengah bawah (depan CB).\n" +
//                "• Tidak maju terlalu tinggi.\n" +
//                "• Tetap berada di grid poros sebagai jalur sirkulasi bola.",

//                "• Selalu sediakan diri sebagai back-passing option.\n" +
//                "• Jangan keluar dari zona tengah karena akan meninggalkan ruang kosong.\n" +
//                "• Pilih passing aman dan cepat untuk menjaga ritme permainan.",
//            };
//        }
//        else if (position.Equals("Gelandang Tengah", System.StringComparison.OrdinalIgnoreCase))
//        {
//            role = "CM";
//            descriptions = new string[]
//            {
//                "CM adalah penghubung utama antara bertahan dan menyerang. Tugasnya menjaga ritme, membantu distribusi bola, serta mengisi ruang kosong untuk tetap menjaga struktur tim.",

//                "• Berada di zona tengah-tengah lapangan.\n" +
//                "• Bergerak horizontal mengikuti aliran bola.\n" +
//                "• Tidak boleh terlalu maju (zona AM) atau terlalu turun (zona DM).",

//                "• Selalu bergerak mencari ruang agar mudah menerima bola.\n" +
//                "• Pastikan tetap berada di jalur tengah untuk menjaga koneksi antarlini.\n" +
//                "• Pilih kombinasi dribble pendek dan passing untuk menjaga tempo.",
//            };
//        }

//        UpdateContent(0);
//        Tab(0);
//    }

//    public void Tab(int index)
//    {
//        if (index == currentTabIndex)
//            return;

//        if (index == 0)
//        {
//            tabAnimator.SetTrigger("TabText");
//        }
//        else if (index == 1)
//        {
//            tabAnimator.SetTrigger("TabVideo");
//        }

//        StartCoroutine(SwitchTabAfterAnimation(index));
//    }

//    private IEnumerator SwitchTabAfterAnimation(int newIndex)
//    {
//        // Tunggu durasi animasi (misalnya 0.5 detik, sesuaikan dengan animator kamu)
//        yield return new WaitForSeconds(0.1f);

//        // Matikan semua panel dulu
//        textPanel.SetActive(false);
//        videoPanel.SetActive(false);

//        // Aktifkan panel sesuai tab baru
//        if (newIndex == 0)
//            textPanel.SetActive(true);
//        else if (newIndex == 1)
//            videoPanel.SetActive(true);

//        UpdateTabs(newIndex);

//        // Simpan index tab aktif sekarang
//        currentTabIndex = newIndex;
//    }

//    void UpdateTabs(int index)
//    {
//        for (int i = 0; i < tabs.Length; i++)
//        {
//            Image img = tabs[i].GetComponent<Image>();
//            if (img != null)
//            {
//                img.color = (i == index) ? activeColorTab : inactiveColorTab;
//            }
//        }
//    }

//    public void ShowSlide(int newIndex)
//    {
//        if (newIndex == currentIndex || newIndex < 0 || newIndex >= descriptions.Length)
//            return;


//        StartCoroutine(SwitchSlide(newIndex));
//    }

//    IEnumerator SwitchSlide(int newIndex)
//    {
//        cardAnimator.SetTrigger("SlideLeft");

//        yield return new WaitForSeconds(transitionDelay);

//        currentIndex = newIndex;
//        UpdateContent(currentIndex);

//    }

//    void UpdateContent(int index)
//    {
//        roleText.text = role;
//        positionText.text = position;
//        typeDescText.text = typeDesc[index];
//        descText.text = descriptions[index];
//        UpdateIndicators(index);
//    }

//    void UpdateIndicators(int index)
//    {
//        for (int i = 0; i < indicators.Length; i++)
//        {
//            indicators[i].color = (i == index) ? activeColor : inactiveColor;
//        }
//    }


//    public void BackToMenu()
//    {
//        Debug.Log("BackToMenu");
//        PlayerPrefs.SetString(keyClicked, "");
//        SceneManager.LoadScene(nextSceneName);
//    }
//}

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video; // Tambahkan ini untuk Video Player

public class TutorialScript : MonoBehaviour
{
    public string nextSceneName = "MenuScene";
    private string keyClicked = "Position Clicked";

    [Header("UI Elements")]
    public TMP_Text typeDescText;
    public TMP_Text roleText;
    public TMP_Text positionText;
    public TMP_Text descText;

    [Header("Slides")]
    [TextArea(3, 5)] public string[] descriptions;
    string role;
    string position;
    private string[] typeDesc =
    {
        "Penjelasan Role",
        "Grid Ideal",
        "Tips Bermain"
    };

    [Header("Video Player")]
    public VideoPlayer videoPlayer;
    public VideoClip videoAM;  // Video untuk Gelandang Serang (AM)
    public VideoClip videoDM;  // Video untuk Gelandang Bertahan (DM)
    public VideoClip videoCM;  // Video untuk Gelandang Tengah (CM)

    [Header("Indicators (lingkaran)")]
    public Image[] indicators;
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(1, 1, 1, 0.3f);

    [Header("TabAnimation")]
    public Button[] tabs;
    public Animator tabAnimator;
    public GameObject textPanel;
    public GameObject videoPanel;
    public Color activeColorTab = new Color(0.8f, 0.8f, 0.8f);
    public Color inactiveColorTab = Color.white;

    [Header("Animation")]
    public Animator cardAnimator;
    public float transitionDelay = 0.4f;

    private int currentIndex = 0;
    private int currentTabIndex = 0;

    void Start()
    {
        position = PlayerPrefs.GetString(keyClicked);
        Debug.Log("Position dari PlayerPrefs: '" + position + "'");
        Debug.Log("Panjang string: " + position.Length);

        if (position.Equals("Gelandang Serang", System.StringComparison.OrdinalIgnoreCase))
        {
            role = "AM";
            SetupVideo(videoAM);
            descriptions = new string[]
            {
                "AM berfungsi sebagai kreator serangan. Tugas utamanya menciptakan peluang, masuk ke ruang antar lini, dan menghubungkan bola ke winger atau striker.",

                "• Berada di zona tengah atas (depan CM).\n" +
                "• Masuk ke ruang antarlini antara gelandang dan bek lawan.\n" +
                "• Tidak turun terlalu rendah dan tidak melebar ke sisi sayap.",

                "• Cari celah di belakang gelandang lawan untuk menerima bola.\n" +
                "• Gunakan dribble pendek untuk membuka ruang.\n" +
                "• Utamakan passing progresif ke sayap atau striker.",
            };
        }
        else if (position.Equals("Gelandang Bertahan", System.StringComparison.OrdinalIgnoreCase))
        {
            role = "DM";
            SetupVideo(videoDM);
            descriptions = new string[]
            {
                "DM bertugas menjaga keseimbangan tim saat menyerang. Fokus utama adalah menjaga posisi di depan bek tengah, menjadi opsi passing aman, serta mengatur aliran bola dari belakang ke tengah.",

                "• Berada di zona tengah bawah (depan CB).\n" +
                "• Tidak maju terlalu tinggi.\n" +
                "• Tetap berada di grid poros sebagai jalur sirkulasi bola.",

                "• Selalu sediakan diri sebagai back-passing option.\n" +
                "• Jangan keluar dari zona tengah karena akan meninggalkan ruang kosong.\n" +
                "• Pilih passing aman dan cepat untuk menjaga ritme permainan.",
            };
        }
        else if (position.Equals("Gelandang Tengah", System.StringComparison.OrdinalIgnoreCase))
        {
            role = "CM";
            SetupVideo(videoCM);
            descriptions = new string[]
            {
                "CM adalah penghubung utama antara bertahan dan menyerang. Tugasnya menjaga ritme, membantu distribusi bola, serta mengisi ruang kosong untuk tetap menjaga struktur tim.",

                "• Berada di zona tengah-tengah lapangan.\n" +
                "• Bergerak horizontal mengikuti aliran bola.\n" +
                "• Tidak boleh terlalu maju (zona AM) atau terlalu turun (zona DM).",

                "• Selalu bergerak mencari ruang agar mudah menerima bola.\n" +
                "• Pastikan tetap berada di jalur tengah untuk menjaga koneksi antarlini.\n" +
                "• Pilih kombinasi dribble pendek dan passing untuk menjaga tempo.",
            };
        }

        UpdateContent(0);
        Tab(0);
    }

    // Method untuk setup video sesuai role
    void SetupVideo(VideoClip clip)
    {
        if (videoPlayer != null && clip != null)
        {
            videoPlayer.clip = clip;
            videoPlayer.isLooping = true; // Video akan loop
            videoPlayer.Prepare(); // Persiapkan video
        }
        else
        {
            Debug.LogWarning("VideoPlayer atau VideoClip tidak ditemukan!");
        }
    }

    public void Tab(int index)
    {
        if (index == currentTabIndex)
            return;

        if (index == 0)
        {
            tabAnimator.SetTrigger("TabText");
            // Pause video saat di tab text
            if (videoPlayer != null && videoPlayer.isPlaying)
                videoPlayer.Pause();
        }
        else if (index == 1)
        {
            tabAnimator.SetTrigger("TabVideo");
            // Play video saat di tab video
            if (videoPlayer != null && !videoPlayer.isPlaying)
                videoPlayer.Play();
        }

        StartCoroutine(SwitchTabAfterAnimation(index));
    }

    private IEnumerator SwitchTabAfterAnimation(int newIndex)
    {
        yield return new WaitForSeconds(0.1f);

        textPanel.SetActive(false);
        videoPanel.SetActive(false);

        if (newIndex == 0)
            textPanel.SetActive(true);
        else if (newIndex == 1)
            videoPanel.SetActive(true);

        UpdateTabs(newIndex);
        currentTabIndex = newIndex;
    }

    void UpdateTabs(int index)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            Image img = tabs[i].GetComponent<Image>();
            if (img != null)
            {
                img.color = (i == index) ? activeColorTab : inactiveColorTab;
            }
        }
    }

    public void ShowSlide(int newIndex)
    {
        if (newIndex == currentIndex || newIndex < 0 || newIndex >= descriptions.Length)
            return;

        StartCoroutine(SwitchSlide(newIndex));
    }

    IEnumerator SwitchSlide(int newIndex)
    {
        cardAnimator.SetTrigger("SlideLeft");
        yield return new WaitForSeconds(transitionDelay);
        currentIndex = newIndex;
        UpdateContent(currentIndex);
    }

    void UpdateContent(int index)
    {
        roleText.text = role;
        positionText.text = position;
        typeDescText.text = typeDesc[index];
        descText.text = descriptions[index];
        UpdateIndicators(index);
    }

    void UpdateIndicators(int index)
    {
        for (int i = 0; i < indicators.Length; i++)
        {
            indicators[i].color = (i == index) ? activeColor : inactiveColor;
        }
    }

    public void BackToMenu()
    {
        Debug.Log("BackToMenu");
        // Stop video sebelum pindah scene
        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();

        PlayerPrefs.SetString(keyClicked, "");
        SceneManager.LoadScene(nextSceneName);
    }

    void OnDestroy()
    {
        // Pastikan video berhenti saat object di-destroy
        if (videoPlayer != null)
            videoPlayer.Stop();
    }
}