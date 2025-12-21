using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int goldAmount = 10;

    [Header("Sound")]
    public AudioClip pickupSound;
    [Range(0f, 1f)] public float volume = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.TotalGold += goldAmount;

            Debug.Log("Picked up coin! Total Gold: " + GameManager.TotalGold);

            if (pickupSound)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, volume);

            Destroy(gameObject);
        }
    }
}
