using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BossCutscene : MonoBehaviour
{
    [Header("Actors")]
    public Transform boss;
    public Transform player;
    public MonoBehaviour playerController;

    [Header("Camera")]
    public Camera mainCamera;
    public MonoBehaviour cameraFollowScript;
    public float cameraMoveSpeed = 2f;
    public float cameraShakeIntensity = 0.15f;

    [Header("UI")]
    public GameObject canvas;
    public ShopUI shopUI;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip bossDeathSound;
    public AudioClip revelationSound;
    public AudioClip dramaticSound;

    private bool cutsceneActive = false;
    private Vector3 originalCameraPos;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (playerController == null && player != null)
            playerController = player.GetComponent<PlayerController>();

        if (cameraFollowScript == null)
            cameraFollowScript = FindObjectOfType<CameraFollow>();
    }

    public void TriggerBossDefeatCutscene()
    {
        if (cutsceneActive) return;
        StartCoroutine(BossDefeatSequence());
    }

    IEnumerator BossDefeatSequence()
    {
        cutsceneActive = true;
        // Hide gameplay UI, keep dialogue UI
        if (canvas != null) canvas.SetActive(false);

        // Disable player control
        if (playerController != null)
            playerController.enabled = false;

        // Disable camera follow
        if (cameraFollowScript != null)
            cameraFollowScript.enabled = false;

        if (mainCamera != null)
            originalCameraPos = mainCamera.transform.position;

        // Focus on boss
        if (boss != null)
            yield return StartCoroutine(PanCameraTo(boss.position, cameraMoveSpeed));

        PlaySound(bossDeathSound);
        yield return new WaitForSeconds(2f);

        // Boss dialogue
        yield return StartCoroutine(ShowDialogue("BOSS", "You... defeated me...", boss, 2.5f));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(ShowDialogue("BOSS", "But you're still a fool.", boss, 2.5f));
        yield return new WaitForSeconds(0.8f);

        // Focus on player
        if (player != null)
            yield return StartCoroutine(PanCameraTo(player.position, cameraMoveSpeed));

        yield return new WaitForSeconds(1f);

        // Back to boss
        if (boss != null)
            yield return StartCoroutine(PanCameraTo(boss.position, cameraMoveSpeed));

        PlaySound(revelationSound);
        yield return StartCoroutine(CameraShake(cameraShakeIntensity, 0.4f));

        yield return StartCoroutine(ShowDialogue("BOSS", "That merchant...", boss, 2f));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(ShowDialogue("BOSS", "He'll NEVER give you the key.", boss, 3f));
        yield return new WaitForSeconds(1f);

        PlaySound(dramaticSound);
        yield return StartCoroutine(CameraShake(cameraShakeIntensity * 1.5f, 0.6f));

        yield return StartCoroutine(ShowDialogue("BOSS", "If you want freedom...", boss, 2.5f));
        yield return new WaitForSeconds(0.8f);
        yield return StartCoroutine(ShowDialogue("BOSS", "You'll have to take it yourself.", boss, 3.5f));
        yield return new WaitForSeconds(1.5f);

        // Focus on player for reaction
        if (player != null)
            yield return StartCoroutine(PanCameraTo(player.position, cameraMoveSpeed * 1.5f));

        yield return new WaitForSeconds(2f);

        HideDialogue();

        yield return new WaitForSeconds(1f);

        // Restore camera follow
        if (cameraFollowScript != null)
            cameraFollowScript.enabled = true;

        // Enable player control
        if (playerController != null)
            playerController.enabled = true;

        if (canvas != null) canvas.SetActive(true);

        cutsceneActive = false;

        Debug.Log("Boss cutscene complete. Merchant murder flag set.");
    }

    IEnumerator PanCameraTo(Vector3 targetPos, float speed)
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

    IEnumerator CameraShake(float intensity, float duration)
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

    IEnumerator ShowDialogue(string speakerName, string text, Transform speaker, float duration)
    {
        if (shopUI != null)
            shopUI.ShowNarrative(speakerName, text);

        if (speaker != null && mainCamera != null)
            yield return StartCoroutine(PanCameraTo(speaker.position, cameraMoveSpeed * 1.5f));

        yield return new WaitForSeconds(duration);
    }

    void HideDialogue()
    {
        if (shopUI != null)
            shopUI.CloseShop();
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}