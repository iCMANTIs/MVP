using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Patrol,
    Investigate,
    Search,
    Chase,
    Attack,
    FleeFromSound,
    AlertInvestigate
}

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;

    [Header("State")]
    public EnemyState currentState = EnemyState.Patrol;

    [Header("Random Patrol")]
    public Transform patrolCenter;
    public float patrolRadius = 8f;
    public float minPatrolDistance = 3f;
    public float waitTimeAtPoint = 1.5f;

    [Header("Search")]
    public float searchRadius = 5f;
    public float searchDuration = 6f;
    public float minSearchDistance = 1.5f;

    [Header("Memory")]
    public float memoryTime = 3f;

    private float memoryTimer;

    [Header("Chase")]
    public Transform currentTarget;
    public float attackDistance = 1.5f;
    [Header("Stun")]
    public bool isStunned;
    private float stunTimer;

    private float waitTimer;
    private bool waiting;

    [Header("Search Expansion")]
    public float startSearchRadius = 3f;
    public float maxSearchRadius = 9f;
    public float searchRadiusIncrease = 2f;
    public float searchExpandInterval = 3f;

    private float currentSearchRadius;
    private float searchExpandTimer;

    private Vector3 lastKnownPosition;
    private float searchTimer;

    [Header("Whistle")]
    public float lureStopDistance = 2.5f;
    public float fleeDistance = 8f;
    public float pushAwaySpeed = 1.5f;
    public float lureSpeed = 1.8f;
    public float alertSpeed = 4f;
    public float normalSpeed = 2f;
    public float alertSearchRadius = 3f;
    public float alertSearchDuration = 4f;

    private Vector3 fleeTargetPosition;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        GoToRandomPatrolPoint();
    }

    private void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0f)
            {
                RecoverFromStun();
            }

            return;
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                break;

            case EnemyState.Investigate:
                UpdateInvestigate();
                break;

            case EnemyState.Search:
                UpdateSearch();
                break;

            case EnemyState.Chase:
                UpdateChase();
                break;

            case EnemyState.Attack:
                UpdateAttack();
                break;

            case EnemyState.FleeFromSound:
                UpdateFleeFromSound();
                break;

            case EnemyState.AlertInvestigate:
                UpdateAlertInvestigate();
                break;
        }
    }

    private void UpdatePatrol()
    {
        if (waiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                waiting = false;
                GoToRandomPatrolPoint();
            }

            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            waiting = true;
            waitTimer = waitTimeAtPoint;
        }
    }

    private void UpdateInvestigate()
    {
        agent.SetDestination(lastKnownPosition);

        if (!agent.pathPending && agent.remainingDistance < lureStopDistance)
        {
            agent.speed = normalSpeed;
            EnterPatrol();
        }
    }

    private void UpdateSearch()
    {
        searchTimer -= Time.deltaTime;
        searchExpandTimer -= Time.deltaTime;

        if (searchExpandTimer <= 0f)
        {
            currentSearchRadius += searchRadiusIncrease;
            currentSearchRadius = Mathf.Min(currentSearchRadius, maxSearchRadius);

            searchExpandTimer = searchExpandInterval;

            Debug.Log("Search radius expanded to: " + currentSearchRadius);
        }

        if (!agent.pathPending && agent.remainingDistance < 0.7f)
        {
            GoToRandomSearchPoint();
        }

        if (searchTimer <= 0f)
        {
            EnterPatrol();
        }
    }

    private void UpdateChase()
    {
        if (currentTarget == null)
        {
            EnterSearch(lastKnownPosition);
            return;
        }

        lastKnownPosition = currentTarget.position;
        memoryTimer -= Time.deltaTime;

        agent.SetDestination(lastKnownPosition);

        float distance = Vector3.Distance(transform.position, currentTarget.position);

        if (distance <= attackDistance)
        {
            EnterAttack();
            return;
        }

        if (memoryTimer <= 0f)
        {
            currentTarget = null;
            EnterSearch(lastKnownPosition);
        }
    }

    private void UpdateAttack()
    {
        //agent.ResetPath();
        Debug.Log("Enemy attacks!");
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }

        if (currentTarget == null)
        {
            EnterSearch(lastKnownPosition);
            return;
        }

        float distanceToTarget = Vector3.Distance(
            transform.position,
            currentTarget.position
        );

        // distance judgment
        if (distanceToTarget > attackDistance)
        {
            currentState = EnemyState.Chase;
            return;
        }

        PlayerHealth playerHealth =
            currentTarget.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1);

            Debug.Log(
                $"{name} attacked {currentTarget.name}. " +
                $"Remaining health: {playerHealth.CurrentHealth}"
            );
        }
        else
        {
            Debug.LogWarning(
                $"No PlayerHealth found on {currentTarget.name}.",
                this
            );
        }

        // attack finish
        currentTarget = null;
        EnterSearch(lastKnownPosition);
    }

    public void SeePlayer(Transform player)
    {
        currentTarget = player;
        lastKnownPosition = player.position;

        memoryTimer = memoryTime;

        waiting = false;

        currentState = EnemyState.Chase;
    }

    public void LosePlayer()
    {
        if (currentState == EnemyState.Chase)
        {
            EnterSearch(lastKnownPosition);
        }
    }

    public void HearSound(Vector3 soundPosition)
    {
        if (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
            return;

        lastKnownPosition = soundPosition;
        currentState = EnemyState.Investigate;
        waiting = false;

        agent.SetDestination(lastKnownPosition);

        Debug.Log("Enemy heard sound: " + soundPosition);
    }

    private void EnterSearch(Vector3 position)
    {
        currentState = EnemyState.Search;
        searchTimer = searchDuration;

        currentSearchRadius = startSearchRadius;
        searchExpandTimer = searchExpandInterval;

        lastKnownPosition = position;
        currentTarget = null;
        waiting = false;

        GoToRandomSearchPoint();

        Debug.Log("Enemy searching around last known position.");
    }

    private void EnterPatrol()
    {
        currentState = EnemyState.Patrol;
        currentTarget = null;
        waiting = false;

        GoToRandomPatrolPoint();

        Debug.Log("Enemy returned to patrol.");
    }

    private void EnterAttack()
    {
        currentState = EnemyState.Attack;
    }

    public void HearWhistle(Vector3 soundPosition, WhistleType whistleType)
    {
        if (isStunned)
            return;

        if (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
            return;

        waiting = false;
        currentTarget = null;

        switch (whistleType)
        {
            case WhistleType.PushAway:
                EnterFleeFromSound(soundPosition);
                break;

            case WhistleType.Lure:
                EnterLure(soundPosition);
                break;

            case WhistleType.Alert:
                EnterAlertInvestigate(soundPosition);
                break;
        }
    }

    void EnterFleeFromSound(Vector3 soundPosition)
    {
        currentState = EnemyState.FleeFromSound;
        agent.speed = pushAwaySpeed;

        Vector3 awayDir = transform.position - soundPosition;
        awayDir.y = 0f;

        if (awayDir.sqrMagnitude < 0.001f)
            awayDir = -transform.forward;

        awayDir.Normalize();

        Vector3 target = transform.position + awayDir * fleeDistance;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
            fleeTargetPosition = hit.position;
        else
            fleeTargetPosition = target;

        agent.SetDestination(fleeTargetPosition);
    }

    void UpdateFleeFromSound()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.7f)
        {
            agent.speed = normalSpeed;
            EnterPatrol();
        }
    }

    void EnterLure(Vector3 soundPosition)
    {
        currentState = EnemyState.Investigate;
        agent.speed = lureSpeed;

        lastKnownPosition = soundPosition;
        agent.SetDestination(lastKnownPosition);
    }

    void EnterAlertInvestigate(Vector3 soundPosition)
    {
        currentState = EnemyState.AlertInvestigate;
        agent.speed = alertSpeed;

        lastKnownPosition = soundPosition;
        agent.SetDestination(lastKnownPosition);
    }

    void UpdateAlertInvestigate()
    {
        agent.SetDestination(lastKnownPosition);

        if (!agent.pathPending && agent.remainingDistance < 0.8f)
        {
            agent.speed = normalSpeed;
            EnterAlertSearch(lastKnownPosition);
        }
    }

    void EnterAlertSearch(Vector3 position)
    {
        currentState = EnemyState.Search;

        searchTimer = alertSearchDuration;
        currentSearchRadius = alertSearchRadius;
        searchExpandTimer = 999f;

        lastKnownPosition = position;
        currentTarget = null;
        waiting = false;

        GoToRandomSearchPoint();
    }


    private void GoToRandomPatrolPoint()
    {
        Vector3 center = patrolCenter != null ? patrolCenter.position : transform.position;

        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection += center;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                float distance = Vector3.Distance(transform.position, hit.position);

                if (distance >= minPatrolDistance)
                {
                    agent.SetDestination(hit.position);
                    Debug.DrawLine(transform.position, hit.position, Color.green, 2f);
                    return;
                }
            }
        }
    }

    private void GoToRandomSearchPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * currentSearchRadius;
            randomDirection += lastKnownPosition;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, currentSearchRadius, NavMesh.AllAreas))
            {
                float distance = Vector3.Distance(transform.position, hit.position);

                if (distance >= minSearchDistance)
                {
                    agent.SetDestination(hit.position);
                    Debug.DrawLine(transform.position, hit.position, Color.yellow, 2f);
                    return;
                }
            }
        }
    }

    public void Stun(float duration)
    {
        if (isStunned)
            return;

        isStunned = true;
        stunTimer = duration;

        agent.ResetPath();
        agent.isStopped = true;

        Debug.Log("Enemy stunned!");
    }

    private void RecoverFromStun()
    {
        isStunned = false;

        agent.isStopped = false;

        Debug.Log("Enemy recovered.");

        if (currentTarget != null)
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            EnterSearch(lastKnownPosition);
        }
    }

    public void ReceiveDirectorHint(Vector3 hintPosition)
    {
        if (isStunned)
            return;

        if (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
            return;

        lastKnownPosition = hintPosition;
        currentState = EnemyState.Investigate;
        waiting = false;

        agent.SetDestination(lastKnownPosition);

        Debug.Log("Director gave hint: " + hintPosition);
    }

    public void InvestigateVisualClue(Vector3 cluePosition)
    {
        if (isStunned)
            return;


        if (currentState == EnemyState.Chase ||
            currentState == EnemyState.Attack)
        {
            return;
        }

        currentTarget = null;
        lastKnownPosition = cluePosition;
        currentState = EnemyState.Investigate;
        waiting = false;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(lastKnownPosition);
        }

        Debug.Log(
            "Enemy saw Sister's flashlight and is investigating: "
            + cluePosition
        );
    }

    public void BackOff(Vector3 safePosition)
    {
        if (isStunned)
            return;

        if (currentState == EnemyState.Attack)
            return;

        currentTarget = null;
        currentState = EnemyState.Patrol;
        waiting = false;

        agent.SetDestination(safePosition);

        Debug.Log("Director told enemy to back off.");
    }
}