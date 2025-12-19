using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    public int hp = 3;
    public GameObject coinPrefab;

    [Header("Knockback Settings")]
    public float knockbackDeath = 15f;
    public float knockbackAlive = 5f;
    public float stunDuration = 0.2f;

    private Color color;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private EnemyAI aiScript;
    private bool isDead = false; // Prevents double-death logic

    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        aiScript = GetComponent<EnemyAI>();
        if (sr != null) color = sr.color;
    }

    public void TakeDamage(int damage, Transform source)
    {
        if (isDead) return; // Don't hit him if he's already dying

        hp -= damage;
        Debug.Log($"{gameObject.name} HP: {hp}");

        bool killingBlow = hp <= 0;
        float force = killingBlow ? knockbackDeath : knockbackAlive;

        // 1. Apply Physics (Knockback)
        if (rb != null && source != null)
        {
            // Stop AI movement logic so physics can take over
            if (aiScript != null) aiScript.enabled = false;

            rb.drag = 10f;
            rb.velocity = Vector2.zero;
            Vector2 dir = (transform.position - source.position).normalized;
            rb.AddForce(dir * force, ForceMode2D.Impulse);

            // If he survives, we need to recover. If he dies, DeathRoutine handles it.
            if (!killingBlow)
            {
                StartCoroutine(RecoverRoutine());
            }
        }

        // 2. Visual Flash
        if (sr != null && !killingBlow)
        {
            sr.color = Color.red;
            Invoke("ResetColor", 0.1f);
        }

        // 3. Handle Death
        if (killingBlow)
        {
            StartCoroutine(DeathRoutine());
        }
    }

    IEnumerator RecoverRoutine()
    {
        yield return new WaitForSeconds(stunDuration);
        if (aiScript != null && !isDead) aiScript.enabled = true;
    }

    IEnumerator DeathRoutine()
    {
        isDead = true;

        // A. Visuals: Turn Grey
        if (sr != null) sr.color = Color.grey;

        // B. Disable Combat: Make sure he can't hurt you anymore
        if (aiScript != null)
        {
            aiScript.enabled = false; // Stop thinking/moving

            // Explicitly turn off the sword collider if it was active
            if (aiScript.swordCollider != null)
            {
                aiScript.swordCollider.enabled = false;
            }
        }

        // C. Linger: Wait 2 seconds while the body slides to a stop
        yield return new WaitForSeconds(2f);

        // D. Loot & Destroy
        if (coinPrefab != null)
        {
            Instantiate(coinPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    void ResetColor()
    {
        if (sr != null && !isDead) sr.color = color;
    }
}