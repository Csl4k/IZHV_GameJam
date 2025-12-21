using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HUDController : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text goldText;
    public TMP_Text priceText;

    [Header("Stats UI Backgrounds")]
    public GameObject potionBG;
    public GameObject swordBG;
    public GameObject armorBG;
    public GameObject torchBG;

    [Header("Stats UI Text")]
    public TMP_Text potionText;
    public TMP_Text swordText;
    public TMP_Text armorText;
    public TMP_Text torchText;


    [Header("Health UI")]
    public Slider healthSlider;

    private PlayerHealth playerHealth;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (potionBG) potionBG.SetActive(false);
        if (swordBG) swordBG.SetActive(false);
        if (armorBG) armorBG.SetActive(false);
        if (torchBG) torchBG.SetActive(false);
        BindToPlayer();
        RefreshHealthUI();
    }
    void RefreshStatsUI()
    {
        if (potionBG) potionBG.SetActive(true);
        if (swordBG) swordBG.SetActive(true);
        if (armorBG) armorBG.SetActive(true);
        if (torchBG) torchBG.SetActive(GameManager.TorchLevel > 0);

        if (potionText)
            potionText.text = $"Potions: {GameManager.HealthPotion}";

        if (swordText)
            swordText.text = $"Sword sharpness: {GameManager.SwordLevel}";

        if (armorText)
            armorText.text = $"Armor plates: {GameManager.ArmorLevel}";

        if (torchText)
        {
            if (GameManager.TorchLevel > 0)
                torchText.text = "Torch bought";
            else
                torchText.text = "No torch";
        }
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindToPlayer();
        RefreshHealthUI();
    }

    void BindToPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        playerHealth = playerObj ? playerObj.GetComponent<PlayerHealth>() : null;
    }

    void Update()
    {
        if (goldText) goldText.text = "GOLD: " + GameManager.TotalGold;
        if (priceText) priceText.text = "FREEDOM: " + GameManager.GetFreedomPrice();


        RefreshHealthUI();
        RefreshStatsUI();
    }

    void RefreshHealthUI()
    {
        if (healthSlider == null) return;

        if (playerHealth == null)
        {
            BindToPlayer();
            if (playerHealth == null)
            {
                healthSlider.value = 0;
                return;
            }
        }

        healthSlider.maxValue = playerHealth.maxHealth;
        healthSlider.value = playerHealth.CurrentHealth;
    }
}
