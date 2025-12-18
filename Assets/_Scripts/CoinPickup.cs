using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int goldAmount = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.TotalGold += goldAmount;

            Debug.Log("Picked up coin! Total Gold: " + GameManager.TotalGold);

            Destroy(gameObject);
        }
    }
}