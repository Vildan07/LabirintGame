using UnityEngine;
using System.Collections;

public class PlayerPowerPallet : MonoBehaviour
{
    [Header("Power Settings")]
    [SerializeField] private float powerDuration = 15f;

    [Header("Persistent player VFX (child object)")]
    [SerializeField] private GameObject activePowerVfx;

    private bool isPoweredUp;
    private float powerEndTime;
    private Coroutine powerCoroutine;

    private void Awake()
    {
        if (activePowerVfx != null) activePowerVfx.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PowerPallet")) return;

        PickupEffects effects = other.GetComponentInParent<PickupEffects>();
        if (effects != null) effects.Play();
        AudioManager.Instance?.PlayPower();
        ActivatePower();
        Destroy(other.gameObject);
    }

    private void ActivatePower()
    {
        if (powerCoroutine != null) StopCoroutine(powerCoroutine);
        powerCoroutine = StartCoroutine(PowerMode());
    }

    private IEnumerator PowerMode()
    {
        isPoweredUp = true;
        powerEndTime = Time.time + powerDuration;
        if (activePowerVfx != null) activePowerVfx.SetActive(true);
        Debug.Log("POWER MODE ON!");

        yield return new WaitForSeconds(powerDuration);

        isPoweredUp = false;
        powerEndTime = 0f;
        if (activePowerVfx != null) activePowerVfx.SetActive(false);
        Debug.Log("POWER MODE OFF!");
        powerCoroutine = null;
    }

    private void OnDisable()
    {
        if (powerCoroutine != null)
        {
            StopCoroutine(powerCoroutine);
            powerCoroutine = null;
        }
        isPoweredUp = false;
        powerEndTime = 0f;
        if (activePowerVfx != null) activePowerVfx.SetActive(false);
    }

    public bool IsPoweredUp() => isPoweredUp;
    public float GetRemainingPowerTime() =>
        isPoweredUp ? Mathf.Max(0f, powerEndTime - Time.time) : 0f;
    public float GetPowerDuration() => powerDuration;
}
