using UnityEngine;

// Attach to each collectible. This class does not handle triggers:
// existing player scripts call Play() when collection succeeds.
public class PickupEffects : MonoBehaviour
{
    [Header("VFX to spawn once at pickup")]
    [SerializeField] private GameObject pickupVfxPrefab;
    [SerializeField] private float effectLifetime = 3f;
    [SerializeField] private Vector3 spawnOffset;

    public void Play()
    {
        if (pickupVfxPrefab == null) return;

        GameObject effect = Instantiate(
            pickupVfxPrefab,
            transform.position + spawnOffset,
            Quaternion.identity
        );
        Destroy(effect, Mathf.Max(0.1f, effectLifetime));
    }
}
