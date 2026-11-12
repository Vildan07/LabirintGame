using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Health pickup")]
    [SerializeField] private string healthPickupTag = "Health";
    [SerializeField] private float healthPickupAmount = 25f;

    private float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        Debug.Log("Player Health: " + currentHealth + "%");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(healthPickupTag)) return;
        if (currentHealth <= 0f || currentHealth >= maxHealth) return;

        float previous = currentHealth;
        Heal(healthPickupAmount);
        if (currentHealth <= previous) return;

        PickupEffects effects = other.GetComponentInParent<PickupEffects>();
        if (effects != null) effects.Play();
        AudioManager.Instance?.PlayHealth();
        Destroy(other.gameObject);
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || currentHealth <= 0f) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
        Debug.Log("Player Damage: -" + damage);
        Debug.Log("Player Health: " + currentHealth + "%");

        if (currentHealth <= 0f) Die();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || currentHealth <= 0f) return;

        float previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        Debug.Log("Player Healed: +" + (currentHealth - previousHealth));
        Debug.Log("Player Health: " + currentHealth + "%");
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        // UIManager still detects zero HP and opens GameOver.
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
}
