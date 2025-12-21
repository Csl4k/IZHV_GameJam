using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource source;

    [Header("Clips")]
    public AudioClip swingClip;
    public AudioClip dealDamageClip;
    public AudioClip takeDamageClip;
    public AudioClip parrySuccessClip;
    public AudioClip rollClip;

    [Header("Variation")]
    [Range(0f, 0.15f)] public float pitchJitter = 0.05f;

    void Awake()
    {
        if (!source) source = GetComponent<AudioSource>();
    }

    void Play(AudioClip clip, float volume = 1f)
    {
        if (!clip || !source) return;

        float oldPitch = source.pitch;
        source.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        source.PlayOneShot(clip, volume);
        source.pitch = oldPitch;
    }

    public void PlaySwing() => Play(swingClip);
    public void PlayDealDamage() => Play(dealDamageClip);
    public void PlayTakeDamage() => Play(takeDamageClip);
    public void PlayParrySuccess() => Play(parrySuccessClip);
    public void PlayRoll() => Play(rollClip);
}
