using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Stats")]
    public int baseHP = 50;
    private int maxHP;
    private int currentHP;
    public float healthPerMerchant = 0.3f;

    [Header("Knockback Settings")]
    public float knockbackForce = 8f;
    public float stunDuration = 0.3f;

    [Header("Visuals")]
    public SpriteRenderer sr;
    public Color damageColor = Color.red;

    [Header("UI")]
    public GameObject healthBarPrefab;
    private BossHealthBar healthBar;

    private Rigidbody2D rb;
    private BossAI aiScript;
    private bool isDead = false;


    public float HealthPercent => (maxHP <= 0) ? 1f : (float)currentHP / maxHP;

    void Start()
    {


        int tier = Mathf.Max(0, GameManager.MerchantIndex);
        float mult = 1f + healthPerMerchant * tier;
        maxHP = Mathf.Max(1, Mathf.RoundToInt(baseHP * mult));
        currentHP = maxHP;

        rb = GetComponent<Rigidbody2D>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.angularVelocity = 0f;
        }
        aiScript = GetComponent<BossAI>();

        if (healthBarPrefab != null)
        {
            GameObject hbObj = Instantiate(healthBarPrefab, transform);
            healthBar = hbObj.GetComponent<BossHealthBar>();
            if (healthBar != null)
            {
                healthBar.SetMaxHealth(maxHP);
                healthBar.SetHealth(currentHP);
            }
        }
    }

    public void SetHealthBarVisible(bool visible)
    {
        if (healthBar != null)
            healthBar.SetVisible(visible);
    }


    public void TakeDamage(int damage, Transform source)
    {
        if (isDead) return;

        currentHP -= damage;

        if (healthBar != null)
            healthBar.SetHealth(currentHP);

        // Knockback
        if (rb != null && source != null)
        {
            Vector2 dir = (transform.position - source.position).normalized;
            rb.velocity = Vector2.zero;
            rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        }

        if (aiScript != null)
        {
            aiScript.PlayHitFeedback(); // RED on hit

            if (damage >= 3)
            {
                aiScript.Stun(stunDuration);
            }
        }

        else
        {
            StartCoroutine(FlashDamageFallback());
        }

        if (currentHP <= 0)
            Die();
    }

    System.Collections.IEnumerator FlashDamageFallback()
    {
        if (sr == null) yield break;

        Color before = sr.color;
        sr.color = damageColor;
        yield return new WaitForSeconds(0.08f);
        if (!isDead) sr.color = before;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        SetHealthBarVisible(false);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        if (sr != null)
            sr.color = Color.gray;

        // Stop boss AI
        if (aiScript != null)
        {
            aiScript.OnBossDeath();
            aiScript.enabled = false;
        }


        // Boss defeated flag
        GameManager.SetBossDefeated();

        // Trigger cutscene
        StartCoroutine(TriggerDefeatCutscene());
    }

    System.Collections.IEnumerator TriggerDefeatCutscene()
    {
        yield return new WaitForSeconds(1.2f);

        BossCutscene cutscene = FindObjectOfType<BossCutscene>();
        if (cutscene != null)
            cutscene.TriggerBossDefeatCutscene();
    }

    public bool IsDead => isDead;
    public int CurrentHP => currentHP;
}
