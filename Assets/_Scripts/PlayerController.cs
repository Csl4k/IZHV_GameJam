using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))] // forces Unity to add a Rigidbody2D
public class PlayerController : MonoBehaviour
{
    [Header("Global")]
    public Camera mainCamera;

    [Header("Gameplay")]
    public float health = 10.0f;
    public float maxHealth = 10.0f;
    public float speed = 5.0f;

    [Tooltip("Time in seconds before player can take damage again")]
    public float damageDelay = 0.7f;

    [Header("Combat")]
    public GameObject attackHitbox;
    public float attackDuration = 0.2f;

    private Rigidbody2D mRigidBody;
    private Vector2 mMoveInput = Vector2.zero;
    private Vector2 mLookInput = Vector2.zero;
    private float mDamageCooldown = 0.0f;
    private bool mIsAttacking = false;

    private void Awake()
    {
        mRigidBody = GetComponent<Rigidbody2D>();

        if (mainCamera == null) mainCamera = Camera.main;

        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    void Update()
    {
        // cooldown timer logic
        if (mDamageCooldown > 0)
        {
            mDamageCooldown -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        // movement
        Vector2 targetPosition = mRigidBody.position + mMoveInput * speed * Time.fixedDeltaTime;
        mRigidBody.MovePosition(targetPosition);

        // rotation
        RotatePlayer();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        // WASD or gamepad left stick
        mMoveInput = ctx.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        // mouse position or right stick
        mLookInput = ctx.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        // "started" - button down. 
        if (ctx.started && !mIsAttacking)
        {
            StartCoroutine(PerformAttack());
        }
    }

    private void RotatePlayer()
    {
        if (mLookInput == Vector2.zero) return;

        // fix camera depth
        Vector3 screenPos = new Vector3(mLookInput.x, mLookInput.y, -mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(screenPos);
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        mRigidBody.rotation = angle - 90f;
    }

    private System.Collections.IEnumerator PerformAttack()
    {
        mIsAttacking = true;

        if (attackHitbox != null) attackHitbox.SetActive(true);

        yield return new WaitForSeconds(attackDuration);

        if (attackHitbox != null) attackHitbox.SetActive(false);

        mIsAttacking = false;
    }

    public void DamagePlayer(float damage)
    {
        // check cooldown
        if (mDamageCooldown > 0) return;

        health -= damage;
        mDamageCooldown = damageDelay;

        Debug.Log($"Player hit! HP: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player Died");

        gameObject.SetActive(false);
    }
}