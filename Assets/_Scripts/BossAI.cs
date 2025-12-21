using UnityEngine;
using System.Collections;

public class BossAI : MonoBehaviour
{
    [Header("Setup")]
    public Transform player;
    public Collider2D swordCollider;
    public Transform weaponPivot;
    public LayerMask obstacleLayer;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float chargeSpeed = 12f;
    public float teleportRange = 8f;

    [Header("Attack Ranges")]
    public float detectionRange = 15f;
    public float meleeRange = 3f;
    public float chargeRange = 10f;

    [Header("Combo Recovery")]
    public float comboRecoveryTime = 0.8f;


    [Header("Targeting / Aggro")]
    public float aggroRange = 12f;
    public float disengageRange = 18f;

    private bool isTargeting = false;
    public bool IsTargeting => isTargeting;

    private const float DETECTION_RANGE_CONST = 30f;
    private const float MELEE_RANGE_CONST = 3f;
    private const float CHARGE_RANGE_CONST = 10f;

    private const float DISENGAGE_RANGE_CONST = 40f;
    private const float DASH_TRIGGER_CONST = 10f;


    [System.Serializable]
    public class ComboStep
    {
        public string animatorTrigger = "Attack"; 
        public float windup = 0.10f;              
        public float active = 0.08f;             
        public float recovery = 0.18f;
        public float lungeSpeed = 3.5f; 
        public float lungeTime = 0.08f;
    }


    private const float CHASE_STOP_DISTANCE = 1.8f;   // how close boss will approach
    private const float CHASE_BUFFER = 0.25f;         // prevents jitter when hovering at the edge


    [Header("Combo Tuning")]
    public ComboStep[] combo = new ComboStep[]
    {
    new ComboStep(){ animatorTrigger="Attack1", windup=0.10f, active=0.08f, recovery=0.16f, lungeSpeed=3.5f, lungeTime=0.08f },
    new ComboStep(){ animatorTrigger="Attack2", windup=0.08f, active=0.07f, recovery=0.16f, lungeSpeed=4.0f, lungeTime=0.08f },
    new ComboStep(){ animatorTrigger="Attack3", windup=0.12f, active=0.10f, recovery=0.22f, lungeSpeed=4.5f, lungeTime=0.10f },
    };

    public float comboStepRefaceSpeed = 999f; // basically "snap" to player direction between steps
    public float comboMaxAngleToContinue = 110f; // if player is way behind, stop combo


    [Header("Dash (replaces Teleport)")]
    public float dashSpeed = 16f;
    public float dashDuration = 0.65f;
    public float dashWindup = 0.15f;
    public float dashTriggerDistance = 10f;
    public float dashStopDistanceFromPlayer = 1.5f; // stop early if we get close
    public bool dashDealsDamage = true;             // keeps sword hitbox on during dash


    [Header("Attack Cooldowns")]
    public float comboAttackCooldown = 3f;
    public float chargeAttackCooldown = 5f;
    public float spinAttackCooldown = 7f;
    public float teleportCooldown = 10f;

    [Header("Phase 2 (50% HP)")]
    public GameObject shockwavePrefab;
    public float shockwaveInterval = 8f;

    [Header("Phase 3 (25% HP)")]
    public GameObject shadowClonePrefab;
    public int shadowCloneCount = 2;
    public float berserkSpeedMultiplier = 1.5f;

    [Header("Visual Effects")]
    public SpriteRenderer spriteRenderer;
    public GameObject teleportEffectPrefab;
    public GameObject chargeWarningPrefab;
    public ParticleSystem hitParticles;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip roarSound;
    public AudioClip chargeSound;
    public AudioClip teleportSound;
    public AudioClip spinSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    // References
    private Rigidbody2D rb;
    private Animator animator;
    private BossHealth bossHealth;

    // State flags
    private bool isDead = false;
    private bool isStunned = false;
    private bool isAttacking = false;

    // Phase tracking
    private int currentPhase = 1;
    private bool phase2Triggered = false;
    private bool phase3Triggered = false;

