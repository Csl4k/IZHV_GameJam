using UnityEngine;

public class EnemySounds : MonoBehaviour
{
    [Header("Combat Sounds")]
    public AudioClip attackSound;
    public AudioClip[] attackGruntSounds;

    [Header("Damage Sounds")]
    public AudioClip takeDamageSound;
    public AudioClip deathSound;
    public AudioClip stunSound;

    [Header("Movement Sounds")]
    public AudioClip alertSound;
    public AudioClip rushSound;

    [Header("Audio Source")]
    public AudioSource audioSource;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayAttack()
    {
        PlaySound(attackSound);
    }

    public void PlayAttackGrunt()
    {
        if (attackGruntSounds.Length > 0)
            PlayRandomSound(attackGruntSounds);
    }

    public void PlayTakeDamage()
    {
        PlaySound(takeDamageSound);
    }

    public void PlayDeath()
    {
        PlaySound(deathSound);
    }

    public void PlayStun()
    {
        PlaySound(stunSound);
    }

    public void PlayAlert()
    {
        PlaySound(alertSound);
    }

    public void PlayRush()
    {
        PlaySound(rushSound);
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