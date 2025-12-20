using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MerchantDeathCutscene : MonoBehaviour
{
    [Header("Screen Fade")]
    public UnityEngine.UI.Image fadeImage;

    [Header("Actors")]
    public Transform gregorTransform;
    public GameObject gregorObject;
    public SpriteRenderer gregorSprite;
    public Sprite gregorDeadSprite;

    [Header("Viktor (Replacement)")]
    public GameObject viktorObject;                 // Prefab or disabled GameObject
    public Transform viktorSpawnPoint;              // Where Viktor ends up (Gregor's bed position)
    public Transform viktorThrowStartPoint;         // Offscreen position where guards throw from

    [Header("Guards")]
    public GameObject guard1Object;                 // First guard to enter
    public GameObject guard2Object;                 // Second guard
    public GameObject headGuardObject;              // Head guard (speaks most)
    public Transform guard1EntryPoint;              // Where guard1 enters from
    public Transform guard2EntryPoint;              // Where guard2 enters from
    public Transform headGuardEntryPoint;           // Where head guard enters from
    public Transform guard1FinalPosition;           // Where they stand during dialogue
    public Transform guard2FinalPosition;
    public Transform headGuardFinalPosition;

    public Transform guardThrowPosition;

    [Header("Player")]
    public Transform player;
    public MonoBehaviour playerController;
    public float playerWalkSpeed = 3.0f;
    public Collider2D[] playerColliders;

    [Header("Escape / Door Waypoints")]
    public Transform playerThrowPosition;
    public Transform keyPickupPoint;                // Where player stands to take the key (near Gregor)
    public Transform doorUnlockPoint;               // Where player uses the key (at the door)
    public Transform outsideCellPoint;              // Just outside the cell door
    public Transform outsideLeadPoint;              // Further point outside (hallway/yard) where camera should follow
    public float escapeWalkSpeed = 3.5f;

    [Header("Camera")]
    public Camera mainCamera;
    public MonoBehaviour cameraFollowScript;        // Your CameraFollow script
    public float cameraShakeIntensity = 0.2f;
    public float cameraShakeDuration = 0.4f;
    public float cameraMoveSpeed = 2.5f;            // Smooth camera panning
    public float cameraZoomDramatic = 3.5f;         // Closer zoom for dramatic moments
    public float cameraZoomNormal = 5.0f;           // Normal orthographic size

    [Header("Timing")]
    public float pauseAfterMurder = 2.0f;           // Silent pause on body
    public float dialogueAutoAdvanceTime = 3.0f;    // Time per dialogue line
    public float throwDuration = 0.8f;              // Viktor throw animation time

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip stabSound;
    public AudioClip bodyFallSound;
    public AudioClip guardFootstepsSound;
    public AudioClip doorSlamSound;
    public AudioClip guardShoutSound;
    public AudioClip heartbeatSound;

    [Header("Audio")]
    public AudioClip keyJingleSound;
    public AudioClip doorUnlockSound;

    [Header("UI")]
    public ShopUI shopUI;
    public GameObject buttons;
    public GameObject dialoguePanel;                // UI panel for guard dialogue
    public TMPro.TMP_Text dialogueText;             // Text component
    public TMPro.TMP_Text speakerNameText;          // "HEAD GUARD" etc.

    private Vector3 originalCameraPos;
    private float originalCameraSize;
    private bool cutsceneActive = false;


    private float dialogueFontSizeDefault;
    private TMPro.FontStyles dialogueFontStyleDefault;
    private TMPro.TextAlignmentOptions dialogueAlignmentDefault;
    private Color dialogueColorDefault;

    void Start()
    {

        if ((playerColliders == null || playerColliders.Length == 0) && player != null)
            playerColliders = player.GetComponentsInChildren<Collider2D>();


        if (dialogueText != null)
        {
            dialogueFontSizeDefault = dialogueText.fontSize;
            dialogueFontStyleDefault = dialogueText.fontStyle;
            dialogueAlignmentDefault = dialogueText.alignment;
            dialogueColorDefault = dialogueText.color;
        }

        if (shopUI != null)
        {
            shopUI.CloseShop();
            if (shopUI.rootPanel != null)
                shopUI.rootPanel.SetActive(false);
        }
    }

    public void TriggerDeathSequence()
    {
        if (viktorObject != null) viktorObject.SetActive(false);
        if (guard1Object != null) guard1Object.SetActive(false);
        if (guard2Object != null) guard2Object.SetActive(false);
        if (headGuardObject != null) headGuardObject.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        buttons.SetActive(false);
        if (cutsceneActive) return;
        cutsceneActive = true;

        StartCoroutine(DeathSequenceCoroutine());
    }

    private IEnumerator DeathSequenceCoroutine()
    {
        // ═══════════════════════════════════════════════════════════
        // PHASE 0: ARGUMENT + INTENT
        // ═══════════════════════════════════════════════════════════

        if (playerController != null)
            playerController.enabled = false;

        if (shopUI != null)
        {
            shopUI.CloseShop();
            if (shopUI.rootPanel != null)
                shopUI.rootPanel.SetActive(false);
        }

        if (mainCamera != null)
        {
            originalCameraPos = mainCamera.transform.position;
            originalCameraSize = mainCamera.orthographicSize;
        }

        if (cameraFollowScript != null)
            cameraFollowScript.enabled = false;


        yield return StartCoroutine(PanCameraTo(player.position, cameraMoveSpeed * 1.2f));

        yield return StartCoroutine(ShowDialogue("YOU", "You were just going to keep raising prices.", player, 2.7f));
        yield return StartCoroutine(ShowDialogue("YOU", "I was never going to get enough money.", player, 2.7f));
        yield return StartCoroutine(ShowDialogue("YOU", "So I'm taking the key.", player, 2.2f));

        if (keyPickupPoint != null && player != null)
        {
            yield return StartCoroutine(FollowCameraForMove(player, keyPickupPoint.position, playerWalkSpeed, 0.18f));
        }

        // ═══════════════════════════════════════════════════════════
        // PHASE 1: THE MURDER
        // ═══════════════════════════════════════════════════════════

        PlaySound(stabSound);

        if (mainCamera != null)
            yield return StartCoroutine(CameraShake(cameraShakeIntensity * 0.5f, 0.3f));

        if (gregorSprite != null && gregorDeadSprite != null)
        {
            gregorSprite.sprite = gregorDeadSprite;
        }

        yield return new WaitForSeconds(0.2f);
        PlaySound(bodyFallSound);


        yield return StartCoroutine(PanCameraTo(gregorTransform.position, cameraMoveSpeed * 1.5f));

        if (heartbeatSound != null)
        {
            audioSource?.PlayOneShot(heartbeatSound);
        }

        yield return new WaitForSeconds(pauseAfterMurder);

        // ═══════════════════════════════════════════════════════════
        // PHASE 1.5: STEAL KEY + ESCAPE OUTSIDE
        // ═══════════════════════════════════════════════════════════

        yield return StartCoroutine(ShowDialogue("YOU", "(You take the key.)", player, 1.4f));
        PlaySound(keyJingleSound);

        SetPlayerCollision(false);

        if (doorUnlockPoint != null && player != null)
        {
            yield return StartCoroutine(FollowCameraForMove(player, doorUnlockPoint.position, escapeWalkSpeed, 0.18f));
        }

        yield return StartCoroutine(ShowDialogue("YOU", "(The key turns.)", player, 1.2f));
        PlaySound(doorUnlockSound);


        if (outsideCellPoint != null && player != null)
        {
            yield return StartCoroutine(FollowCameraForMove(player, outsideCellPoint.position, escapeWalkSpeed, 0.18f));
        }


        if (outsideLeadPoint != null && player != null)
        {
            StartCoroutine(FollowCameraForMove(player, outsideLeadPoint.position, escapeWalkSpeed, 0.18f));
        }

        // ═══════════════════════════════════════════════════════════
        // PHASE 2: GUARDS DISCOVER THE MURDER
        // ═══════════════════════════════════════════════════════════

        if (guardFootstepsSound != null)
            audioSource?.PlayOneShot(guardFootstepsSound);


        yield return new WaitForSeconds(0.5f);

        PlaySound(doorSlamSound);
        yield return StartCoroutine(CameraShake(cameraShakeIntensity, cameraShakeDuration));

        if (guard1Object != null)
        {
            guard1Object.SetActive(true);
            guard1Object.transform.position = guard1EntryPoint.position;
            StartCoroutine(MoveActorTo(guard1Object.transform, guard1FinalPosition.position, playerWalkSpeed * 2.5f));
        }

        if (guard2Object != null)
        {
            guard2Object.SetActive(true);
            guard2Object.transform.position = guard2EntryPoint.position;
            StartCoroutine(MoveActorTo(guard2Object.transform, guard2FinalPosition.position, playerWalkSpeed * 2.5f));
        }

        yield return new WaitForSeconds(1.0f);

        PlaySound(guardShoutSound);


        yield return new WaitForSeconds(2.0f);
        if (gregorTransform != null)
        {
            yield return StartCoroutine(PanCameraTo(gregorTransform.position, cameraMoveSpeed * 2.0f));
            yield return new WaitForSeconds(1.0f);
        }

        GameManager.RegisterMerchantMurder();

        string prevMerchant = GameManager.GetPreviousMerchantName();
        string myName = GameManager.GetCurrentMerchantName();

        yield return StartCoroutine(ShowDialogue("GUARD", $"By the gods— {prevMerchant}!", guard1Object != null ? guard1Object.transform : null, 2f));
        yield return StartCoroutine(ShowDialogue("GUARD", "He's dead! The new inmate killed him!", guard2Object != null ? guard2Object.transform : null, 2.5f));

        // ═══════════════════════════════════════════════════════════
        // PHASE 3: HEAD GUARD CONFRONTATION
        // ═══════════════════════════════════════════════════════════

        if (headGuardObject != null)
        {
            headGuardObject.SetActive(true);
            headGuardObject.transform.position = headGuardEntryPoint.position;
            yield return StartCoroutine(MoveActorTo(headGuardObject.transform, headGuardFinalPosition.position, playerWalkSpeed));
        }

        
        yield return StartCoroutine(PanCameraTo(headGuardObject.transform.position, cameraMoveSpeed));
        yield return StartCoroutine(ZoomCamera(cameraZoomDramatic, 1.5f));

        yield return StartCoroutine(ShowDialogue("HEAD GUARD", "Do you have ANY idea what you've done?", headGuardObject.transform, 3f));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(ShowDialogue("HEAD GUARD", $"{prevMerchant} was a MODEL prisoner.", headGuardObject.transform, 2.5f));
        yield return StartCoroutine(ShowDialogue("HEAD GUARD", "He was on the REFORM COUNCIL.", headGuardObject.transform, 2.5f));
        yield return StartCoroutine(ShowDialogue("HEAD GUARD", "He was helping with your appeal!", headGuardObject.transform, 3f));

        yield return StartCoroutine(PanCameraTo(player.position, cameraMoveSpeed * 1.2f));
        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(PanCameraTo(headGuardObject.transform.position, cameraMoveSpeed));
        yield return StartCoroutine(ShowDialogue("HEAD GUARD", "You were set for release...", headGuardObject.transform, 3f));

        //yield return new WaitForSeconds(3f);
        
        yield return StartCoroutine(ShowDialogue("HEAD GUARD", "TOMORROW.", headGuardObject.transform, 0f));
        yield return StartCoroutine(CameraShake(cameraShakeIntensity * 1.5f, 0.6f));

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(ShowDialogue("HEAD GUARD", "Now?", headGuardObject.transform, 2f));
        
        yield return StartCoroutine(ShowDialogue("HEAD GUARD", "Ten more years. Minimum.", headGuardObject.transform, 0f));
        yield return StartCoroutine(CameraShake(cameraShakeIntensity, cameraShakeDuration));
        yield return new WaitForSeconds(2.5f);

        if (gregorObject != null) gregorObject.SetActive(false);

        if (guard1Object != null)
            StartCoroutine(MoveActorTo(guard1Object.transform, player.position + Vector3.left * 1.5f, playerWalkSpeed));

        if (guard2Object != null)
            StartCoroutine(MoveActorTo(guard2Object.transform, player.position + Vector3.right * 1.5f, playerWalkSpeed));

        yield return new WaitForSeconds(1.0f);

        yield return StartCoroutine(ShowDialogue("HEAD GUARD", "Drag him back to his cell.", headGuardObject.transform, 2.5f));
        HideDialogue();

        if (outsideCellPoint != null && playerThrowPosition != null && player != null)
        {

            if (guard1Object != null)
                StartCoroutine(MoveActorTo(guard1Object.transform, outsideCellPoint.position + new Vector3(-0.8f, 0, 0), escapeWalkSpeed));

            if (guard2Object != null)
                StartCoroutine(MoveActorTo(guard2Object.transform, outsideCellPoint.position + new Vector3(0.8f, 0, 0), escapeWalkSpeed));

            yield return StartCoroutine(FollowCameraForMove(player, outsideCellPoint.position, escapeWalkSpeed, 0.18f));

            PlaySound(bodyFallSound);
            yield return StartCoroutine(CameraShake(cameraShakeIntensity, 0.2f));


            yield return StartCoroutine(FollowCameraForMove(player, playerThrowPosition.position, escapeWalkSpeed * 2.5f, 0.1f));
        }

        SetPlayerCollision(true);

        // ═══════════════════════════════════════════════════════════
        // PHASE 5: VIKTOR INTRODUCTION
        // ═══════════════════════════════════════════════════════════

        if (gregorObject != null) gregorObject.SetActive(false);
        if (guard1Object != null) guard1Object.SetActive(false);
        if (guard2Object != null) guard2Object.SetActive(false);
        if (headGuardObject != null) headGuardObject.SetActive(false);

        if (viktorObject != null && viktorThrowStartPoint != null)
        {
            viktorObject.transform.position = viktorThrowStartPoint.position;
            viktorObject.SetActive(true);
        }

        Transform focusPoint = (guardThrowPosition != null) ? guardThrowPosition : viktorThrowStartPoint;

        if (guard1Object != null)
        {
            guard1Object.SetActive(true);

            if (guardThrowPosition != null)
                guard1Object.transform.position = guardThrowPosition.position;
            else
                guard1Object.transform.position = guard1EntryPoint.position;
        }

        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(FadeToBlack(2f));
        yield return new WaitForSeconds(2.5f);
        yield return StartCoroutine(FadeFromBlack(2f));

        if (focusPoint != null)
        {
            yield return StartCoroutine(PanCameraTo(focusPoint.position, cameraMoveSpeed));
        }

        PlaySound(doorSlamSound);
        yield return new WaitForSeconds(1f);




        yield return StartCoroutine(ShowDialogue("GUARD", "Your new cellmate. Behave this time.", guard1Object != null ? guard1Object.transform : null, 2.5f));
        HideDialogue();

        if (viktorObject != null && viktorSpawnPoint != null && viktorThrowStartPoint != null)
        {

            yield return StartCoroutine(ThrowActorWithCamera(viktorObject.transform, viktorThrowStartPoint.position, viktorSpawnPoint.position, throwDuration));

            PlaySound(bodyFallSound);
            yield return StartCoroutine(CameraShake(cameraShakeIntensity * 0.7f, 0.3f));
        }

        yield return new WaitForSeconds(0.3f);
        PlaySound(doorSlamSound);

        if (guard1Object != null) guard1Object.SetActive(false);

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(PanCameraTo(viktorSpawnPoint.position, cameraMoveSpeed));


        yield return new WaitForSeconds(0.8f);

        // Advance the merchant chain now that the player has murdered the current merchant.
        // This ensures the newly introduced merchant speaks about the correct previous merchant.


        yield return StartCoroutine(ShowDialogue(myName.ToUpperInvariant(), "...Hey.", viktorObject.transform, 2f));
        yield return StartCoroutine(ShowDialogue(myName.ToUpperInvariant(), $"Name's {myName}.", viktorObject.transform, 2.5f));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(ShowDialogue(myName.ToUpperInvariant(), $"Heard you killed {prevMerchant}. Bold.", viktorObject.transform, 3f));
        yield return new WaitForSeconds(0.8f);
        yield return StartCoroutine(ShowDialogue(myName.ToUpperInvariant(), "Me? I don't judge.", viktorObject.transform, 2.5f));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(ShowDialogue(myName.ToUpperInvariant(), "I just do business.", viktorObject.transform, 3f));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(ShowDialogue(myName.ToUpperInvariant(), "Want out?", viktorObject.transform, 2.5f));
        yield return new WaitForSeconds(0.8f);
        yield return StartCoroutine(ShowDialogue(myName.ToUpperInvariant(), "100 Gold. Same deal.", viktorObject.transform, 3.5f));


        HideDialogue();



        if (shopUI != null)
        {
            shopUI.CloseShop();
            if (shopUI.rootPanel != null)
                shopUI.rootPanel.SetActive(false);
        }

        // GameManager.StoryState = 1; 
        // GameManager.RunCount++; 

        if (mainCamera != null)
            mainCamera.orthographicSize = originalCameraSize;

        if (cameraFollowScript != null)
            cameraFollowScript.enabled = true;

        if (playerController != null)
            playerController.enabled = true;

        cutsceneActive = false;
        buttons.SetActive(true);

        Debug.Log("Merchant Death Cutscene Complete.");
    }

    // ═══════════════════════════════════════════════════════════
    // HELPER FUNCTIONS
    // ═══════════════════════════════════════════════════════════

    private void SetPlayerCollision(bool enabled)
    {
        if (playerColliders == null) return;
        for (int i = 0; i < playerColliders.Length; i++)
        {
            if (playerColliders[i] != null)
                playerColliders[i].enabled = enabled;
        }
    }

    private IEnumerator FollowCameraForMove(Transform actor, Vector3 targetPos, float speed, float followSmooth)
    {
        if (actor == null)
            yield break;

        while (Vector3.Distance(actor.position, targetPos) > 0.02f)
        {
            actor.position = Vector3.MoveTowards(actor.position, targetPos, speed * Time.deltaTime);

            if (mainCamera != null)
            {
                Vector3 camPos = mainCamera.transform.position;
                Vector3 desired = new Vector3(actor.position.x, actor.position.y, camPos.z);
                mainCamera.transform.position = Vector3.Lerp(camPos, desired, followSmooth);
            }

            yield return null;
        }

        actor.position = targetPos;

        if (mainCamera != null)
        {
            Vector3 camPos = mainCamera.transform.position;
            mainCamera.transform.position = new Vector3(actor.position.x, actor.position.y, camPos.z);
        }
    }

    private IEnumerator PanCameraTo(Vector3 targetPos, float speed)
    {
        if (mainCamera == null) yield break;

        Vector3 startPos = mainCamera.transform.position;
        Vector3 endPos = new Vector3(targetPos.x, targetPos.y, startPos.z);

        float distance = Vector3.Distance(startPos, endPos);
        float duration = (speed <= 0.001f) ? 0.001f : (distance / speed);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        mainCamera.transform.position = endPos;
    }

    private IEnumerator ZoomCamera(float targetSize, float duration)
    {
        if (mainCamera == null) yield break;

        float startSize = mainCamera.orthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }

        mainCamera.orthographicSize = targetSize;
    }

    private IEnumerator CameraShake(float intensity, float duration)
    {
        if (mainCamera == null) yield break;

        Vector3 originalPos = mainCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-intensity, intensity);
            float y = Random.Range(-intensity, intensity);
            mainCamera.transform.position = originalPos + new Vector3(x, y, 0);
            yield return null;
        }

        mainCamera.transform.position = originalPos;
    }

    private IEnumerator MoveActorTo(Transform actor, Vector3 targetPos, float speed)
    {
        if (actor == null) yield break;

        Vector3 startPos = actor.position;
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = (speed <= 0.001f) ? 0.001f : (distance / speed);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            actor.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        actor.position = targetPos;
    }

    private IEnumerator ThrowActor(Transform actor, Vector3 startPos, Vector3 endPos, float duration)
    {
        if (actor == null) yield break;

        float elapsed = 0f;
        float arcHeight = 2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            float arc = arcHeight * (1 - Mathf.Pow(2 * t - 1, 2));
            currentPos.y += arc;
            actor.position = currentPos;
            yield return null;
        }
        actor.position = endPos;
    }

    private IEnumerator ThrowActorWithCamera(Transform actor, Vector3 startPos, Vector3 endPos, float duration)
    {
        if (actor == null) yield break;

        actor.position = startPos;
        float elapsed = 0f;
        float arcHeight = 2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            float arc = arcHeight * (1 - Mathf.Pow(2 * t - 1, 2));
            currentPos.y += arc;

            actor.position = currentPos;

            if (mainCamera != null)
            {
                Vector3 camPos = mainCamera.transform.position;
                mainCamera.transform.position = Vector3.Lerp(
                    camPos,
                    new Vector3(actor.position.x, actor.position.y, camPos.z),
                    0.25f
                );
            }
            yield return null;
        }
        actor.position = endPos;

        if (mainCamera != null)
        {
            Vector3 camPos = mainCamera.transform.position;
            mainCamera.transform.position = new Vector3(actor.position.x, actor.position.y, camPos.z);
        }
    }

    private IEnumerator ShowDialogue(string speakerName, string text, Transform speaker, float duration, bool emphasize = false)
    {
        // Use the same narrative UI IntroSequence uses
        if (shopUI != null)
            shopUI.ShowNarrative(speakerName, text);

        // Keep your camera focus behavior
        if (speaker != null && mainCamera != null)
            yield return StartCoroutine(PanCameraTo(speaker.position, cameraMoveSpeed * 1.5f));

        yield return new WaitForSeconds(duration);
    }

    private void HideDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.fontStyle = dialogueFontStyleDefault;
            dialogueText.fontSize = dialogueFontSizeDefault;
            dialogueText.alignment = dialogueAlignmentDefault;
            dialogueText.color = dialogueColorDefault;
        }
    }


    private IEnumerator FadeToBlack(float duration)
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        Color c = fadeImage.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, elapsed / duration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeImage.color = c;
    }

    private IEnumerator FadeFromBlack(float duration)
    {
        if (fadeImage == null) yield break;

        Color c = fadeImage.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / duration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0f;
        fadeImage.color = c;
    }

    private IEnumerator ShowTextOnBlack(string text, float duration)
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueText != null)
        {
            dialogueText.text = text;
            dialogueText.color = Color.white;
            dialogueText.alignment = TMPro.TextAlignmentOptions.Center;
            dialogueText.fontStyle = TMPro.FontStyles.Normal;
            dialogueText.fontSize = dialogueFontSizeDefault;
        }
        if (speakerNameText != null) speakerNameText.text = "";

        yield return new WaitForSeconds(duration);

        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.color = dialogueColorDefault;
            dialogueText.alignment = dialogueAlignmentDefault;
            dialogueText.fontStyle = dialogueFontStyleDefault;
            dialogueText.fontSize = dialogueFontSizeDefault;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}