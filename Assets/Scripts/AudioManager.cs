using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources (2D)")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Pickup SFX")]
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private AudioClip starSound;
    [SerializeField] private AudioClip healthSound;
    [SerializeField] private AudioClip powerSound;

    [Header("UI SFX")]
    [SerializeField] private AudioClip buttonSound;

    [Header("Music for each panel")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip pauseMusic;
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private AudioClip gameOverMusic;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate AudioManager removed.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (musicSource == null || sfxSource == null)
        {
            Debug.LogError("AudioManager: assign Music Source and SFX Source in Inspector.");
            enabled = false;
            return;
        }

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.ignoreListenerPause = true;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.ignoreListenerPause = true;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void PlayCoin() => PlaySfx(coinSound);
    public void PlayStar() => PlaySfx(starSound);
    public void PlayHealth() => PlaySfx(healthSound);
    public void PlayPower() => PlaySfx(powerSound);
    public void PlayButton() => PlaySfx(buttonSound);

    public void PlaySfx(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayMainMenu() => PlayMusic(mainMenuMusic);
    public void PlayGameplay() => PlayMusic(gameplayMusic);
    public void PlayPause() => PlayMusic(pauseMusic);
    public void PlayVictory() => PlayMusic(victoryMusic);
    public void PlayGameOver() => PlayMusic(gameOverMusic);

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null) return;
        if (clip == null)
        {
            musicSource.Stop();
            musicSource.clip = null;
            return;
        }
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.Play();
    }
}
