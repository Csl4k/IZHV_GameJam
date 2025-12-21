using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Settings")]
    public int baseHealth = 3;     // starts at 3
    public int maxHealth;         // computed from armor
    public float knockbackForce = 15f;
    public float deathKnockbackForce = 50f; 
    public float flashDuration = 0.1f;
    public float stunDuration = 0.2f;

    [Header("Invulnerability")]
    public float invulnerabilityDuration = 1.0f;


    private PlayerController movementScript;
    private int currentHealth;
    private bool isDead = false;
    private Rigidbody2D rb;
    [Header("Visuals")]
    public SpriteRenderer sr;

    private Color originalColor;
    private float defaultDrag;
    public bool invulnerable = false;

    [Header("Death Visual")]
    public Sprite deathSprite;

    private PlayerAudio playerAudio;


    void Start()
    {
        maxHealth = ComputeMaxHealth();
        currentHealth = maxHealth;
        playerAudio = GetComponent<PlayerAudio>();

        rb = GetComponent<Rigidbody2D>();
        if (sr != null) originalColor = sr.color;

        // remember normal drag
        if (rb != null) defaultDrag = rb.drag;

        movementScript = GetComponent<PlayerController>();
        if (movementScript == null) Debug.LogError("Missing PlayerController!");
    }

    private int GetExtraHitsFromArmor(int armorLevel)
    {
        // Extra hits at armor: 1,3,6,10,15... (triangular milestones)
        int n = 0;
        int needed = 1;
        while (armorLevel >= needed)
        {
            n++;
            needed += (n + 1);
        }
        return n;
    }

    private int ComputeMaxHealth()
    {
        int armorLevel = Mathf.Max(0, GameManager.ArmorLevel);
        int extra = GetExtraHitsFromArmor(armorLevel);
        return baseHealth + extra;
    }

    public bool CanUsePotion()
    {
        if (isDead) return false;
        if (GameManager.HealthPotion <= 0) return false;
        if (currentHealth >= maxHealth) return false;
        return true;
    }

    public void UsePotion()
    {
        if (isDead) return;
        if (GameManager.HealthPotion <= 0) return;
        if (currentHealth >= maxHealth) return;

        GameManager.HealthPotion--;

        int healAmount = Mathf.CeilToInt(maxHealth / 3f); // ~1/3 max HP
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GameManager.TotalGold += 1000;
            Debug.Log("Cheat: +1000 gold");
        }
    }

    public void RecalculateHealthFromArmor(bool healToFull = true)
    {
        int oldMax = maxHealth;
        maxHealth = ComputeMaxHealth();

        if (healToFull)
            currentHealth = maxHealth;
        else
            currentHealth += (maxHealth - oldMax);
    }

    public void EnableInvulnerability()
    {
        invulnerable = true;
    }

    public void DisableInvulnerability()
    {
        invulnerable = false;
    }



    public void TakeDamage(int damage, Transform source)
    {
        if (isDead) return;
        if (invulnerable) return;
        playerAudio?.PlayTakeDamage();

        int resetTier = Mathf.Max(0, GameManager.MerchantIndex); // 0=Gregor run, 1=Viktor run, 2=Viktor II...
        float resetMultiplier = 1f + 0.12f * resetTier;          // tweak 0.12f (12% per reset)
        float incoming = Mathf.Max(1f, damage) * resetMultiplier;

        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(incoming));
        currentHealth -= finalDamage;

        //Debug.Log("Player HP: " + currentHealth);

        // determine which force to use
        float forceToUse = (currentHealth <= 0) ? deathKnockbackForce : knockbackForce;

        // knockback
        if (currentHealth > 0 || rb != null)
        {
            ApplyKnockback(source, forceToUse);
        }

        StartCoroutine(FlashRedRoutine());

        // start i-frames
        StartCoroutine(InvulnerabilityRoutine());

        if (currentHealth > 0)
        {
            StartCoroutine(RecoverRoutine());
        }
        else
        {
            StartCoroutine(DeathRoutine());
        }

    }

    IEnumerator InvulnerabilityRoutine()
    {
        invulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);

        invulnerable = false;
    }


    void ApplyKnockback(Transform source, float force)
    {
        if (rb != null && source != null)
        {
            if (movementScript != null) movementScript.enabled = false;

            rb.drag = 10f; // high drag for fast stop
            rb.velocity = Vector2.zero;

            Vector2 knockbackDir = (transform.position - source.position).normalized;

            rb.AddForce(knockbackDir * force, ForceMode2D.Impulse);
        }
    }

    IEnumerator RecoverRoutine()
    {
        yield return new WaitForSeconds(stunDuration);

        // reset Physics
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.drag = defaultDrag;
        }

        // re-enable controls
        if (movementScript != null && !isDead) movementScript.enabled = true;
    }

    IEnumerator DeathRoutine()
    {
        isDead = true;

        if (sr != null && deathSprite != null)
        {
            sr.sprite = deathSprite;
            sr.flipX = false; // optional: ensure consistent facing
        }


        yield return new WaitForSeconds(1.0f);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        yield return new WaitForSeconds(1.0f);

        GameManager.AdvanceRun();

        SceneManager.LoadScene("HubScene");
    }

    IEnumerator FlashRedRoutine()
    {
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            if (!isDead) sr.color = originalColor;
        }
    }

    public int CurrentHealth => currentHealth;


}