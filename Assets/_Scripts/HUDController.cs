using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HUDController : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text goldText;
    public TMP_Text priceText;

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
        BindToPlayer();
        RefreshHealthUI();
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
        if (priceText) priceText.text = "PRICE: " + GameManager.CurrentPrice;

        RefreshHealthUI();
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