    // Timers
    private float lastComboTime = -999f;
    private float lastChargeTime = -999f;
    private float lastSpinTime = -999f;
    private float lastTeleportTime = -999f;

    // Coroutines
    private Coroutine attackCoroutine;
    private Coroutine stunCoroutine;
    private Coroutine shockwaveLoopCoroutine;

    private bool comboStepFinished = false;


    // Visual baseline
    private Color phaseBaseColor = Color.white;
    public Color PhaseBaseColor => phaseBaseColor;
    // Add with the other setup fields
    public Transform rotatePivot;

    public bool IsAttacking => isAttacking;
    public bool IsDead => isDead;

    private enum BossState { Idle, Chase }
    private BossState currentState = BossState.Idle;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bossHealth = GetComponent<BossHealth>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            phaseBaseColor = spriteRenderer.color;

        if (swordCollider != null)
        {
            swordCollider.enabled = false;
            swordCollider.isTrigger = true; // important for OnTriggerEnter2D sword scripts
        }
    }

    void Awake()
    {
        // Force correct ranges every play session (ignore inspector misconfig)
        detectionRange = DETECTION_RANGE_CONST;
        meleeRange = MELEE_RANGE_CONST;
        chargeRange = CHARGE_RANGE_CONST;

        disengageRange = DISENGAGE_RANGE_CONST;
        dashTriggerDistance = DASH_TRIGGER_CONST;

    }


    void Update()
    {
        if (isDead || player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // Acquire target
        if (!isTargeting)
        {
            if (distToPlayer <= aggroRange)
                SetTargeting(true);
            else
            {
                // not targeting -> idle, do nothing
                if (rb != null) rb.velocity = Vector2.zero;
                return;
            }
        }
        else
        {
            if (disengageRange > 0f && distToPlayer >= disengageRange)
            {
                SetTargeting(false);
                return;
            }
        }

        if (isStunned) return;

        CheckPhaseTransition();
        UpdateState();
    }


    void FixedUpdate()
    {
        if (isDead || isStunned || isAttacking) return;

        if (currentState == BossState.Chase)
            ChasePlayer();
    }

    void SetTargeting(bool value)
    {
        if (isTargeting == value) return;
        isTargeting = value;

        // show/hide healthbar when targeting changes
        if (bossHealth != null)
            bossHealth.SetHealthBarVisible(isTargeting);

        // stop moving when we lose target
        if (!isTargeting && rb != null)
            rb.velocity = Vector2.zero;
    }


    // Called by BossHealth on death
    public void OnBossDeath()
    {
        if (isDead) return;
        isDead = true;

        if (bossHealth != null)
            bossHealth.SetHealthBarVisible(false);


        CancelAttackImmediate();

        if (shockwaveLoopCoroutine != null)
        {
            StopCoroutine(shockwaveLoopCoroutine);
            shockwaveLoopCoroutine = null;
        }

        PlaySound(deathSound);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        if (animator != null)
            animator.SetTrigger("Death");

        if (swordCollider != null)
            swordCollider.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.gray;
    }

    void CheckPhaseTransition()
    {
        if (bossHealth == null) return;

        float healthPercent = bossHealth.HealthPercent;

        if (healthPercent <= 0.5f && !phase2Triggered)
        {
            phase2Triggered = true;
            currentPhase = 2;
            StartCoroutine(EnterPhase2());
        }

        if (healthPercent <= 0.25f && !phase3Triggered)
        {
            phase3Triggered = true;
            currentPhase = 3;
            StartCoroutine(EnterPhase3());
        }
    }

    IEnumerator EnterPhase2()
    {
        PlaySound(roarSound);

        SetPhaseColor(new Color(1f, 0.7f, 0.7f));

        yield return new WaitForSeconds(1f);

        if (shockwaveLoopCoroutine != null) StopCoroutine(shockwaveLoopCoroutine);
        shockwaveLoopCoroutine = StartCoroutine(ShockwaveLoop());
    }

    IEnumerator EnterPhase3()
    {
        PlaySound(roarSound);

        // Spawn shadow clones
        for (int i = 0; i < shadowCloneCount; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);
            if (shadowClonePrefab != null)
                Instantiate(shadowClonePrefab, spawnPos, Quaternion.identity);
        }

        SetPhaseColor(new Color(0.8f, 0.3f, 0.3f));

        moveSpeed *= berserkSpeedMultiplier;
        chargeSpeed *= berserkSpeedMultiplier;

        yield return new WaitForSeconds(1f);
    }

    IEnumerator ShockwaveLoop()
    {
        while (!isDead && currentPhase >= 2)
        {
            yield return new WaitForSeconds(shockwaveInterval);
            if (!isDead && !isStunned)
                SpawnShockwave();
        }
    }

    void SpawnShockwave()
    {
        if (shockwavePrefab != null)
        {
            Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
            PlaySound(spinSound);
        }
    }

    void UpdateState()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (isAttacking || isStunned) return;

        // Build a candidate list instead of if/else starvation.
        var options = new System.Collections.Generic.List<(System.Func<IEnumerator> attack, float weight)>();

        if (dist <= meleeRange && CanComboAttack())
        {
            float w = Mathf.Lerp(2.2f, 1.2f, dist / Mathf.Max(0.01f, meleeRange));
            options.Add((PerformComboAttack, w));
        }

        if (CanSpinAttack())
        {

            float ideal = meleeRange * 1.2f;
            float falloff = Mathf.Clamp01(1f - Mathf.Abs(dist - ideal) / (meleeRange * 1.5f));
            float w = 0.6f + 1.4f * falloff;

            if (dist <= meleeRange * 2.5f)
                options.Add((PerformSpinAttack, w));
        }

        if (dist <= chargeRange && dist >= meleeRange && CanChargeAttack())
        {
            float t = Mathf.InverseLerp(meleeRange, chargeRange, dist);
            float w = Mathf.Lerp(0.7f, 2.0f, t);
            options.Add((PerformChargeAttack, w));
        }

        if (dist >= dashTriggerDistance && CanTeleport())
        {
            float t = Mathf.InverseLerp(dashTriggerDistance, aggroRange, dist);
            float w = Mathf.Lerp(0.8f, 1.6f, t);
            options.Add((PerformLongDash, w));
        }

        if (options.Count > 0)
        {
            var chosen = PickWeighted(options);
            StartAttack(chosen());
            currentState = BossState.Idle;
            return;
        }

        // Otherwise: chase
        currentState = BossState.Chase;
    }

    System.Func<IEnumerator> PickWeighted(
        System.Collections.Generic.List<(System.Func<IEnumerator> attack, float weight)> options)
    {
        float total = 0f;
        for (int i = 0; i < options.Count; i++) total += Mathf.Max(0.0001f, options[i].weight);

        float r = Random.value * total;
        float acc = 0f;

        for (int i = 0; i < options.Count; i++)
        {
            acc += Mathf.Max(0.0001f, options[i].weight);
            if (r <= acc) return options[i].attack;
        }

        return options[options.Count - 1].attack;
    }

    void ChasePlayer()
    {
        if (rb == null || player == null) return;

        Vector2 toPlayer = (Vector2)player.position - rb.position;
        float dist = toPlayer.magnitude;

        if (dist <= CHASE_STOP_DISTANCE)
        {
            rb.velocity = Vector2.zero;
            RotateToPlayer();
            currentState = BossState.Idle;
            return;
        }

        float slowStart = CHASE_STOP_DISTANCE + CHASE_BUFFER;
        float speedMult = 1f;

        if (dist < slowStart)
        {
            speedMult = Mathf.InverseLerp(CHASE_STOP_DISTANCE, slowStart, dist);
        }

        Vector2 dir = toPlayer.normalized;
        rb.velocity = dir * (moveSpeed * speedMult);
        RotateToPlayer();
    }


    IEnumerator PerformLongDash()
    {
        isAttacking = true;
        lastTeleportTime = Time.time;

        RotateToPlayer();

        if (teleportEffectPrefab != null)
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
        PlaySound(teleportSound);

        SetTint(phaseBaseColor, 0.35f);
        yield return new WaitForSeconds(dashWindup);
        RestorePhaseColor();

        Transform basis = (rotatePivot != null) ? rotatePivot : transform;
        Vector2 dir = (Vector2)basis.up;

        float elapsed = 0f;

        if (swordCollider != null)
            swordCollider.enabled = dashDealsDamage;

        while (elapsed < dashDuration && !isDead && !isStunned)
        {
            elapsed += Time.deltaTime;

            if (player != null)
            {
                float d = Vector2.Distance(transform.position, player.position);
                if (d <= dashStopDistanceFromPlayer)
                    break;
            }

            if (rb != null)
            {
                Vector2 current = rb.position;
                Vector2 next = current + dir * dashSpeed * Time.deltaTime;

                float radius = 0.4f;
                RaycastHit2D hit = Physics2D.CircleCast(current, radius, dir, (next - current).magnitude, obstacleLayer);
                if (hit.collider != null)
                {
                    rb.position = hit.centroid;
                    break;
                }

                rb.MovePosition(next);
            }
            else
            {
                transform.position += (Vector3)(dir * dashSpeed * Time.deltaTime);
            }

            yield return null;
        }

        if (rb != null) rb.velocity = Vector2.zero;
        if (swordCollider != null) swordCollider.enabled = false;

        isAttacking = false;
        attackCoroutine = null;
    }


    void StartAttack(IEnumerator routine)
    {
        CancelAttackImmediate();
        attackCoroutine = StartCoroutine(routine);
    }

    void CancelAttackImmediate()
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

    bool CanComboAttack() => Time.time >= lastComboTime + comboAttackCooldown;
    bool CanChargeAttack() => Time.time >= lastChargeTime + chargeAttackCooldown;
    bool CanSpinAttack() => Time.time >= lastSpinTime + spinAttackCooldown;
    bool CanTeleport() => Time.time >= lastTeleportTime + teleportCooldown;

    void RotateToPlayer()
    {
        if (rotatePivot == null || player == null) return;

        Vector2 direction = (Vector2)player.position - (Vector2)rotatePivot.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rotatePivot.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        if (spriteRenderer != null && Mathf.Abs(direction.x) > 0.01f)
            spriteRenderer.flipX = direction.x < 0;
    }


    IEnumerator PerformComboAttack()
    {
        isAttacking = true;
        lastComboTime = Time.time;
        RestorePhaseColor();
        if (rb != null) rb.velocity = Vector2.zero;
        if (swordCollider != null) swordCollider.enabled = false;

        // 3-hit combo by default
        int steps = 3;

        for (int i = 0; i < steps; i++)
        {
            RestorePhaseColor();
            if (isDead || isStunned) break;

            // Face player at the start of each swing
            RotateToPlayer();

            // stop combo if player got way behind
            if (player != null && rotatePivot != null)
            {
                Vector2 toPlayer = ((Vector2)player.position - (Vector2)rotatePivot.position).normalized;
                float angle = Vector2.Angle(rotatePivot.up, toPlayer);
                if (angle > comboMaxAngleToContinue)
                    break;
            }

            comboStepFinished = false;

            if (animator != null)
            {
                animator.SetInteger("Combo", i + 1);
                animator.SetTrigger("Attack");
            }

            float timeout = 1.1f;
            float start = Time.time;

            while (!comboStepFinished && !isDead && !isStunned && (Time.time - start) < timeout)
                yield return null;

            comboStepFinished = true;


            if (rb != null) rb.velocity = Vector2.zero;
            if (swordCollider != null) swordCollider.enabled = false;
        }

        if (rb != null) rb.velocity = Vector2.zero;
        if (swordCollider != null) swordCollider.enabled = false;

        // reset combo param
        if (animator != null) animator.SetInteger("Combo", 0);

        yield return StartCoroutine(ComboRecovery());

        isAttacking = false;
        attackCoroutine = null;
    }

    IEnumerator ComboRecovery()
    {
        // hard stop
        if (rb != null)
            rb.velocity = Vector2.zero;

        // lock movement & attacks
        isStunned = true;


        yield return new WaitForSeconds(comboRecoveryTime);

        isStunned = false;
    }



    IEnumerator PerformChargeAttack()
    {
        isAttacking = true;
        lastChargeTime = Time.time;

        GameObject warning = null;
        if (chargeWarningPrefab != null)
            warning = Instantiate(chargeWarningPrefab, transform.position, Quaternion.identity);

        SetTint(Color.red, 0.8f);
        PlaySound(chargeSound);
        yield return new WaitForSeconds(0.8f);

        if (warning != null) Destroy(warning);

        Transform basis = (rotatePivot != null) ? rotatePivot : transform;
        rb.velocity = (Vector2)basis.up * chargeSpeed;

        if (swordCollider != null) swordCollider.enabled = true;

        yield return new WaitForSeconds(0.6f);

        if (swordCollider != null) swordCollider.enabled = false;
        rb.velocity = Vector2.zero;

        RestorePhaseColor();

        isAttacking = false;
        attackCoroutine = null;
    }

    IEnumerator PerformSpinAttack()
    {
        isAttacking = true;
        lastSpinTime = Time.time;

        PlaySound(spinSound);
        rb.velocity = Vector2.zero;

        if (swordCollider != null) swordCollider.enabled = true;

        float spinDuration = 0.9f;
        float elapsed = 0f;
        Quaternion startLocal = (weaponPivot != null) ? weaponPivot.localRotation : Quaternion.identity;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spinDuration);
            float z = Mathf.Lerp(0f, 360f, t);
            if (weaponPivot != null)
                weaponPivot.localRotation = startLocal * Quaternion.Euler(0f, 0f, z);

            yield return null;
        }

        if (weaponPivot != null)
            weaponPivot.localRotation = startLocal;

        if (swordCollider != null) swordCollider.enabled = false;

        isAttacking = false;
        attackCoroutine = null;
    }

    public void PlayHitFeedback()
    {
        if (hitParticles != null) hitParticles.Play();
        PlaySound(hurtSound);
        StartCoroutine(FlashRed());
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer == null) yield break;

        Color before = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.08f);

        if (!isDead && !isStunned)
            spriteRenderer.color = before;
    }

    public void Stun(float duration)
    {
        if (isDead) return;

        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        CancelAttackImmediate();

        if (spriteRenderer != null)
            spriteRenderer.color = Color.yellow;

        yield return new WaitForSeconds(duration);

        isStunned = false;
        RestorePhaseColor();

        stunCoroutine = null;
    }

    void SetPhaseColor(Color c)
    {
        phaseBaseColor = c;
        RestorePhaseColor();
    }

    // Called by Animation Events
    public void AE_SwordOn()
    {
        if (isDead || isStunned) return;
        if (swordCollider != null) swordCollider.enabled = true;
    }

    public void AE_SwordOff()
    {
        if (swordCollider != null) swordCollider.enabled = false;
    }

    public void AE_StopMovement()
    {
        if (rb != null) rb.velocity = Vector2.zero;
    }
    public void AE_EndAttackStep()
    {
        comboStepFinished = true;
    }

    public void AE_Lunge(float speed)
    {
        if (isDead || isStunned) return;
        if (rb == null) return;

        Transform basis = (rotatePivot != null) ? rotatePivot : transform;
        rb.velocity = (Vector2)basis.up * speed;

    }


    void RestorePhaseColor()
    {
        SetTint(phaseBaseColor, 1f);
    }

    void SetTint(Color c, float alpha)
    {
        if (spriteRenderer == null) return;

        spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
