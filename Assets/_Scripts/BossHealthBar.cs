using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Boss health bar UI that follows the boss
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthSlider;
    public TMP_Text healthText;
    public GameObject barContainer;

    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 2.5f, 0);
    public bool followBoss = true;

    private Transform bossTransform;
    private int maxHealth;
    private int currentHealth;
    private Camera mainCamera;

    void Start()
    {

        bossTransform = transform.parent;
        mainCamera = Camera.main;

        if (barContainer != null)
            barContainer.SetActive(false); // default hidden

    }

    public void SetVisible(bool visible)
    {
        if (barContainer != null)
            barContainer.SetActive(visible);
    }


    void LateUpdate()
    {
        if (followBoss && bossTransform != null && mainCamera != null)
        {
            // Position above boss
            Vector3 worldPos = bossTransform.position + offset;
            transform.position = worldPos;

            // Face camera
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }

    public void SetMaxHealth(int max)
    {
        maxHealth = max;
        currentHealth = max;

        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = max;
        }

        UpdateText();
    }

    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        UpdateText();
    }

    void UpdateText()
    {
        if (healthText != null)
            healthText.text = $"{currentHealth} / {maxHealth}";
    }
}