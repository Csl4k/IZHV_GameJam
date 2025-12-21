using UnityEngine;

public class PlayerParry : MonoBehaviour
{
    [Header("Parry Settings")]
    public float stunDuration = 2.0f;
    private PlayerAudio playerAudio;

    private void Awake()
    {
        playerAudio = GetComponentInParent<PlayerAudio>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("EnemyAttack")) return;

        // NORMAL ENEMY
        EnemyAI enemyAI = other.GetComponentInParent<EnemyAI>();
        if (enemyAI != null && enemyAI.IsAttacking)
        {
            EnemyHealth enemy = enemyAI.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.Stun(stunDuration);
                Debug.Log("PARRY SUCCESS (ENEMY)");
                playerAudio?.PlayParrySuccess();

            }
            return;
        }
        // BOSS
        BossAI bossAI = other.GetComponentInParent<BossAI>();
        if (bossAI != null && bossAI.IsAttacking)
        {
            bossAI.Stun(stunDuration);
            Debug.Log("PARRY SUCCESS (BOSS)");
            playerAudio?.PlayParrySuccess();

        }

    }
}
