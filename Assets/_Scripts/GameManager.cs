using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static int TotalGold = 0;
    public static int RunCount = 1;
    public static int CurrentPrice = 100;

    public static void CalculateNextPrice()
    {
        float multiplier = Mathf.Pow(1.5f, RunCount);
        CurrentPrice = Mathf.RoundToInt(100 * multiplier);
    }

    public static void AdvanceRun()
    {
        RunCount++;
        CalculateNextPrice();

        SceneManager.LoadScene("HubScene");
    }
}