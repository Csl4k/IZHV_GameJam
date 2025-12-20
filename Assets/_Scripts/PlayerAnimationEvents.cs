using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerController playerController;
    private PlayerSword swordScript;
    public GameObject parryHitbox;

    void Start()
    {
        playerController = GetComponentInParent<PlayerController>();

        swordScript = GetComponentInChildren<PlayerSword>(true);
    }

    public void EnableHitbox()
    {
        if (swordScript != null)
            swordScript.gameObject.SetActive(true);
    }

    public void DisableHitbox()
    {
        if (swordScript != null)
            swordScript.gameObject.SetActive(false);
    }

    public void StepForward(float force)
    {
        if (playerController != null)
        {
            playerController.ApplyAttackMovement(force);
        }
    }

    public void DodgeBurst(float force)
    {
        if (playerController != null)
            playerController.ApplyDodgeImpulse(force);
    }

    public void EndDodge()
    {
        if (playerController != null)
            playerController.FinishDodgeAnimation();
    }


    public void EnableParry()
    {
        if (parryHitbox != null) parryHitbox.SetActive(true);
    }

    public void DisableParry()
    {
        if (parryHitbox != null) parryHitbox.SetActive(false);
    }

    public void EndParry()
    {
        if (playerController != null)
        {
            playerController.FinishParryAnimation();
        }
    }

    public void EndAttack()
    {
        if (playerController != null)
        {
            playerController.FinishAttackAnimation();
        }
    }
}