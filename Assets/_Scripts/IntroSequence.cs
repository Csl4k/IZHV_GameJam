using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class IntroSequence : MonoBehaviour
{
    [Header("Actors")]
    public Transform player;
    public Transform leftGuard;
    public Transform rightGuard;
    public Transform merchantPosition;
    public GameObject merchantObject;

    [Header("Animation Settings")]
    public float walkSpeed = 2.5f;
    public float throwSpeed = 8f;
    public float offScreenDistance = 12f;

    [Header("Camera")]
    public Camera mainCamera;
    public MonoBehaviour cameraFollowScript;
    public float cameraShakeIntensity = 0.15f;
    public float cameraShakeDuration = 0.3f;
    public float panSpeed = 0.5f;

    [Header("Merchant")]
    public Sprite merchantPrisonerSprite;


    [Header("UI")]
    public GameObject canvas;
    public ShopUI ui;
    public GameObject skipPromptUI;
    public Image skipProgressBar;

    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip doorSlamSFX;
    public AudioClip footstepsSFX;
    public AudioClip throwSFX;
    public AudioClip chainsSFX;

    // Internal State
    private Vector3 originalCameraPos;
    private Vector3 finalPlayerPos; 
    private Coroutine currentCutscene;
    private float skipTimer = 0f;
    private float timeToSkip = 1.5f; 

    void Start()
    {

        finalPlayerPos = player.position;

        if (GameManager.RunCount == 1 && GameManager.StoryState == 0)
        {
            if (skipPromptUI != null) skipPromptUI.SetActive(true);
            currentCutscene = StartCoroutine(PlayIntro());
        }
        else
        {
            PerformSkip();
        }
    }

    void Update()
    {

        if (Input.GetKey(KeyCode.Space))
        {
            skipTimer += Time.deltaTime;

            if (skipProgressBar != null)
                skipProgressBar.fillAmount = skipTimer / timeToSkip;

            if (skipTimer >= timeToSkip)
            {
                PerformSkip();
            }
        }
        else
        {
            skipTimer = 0f;
            if (skipProgressBar != null) skipProgressBar.fillAmount = 0f;
        }
    }

    void PerformSkip()
    {
        if (currentCutscene != null) StopCoroutine(currentCutscene);

        if (audioSource) audioSource.Stop();
        ui.CloseShop();

        if (leftGuard) leftGuard.gameObject.SetActive(false);
        if (rightGuard) rightGuard.gameObject.SetActive(false);

        if (merchantObject) merchantObject.SetActive(true);

        player.position = finalPlayerPos;

        mainCamera.transform.position = new Vector3(finalPlayerPos.x, finalPlayerPos.y, mainCamera.transform.position.z);

        var playerControl = player.GetComponent<MonoBehaviour>();
        if (playerControl) playerControl.enabled = true;
        if (cameraFollowScript) cameraFollowScript.enabled = true;

        GameManager.StoryState = 1;
        if (canvas != null) canvas.SetActive(true);

        if (skipPromptUI != null) skipPromptUI.SetActive(false);
        if (merchantObject)
        {
            merchantObject.SetActive(true);

            var sr = merchantObject.GetComponent<SpriteRenderer>();
            if (sr && merchantPrisonerSprite)
                sr.sprite = merchantPrisonerSprite;
        }

        Destroy(this);
    }

    void EndSequenceImmediate()
    {
        if (leftGuard != null) leftGuard.gameObject.SetActive(false);
        if (rightGuard != null) rightGuard.gameObject.SetActive(false);
        if (skipPromptUI != null) skipPromptUI.SetActive(false);
        Destroy(this);
    }

    IEnumerator PlayIntro()
    {
        if (canvas != null) canvas.SetActive(false);
        var playerControl = player.GetComponent<MonoBehaviour>();
        if (playerControl) playerControl.enabled = false;
        if (cameraFollowScript) cameraFollowScript.enabled = false;

        if (merchantObject) merchantObject.SetActive(false);

        Vector3 finalLeftPos = leftGuard.position;
        Vector3 finalRightPos = rightGuard.position;


        Vector3 startOffset = new Vector3(offScreenDistance, 0f, 0f);
        Vector3 startLeftPos = finalLeftPos - startOffset;
        Vector3 startRightPos = finalRightPos - startOffset;
        Vector3 playerStartPos = (startLeftPos + startRightPos) / 2f;

        player.position = playerStartPos;
        leftGuard.position = startLeftPos;
        rightGuard.position = startRightPos;
        leftGuard.gameObject.SetActive(true);
        rightGuard.gameObject.SetActive(true);

        float camZ = mainCamera.transform.position.z;
        Vector3 centerDoorPos = (finalLeftPos + finalRightPos) / 2f;
        originalCameraPos = new Vector3(centerDoorPos.x, centerDoorPos.y, camZ);
        mainCamera.transform.position = originalCameraPos;

        yield return new WaitForSeconds(0.5f);

        if (audioSource && footstepsSFX)
        {
            audioSource.clip = footstepsSFX;
            audioSource.loop = true;
            audioSource.Play();
        }

        float enterDuration = Vector3.Distance(startLeftPos, finalLeftPos) / walkSpeed;
        float elapsed = 0f;

        while (elapsed < enterDuration)
        {
            float t = elapsed / enterDuration;
            leftGuard.position = Vector3.Lerp(startLeftPos, finalLeftPos, t);
            rightGuard.position = Vector3.Lerp(startRightPos, finalRightPos, t);
            player.position = (leftGuard.position + rightGuard.position) / 2f;
            elapsed += Time.deltaTime;
            yield return null;
        }

        leftGuard.position = finalLeftPos;
        rightGuard.position = finalRightPos;
        player.position = (finalLeftPos + finalRightPos) / 2f;

        if (audioSource) audioSource.Stop();
        yield return new WaitForSeconds(0.3f);

        if (audioSource && doorSlamSFX) audioSource.PlayOneShot(doorSlamSFX);
        yield return StartCoroutine(CameraShake(cameraShakeIntensity, cameraShakeDuration));
        yield return new WaitForSeconds(0.4f);


        yield return StartCoroutine(SmoothCameraMove(mainCamera.transform, GetCamPos(rightGuard), panSpeed));
        ui.ShowNarrative("Head Guard", "Another one. Cell 237.");
        yield return new WaitForSeconds(2.5f);

        yield return StartCoroutine(SmoothCameraMove(mainCamera.transform, GetCamPos(leftGuard), panSpeed));
        ui.ShowNarrative("Left Guard", "What's he in for?");
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(SmoothCameraMove(mainCamera.transform, GetCamPos(rightGuard), panSpeed));
        ui.ShowNarrative("Head Guard", "Does it matter?");
        yield return new WaitForSeconds(2f);

        ui.CloseShop();

        yield return StartCoroutine(SmoothCameraMove(mainCamera.transform, originalCameraPos, panSpeed));

        Vector3 throwStartPos = player.position;
        float throwDuration = Vector3.Distance(throwStartPos, finalPlayerPos) / throwSpeed;
        elapsed = 0f;

        if (audioSource && throwSFX) audioSource.PlayOneShot(throwSFX);

        while (elapsed < throwDuration)
        {
            float t = 1f - Mathf.Pow(1f - (elapsed / throwDuration), 3);
            player.position = Vector3.Lerp(throwStartPos, finalPlayerPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        player.position = finalPlayerPos;

        yield return StartCoroutine(CameraShake(cameraShakeIntensity * 1.5f, cameraShakeDuration * 0.8f));
        if (audioSource && chainsSFX) audioSource.PlayOneShot(chainsSFX);
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(SmoothCameraMove(mainCamera.transform, GetCamPos(rightGuard), panSpeed));

        ui.ShowNarrative("Head Guard", "Don't get comfortable.");
        yield return new WaitForSeconds(2.5f);

        ui.ShowNarrative("Head Guard", "You're never leaving.");
        yield return new WaitForSeconds(3f);

        ui.CloseShop();
        yield return new WaitForSeconds(0.5f);

        StartCoroutine(SmoothCameraMove(mainCamera.transform, originalCameraPos, 1.0f));

        float exitDuration = 2.5f;
        elapsed = 0f;

        if (audioSource && footstepsSFX)
        {
            audioSource.clip = footstepsSFX;
            audioSource.loop = true;
            audioSource.Play();
        }

        while (elapsed < exitDuration)
        {
            float t = elapsed / exitDuration;
            leftGuard.position = Vector3.Lerp(finalLeftPos, startLeftPos, t);
            rightGuard.position = Vector3.Lerp(finalRightPos, startRightPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (audioSource) audioSource.Stop();

        leftGuard.gameObject.SetActive(false);
        rightGuard.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);

        if (merchantObject)
        {
            merchantObject.SetActive(true);

            var sr = merchantObject.GetComponent<SpriteRenderer>();
            if (sr && merchantPrisonerSprite)
                sr.sprite = merchantPrisonerSprite;
        }

        Vector3 targetPos = mainCamera.transform.position;

        if (merchantPosition != null)
        {

            targetPos = merchantPosition.position;
        }
        else if (merchantObject != null)
        {

            targetPos = merchantObject.transform.position;
            Debug.LogWarning("IntroSequence: 'Merchant Position' is not assigned in Inspector. Focusing on Merchant Object instead.");
        }


        Vector3 merchantCamPos = new Vector3(targetPos.x, targetPos.y, camZ);
        yield return StartCoroutine(SmoothCameraMove(mainCamera.transform, merchantCamPos, 2.5f));

        yield return new WaitForSeconds(0.5f);


        if (ui != null)
        {
            ui.ShowNarrative("???", "...");
            yield return new WaitForSeconds(2f);
            ui.ShowNarrative("???", "Ah... Fresh meat.");
            yield return new WaitForSeconds(2.5f);
            ui.ShowNarrative("Gregor", "Name's Gregor.");
            yield return new WaitForSeconds(2f);
            ui.ShowNarrative("Gregor", "Been here long enough to know the game.");
            yield return new WaitForSeconds(3f);
            ui.ShowNarrative("Gregor", "Want out?");
            yield return new WaitForSeconds(2f);
            ui.ShowNarrative("Gregor", "Come talk to me. I can give you a key to the door.");
            yield return new WaitForSeconds(4f);
            ui.ShowNarrative("Gregor", "...For a price.");
            yield return new WaitForSeconds(3f);

            ui.CloseShop();
        }


        Vector3 playerCamPos = new Vector3(player.position.x, player.position.y, camZ);
        yield return StartCoroutine(SmoothCameraMove(mainCamera.transform, playerCamPos, 1.5f));


        if (playerControl) playerControl.enabled = true;
        if (cameraFollowScript) cameraFollowScript.enabled = true;
        GameManager.StoryState = 1;

        if (canvas != null) canvas.SetActive(true);
        if (skipPromptUI != null) skipPromptUI.SetActive(false);

        Destroy(this);
    }

    Vector3 GetCamPos(Transform target)
    {
        return new Vector3(target.position.x, target.position.y, mainCamera.transform.position.z);
    }

    IEnumerator SmoothCameraMove(Transform target, Vector3 destination, float time)
    {
        Vector3 start = target.position;
        float elapsed = 0f;
        while (elapsed < time)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / time);
            target.position = Vector3.Lerp(start, destination, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.position = destination;
    }

    IEnumerator CameraShake(float intensity, float duration)
    {
        Vector3 originalPos = mainCamera.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            mainCamera.transform.position = originalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.position = originalPos;
    }
}