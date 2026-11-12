
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerHealthPickup : MonoBehaviour
{
    [Header("Health Pickup Settings")]
    [SerializeField] private float healAmount = 25f;

    [SerializeField] private bool collectAtFullHealth = false;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object is a health pickup
        if (!other.CompareTag("Health"))
            return;

        // Prevent collecting if the player is dead
        if (playerHealth.GetCurrentHealth() <= 0f)
            return;

        // Check if health is already full
        if (!collectAtFullHealth &&
            playerHealth.GetCurrentHealth() >=
            playerHealth.GetMaxHealth())
        {
            Debug.Log("Health is already full!");
            return;
        }

        // Restore player's health
        playerHealth.Heal(healAmount);

        Debug.Log("Health pickup collected!");

        // Remove the collected item
        Destroy(other.gameObject);
    }
}
