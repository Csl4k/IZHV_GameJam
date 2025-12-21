using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Global")]
    public Camera mainCamera;
    public Animator animator;

    [Header("Stats")]
    public float health = 10.0f;
    public float speed = 5.0f;

    [SerializeField] private SpriteRenderer spriteRenderer;


    [Header("Combo Settings")]
    public float comboResetTime = 1.0f;
    public float lungeForce = 10f;
    public float stepForce = 3f;

    [Header("Dodge Settings")]
    public float dodgeImpulse = 8f;
    public float dodgeCooldown = 0.45f;

    public Transform aimOrigin;

    private bool isDodging = false;
    private float lastDodgeTime = -999f;
    private Vector2 lastMoveDir = Vector2.up;

    private Rigidbody2D mRigidBody;
    private Vector2 mMoveInput = Vector2.zero;
    private Vector2 mLookInput = Vector2.zero;
    private Vector2 dodgeDir = Vector2.up;

    // Combo Variables
    private int comboCounter = 0;
    private float lastAttackTime = 0;
    private bool isAttacking = false;
    private bool isParrying = false;
    private bool isHealing = false;

    private void Awake()
    {

        mRigidBody = GetComponent<Rigidbody2D>();
        InitializeReferences();
    }

    void Start()
    {
        InitializeReferences();
    }

    // NEW: This function ensures all references are properly set
    private void InitializeReferences()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (animator == null) animator = GetComponent<Animator>();

        // Reset all state variables when scene loads
        isDodging = false;
        isAttacking = false;
        isParrying = false;
        comboCounter = 0;

        // Ensure rigidbody is properly configured
        if (mRigidBody != null)
        {
            mRigidBody.velocity = Vector2.zero;
            mRigidBody.angularVelocity = 0f;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // CRITICAL FIX: Reinitialize everything after scene change
        InitializeReferences();

        // Reset input states
        mMoveInput = Vector2.zero;
        mLookInput = Vector2.zero;
        lastMoveDir = Vector2.up;
    }

    void Update()
    {
        if (!isDodging) RotatePlayer();

        if (Time.time - lastAttackTime > comboResetTime && !isAttacking)
        {
            comboCounter = 0;
            if (animator != null) animator.SetInteger("Combo", 0);
        }
    }


    void FixedUpdate()
    {

        if (!isAttacking && !isParrying && !isDodging && !isHealing)
        {
            Vector2 targetPosition = mRigidBody.position + mMoveInput * speed * Time.fixedDeltaTime;
            mRigidBody.MovePosition(targetPosition);
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        mMoveInput = ctx.ReadValue<Vector2>();

        if (mMoveInput.sqrMagnitude > 0.01f)
            lastMoveDir = mMoveInput.normalized;

        if (spriteRenderer != null && Mathf.Abs(mMoveInput.x) > 0.01f)
        {
            spriteRenderer.flipX = mMoveInput.x < 0;
        }
    }

    public void OnHeal(InputAction.CallbackContext ctx)
    {
        if (GameManager.IsUIOpen) return;
        if (!ctx.started) return;

        if (isDodging || isAttacking || isParrying || isHealing) return;

        // Must have something to heal with
        var hp = GetComponent<PlayerHealth>();
        if (hp == null) return;
        if (!hp.CanUsePotion()) return; // we’ll add this helper below

        isHealing = true;
        if (animator != null) animator.SetTrigger("Heal");
    }


    public void OnDodge(InputAction.CallbackContext ctx)
    {

        if (GameManager.IsUIOpen) return;
        if (!ctx.started) return;

        if (isAttacking || isParrying || isDodging || isHealing) return;
        if (Time.time < lastDodgeTime + dodgeCooldown) return;

        lastDodgeTime = Time.time;
        isDodging = true;

        // stop drifting
        mRigidBody.velocity = Vector2.zero;

        dodgeDir = (mMoveInput.sqrMagnitude > 0.01f) ? mMoveInput.normalized : lastMoveDir;
        if (dodgeDir.sqrMagnitude < 0.01f) dodgeDir = Vector2.up;



        if (animator != null) animator.SetTrigger("Dodge");
    }

    public void ApplyDodgeImpulse(float eventModifier)
    {
        // called from animation event at the "burst" frame
        mRigidBody.velocity = Vector2.zero;

        // Prefer live input; fallback to last move direction; final fallback = forward
        Vector2 dir = dodgeDir;

        mRigidBody.AddForce(dir * dodgeImpulse * eventModifier, ForceMode2D.Impulse);
        Debug.Log($"[DODGE IMPULSE] dir={dir} velAfter={mRigidBody.velocity}");
    }

    public void FinishDodgeAnimation()
    {
        Debug.Log($"[DODGE END] time={Time.time:F3} velBeforeZero={mRigidBody.velocity}");
        isDodging = false;
        mRigidBody.velocity = Vector2.zero;
    }


    public void OnLook(InputAction.CallbackContext ctx) => mLookInput = ctx.ReadValue<Vector2>();

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (GameManager.IsUIOpen) return;
        if (ctx.started)
        {
            if (isDodging) return;
            if (isAttacking) return;
            if (isHealing) return;
            PerformComboAttack();
        }
    }

    public void OnParry(InputAction.CallbackContext ctx)
    {
        if (GameManager.IsUIOpen) return;
        if (isDodging) return;
        if (isHealing) return;
        if (ctx.started && !isAttacking && !isParrying)
        {
            StartCoroutine(PerformParry());
        }
    }

    System.Collections.IEnumerator PerformParry()
    {
        isParrying = true;

        // Kill momentum so you don't slide while parrying
        mRigidBody.velocity = Vector2.zero;

        if (animator != null) animator.SetTrigger("Parry");

        // Safety Net: In case Animation Event fails
        yield return new WaitForSeconds(1.0f);
        if (isParrying) FinishParryAnimation();
    }

    public void FinishHealAnimation()
    {
        isHealing = false;
    }


    public void FinishParryAnimation()
    {
        isParrying = false;
        // Ensure hitbox is off in case we were interrupted
        BroadcastMessage("DisableParry", SendMessageOptions.DontRequireReceiver);
    }

    void PerformComboAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        comboCounter++;
        if (comboCounter > 3) comboCounter = 1; // Clamp to max 3

        if (animator != null)
        {
            animator.SetInteger("Combo", comboCounter);
            animator.SetTrigger("Attack");
        }
    }

    public void ApplyAttackMovement(float eventModifier)
    {
        mRigidBody.velocity = Vector2.zero;

        // Use aim direction instead of player rotation
        Vector2 forceDir = (aimOrigin != null) ? (Vector2)aimOrigin.up : lastMoveDir;
        if (forceDir.sqrMagnitude < 0.01f) forceDir = Vector2.up;

        float force = (comboCounter == 3) ? lungeForce : stepForce;
        mRigidBody.AddForce(forceDir * force * eventModifier, ForceMode2D.Impulse);
    }

    public void FinishAttackAnimation()
    {
        isAttacking = false;
        // Reset velocity again at the end so you don't slide forever after a hit
        mRigidBody.velocity = Vector2.zero;
    }

    private void RotatePlayer()
    {
        if (!aimOrigin || !mainCamera) return;

        Vector2 look = mLookInput;

        // If look values are small, assume this is a stick direction (-1..1)
        bool looksLikeStick = look.sqrMagnitude <= 1.05f;

        Vector2 dir;

        if (looksLikeStick)
        {
            // Stick mode: look IS a direction
            if (look.sqrMagnitude < 0.01f) return; // no stick input
            dir = look.normalized;
        }
        else
        {
            // Mouse mode: look IS screen position in pixels
            float z = Mathf.Abs(mainCamera.transform.position.z - aimOrigin.position.z);
            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(new Vector3(look.x, look.y, z));
            dir = ((Vector2)mouseWorld - (Vector2)aimOrigin.position).normalized;
            if (dir.sqrMagnitude < 0.0001f) return;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        aimOrigin.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

}