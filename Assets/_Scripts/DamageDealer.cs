using UnityEngine;

public class PlayerSword : MonoBehaviour
{
    public int baseDamage = 2;

    private void OnTriggerEnter2D(Collider2D other)
    {

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            // pass 'transform.parent' (The Player) as the source.
            // if the sword is not childed to the player, just use 'transform'.
            Transform source = transform.parent != null ? transform.parent : transform;

            // Non-linear infinite scaling
            float bonus = 2.2f * Mathf.Log(1f + GameManager.SwordLevel); // tweak 2.2f
            int damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage + bonus));

            enemy.TakeDamage(damage, source);
        }
    }
}