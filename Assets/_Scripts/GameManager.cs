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

    // Merchant chain:
    // 0 = Gregor
    // 1 = Viktor
    // 2 = Viktor II
    // 3 = Viktor III ...
    public static int MerchantIndex = 0;
    public static string LastMurderedMerchantName = "Gregor";

    public static int ShopInflationLevel = 0;
    public static bool IsUIOpen = false;


    // Small helper for nice UI naming (II, III, IV...)
    private static string ToRoman(int number)
    {
        // We only need small values; keep it simple.
        if (number <= 0) return "";
        string[] thousands = { "", "M", "MM", "MMM" };
        string[] hundreds =  { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" };
        string[] tens =      { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" };
        string[] ones =      { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };

        int t = number / 1000;
        int h = (number % 1000) / 100;
        int te = (number % 100) / 10;
        int o = number % 10;
        return thousands[Mathf.Clamp(t, 0, thousands.Length - 1)] +
               hundreds[h] +
               tens[te] +
               ones[o];
    }


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

    public static string GetMerchantDisplayName(int merchantIndex)
    {
        if (merchantIndex <= 0) return "Gregor";
        if (merchantIndex == 1) return "Viktor";
        return $"Viktor {ToRomanNumeral(merchantIndex)}";
    }

    public static string GetCurrentMerchantName()
    {
        return GetMerchantDisplayName(MerchantIndex);
    }

    public static string GetPreviousMerchantName()
    {
        return string.IsNullOrWhiteSpace(LastMurderedMerchantName) ? "Gregor" : LastMurderedMerchantName;
    }

    /// <summary>
    /// Call this when the player murders the currently active merchant.
    /// Updates the 'previous merchant' name and advances to the next merchant in the chain.
    /// </summary>
    public static void RegisterMerchantMurder()
    {
        LastMurderedMerchantName = GetCurrentMerchantName();
        MerchantIndex = Mathf.Max(0, MerchantIndex + 1);

        // We only need StoryState for gating the intro; after the first merchant dies, we stay in "post-intro".
        StoryState = 1;
    }

    private static string ToRomanNumeral(int number)
    {
        // We use Roman numerals only for Viktor 2+ for clarity.
        // 2 -> II, 3 -> III, 4 -> IV, ...
        if (number <= 0) return "";

        (int value, string symbol)[] map = new (int, string)[]
        {
            (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"),
            (100, "C"), (90, "XC"), (50, "L"), (40, "XL"),
            (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I")
        };

        int n = number;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < map.Length; i++)
        {
            while (n >= map[i].value)
            {
                sb.Append(map[i].symbol);
                n -= map[i].value;
            }
        }
        return sb.ToString();
    }
}