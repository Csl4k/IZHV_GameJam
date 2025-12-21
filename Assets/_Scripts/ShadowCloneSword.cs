using UnityEngine;

public class ShadowCloneSword : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHP = other.GetComponent<PlayerHealth>();

            if (playerHP != null)
            {
                Transform source = transform.parent != null ? transform.parent : transform;
                playerHP.TakeDamage(damage, source);
            }
        }
    }
}