using UnityEngine;

public class PlayerSword : MonoBehaviour
{
    public int baseDamage = 2;
    private PlayerAudio playerAudio;

    void Awake()
    {
        playerAudio = GetComponentInParent<PlayerAudio>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Transform source = transform.parent != null ? transform.parent : transform;

        // Non-linear infinite scaling
        float bonus = 2.2f * Mathf.Log(1f + GameManager.SwordLevel);
        int damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage + bonus));
        
        // NORMAL ENEMIES
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            playerAudio?.PlayDealDamage();
            enemy.TakeDamage(damage, source);
            return;
        }

        // BOSS
        BossHealth boss = other.GetComponent<BossHealth>();
        if (boss != null)
        {
            playerAudio?.PlayDealDamage();
            boss.TakeDamage(damage, source);
        }
    }
}
