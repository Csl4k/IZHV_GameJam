using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;

    [Header("Buttons")]
    public Button startButton;
    public Button optionsButton;
    public Button quitButton;
    public Button backButton;

    [Header("Options")]
    public Slider volumeSlider;           // 0..1
    public string volumePrefKey = "MasterVolume";

    [Header("Scene")]
    public string gameSceneName = "HubScene";

    [Header("UI Audio")]
    public AudioSource uiAudioSource;
    public AudioClip hoverClip;
    public AudioClip clickClip;
    public AudioClip startGameClip;

    void Start()
    {
        ShowMainMenu();

        // Button listeners
        if (startButton) startButton.onClick.AddListener(() => StartGame());
        if (optionsButton) optionsButton.onClick.AddListener(() => { PlayClick(); ShowOptions(); });
        if (quitButton) quitButton.onClick.AddListener(() => { PlayClick(); QuitGame(); });
        if (backButton) backButton.onClick.AddListener(() => { PlayClick(); ShowMainMenu(); });

        WireButtonSounds(startButton);
        WireButtonSounds(optionsButton);
        WireButtonSounds(quitButton);
        WireButtonSounds(backButton);

        // Options
        SetupVolume();
    }

    void SetupVolume()
    {
        if (!volumeSlider) return;

        float saved = Mathf.Clamp01(PlayerPrefs.GetFloat(volumePrefKey, 1f));
        volumeSlider.SetValueWithoutNotify(saved);
        AudioListener.volume = saved;

        volumeSlider.onValueChanged.AddListener(SetMasterVolume);
    }

    void SetMasterVolume(float value)
    {
        value = Mathf.Clamp01(value);
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(volumePrefKey, value);
        PlayerPrefs.Save();
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(true);
        if (optionsPanel) optionsPanel.SetActive(false);
    }

    public void ShowOptions()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(true);
    }

    public void StartGame()
    {
        // special start sound (fallback to click)
        if (startGameClip) Play(startGameClip);
        else PlayClick();

        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- Audio helpers ----------
    void PlayHover() => Play(hoverClip);
    void PlayClick() => Play(clickClip);

    void Play(AudioClip clip)
    {
        if (!clip || !uiAudioSource) return;
        uiAudioSource.PlayOneShot(clip);
    }


    void WireButtonSounds(Button btn)
    {
        if (!btn) return;

        if (btn == startButton)
            btn.onClick.AddListener(PlayClick);


        var trigger = btn.GetComponent<EventTrigger>();
        if (!trigger) trigger = btn.gameObject.AddComponent<EventTrigger>();
        if (trigger.triggers == null) trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entry.callback.AddListener(_ => PlayHover());
        trigger.triggers.Add(entry);
    }
}
