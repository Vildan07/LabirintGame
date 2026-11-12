
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class AutoDoor : MonoBehaviour
{
    [Header("Door Animators")]
    [SerializeField] private Animator topAnimator;
    [SerializeField] private Animator bottomAnimator;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string nearbyParameter = "character_nearby";

    private readonly HashSet<Collider> playerColliders =
        new HashSet<Collider>();

    private void Awake()
    {
        BoxCollider trigger = GetComponent<BoxCollider>();
        trigger.isTrigger = true;

        if (topAnimator == null || bottomAnimator == null)
        {
            Debug.LogError(
                "AutoDoor: Assign both door animators!",
                this
            );
            enabled = false;
            return;
        }

        SetDoorOpen(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerColliders.Add(other);

        SetDoorOpen(true);

        Debug.Log("DOOR OPEN: " + other.name);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerColliders.Remove(other);

        if (playerColliders.Count == 0)
        {
            SetDoorOpen(false);

            Debug.Log("DOOR CLOSE: Player left");
        }
    }

    private bool IsPlayer(Collider other)
    {
        if (other.CompareTag(playerTag))
            return true;

        Transform root = other.transform.root;

        return root.CompareTag(playerTag);
    }

    private void SetDoorOpen(bool open)
    {
        if (topAnimator != null)
            topAnimator.SetBool(nearbyParameter, open);

        if (bottomAnimator != null)
            bottomAnimator.SetBool(nearbyParameter, open);
    }

    private void OnDisable()
    {
        playerColliders.Clear();
        SetDoorOpen(false);
    }
}
