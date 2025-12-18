using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Setup")]
    public Transform player;
    public Collider2D swordCollider;
    public Transform weaponPivot;

    [Header("Ranges")]
    public float detectRange = 10f;
    public float rushTriggerDistance = 6f;
    public float attackRange = 2f;

    [Header("Stats")]
    public float moveSpeed = 3f;
    public float attackCooldown = 2f;

    [Header("Attack Speeds")]
    public float longRushSpeed = 12f;
    public float shortLungeSpeed = 6f;

    private bool isAttacking = false;
    private Rigidbody2D rb;
    private Rigidbody2D playerRb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody2D>();
        }

        if (swordCollider != null) swordCollider.enabled = false;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // rotation
        if (distance < detectRange && !isAttacking)
        {
            Vector3 direction = player.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // behavior of the enemy
        if (distance < detectRange && !isAttacking)
        {
            // long Rush
            if (distance > rushTriggerDistance)
            {
                StartCoroutine(DashAttackRoutine(true));
            }
            // walk
            else if (distance > attackRange)
            {
                // move towards the player
                transform.position += transform.right * moveSpeed * Time.deltaTime;
            }
            // fight
            else
            {
                // swing / lunge
                if (Random.value > 0.5f) StartCoroutine(SwingRoutine());
                else StartCoroutine(DashAttackRoutine(false));
            }
        }
    }

    IEnumerator DashAttackRoutine(bool isLongRush)
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        float speed = isLongRush ? longRushSpeed : shortLungeSpeed;
        float duration = isLongRush ? 0.6f : 0.45f;
        float windup = isLongRush ? 0.5f : 0.3f;

        if (weaponPivot != null) weaponPivot.localRotation = Quaternion.identity;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        Color originalColor = (sr != null) ? sr.color : Color.white;
        if (sr) sr.color = Color.red;

        yield return new WaitForSeconds(windup);

        // aim update
        if (player != null)
        {
            Vector2 dir = player.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (swordCollider != null) swordCollider.enabled = true;
        rb.velocity = transform.right * speed;

        yield return new WaitForSeconds(duration);

        rb.velocity = Vector2.zero;
        if (swordCollider != null) swordCollider.enabled = false;
        if (sr) sr.color = originalColor;

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    IEnumerator SwingRoutine()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        // wind up
        yield return StartCoroutine(RotatePivot(Quaternion.Euler(0, 0, 45), 0.2f));

        // swing
        if (swordCollider != null) swordCollider.enabled = true;

        yield return StartCoroutine(RotatePivot(Quaternion.Euler(0, 0, -135), 0.15f));

        // reset
        yield return StartCoroutine(RotatePivot(Quaternion.Euler(0, 0, 0), 0.2f));

        if (swordCollider != null) swordCollider.enabled = false;
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    IEnumerator RotatePivot(Quaternion targetLocalRot, float duration)
    {
        if (weaponPivot == null) yield break;
        Quaternion startRot = weaponPivot.localRotation;
        float elapsed = 0;
        while (elapsed < duration)
        {
            weaponPivot.localRotation = Quaternion.Slerp(startRot, targetLocalRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        weaponPivot.localRotation = targetLocalRot;
    }
}