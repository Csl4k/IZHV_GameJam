using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Settings")]
    public int maxHealth = 3;
    public float knockbackForce = 15f;
    public float flashDuration = 0.1f;
    public float stunDuration = 0.2f;

    private PlayerController movementScript;

    private int currentHealth;
    private bool isDead = false;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        movementScript = GetComponent<PlayerController>();

        if (movementScript == null)
        {
            Debug.LogError("Could not find 'PlayerController' script on the Player");
        }
    }

    public void TakeDamage(int damage, Transform source)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("Player HP: " + currentHealth);

        StartCoroutine(FlashRedRoutine());

        if (currentHealth > 0)
        {
            ApplyKnockback(source);
        }
        else
        {
            Die();
        }
    }

    void ApplyKnockback(Transform source)
    {
        if (rb != null && source != null)
        {
            // disable Controls
            if (movementScript != null) movementScript.enabled = false;

            // reset velocity and push
            rb.velocity = Vector2.zero;
            Vector2 knockbackDir = (transform.position - source.position).normalized;
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

            StartCoroutine(RecoverRoutine());
        }
    }

    IEnumerator RecoverRoutine()
    {
        yield return new WaitForSeconds(stunDuration);

        // Re-enable controls
        if (movementScript != null) movementScript.enabled = true;
    }

    IEnumerator FlashRedRoutine()
    {
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            sr.color = originalColor;
        }
    }

    void Die()
    {
        isDead = true;
        if (sr != null) sr.color = Color.grey;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        if (movementScript != null) movementScript.enabled = false;

        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(2f); // TODO future death screen
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}