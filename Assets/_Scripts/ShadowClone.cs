using UnityEngine;
using System.Collections;

public class ShadowClone : MonoBehaviour
{
    [Header("Stats")]
    public int hp = 10;
    public float moveSpeed = 3f;
    public float attackRange = 2.5f;
    public float attackCooldown = 2f;

    [Header("Damage")]
    public int damage = 1;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Color shadowColor = new Color(0.3f, 0.3f, 0.5f, 0.7f);

    [Header("References")]
    public Collider2D swordCollider;
    public Transform weaponPivot;

    private Transform player;
    private Rigidbody2D rb;
    private bool isDead = false;
    private bool isAttacking = false;
    private float lastAttackTime = -999f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.color = shadowColor;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(PerformAttack());
        }
        else if (!isAttacking)
        {
            ChasePlayer();
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
        RotateToPlayer();
    }

    void RotateToPlayer()
    {
        if (weaponPivot != null && player != null)
        {
            Vector2 dir = (player.position - weaponPivot.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        rb.velocity = Vector2.zero;

        if (swordCollider != null) swordCollider.enabled = true;

        yield return new WaitForSeconds(0.3f);

        if (swordCollider != null) swordCollider.enabled = false;

        isAttacking = false;
    }

    public void TakeDamage(int damage, Transform source)
    {
        if (isDead) return;

        hp -= damage;

        StartCoroutine(FlashRed());

        if (rb != null && source != null)
        {
            Vector2 dir = (transform.position - source.position).normalized;
            rb.velocity = Vector2.zero;
            rb.AddForce(dir * 8f, ForceMode2D.Impulse);
        }

        if (hp <= 0)
        {
            Die();
        }
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            Color original = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (!isDead) spriteRenderer.color = original;
        }
    }

    void Die()
    {
        isDead = true;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = Color.gray;

        if (swordCollider != null)
            swordCollider.enabled = false;



        Destroy(gameObject, 1f);
    }
}