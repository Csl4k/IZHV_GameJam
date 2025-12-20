using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("Visual Elements")]
    public GameObject rootPanel;
    public TMP_Text nameLabel;
    public TMP_Text dialogueLabel;
    public Image portraitImage;

    [Header("Shop Containers")]
    public GameObject shopButtonsContainer;

    [Header("Buttons")]
    public Button btnPotion;
    public Button btnSword;
    public Button btnArmor;
    public Button btnTorch;
    public Button btnFreedom;
    public Button btnAttack;

    [Header("Button Labels")]
    public TMP_Text txtPotion;
    public TMP_Text txtSword;
    public TMP_Text txtArmor;
    public TMP_Text txtTorch;
    public TMP_Text txtFreedom;

    [Header("Portrait Sprites")]
    public Sprite gregorFace;
    public Sprite viktorFace;
    public Sprite guardFace;
    public Sprite headGuardFace;

    [Header("Kill Button Settings")]
    public int killUnlockRun = 8;
    public Color killButtonColor = new Color(0.8f, 0.1f, 0.1f);

    private MerchantCutscene currentMerchant;

    void Start()
    {
        rootPanel.SetActive(false);

        if (btnAttack)
        {
            ColorBlock colors = btnAttack.colors;
            colors.normalColor = killButtonColor;
            colors.highlightedColor = Color.red;
            btnAttack.colors = colors;
        }
    }


    public void OpenShop(MerchantCutscene merchant)
    {
        GameManager.IsUIOpen = true;

        rootPanel.SetActive(true);
        shopButtonsContainer.SetActive(true);
        currentMerchant = merchant;
        RefreshUI();
    }


    public void ShowNarrative(string name, string text)
    {
        rootPanel.SetActive(true);
        shopButtonsContainer.SetActive(false);

        nameLabel.text = name;
        dialogueLabel.text = text;
    }

    public void CloseShop()
    {
        GameManager.IsUIOpen = false;
        rootPanel.SetActive(false);
        if (portraitImage != null) portraitImage.gameObject.SetActive(false);
    }

    void RefreshUI()
    {
        RefreshPricesAndButtons();
        RefreshMerchantHeader(overwriteDialogue: true);
    }

    void RefreshPricesAndButtons()
    {

        int potionPrice = GameManager.GetPotionPrice(30, GameManager.MerchantIndex);
        int swordPrice = GameManager.GetUpgradePrice(50, GameManager.SwordLevel);
        int armorPrice = GameManager.GetUpgradePrice(40, GameManager.ArmorLevel);
        int torchPrice = GameManager.GetUpgradePrice(100, GameManager.TorchLevel);
        int freedomPrice = GameManager.GetFreedomPrice(100);

        if (txtPotion) txtPotion.text = $"<color=white>Health Potion</color> <color=yellow>{potionPrice}g</color>";
        if (txtSword) txtSword.text = $"<color=white>Sharpen Blade</color> <color=yellow>{swordPrice}g</color>";
        if (txtArmor) txtArmor.text = $"<color=white>Repair Armor</color> <color=yellow>{armorPrice}g</color>";
        if (txtFreedom) txtFreedom.text = $"<size=120%><color=white>FREEDOM</color></size> <color=#FFD700>{freedomPrice}g</color>";

        bool torchOwned = GameManager.TorchLevel > 0;
        if (btnTorch)
            btnTorch.interactable = !torchOwned;

        if (txtTorch)
        {
            if (torchOwned)
                txtTorch.text = "<color=grey>Old Torch (OWNED)</color>";
            else
                txtTorch.text = $"<color=white>Old Torch</color> <color=yellow>{torchPrice}g</color>";
        }


        bool canKillMerchant = GameManager.RunCount >= killUnlockRun;

        if (btnAttack)
        {
            btnAttack.gameObject.SetActive(canKillMerchant);

            if (canKillMerchant)
            {
                TMP_Text attackButtonText = btnAttack.GetComponentInChildren<TMP_Text>();
                if (attackButtonText)
                {
                    attackButtonText.text = "<color=red>[STRIKE]</color>";
                }

                btnAttack.onClick.RemoveAllListeners();
                btnAttack.onClick.AddListener(ClickAttack);
            }
        }
    }

    void RefreshMerchantHeader(bool overwriteDialogue)
    {
        if (currentMerchant == null) return;

        // Always pull the *current* merchant name from the global merchant chain.
        // This guarantees Viktor II, Viktor III, ... are named correctly everywhere.
        string merchantName = GameManager.GetCurrentMerchantName();
        Sprite merchantPortrait = (GameManager.MerchantIndex <= 0) ? gregorFace : viktorFace;

        nameLabel.text = merchantName;

        if (overwriteDialogue)
        {
            dialogueLabel.text = currentMerchant.GetMerchantDialogue();
        }

        if (portraitImage != null && merchantPortrait != null)
        {
            portraitImage.sprite = merchantPortrait;
            portraitImage.gameObject.SetActive(true);
        }
    }


    public void BuyPotion() => AttemptBuy(30, "Health Potion", "potion");
    public void BuySword() => AttemptBuy(50, "Sharp Blade", "sword");
    public void BuyArmor() => AttemptBuy(40, "Iron Armor", "armor");
    public void BuyTorch() => AttemptBuy(100, "Old Torch", "torch");

    public void BuyFreedom()
    {
        int cost = GameManager.GetFreedomPrice(100);

        if (GameManager.TotalGold >= cost)
        {

            dialogueLabel.text = GetMerchantName() + ": \"Wait... you actually have it?\n\n...I... uh... Come back tomorrow.\"";
            
        }
        else
        {
            int shortage = cost - GameManager.TotalGold;
            
            string[] failLines = new string[]
            {
                $"\"You're short by {shortage}g. Get back in the hole.\"",
                $"\"Not even close. You need {shortage}g more.\"",
                $"\"Come back when you're serious. {shortage}g short.\"",
                $"\"Freedom isn't free. Still need {shortage}g.\"",
                $"\"Math isn't your strong suit, huh? {shortage}g short.\"",
            };

            dialogueLabel.text = GetMerchantName() + ": " + failLines[Random.Range(0, failLines.Length)];
        }
    }


    public void ClickAttack()
    {
        if (currentMerchant != null)
        {
            if (GameManager.RunCount == killUnlockRun) // First time seeing the option
            {
                dialogueLabel.text = "<color=red>You feel a dark impulse...\n\nAre you sure?</color>";
                
                // For safety, require double-click or separate confirm button
                // For now, we'll trigger immediately
            }

            currentMerchant.StartMurderSequence();
            CloseShop();
        }
    }

    void AttemptBuy(int baseCost, string itemName, string itemType)
    {
        int cost =
        (itemType == "potion") ? GameManager.GetPotionPrice(baseCost, GameManager.MerchantIndex) :
        (itemType == "sword") ? GameManager.GetUpgradePrice(baseCost, GameManager.SwordLevel) :
        (itemType == "armor") ? GameManager.GetUpgradePrice(baseCost, GameManager.ArmorLevel) :
        (itemType == "torch") ? GameManager.GetUpgradePrice(baseCost, GameManager.TorchLevel) :
        GameManager.GetGoodsPrice(baseCost); // fallback


        if (itemType == "potion" && GameManager.HealthPotion >= 5)
        {
            dialogueLabel.text = GetMerchantName() + ": \"No more. You can only carry 5.\"";
            RefreshPricesAndButtons();
            return;
        }

        // Check if already owned (torch only)
        if (itemType == "torch" && GameManager.TorchLevel > 0)
        {
            dialogueLabel.text = GetMerchantName() + ": \"You already bought that.\"";
            RefreshPricesAndButtons();
            return;
        }

        // Check if player can afford
        if (GameManager.TotalGold >= cost)
        {
            // Deduct gold
            GameManager.TotalGold -= cost;

            // Give item
            switch (itemType)
            {
                case "potion":
                    GameManager.HealthPotion++;
                    break;
                case "sword":
                    GameManager.SwordLevel++;
                    break;
                case "armor":
                    GameManager.ArmorLevel++;
                    var ph = FindObjectOfType<PlayerHealth>();
                    if (ph != null) ph.RecalculateHealthFromArmor(healToFull: false);
                    break;
                case "torch":
                    GameManager.TorchLevel = 1;
                    break;
            }

            // Success feedback with variety
            string[] successLines = new string[]
            {
                "\"Sold. Pleasure doing business.\"",
                "\"Smart purchase. You'll need it down there.\"",
                "\"There you go. Don't die too quick now.\"",
                "\"Good choice. That'll keep you alive... maybe.\"",
                "\"One step closer to... something.\"",
                "\"Business is good today.\"",
            };

            dialogueLabel.text = GetMerchantName() + ": " + successLines[Random.Range(0, successLines.Length)];

            RefreshPricesAndButtons();
        }
        else
        {
            // Not enough gold
            int shortage = cost - GameManager.TotalGold;

            string[] failLines = new string[]
            {
                "\"No gold, no goods. Beat it.\"",
                $"\"You're {shortage}g short. Come back when you're serious.\"",
                "\"Not enough. Go earn some more.\"",
                "\"Empty pockets? Get back in the tunnels.\"",
                $"\"Need {shortage}g more. Chop chop.\"",
            };

            dialogueLabel.text = GetMerchantName() + ": " + failLines[Random.Range(0, failLines.Length)];
        }
    }

    string GetMerchantName()
    {
        // Used for UI strings like "Gregor: ...".
        return GameManager.GetCurrentMerchantName();
    }
}
