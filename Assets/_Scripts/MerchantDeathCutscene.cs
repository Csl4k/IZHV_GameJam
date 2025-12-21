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

    [Header("Merchant")]
    public Sprite merchantPrisonerSprite;

    [Header("Viktor (Replacement)")]
    public GameObject viktorObject;
    public Transform viktorSpawnPoint;
    public Transform viktorThrowStartPoint;

    [Header("Guards")]
    public GameObject guard1Object;
    public GameObject guard2Object;
    public GameObject headGuardObject;
    public Transform guard1EntryPoint;
    public Transform guard2EntryPoint;
    public Transform headGuardEntryPoint;
    public Transform guard1FinalPosition;
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
    public Transform keyPickupPoint;
    public Transform doorUnlockPoint;
    public Transform outsideCellPoint;
    public Transform outsideLeadPoint;
    public float escapeWalkSpeed = 3.5f;

    [Header("Camera")]
    public Camera mainCamera;
    public MonoBehaviour cameraFollowScript;
    public float cameraShakeIntensity = 0.2f;
    public float cameraShakeDuration = 0.4f;
    public float cameraMoveSpeed = 2.5f;
    public float cameraZoomDramatic = 3.5f;
    public float cameraZoomNormal = 5.0f;

    [Header("Timing")]
    public float pauseAfterMurder = 2.0f;
    public float dialogueAutoAdvanceTime = 3.0f;
    public float throwDuration = 0.8f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip stabSound;
    public AudioClip bodyFallSound;
    public AudioClip guardFootstepsSound;
    public AudioClip doorSlamSound;
    public AudioClip guardShoutSound;
    public AudioClip heartbeatSound;
    public AudioClip keyJingleSound;
    public AudioClip doorUnlockSound;

    [Header("UI")]
    public GameObject canvas;
    public ShopUI shopUI;
    public GameObject buttons;
    public GameObject dialoguePanel;
    public TMPro.TMP_Text dialogueText;
    public TMPro.TMP_Text speakerNameText;

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
        if (canvas != null) canvas.SetActive(false);
        StartCoroutine(DeathSequenceCoroutine());
    }

    private IEnumerator DeathSequenceCoroutine()
    {
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

        yield return StartCoroutine(ShowDialogue("YOU", "(You take the key.)", player, 1.4f));
        PlaySound(keyJingleSound);

        SetPlayerCollision(false);

        if (doorUnlockPoint != null && player != null)
        {
            yield return StartCoroutine(FollowCameraForMove(player, doorUnlockPoint.position, escapeWalkSpeed, 0.18f));
        }

        yield return StartCoroutine(ShowDialogue("YOU", "(The key turns.)", player, 1.2f));
        PlaySound(doorUnlockSound);
        HideDialogue();

        if (outsideCellPoint != null && player != null)
        {
            yield return StartCoroutine(FollowCameraForMove(player, outsideCellPoint.position, escapeWalkSpeed, 0.18f));
        }

        if (outsideLeadPoint != null && player != null)
        {
            StartCoroutine(FollowCameraForMove(player, outsideLeadPoint.position, escapeWalkSpeed, 0.18f));
        }

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

        yield return StartCoroutine(ShowDialogue("GUARD", $"By the gods {prevMerchant}!", guard1Object != null ? guard1Object.transform : null, 2f));
        yield return StartCoroutine(ShowDialogue("GUARD", "He's dead! The new inmate killed him!", guard2Object != null ? guard2Object.transform : null, 2.5f));

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

        if (gregorObject != null) gregorObject.SetActive(false);
        if (guard1Object != null) guard1Object.SetActive(false);
        if (guard2Object != null) guard2Object.SetActive(false);
        if (headGuardObject != null) headGuardObject.SetActive(false);

        var sr = viktorObject.GetComponent<SpriteRenderer>();
        if (sr != null && merchantPrisonerSprite != null)
        {
            sr.sprite = merchantPrisonerSprite;
        }

        // CRITICAL FIX: Properly initialize Viktor with prisoner sprite
        if (viktorObject != null && viktorThrowStartPoint != null)
        {
            viktorObject.transform.position = viktorThrowStartPoint.position;

            // Set sprite BEFORE activating the object
            sr = viktorObject.GetComponent<SpriteRenderer>();
            if (sr != null && merchantPrisonerSprite != null)
            {
                sr.sprite = merchantPrisonerSprite;
            }


            viktorObject.SetActive(true);
            if (sr != null && merchantPrisonerSprite != null)
            {
                sr.sprite = merchantPrisonerSprite;
            }
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
        sr = viktorObject.GetComponent<SpriteRenderer>();
        if (sr != null && merchantPrisonerSprite != null)
        {
            sr.sprite = merchantPrisonerSprite;
        }
        yield return StartCoroutine(ShowDialogue("GUARD", "Your new cellmate. Behave this time.", guard1Object != null ? guard1Object.transform : null, 2.5f));
        HideDialogue();

        if (viktorObject != null && viktorSpawnPoint != null && viktorThrowStartPoint != null)
        {
            sr = viktorObject.GetComponent<SpriteRenderer>();
            if (sr != null && merchantPrisonerSprite != null)
            {
                sr.sprite = merchantPrisonerSprite;
            }
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
        yield return StartCoroutine(ShowDialogue(myName.ToUpperInvariant(), "200 Gold. Same deal.", viktorObject.transform, 3.5f));

        HideDialogue();

        if (shopUI != null)
        {
            shopUI.CloseShop();
            if (shopUI.rootPanel != null)
                shopUI.rootPanel.SetActive(false);
        }

        if (canvas != null) canvas.SetActive(true);

        if (mainCamera != null)
            mainCamera.orthographicSize = originalCameraSize;

        if (cameraFollowScript != null)
            cameraFollowScript.enabled = true;

        if (playerController != null)
            playerController.enabled = true;

        GameManager.ResetAfterMerchantMurder();
        cutsceneActive = false;
        buttons.SetActive(true);

        GameManager.IsUIOpen = false;

        if (shopUI != null)
            shopUI.CloseShop();

        Debug.Log("Merchant Death Cutscene Complete.");
    }

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
        if (shopUI != null)
            shopUI.ShowNarrative(speakerName, text);

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
        fadeImage.gameObject.SetActive(false);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}