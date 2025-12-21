using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Setup")]
    public Transform player;
    public Collider2D swordCollider;
    public Transform weaponPivot;
    public Transform rotatePivot;

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
    private bool isStunned = false;

    private bool isDead = false;
    public bool IsDead => isDead;

    private bool canSeePlayerCached = false;
    public bool IsAware => canSeePlayerCached || alertTimer > 0f;

    private Rigidbody2D rb;
    private Rigidbody2D playerRb;

    private float baseDetectRange;
    private SpriteRenderer sr;
    private Vector2 lastSeenPosition;
    private float alertTimer = 0f;

    private Color originalColor;



    private Coroutine stunCoroutine;
    private Color baseColor;
    public bool IsStunned => isStunned;
    public Color BaseColor => baseColor;

    private Coroutine attackCoroutine;

    public bool IsAttacking => isAttacking;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        baseDetectRange = detectRange;


        if (sr != null) baseColor = sr.color;


        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerRb = playerObj.GetComponent<Rigidbody2D>();
            }
        }


        if (swordCollider != null) swordCollider.enabled = false;
    }



    void Update()
    {
        if (player == null) return;
        if (isStunned) return;

        float distance = Vector2.Distance(transform.position, player.position);

        bool wallInWay = Physics2D.Linecast(transform.position, player.position, obstacleLayer);
        bool canSeePlayer = (distance < detectRange) && !wallInWay;
        canSeePlayerCached = canSeePlayer;

        if (canSeePlayer)
        {
            lastSeenPosition = player.position;
            alertTimer = 10f;
            detectRange = baseDetectRange * 2f;
        }

        if (!isAttacking)
        {
            if (canSeePlayer) RotateTowards(player.position);
            else if (alertTimer > 0) RotateTowards(lastSeenPosition);
        }

        if (!isAttacking)
        {
            if (canSeePlayer)
            {
                if (distance > rushTriggerDistance)
                {
                    StartAttack(DashAttackRoutine(true));
                }
                else if (distance > attackRange)
                {
                    Vector2 moveDir = ((Vector2)player.position - (Vector2)transform.position).normalized;
                    transform.position += (Vector3)(moveDir * moveSpeed * Time.deltaTime);
                    UpdateSpriteFlip(moveDir);
                }
                else
                {
                    if (Random.value > 0.5f) StartAttack(SwingRoutine());
                    else StartAttack(DashAttackRoutine(false));
                }
            }
            else if (alertTimer > 0)
            {
                alertTimer -= Time.deltaTime;

                float distToTarget = Vector2.Distance(transform.position, lastSeenPosition);
                if (distToTarget > 0.5f)
                {
                    transform.position = Vector2.MoveTowards(transform.position, lastSeenPosition, moveSpeed * Time.deltaTime);
                    Vector2 moveDir = (lastSeenPosition - (Vector2)transform.position).normalized;
                    UpdateSpriteFlip(moveDir);
                }
            }
            else
            {
                detectRange = baseDetectRange;
            }
        }
    }

    private void StartAttack(IEnumerator routine)
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(routine);
    }

    private void RotateTowards(Vector2 targetPos)
    {
        if (rotatePivot == null) return;

        Vector2 direction = targetPos - (Vector2)rotatePivot.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rotatePivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    private void UpdateSpriteFlip(Vector2 moveDir)
    {
        if (sr == null) return;
        if (Mathf.Abs(moveDir.x) > 0.01f)
            sr.flipX = moveDir.x < 0;
    }


    private void CancelCurrentAttackImmediate()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        isAttacking = false;

        if (rb != null) rb.velocity = Vector2.zero;

        if (swordCollider != null) swordCollider.enabled = false;

        if (weaponPivot != null) weaponPivot.localRotation = Quaternion.identity;
    }

    private void SetColorSafe(Color c)
    {
        if (sr == null) return;
        if (isStunned) return; // lock color during stun
        sr.color = c;
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

        SetColorSafe(Color.red);

        float timer = 0f;
        while (timer < windup)
        {
            if (!this.enabled) yield break;
            if (isStunned) yield break;

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

        if (isStunned) yield break;

        if (swordCollider != null) swordCollider.enabled = true;

        Transform dashBasis = (rotatePivot != null) ? rotatePivot : transform;
        rb.velocity = (Vector2)dashBasis.right * speed;


        timer = 0f;
        while (timer < duration)
        {
            if (isStunned) yield break;
            timer += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        rb.drag = startDrag;

        if (swordCollider != null) swordCollider.enabled = false;

        SetColorSafe(baseColor);

        timer = 0f;
        while (timer < attackCooldown)
        {
            if (isStunned) yield break;
            timer += Time.deltaTime;
            yield return null;
        }

        isAttacking = false;
        attackCoroutine = null;
    }

    IEnumerator SwingRoutine()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;


        yield return StartCoroutine(RotatePivot(Quaternion.Euler(0, 0, 45), 0.2f));
        if (isStunned) yield break;

        if (swordCollider != null) swordCollider.enabled = true;

        yield return StartCoroutine(RotatePivot(Quaternion.Euler(0, 0, -135), 0.15f));
        if (isStunned) yield break;

        yield return StartCoroutine(RotatePivot(Quaternion.Euler(0, 0, 0), 0.2f));
        if (swordCollider != null) swordCollider.enabled = false;


        float timer = 0f;
        while (timer < attackCooldown)
        {
            if (isStunned) yield break;
            timer += Time.deltaTime;
            yield return null;
        }

        isAttacking = false;
        attackCoroutine = null;
    }

    IEnumerator RotatePivot(Quaternion targetLocalRot, float duration)
    {
        if (weaponPivot == null) yield break;

        Quaternion startRot = weaponPivot.localRotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (isStunned) yield break;
            weaponPivot.localRotation = Quaternion.Slerp(startRot, targetLocalRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        weaponPivot.localRotation = targetLocalRot;
    }

    public void ApplyStun(float duration)
    {
        if (isDead) return;

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        if (isDead) yield break;

        isStunned = true;
        CancelCurrentAttackImmediate();

        if (sr != null) sr.color = Color.yellow;

        yield return new WaitForSeconds(duration);

        if (isDead) yield break;

        isStunned = false;
        if (sr != null) sr.color = baseColor;

        stunCoroutine = null;
    }


    public void OnDeath()
    {
        isDead = true;
        isStunned = false;

        // stop only what can "restore" visuals / keep attacking
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

        CancelCurrentAttackImmediate();

        if (swordCollider != null) swordCollider.enabled = false;
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
