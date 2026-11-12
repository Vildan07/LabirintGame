
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAIMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform player;
    [SerializeField] private Transform guardPoint;
    [SerializeField] private EnemyEffects effects;

    [Header("Guard")]
    [SerializeField] private float guardRadius = 5f;

    [Header("Chase")]
    [SerializeField] private float chaseTimeMin = 20f;
    [SerializeField] private float chaseTimeMax = 30f;

    [Header("Attack")]
    [SerializeField] private float attackDistance = 2f;
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float attackInterval = 1f;

    [Header("Animator Parameters")]
    [SerializeField] private string walkingParameter = "IsWalking";
    [SerializeField] private string attackingParameter = "IsAttacking";

    private bool isChasing;
    private bool isReturning;
    private bool isAttacking;

    private float chaseEndTime;
    private float nextAttackTime;

    private PlayerHealth playerHealth;
    private PlayerPowerPallet playerPower;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (effects == null)
            effects = GetComponent<EnemyEffects>();

        if (player != null)
            CachePlayerComponents();
    }

    private void Start()
    {
        if (guardPoint == null)
            guardPoint = transform;

        if (player == null)
        {
            Debug.LogWarning("Enemy: Player not assigned!");
            enabled = false;
            return;
        }

        CachePlayerComponents();

        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogError("Enemy is not on NavMesh!");
            enabled = false;
            return;
        }

        agent.isStopped = true;
        SetWalking(false);
        SetAttacking(false);
    }

    private void CachePlayerComponents()
    {
        playerHealth = player.GetComponent<PlayerHealth>();
        playerPower = player.GetComponent<PlayerPowerPallet>();
    }

    private void Update()
    {
        if (player == null || agent == null ||
            !agent.isOnNavMesh)
            return;

        // UIManager pauses the game using Time.timeScale = 0.
        if (Time.timeScale <= 0f)
            return;

        if (playerHealth != null &&
            playerHealth.GetCurrentHealth() <= 0f)
        {
            StopEnemy();
            return;
        }

        if (isChasing)
        {
            UpdateChase();
            return;
        }

        if (isReturning)
        {
            UpdateReturn();
            return;
        }

        UpdateGuard();
    }

    // GUARD

    private void UpdateGuard()
    {
        agent.isStopped = true;

        SetWalking(false);
        SetAttacking(false);

        float distance = Vector3.Distance(
            guardPoint.position,
            player.position
        );

        if (distance <= guardRadius)
            StartChase();
    }

    // START CHASE

    private void StartChase()
    {
        if (isChasing || isReturning)
            return;

        isChasing = true;
        isAttacking = false;

        agent.isStopped = false;

        float duration = Random.Range(
            Mathf.Min(chaseTimeMin, chaseTimeMax),
            Mathf.Max(chaseTimeMin, chaseTimeMax)
        );

        chaseEndTime = Time.time + duration;

        effects?.PlayAlert();
        effects?.StartChase();

        Debug.Log("Enemy CHASE: " + duration + " sec");
    }

    // CHASE

    private void UpdateChase()
    {
        if (Time.time >= chaseEndTime)
        {
            StartReturn();
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance <= attackDistance)
        {
            Attack();
            return;
        }

        if (isAttacking)
        {
            isAttacking = false;
            SetAttacking(false);
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);

        SetWalking(true);
    }

    // ATTACK


    private void Attack()
    {
        agent.isStopped = true;

        SetWalking(false);
        SetAttacking(true);

        if (!isAttacking)
        {
            isAttacking = true;
        }

        // Face the player
        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                10f * Time.deltaTime
            );
        }

        // Damage, SFX and VFX will be triggered
        // by the Animation Event instead.
    }


    // RETURN

    private void StartReturn()
    {
        isChasing = false;
        isReturning = true;
        isAttacking = false;

        effects?.StopChase();

        agent.isStopped = false;

        SetWalking(true);
        SetAttacking(false);

        agent.SetDestination(guardPoint.position);

        Debug.Log("Enemy returning home");
    }

    private void UpdateReturn()
    {
        agent.isStopped = false;
        agent.SetDestination(guardPoint.position);

        SetWalking(true);
        SetAttacking(false);

        if (!agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance + 0.2f)
        {
            FinishReturn();
        }
    }

    private void FinishReturn()
    {
        isReturning = false;
        isAttacking = false;

        agent.isStopped = true;

        SetWalking(false);
        SetAttacking(false);

        effects?.StopChase();

        Debug.Log("Enemy returned home");
    }

    private void StopEnemy()
    {
        isChasing = false;
        isReturning = false;
        isAttacking = false;

        agent.isStopped = true;

        SetWalking(false);
        SetAttacking(false);

        effects?.StopChase();
    }

    private void OnDisable()
    {
        effects?.StopChase();
    }

    // ANIMATION

    private void SetWalking(bool value)
    {
        if (animator != null)
            animator.SetBool(walkingParameter, value);
    }

    private void SetAttacking(bool value)
    {
        if (animator != null)
            animator.SetBool(attackingParameter, value);
    }

    // GIZMOS


    public void AttackImpact()
    {
        if (!isChasing || !isAttacking ||
            player == null || Time.timeScale <= 0f)
            return;

        if (playerHealth == null ||
            playerHealth.GetCurrentHealth() <= 0f)
            return;

        // Protect against duplicate animation events.
        if (Time.time < nextAttackTime)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // Do not hit a player who moved away.
        if (distance > attackDistance + 0.3f)
            return;

        nextAttackTime =
            Time.time + Mathf.Max(0.1f, attackInterval);

        // Swing / impact sound
        effects?.PlayAttack();

        if (playerPower != null &&
            playerPower.IsPoweredUp())
        {
            effects?.PlayShieldHit(player.position);
            return;
        }

        playerHealth.TakeDamage(attackDamage);
        effects?.PlayPlayerHit(player.position);
    }

    private void OnDrawGizmosSelected()
    {
        if (guardPoint == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            guardPoint.position, guardRadius);
    }
}
