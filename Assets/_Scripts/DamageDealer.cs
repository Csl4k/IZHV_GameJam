using UnityEngine;

public class PlayerSword : MonoBehaviour
{
    public int baseDamage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            // pass 'transform.parent' (The Player) as the source.
            // if the sword is not childed to the player, just use 'transform'.
            Transform source = transform.parent != null ? transform.parent : transform;

            int damage = baseDamage + GameManager.SwordLevel / 2;
            enemy.TakeDamage(damage, source);
        }
    }
}