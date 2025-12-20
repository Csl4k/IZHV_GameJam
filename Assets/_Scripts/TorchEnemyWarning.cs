using UnityEngine;
using TMPro;

public class TorchEnemyWarning : MonoBehaviour
{
    public RectTransform indicator;     // drag TorchIndicator here
    public Camera cam;                  // main camera
    public float maxDetectDistance = 25f;
    public float screenOffsetPixels = 80f;

    Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (GameManager.TorchLevel <= 0)
        {
            if (indicator) indicator.gameObject.SetActive(false);
            return;
        }

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null || cam == null || indicator == null) return;

        EnemyAI nearest = FindNearestAwareEnemy();
        if (nearest == null)
        {
            indicator.gameObject.SetActive(false);
            return;
        }

        indicator.gameObject.SetActive(true);

        Vector3 dir = (nearest.transform.position - player.position);
        dir.z = 0f;
        if (dir.sqrMagnitude < 0.0001f)
        {
            indicator.gameObject.SetActive(false);
            return;
        }

        Vector3 dirNorm = dir.normalized;

        Vector3 playerScreen = cam.WorldToScreenPoint(player.position);
        indicator.position = playerScreen + dirNorm * screenOffsetPixels;
    }

    EnemyAI FindNearestAwareEnemy()
    {
        EnemyAI[] enemies = GameObject.FindObjectsOfType<EnemyAI>();
        EnemyAI best = null;
        float bestDist = float.MaxValue;

        foreach (var e in enemies)
        {
            if (e == null) continue;
            if (!e.IsAware) continue;

            float d = Vector3.Distance(player.position, e.transform.position);
            if (d > maxDetectDistance) continue;

            if (d < bestDist)
            {
                bestDist = d;
                best = e;
            }
        }

        return best;
    }
}
