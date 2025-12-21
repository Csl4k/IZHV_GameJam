using UnityEngine;

/// <summary>
/// Boss audio manager - drag sound clips in Inspector
/// </summary>
public class BossSounds : MonoBehaviour
{
    [Header("Combat Sounds")]
    public AudioClip[] attackSounds;
    public AudioClip chargeWindupSound;
    public AudioClip chargeImpactSound;
    public AudioClip shockwaveSound;

    [Header("Phase Transitions")]
    public AudioClip phaseTransitionSound;
    public AudioClip shadowCloneSpawnSound;

    [Header("Damage Sounds")]
    public AudioClip takeDamageSound;
    public AudioClip stunSound;
    public AudioClip deathSound;

    [Header("Movement Sounds")]
    public AudioClip teleportSound;
    public AudioClip landSound;

    [Header("Audio Source")]
    public AudioSource audioSource;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayAttack()
    {
        if (attackSounds.Length > 0)
            PlayRandomSound(attackSounds);
    }

    public void PlayChargeWindup()
    {
        PlaySound(chargeWindupSound);
    }

    public void PlayChargeImpact()
    {
        PlaySound(chargeImpactSound);
    }

    public void PlayShockwave()
    {
        PlaySound(shockwaveSound);
    }

    public void PlayPhaseTransition()
    {
        PlaySound(phaseTransitionSound);
    }

    public void PlayShadowCloneSpawn()
    {
        PlaySound(shadowCloneSpawnSound);
    }

    public void PlayTakeDamage()
    {
        PlaySound(takeDamageSound);
    }

    public void PlayStun()
    {
        PlaySound(stunSound);
    }

    public void PlayDeath()
    {
        PlaySound(deathSound);
    }

    public void PlayTeleport()
    {
        PlaySound(teleportSound);
    }

    public void PlayLand()
    {
        PlaySound(landSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    private void PlayRandomSound(AudioClip[] clips)
    {
        if (clips.Length > 0 && audioSource != null)
        {
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }
}