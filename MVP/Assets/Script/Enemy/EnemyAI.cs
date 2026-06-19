using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Patrol,
    Investigate,
    Search,
    Chase,
    Attack
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

        if (!agent.pathPending && agent.remainingDistance < 0.8f)
        {
            EnterSearch(lastKnownPosition);
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
        if (currentTarget != null)
        {
            lastKnownPosition = currentTarget.position;

            memoryTimer -= Time.deltaTime;

            agent.SetDestination(lastKnownPosition);

            float targetdistance =Vector3.Distance(transform.position,
                                 currentTarget.position);

            if (targetdistance <= attackDistance)
            {
                EnterAttack();
            }

            if (memoryTimer <= 0)
            {
                currentTarget = null;

                EnterSearch(lastKnownPosition);
            }
        }
        else
        {
            EnterSearch(lastKnownPosition);
        }

        lastKnownPosition = currentTarget.position;
        agent.SetDestination(currentTarget.position);

        float distance = Vector3.Distance(transform.position, currentTarget.position);

        if (distance <= attackDistance)
        {
            EnterAttack();
        }
    }

    private void UpdateAttack()
    {
        agent.ResetPath();
        Debug.Log("Enemy attacks!");

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
}