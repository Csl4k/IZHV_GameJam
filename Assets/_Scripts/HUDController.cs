using UnityEngine;
using TMPro; 

public class HUDController : MonoBehaviour
{
    public TMP_Text goldText;
    public TMP_Text priceText;

    void Update()
    {
        goldText.text = "GOLD: " + GameManager.TotalGold.ToString();
        priceText.text = "PRICE: " + GameManager.CurrentPrice.ToString();
    }
}