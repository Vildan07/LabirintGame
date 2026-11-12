
using System.Collections.Generic;
using UnityEngine;

public class PortalTeleport : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private Transform exitPoint;

    [Header("Teleport Settings")]
    [SerializeField] private float teleportCooldown = 1f;

    [Header("Effects (Optional)")]
    [SerializeField] private AudioSource teleportSound;
    [SerializeField] private ParticleSystem teleportEffect;

    [Header("Camera")]
    [SerializeField] private CameraFollow cameraFollow;

    // Shared cooldown for all portals.
    private static readonly Dictionary<int, float>
        nextTeleportTimes = new Dictionary<int, float>();

    private void OnTriggerEnter(Collider other)
    {
        CharacterController controller =
            other.GetComponentInParent<CharacterController>();

        if (controller == null)
            return;

        // Only the player can use the portal.
        if (!controller.CompareTag("Player"))
            return;

        if (exitPoint == null)
        {
            Debug.LogWarning(
                "PortalTeleport: Exit Point is not assigned!",
                this
            );
            return;
        }

        int playerId = controller.gameObject.GetInstanceID();

        // Prevent immediate teleportation back.
        if (nextTeleportTimes.TryGetValue(
                playerId, out float nextAllowedTime))
        {
            if (Time.time < nextAllowedTime)
                return;
        }

        nextTeleportTimes[playerId] =
            Time.time + Mathf.Max(0.2f, teleportCooldown);

        TeleportPlayer(controller);
    }


    private void TeleportPlayer(CharacterController controller)
    {
        Transform player = controller.transform;

        if (teleportSound != null)
            teleportSound.Play();

        if (teleportEffect != null)
            teleportEffect.Play();

        // Disable CharacterController before teleporting.
        controller.enabled = false;

        // Move player to the destination portal.
        player.SetPositionAndRotation(
            exitPoint.position,
            exitPoint.rotation
        );

        // Enable CharacterController again.
        controller.enabled = true;

        // Synchronize rotation with PlayerMovement.
        PlayerMovement movement =
            controller.GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.SyncRotationAfterTeleport();
        }

        // Instantly move camera behind the player.
        if (cameraFollow != null)
        {
            cameraFollow.SnapToPlayer();
        }

        Debug.Log(
            "Player teleported from " +
            gameObject.name +
            " to " +
            exitPoint.name
        );
    }

}
