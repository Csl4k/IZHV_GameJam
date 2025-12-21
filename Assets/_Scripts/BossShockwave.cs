using UnityEngine;

/// <summary>
/// Expanding shockwave that damages player
/// </summary>
public class BossShockwave : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 1;
    public float knockbackForce = 10f;

    [Header("Expansion")]
    public float maxScale = 8f;
    public float expansionSpeed = 5f;
    public float lifetime = 2f;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;

    private float spawnTime;
    private bool hasHitPlayer = false;
    private Collider2D col;

    void Start()
    {
        spawnTime = Time.time;
        col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            gameObject.tag = "EnemyAttack";
        }

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }


    void Update()
    {
        // Expand
        float scale = Mathf.Lerp(0.5f, maxScale, (Time.time - spawnTime) / lifetime);
        transform.localScale = Vector3.one * scale;

        // Fade out
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = Mathf.Lerp(1f, 0f, (Time.time - spawnTime) / lifetime);
            spriteRenderer.color = c;
        }

        // Destroy after lifetime
        if (Time.time >= spawnTime + lifetime)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitPlayer) return;

        if (other.CompareTag("Player"))
        {
            hasHitPlayer = true;

            PlayerHealth playerHP = other.GetComponent<PlayerHealth>();
            if (playerHP != null)
            {
                playerHP.TakeDamage(damage, transform);
            }
        }
    }
}