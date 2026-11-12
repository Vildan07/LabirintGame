
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Camera Position")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 2.5f;
    [SerializeField] private float lookHeight = 1.5f;

    [Header("Smoothing")]
    [SerializeField] private float followSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError(
                "CameraFollow: Player is not assigned!"
            );

            enabled = false;
            return;
        }

        SnapToPlayer();
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        FollowPlayer();
        LookAtPlayer();
    }

    private void FollowPlayer()
    {
        Vector3 desiredPosition =
            player.position
            - player.forward * distance
            + Vector3.up * height;

        float smooth =
            1f - Mathf.Exp(
                -followSpeed * Time.deltaTime
            );

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smooth
        );
    }

    private void LookAtPlayer()
    {
        Vector3 lookPoint =
            player.position + Vector3.up * lookHeight;

        Vector3 lookDirection =
            lookPoint - transform.position;

        if (lookDirection.sqrMagnitude < 0.001f)
            return;

        Quaternion desiredRotation =
            Quaternion.LookRotation(
                lookDirection,
                Vector3.up
            );

        float smooth =
            1f - Mathf.Exp(
                -rotationSpeed * Time.deltaTime
            );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            smooth
        );
    }

    // Instantly move the camera behind the player.
    // Used after teleportation.
    public void SnapToPlayer()
    {
        if (player == null)
            return;

        Vector3 desiredPosition =
            player.position
            - player.forward * distance
            + Vector3.up * height;

        transform.position = desiredPosition;

        Vector3 lookPoint =
            player.position + Vector3.up * lookHeight;

        Vector3 lookDirection =
            lookPoint - transform.position;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    lookDirection,
                    Vector3.up
                );
        }
    }
}
