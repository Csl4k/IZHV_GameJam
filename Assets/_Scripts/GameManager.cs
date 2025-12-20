using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Economy")]
    public static int TotalGold = 0;
    public static int RunCount = 1;

    [Header("Inventory")]
    public static int HealthPotion = 0;
    public static int SwordLevel = 0;
    public static int ArmorLevel = 0;
    public static int TorchLevel = 0;


    [Header("Narrative State")]
    public static int StoryState = 0;

    public static int ShopInflationLevel = 0;
    public static bool IsUIOpen = false;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void AdvanceRun()
    {
        RunCount++;
    }

    public static int GetShopPrice(int basePrice)
    {
        return Mathf.RoundToInt(basePrice * Mathf.Pow(1.5f, RunCount - 1));
    }

    public static int GetGoodsPrice(int basePrice)
    {
        float multiplier = Mathf.Pow(1.5f, ShopInflationLevel);
        return Mathf.RoundToInt(basePrice * multiplier);
    }

    public static int GetFreedomPrice(int basePrice)
    {
        float multiplier = Mathf.Pow(1.5f, Mathf.Max(0, RunCount - 1));
        return Mathf.RoundToInt(basePrice * multiplier);
    }

    public static int CurrentPrice
    {
        get { return GetShopPrice(100); }
    }
}