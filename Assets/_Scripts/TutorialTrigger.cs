using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial UI")]
    public GameObject tutorialPanel;
    public TMP_Text tutorialText;
    public Button closeButton;

    [Header("Tutorial Content")]
    [TextArea(5, 15)]
    public string tutorialMessage = @"<b>CONTROLS</b>

<b>Movement:</b> WASD or Arrow Keys
<b>Attack:</b> Left Mouse Button
<b>Parry:</b> Right Mouse Button (time it right!)
<b>Dodge:</b> Space Bar

<b>OBJECTIVE</b>
Defeat enemies, collect gold, and buy upgrades from merchants.
But beware... a dangerous enemy lurks underground.

Press any key to close";

    [Header("Settings")]
    public bool showOnlyOnce = true;
    public float triggerDistance = 3f;
    public bool autoShow = true;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip tutorialOpenSound;
    public AudioClip tutorialCloseSound;

    [Header("Visual Indicator")]
    public GameObject promptIndicator;
    public string promptText = "Press E for Tutorial";

    private Transform player;
    private bool hasShown = false;
    private bool isPlayerNearby = false;
    private bool isTutorialOpen = false;
    private TMP_Text promptTextComponent;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Setup UI
        if (tutorialPanel) tutorialPanel.SetActive(false);
        if (tutorialText) tutorialText.text = tutorialMessage;
        if (closeButton) closeButton.onClick.AddListener(CloseTutorial);

        // Setup prompt indicator
        if (promptIndicator)
        {
            promptIndicator.SetActive(false);
            promptTextComponent = promptIndicator.GetComponentInChildren<TMP_Text>();
            if (promptTextComponent) promptTextComponent.text = promptText;
        }

        // Check if already shown
        if (showOnlyOnce && PlayerPrefs.GetInt("TutorialShown", 0) == 1)
        {
            hasShown = true;
        }
    }

    void Update()
    {
        if (player == null || hasShown) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool nearbyNow = distance <= triggerDistance;

        // Player entered range
        if (nearbyNow && !isPlayerNearby)
        {
            isPlayerNearby = true;
            OnPlayerEnterRange();
        }
        // Player left range
        else if (!nearbyNow && isPlayerNearby)
        {
            isPlayerNearby = false;
            OnPlayerExitRange();
        }

        // Manual trigger with E key
        if (isPlayerNearby && !isTutorialOpen && Input.GetKeyDown(KeyCode.E))
        {
            ShowTutorial();
        }

        // Close tutorial with any key
        if (isTutorialOpen && Input.anyKeyDown)
        {
            CloseTutorial();
        }
    }

    void OnPlayerEnterRange()
    {
        if (promptIndicator)
            promptIndicator.SetActive(true);

        if (autoShow && !isTutorialOpen)
        {
            ShowTutorial();
        }
    }

    void OnPlayerExitRange()
    {
        if (promptIndicator)
            promptIndicator.SetActive(false);
    }

    public void ShowTutorial()
    {
        if (isTutorialOpen) return;

        isTutorialOpen = true;
        GameManager.IsUIOpen = true;

        if (tutorialPanel) tutorialPanel.SetActive(true);
        if (promptIndicator) promptIndicator.SetActive(false);

        PlaySound(tutorialOpenSound);

        // Pause game (optional)
        Time.timeScale = 0f;

        // Disable player controls
        PlayerController pc = player?.GetComponent<PlayerController>();
        if (pc) pc.enabled = false;
    }

    public void CloseTutorial()
    {
        if (!isTutorialOpen) return;

        isTutorialOpen = false;
        GameManager.IsUIOpen = false;

        if (tutorialPanel) tutorialPanel.SetActive(false);

        PlaySound(tutorialCloseSound);

        // Unpause game
        Time.timeScale = 1f;

        // Re-enable player controls
        PlayerController pc = player?.GetComponent<PlayerController>();
        if (pc) pc.enabled = true;

        // Mark as shown
        if (showOnlyOnce)
        {
            hasShown = true;
            PlayerPrefs.SetInt("TutorialShown", 1);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    // Visualize trigger range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}