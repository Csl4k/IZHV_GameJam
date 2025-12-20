using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Settings")]
    public int maxHealth = 3;
    public float knockbackForce = 15f;
    public float deathKnockbackForce = 50f; 
    public float flashDuration = 0.1f;
    public float stunDuration = 0.2f;

    private PlayerController movementScript;
    private int currentHealth;
    private bool isDead = false;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Color originalColor;
    private float defaultDrag;


    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        // remember normal drag
        if (rb != null) defaultDrag = rb.drag;

        movementScript = GetComponent<PlayerController>();
        if (movementScript == null) Debug.LogError("Missing PlayerController!");
    }

    public void UsePotion()
    {
        if (isDead) return;
        if (GameManager.HealthPotion <= 0) return;
        if (currentHealth >= maxHealth) return;

        GameManager.HealthPotion--;
        currentHealth = Mathf.Min(maxHealth, currentHealth + 1);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GameManager.TotalGold += 1000;
            Debug.Log("Cheat: +1000 gold");
        }
    }

    public void TakeDamage(int damage, Transform source)
    {
        if (isDead) return;
        int reduced = Mathf.Max(1, damage - GameManager.ArmorLevel);

        currentHealth -= reduced;
        //Debug.Log("Player HP: " + currentHealth);

        // determine which force to use
        float forceToUse = (currentHealth <= 0) ? deathKnockbackForce : knockbackForce;

        // knockback
        if (currentHealth > 0 || rb != null)
        {
            ApplyKnockback(source, forceToUse);
        }

        StartCoroutine(FlashRedRoutine());

        if (currentHealth > 0)
        {
            StartCoroutine(RecoverRoutine());
        }
        else
        {
            StartCoroutine(DeathRoutine());
        }
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

        if (sr != null) sr.color = Color.grey;

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