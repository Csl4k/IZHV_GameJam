using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    public int hp = 3;
    public float healthPerMerchant = 0.25f;
    public GameObject coinPrefab;

    [Header("Knockback Settings")]
    public float knockbackDeath = 15f;
    public float knockbackAlive = 5f;
    public float stunDuration = 0.2f;

    [Header("Death Visual")]
    public Sprite deathSprite;


    private Color color;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private EnemyAI aiScript;
    private bool isDead = false; // Prevents double-death logic

    public void Start()
    {
        int tier = Mathf.Max(0, GameManager.MerchantIndex);

        float mult = 1f + healthPerMerchant * tier;

        // Scale hp once per spawn. This increases enemy health each new merchant.
        hp = Mathf.Max(1, Mathf.RoundToInt(hp * mult));

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        aiScript = GetComponent<EnemyAI>();
    }




    public void TakeDamage(int damage, Transform source)
    {
        if (isDead) return;

        hp -= damage;
        //Debug.Log($"{gameObject.name} HP: {hp}");

        bool killingBlow = hp <= 0;
        float force = killingBlow ? knockbackDeath : knockbackAlive;

        if (rb != null && source != null)
        {
            if (aiScript != null) aiScript.enabled = false;

            rb.drag = 10f;
            rb.velocity = Vector2.zero;
            Vector2 dir = (transform.position - source.position).normalized;
            rb.AddForce(dir * force, ForceMode2D.Impulse);

            if (!killingBlow)
            {
                StartCoroutine(RecoverRoutine());
            }
        }

        if (sr != null && !killingBlow)
        {
            sr.color = Color.red;
            Invoke("ResetColor", 0.1f);
        }

        if (killingBlow)
        {
            StartCoroutine(DeathRoutine());
        }
    }

    public void Stun(float duration)
    {
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.ApplyStun(duration);
    }


    IEnumerator RecoverRoutine()
    {
        yield return new WaitForSeconds(stunDuration);
        if (aiScript != null && !isDead) aiScript.enabled = true;
    }

    IEnumerator DeathRoutine()
    {
        isDead = true;
        if (sr != null && deathSprite != null)
        {
            sr.sprite = deathSprite;
            sr.flipX = false;
        }


        if (aiScript != null) aiScript.OnDeath();

        if (sr != null) sr.color = Color.grey;

        if (aiScript != null)
        {
            aiScript.enabled = false;

            if (aiScript.swordCollider != null)
            {
                aiScript.swordCollider.enabled = false;
            }
        }

        yield return new WaitForSeconds(2f);

        if (coinPrefab != null)
        {
            Instantiate(coinPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    void ResetColor()
    {
        if (sr == null || isDead) return;

        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null && ai.IsStunned)
            sr.color = Color.yellow;
        else if (ai != null)
            sr.color = ai.BaseColor;
        else
            sr.color = Color.white;
    }

}