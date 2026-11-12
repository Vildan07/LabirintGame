
using UnityEngine;

public class EnemyEffects : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource chaseSource;

    [Header("Enemy Sounds")]
    [SerializeField] private AudioClip alertSound;
    [SerializeField] private AudioClip chaseLoop;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip playerHitSound;
    [SerializeField] private AudioClip shieldHitSound;
    [SerializeField] private AudioClip enemyHitSound;
    [SerializeField] private AudioClip enemyDeathSound;

    [Header("VFX Prefabs")]
    [SerializeField] private GameObject alertVFX;
    [SerializeField] private GameObject playerHitVFX;
    [SerializeField] private GameObject shieldHitVFX;
    [SerializeField] private GameObject enemyHitVFX;
    [SerializeField] private GameObject enemyDeathVFX;

    [Header("VFX Settings")]
    [SerializeField] private float effectLifetime = 4f;
    [SerializeField] private float alertHeight = 1.5f;
    [SerializeField] private float hitHeight = 1f;

    private void Awake()
    {
        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        if (chaseSource != null)
        {
            chaseSource.loop = true;
            chaseSource.playOnAwake = false;
        }
    }

    public void PlayAlert()
    {
        PlaySound(alertSound);

        SpawnEffect(
            alertVFX,
            transform.position + Vector3.up * alertHeight
        );
    }

    public void StartChase()
    {
        if (chaseSource == null || chaseLoop == null)
            return;

        if (chaseSource.isPlaying)
            return;

        chaseSource.clip = chaseLoop;
        chaseSource.Play();
    }

    public void StopChase()
    {
        if (chaseSource != null)
            chaseSource.Stop();
    }

    public void PlayAttack()
    {
        PlaySound(attackSound);
    }

    public void PlayPlayerHit(Vector3 position)
    {
        PlaySound(playerHitSound);

        SpawnEffect(
            playerHitVFX,
            position + Vector3.up * hitHeight
        );
    }

    public void PlayShieldHit(Vector3 position)
    {
        PlaySound(shieldHitSound);

        SpawnEffect(
            shieldHitVFX,
            position + Vector3.up * hitHeight
        );
    }

    public void PlayHit()
    {
        PlaySound(enemyHitSound);

        SpawnEffect(
            enemyHitVFX,
            transform.position + Vector3.up * hitHeight
        );
    }

    public void PlayDeath()
    {
        StopChase();

        if (enemyDeathSound != null)
        {
            AudioSource.PlayClipAtPoint(
                enemyDeathSound,
                transform.position
            );
        }

        SpawnEffect(
            enemyDeathVFX,
            transform.position + Vector3.up * hitHeight
        );
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    private void SpawnEffect(GameObject prefab, Vector3 position)
    {
        if (prefab == null)
            return;

        GameObject instance = Instantiate(
            prefab,
            position,
            Quaternion.identity
        );

        Destroy(instance, Mathf.Max(0.1f, effectLifetime));
    }
}
