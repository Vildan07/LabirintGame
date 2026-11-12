using UnityEngine;

public class PlayerCollect : MonoBehaviour
{
    [Header("UI Manager")]
    [SerializeField] private UIManager uiManager;

    [Header("Collectible Tags")]
    [SerializeField] private string normalCollectibleTag = "Collectible";
    [SerializeField] private string specialCollectibleTag = "SpecialCollectible";

    [Header("Score Settings")]
    [SerializeField] private int normalScore = 1;
    [SerializeField] private int specialScore = 5;

    private void Awake()
    {
        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager == null)
            Debug.LogError("PlayerCollect: UIManager not found!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (uiManager == null || !uiManager.IsGameplayActive()) return;

        if (other.CompareTag(normalCollectibleTag))
        {
            PlayPickupEffects(other, false);
            uiManager.AddCoins(1);
            uiManager.AddScore(normalScore);
            Debug.Log("Normal collectible collected! +" + normalScore + " Score");
            Debug.Log("Current Score: " + uiManager.GetScore());
            Destroy(other.gameObject);
        }
        else if (other.CompareTag(specialCollectibleTag))
        {
            PlayPickupEffects(other, true);
            uiManager.AddStars(1);
            uiManager.AddScore(specialScore);
            Debug.Log("Special collectible collected! +" + specialScore + " Score");
            Debug.Log("Current Score: " + uiManager.GetScore());
            Destroy(other.gameObject);
        }
    }

    private void PlayPickupEffects(Collider other, bool special)
    {
        PickupEffects effects = other.GetComponentInParent<PickupEffects>();
        if (effects != null) effects.Play();
        if (AudioManager.Instance == null) return;
        if (special) AudioManager.Instance.PlayStar();
        else AudioManager.Instance.PlayCoin();
    }

    public int GetScore()
    {
        return uiManager != null ? uiManager.GetScore() : 0;
    }
}
