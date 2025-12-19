using UnityEngine;

public class PlayerSword : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            // pass 'transform.parent' (The Player) as the source.
            // if the sword is not childed to the player, just use 'transform'.
            Transform source = transform.parent != null ? transform.parent : transform;

            enemy.TakeDamage(damage, source);
        }
    }
}