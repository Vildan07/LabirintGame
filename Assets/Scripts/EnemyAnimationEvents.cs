
using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    [SerializeField]
    private EnemyAIMovement enemyAI;

    private void Awake()
    {
        if (enemyAI == null)
            enemyAI =
                GetComponentInParent<EnemyAIMovement>();
    }

    public void AttackImpact()
    {
        if (enemyAI != null)
            enemyAI.AttackImpact();
    }
}
