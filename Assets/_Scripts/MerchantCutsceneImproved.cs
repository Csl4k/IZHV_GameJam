using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MerchantCutscene : MonoBehaviour
{
    [Header("Visuals")]
    public SpriteRenderer npcSprite;
    public Sprite gregorAlive;
    public Sprite gregorDead;
    public Sprite viktorSprite;

    [Header("Scene Objects")]
    public GameObject guardGroup;
    public GameObject interactionPrompt;

    [Header("Cutscene Manager")]
    public MerchantDeathCutscene deathCutsceneManager;

    // Internal State
    private ShopUI shopUI;
    private bool isInteractionOpen = false;
    private Transform playerTransform;

    public enum MerchantType { Gregor, Viktor }
    public MerchantType merchantType = MerchantType.Gregor;

    void Start()
    {
        shopUI = FindObjectOfType<ShopUI>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        UpdateMerchantAppearance();

        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }

    void UpdateMerchantAppearance()
    {
        // We drive the entire merchant identity off the merchant chain.
        // 0 = Gregor, 1+ = Viktor variants.
        if (GameManager.MerchantIndex <= 0)
        {
            if (npcSprite) npcSprite.sprite = gregorAlive;
            merchantType = MerchantType.Gregor;
        }
        else
        {
            if (npcSprite) npcSprite.sprite = viktorSprite;
            merchantType = MerchantType.Viktor;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = distance < 3.0f;

        if (interactionPrompt != null)
        {
            bool inputEnabled = playerTransform.GetComponent<MonoBehaviour>().enabled;
            interactionPrompt.SetActive(inRange && !isInteractionOpen && inputEnabled);
        }

        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isInteractionOpen) OpenDialogue();
            else CloseDialogue();
        }

        else if (!inRange && isInteractionOpen)
        {
            CloseDialogue();
        }
    }

    void OpenDialogue()
    {
        if (shopUI == null) return;
        isInteractionOpen = true;
        shopUI.OpenShop(this);
    }

    void CloseDialogue()
    {
        if (shopUI == null) return;
        isInteractionOpen = false;
        shopUI.CloseShop();
    }

    public void StartMurderSequence()
    {
        if (deathCutsceneManager != null)
        {
            deathCutsceneManager.TriggerDeathSequence();
        }
        else
        {
            StartCoroutine(PlayTwistFallback());
        }
    }

    IEnumerator PlayTwistFallback()
    {
        CloseDialogue();
        GameManager.ResetAfterMerchantMurder();
        if (playerTransform != null)
        {
            MonoBehaviour controller = playerTransform.GetComponent<MonoBehaviour>();
            if (controller) controller.enabled = false;
        }

        if (npcSprite != null)
            npcSprite.sprite = gregorDead;
        yield return new WaitForSeconds(2f);

        if (guardGroup != null) guardGroup.SetActive(true);

        Debug.Log("GUARD: WHAT HAVE YOU DONE?!");

        yield return new WaitForSeconds(3f);

        // Advance the merchant chain so the *next* merchant correctly references this murder.
        GameManager.RegisterMerchantMurder();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    public string GetMerchantDialogue()
    {
        int run = GameManager.RunCount;
        int price = GameManager.GetFreedomPrice(100);

        switch (merchantType)
        {
            case MerchantType.Gregor:
                return GetGregorDialogue(run, price);
            
            case MerchantType.Viktor:
                return GetViktorDialogue(run, price);
            
            default:
                return "...";
        }
    }

    string GetGregorDialogue(int run, int price)
    {
        if (run == 1)
            return "Name's Gregor. Been here long enough to know the ropes.\n\nThere's a way out through the old tunnels... if you can afford it.";
        
        if (run == 2)
            return "Back so soon? The price went up. Inflation, you know.\n\nDon't look at me like that. Supply and demand.";
        
        if (run == 3)
            return $"Still trying, eh? That's... admirable. In a tragic sort of way.\n\nFreedom costs {price}g now.";
        
        if (run == 4)
            return "The guards killed you again? Shocking.\n\nMaybe invest in better gear. Oh wait, you're broke.";
        
        if (run == 5)
            return $"You're persistent, I'll give you that.\n\nFreedom? {price}g. About 20 more runs at your current rate.";
        
        if (run == 6)
            return "Fun fact: The average prisoner dies 12 times before giving up.\n\nYou're halfway there!";
        
        if (run == 7)
            return $"At some point, you have to ask yourself...\n\n'Is freedom really worth {price}g?'";
        
        if (run == 8)
            return $"The price is {price}g. You have... let me check... not even close.\n\nBut hey, keep the grind going.";
        
        if (run == 9)
            return "You know what's funny? I was lying about the tunnels.\n\n...Just kidding! Or am I?";
        
        if (run == 10)
            return $"{price}g. The math isn't mathing, is it?\n\nYou're never getting out. But please, keep trying.";
        
        // Run 11+
        return $"Run #{run}. Freedom costs {price}g.\n\nYou look tired. Maybe just... stay here forever?";
    }

    string GetViktorDialogue(int run, int price)
    {
        string me = GameManager.GetCurrentMerchantName();
        string prev = GameManager.GetPreviousMerchantName();

        if (run == 1)
            return $"...Hey. Name's {me}.\n\nHeard you killed {prev}. Bold move.";
        
        if (run == 2)
            return $"Me? I don't judge. I just do business.\n\nSame deal as {prev}. Different face.";
        
        if (run == 3)
            return $"Freedom costs {price}g. Same tunnels. Same enemies.\n\nSame outcome, probably.";
        
        if (run == 4)
            return $"Don't get any ideas. I'm tougher than {prev}.\n\n...Not that it matters.";
        
        if (run == 5)
            return $"The price is {price}g now. Surprised?\n\nThe house always wins.";
        
        if (run == 6)
            return $"You killed {prev} thinking it would change things.\n\nHow'd that work out?";
        
        if (run == 7)
            return $"{price}g. Time is money. And you're out of both.";
        
        if (run == 8)
            return "At this point, we both know how this ends.\n\nWhy are you still playing?";
        
        if (run >= 9)
            return $"Run #{run}. Price: {price}g.\n\nYou gonna try to kill me too? Go ahead. See what happens.";
        
        return $"The cycle continues. {price}g for freedom that doesn't exist.";
    }
}
