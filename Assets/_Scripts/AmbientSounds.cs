using UnityEngine;

/// <summary>
/// Ambient sound manager for background music and environment sounds
/// </summary>
public class AmbientSounds : MonoBehaviour
{
    [Header("Music")]
    public AudioClip mainMenuMusic;
    public AudioClip hubMusic;
    public AudioClip combatMusic;
    public AudioClip bossMusic;
    public AudioClip victoryMusic;

    [Header("Ambient")]
    public AudioClip dungeonAmbience;
    public AudioClip windAmbience;
    public AudioClip fireAmbience;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource ambienceSource;

    [Header("Settings")]
    public float musicVolume = 0.7f;
    public float ambienceVolume = 0.3f;
    public float crossfadeDuration = 1f;

    private static AmbientSounds instance;
    public static AmbientSounds Instance => instance;

    void Awake()
    {
        // Singleton pattern to persist across scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetupAudioSources();
    }

    void SetupAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        if (ambienceSource == null)
        {
            ambienceSource = gameObject.AddComponent<AudioSource>();
        }
        ambienceSource.loop = true;
        ambienceSource.volume = ambienceVolume;
    }

    public void PlayMainMenuMusic()
    {
        CrossfadeMusic(mainMenuMusic);
    }

    public void PlayHubMusic()
    {
        CrossfadeMusic(hubMusic);
    }

    public void PlayCombatMusic()
    {
        CrossfadeMusic(combatMusic);
    }

    public void PlayBossMusic()
    {
        CrossfadeMusic(bossMusic);
    }

    public void PlayVictoryMusic()
    {
        CrossfadeMusic(victoryMusic);
    }

    public void PlayDungeonAmbience()
    {
        PlayAmbience(dungeonAmbience);
    }

    public void PlayWindAmbience()
    {
        PlayAmbience(windAmbience);
    }

    public void PlayFireAmbience()
    {
        PlayAmbience(fireAmbience);
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void StopAmbience()
    {
        if (ambienceSource != null)
            ambienceSource.Stop();
    }

    private void CrossfadeMusic(AudioClip newClip)
    {
        if (newClip == null || musicSource.clip == newClip) return;

        StopAllCoroutines();
        StartCoroutine(CrossfadeMusicCoroutine(newClip));
    }

    private System.Collections.IEnumerator CrossfadeMusicCoroutine(AudioClip newClip)
    {
        float elapsed = 0f;

        while (elapsed < crossfadeDuration / 2)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(musicVolume, 0f, elapsed / (crossfadeDuration / 2));
            yield return null;
        }

        musicSource.clip = newClip;
        musicSource.Play();

        elapsed = 0f;
        while (elapsed < crossfadeDuration / 2)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / (crossfadeDuration / 2));
            yield return null;
        }

        musicSource.volume = musicVolume;
    }

    private void PlayAmbience(AudioClip clip)
    {
        if (clip == null || ambienceSource.clip == clip) return;

        ambienceSource.clip = clip;
        ambienceSource.Play();
    }
}