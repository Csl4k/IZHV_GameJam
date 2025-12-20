using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Global")]
    public Camera mainCamera;
    public Animator animator;

    [Header("Stats")]
    public float health = 10.0f;
    public float speed = 5.0f;

    [Header("Combo Settings")]
    public float comboResetTime = 1.0f;
    public float lungeForce = 10f;
    public float stepForce = 3f;

    [Header("Dodge Settings")]
    public float dodgeImpulse = 8f;
    public float dodgeCooldown = 0.45f;

    private bool isDodging = false;
    private float lastDodgeTime = -999f;
    private Vector2 lastMoveDir = Vector2.up;   // fallback direction if no input


    private Rigidbody2D mRigidBody;
    private Vector2 mMoveInput = Vector2.zero;
    private Vector2 mLookInput = Vector2.zero;
    private Vector2 dodgeDir = Vector2.up;

    // Combo Variables
    private int comboCounter = 0;
    private float lastAttackTime = 0;
    private bool isAttacking = false;

    private void Awake()
    {
        mRigidBody = GetComponent<Rigidbody2D>();
        if (mainCamera == null) mainCamera = Camera.main;
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Reset Combo if too much time passed
        if (Time.time - lastAttackTime > comboResetTime && !isAttacking)
        {
            comboCounter = 0;
            animator.SetInteger("Combo", 0);
        }
    }

    void FixedUpdate()
    {
        if (!isAttacking && !isParrying && !isDodging)
        {
            Vector2 targetPosition = mRigidBody.position + mMoveInput * speed * Time.fixedDeltaTime;
            mRigidBody.MovePosition(targetPosition);
        }
        if (!isDodging) RotatePlayer();

    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        mMoveInput = ctx.ReadValue<Vector2>();

        if (mMoveInput.sqrMagnitude > 0.01f)
            lastMoveDir = mMoveInput.normalized;
    }

    public void OnDodge(InputAction.CallbackContext ctx)
    {
        if (GameManager.IsUIOpen) return;
        if (!ctx.started) return;

        if (isAttacking || isParrying || isDodging) return;
        if (Time.time < lastDodgeTime + dodgeCooldown) return;

        lastDodgeTime = Time.time;
        isDodging = true;

        // stop drifting
        mRigidBody.velocity = Vector2.zero;

        dodgeDir = (mMoveInput.sqrMagnitude > 0.01f) ? mMoveInput.normalized : (Vector2)transform.up;
        animator.SetTrigger("Dodge"); // create this Trigger in Animator
    }

    public void ApplyDodgeImpulse(float eventModifier)
{
    // called from animation event at the "burst" frame
    mRigidBody.velocity = Vector2.zero;

    // Prefer live input; fallback to last move direction; final fallback = forward
    Vector2 dir = dodgeDir;
    mRigidBody.AddForce(dir * dodgeImpulse * eventModifier, ForceMode2D.Impulse);

}

public void FinishDodgeAnimation()
{
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
            PerformComboAttack();
        }
    }

    // 1. Add this variable
    private bool isParrying = false;


    // 3. Add the Input Handler
    public void OnParry(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (isDodging) return;
        if (ctx.started && !isAttacking && !isParrying)
        {
            StartCoroutine(PerformParry());
        }
    }

    // 4. The Logic
    System.Collections.IEnumerator PerformParry()
    {
        isParrying = true;

        // Kill momentum so you don't slide while parrying
        mRigidBody.velocity = Vector2.zero;

        animator.SetTrigger("Parry"); // You need to create this Trigger in Animator

        // Safety Net: In case Animation Event fails
        yield return new WaitForSeconds(1.0f);
        if (isParrying) FinishParryAnimation();
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

        animator.SetInteger("Combo", comboCounter);
        animator.SetTrigger("Attack");
    }

    // --- FIX FOR ACCUMULATING FORCE ---
    public void ApplyAttackMovement(float eventModifier)
    {
        // 1. Kill existing momentum so hits don't stack infinitely
        mRigidBody.velocity = Vector2.zero;

        // 2. Calculate direction (Forward relative to player rotation)
        Vector2 forceDir = transform.up;

        // 3. Determine force
        float force = (comboCounter == 3) ? lungeForce : stepForce;

        // 4. Apply Instant Force
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
        if (mLookInput == Vector2.zero) return;

        Vector3 screenPos = new Vector3(mLookInput.x, mLookInput.y, -mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(screenPos);
        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        mRigidBody.rotation = angle - 90f;
    }
}