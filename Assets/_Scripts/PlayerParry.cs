using UnityEngine;

public class PlayerParry : MonoBehaviour
{
    [Header("Parry Settings")]
    public float stunDuration = 2.0f;

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (!other.CompareTag("EnemyAttack")) return;

        EnemyAI ai = other.GetComponentInParent<EnemyAI>();
        if (ai == null) return;

        if (!ai.IsAttacking) return;

        Debug.Log("PARRY SUCCESS!");

        EnemyHealth enemy = ai.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.Stun(stunDuration);
        }
    }
}
