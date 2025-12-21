using UnityEngine;

public class BossSword : MonoBehaviour
{
    [Header("Settings")]
    public int baseDamage = 2;
    public float damageScaling = 0.15f; // 15% per merchant tier
    private Collider2D col;

    private void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            gameObject.tag = "EnemyAttack";
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHP = other.GetComponent<PlayerHealth>();

            if (playerHP != null)
            {
                // Scale damage based on merchant murders (difficulty progression)
                int tier = Mathf.Max(0, GameManager.MerchantIndex);
                float multiplier = 1f + damageScaling * tier;
                int damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * multiplier));

                Transform source = transform.parent != null ? transform.parent : transform;
                playerHP.TakeDamage(damage, source);
            }
        }
    }
}