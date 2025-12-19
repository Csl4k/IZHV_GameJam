using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Setup")]
    public Transform player;
    public Collider2D swordCollider;
    public Transform weaponPivot;

    public LayerMask obstacleLayer;

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

    private float baseDetectRange;
    private Vector2 lastSeenPosition;
    private float alertTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // store the starting range so we can double it later
        baseDetectRange = detectRange;

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

        // vision
        bool wallInWay = Physics2D.Linecast(transform.position, player.position, obstacleLayer);
        bool canSeePlayer = (distance < detectRange) && !wallInWay;

        if (canSeePlayer)
        {
            lastSeenPosition = player.position;
            alertTimer = 10f;
            detectRange = baseDetectRange * 2f;
        }

        if (!isAttacking)
        {
            if (canSeePlayer)
            {
                RotateTowards(player.position);
            }
            else if (alertTimer > 0)
            {
                RotateTowards(lastSeenPosition);
            }
        }

        // behavior
        if (!isAttacking)
        {
            if (canSeePlayer)
            {
                // logic: Rush, Walk, or Fight
                if (distance > rushTriggerDistance)
                {
                    StartCoroutine(DashAttackRoutine(true));
                }
                else if (distance > attackRange)
                {
                    // walk
                    transform.position += transform.right * moveSpeed * Time.deltaTime;
                }
                else
                {
                    // fight
                    if (Random.value > 0.5f) StartCoroutine(SwingRoutine());
                    else StartCoroutine(DashAttackRoutine(false));
                }
            }

            else if (alertTimer > 0)
            {
                alertTimer -= Time.deltaTime;

                // move to the last position from memory
                float distToTarget = Vector2.Distance(transform.position, lastSeenPosition);
                if (distToTarget > 0.5f)
                {
                    transform.position = Vector2.MoveTowards(transform.position, lastSeenPosition, moveSpeed * Time.deltaTime);
                }
            }
            // idle
            else
            {
                detectRange = baseDetectRange; // reset range
            }
        }
    }

    void RotateTowards(Vector2 targetPos)
    {
        Vector3 direction = (Vector3)targetPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    IEnumerator DashAttackRoutine(bool isLongRush)
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        float speed = isLongRush ? longRushSpeed : shortLungeSpeed;
        float duration = isLongRush ? 0.6f : 0.45f;
        float windup = isLongRush ? 0.5f : 0.3f;

        float startDrag = rb.drag;
        rb.drag = 0f;

        if (weaponPivot != null) weaponPivot.localRotation = Quaternion.identity;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        Color originalColor = (sr != null) ? sr.color : Color.white;
        if (sr) sr.color = Color.red;

        float timer = 0f;
        while (timer < windup)
        {
            if (!this.enabled) yield break;

            bool wallInWay = Physics2D.Linecast(transform.position, player.position, obstacleLayer);
            if (!wallInWay)
            {
                lastSeenPosition = player.position;
                RotateTowards(player.position);
            }
            else
            {
                RotateTowards(lastSeenPosition);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (swordCollider != null) swordCollider.enabled = true;

 
        rb.velocity = transform.right * speed;

        yield return new WaitForSeconds(duration);


        rb.velocity = Vector2.zero;
        rb.drag = startDrag;

        if (swordCollider != null) swordCollider.enabled = false;
        if (sr) sr.color = originalColor;

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    IEnumerator SwingRoutine()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        yield return StartCoroutine(RotatePivot(Quaternion.Euler(0, 0, 45), 0.2f));

        if (swordCollider != null) swordCollider.enabled = true;
        yield return StartCoroutine(RotatePivot(Quaternion.Euler(0, 0, -135), 0.15f));

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

    public void BeginImpact()
    {
        this.enabled = false;
        rb.velocity = Vector2.zero;
    }

    public void Recover()
    {
        this.enabled = true;
    }
}