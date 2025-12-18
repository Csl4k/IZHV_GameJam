using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int hp = 3;
    public GameObject coinPrefab; // enemy drops this

    private Color color;
    private SpriteRenderer sr;

    public void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) color = sr.color;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log($"{gameObject.name} took damage! HP: {hp}");

        if (sr != null)
        {
            sr.color = Color.red;
            Invoke("ResetColor", 0.1f);
        }

        if (hp <= 0)
        {
            Die();
        }
    }

    void ResetColor()
    {
        if (sr != null) sr.color = color;
    }

    void Die()
    {
        // drop loot if the slot is not empty
        if (coinPrefab != null)
        {
            Instantiate(coinPrefab, transform.position, Quaternion.identity);
        }

        Debug.Log("Enemy destroyed!");
        Destroy(gameObject);
    }

}