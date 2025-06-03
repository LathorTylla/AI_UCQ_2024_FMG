using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshEscapistEnemy : EnemyBase
{
    public float fleeDuration = 3.0f;
    public float restDuration = 2.0f;
    public float shootingInterval = 2.0f;
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float detectionRadius = 10.0f;
    public float fleeDistance = 5.0f;
    public float lineOfSightCheckInterval = 0.5f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;
    public float visionDuration = 5.0f; // Tiempo para perder visión del jugador
    private NavMeshAgent agent;
    private GameObject player;
    private bool isTired = false;
    private bool canShoot = true;
    private Renderer enemyRenderer;
    private Color originalColor;
    private float timeWithoutVision;
    private bool hasLineOfSight;

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        agent.acceleration = 20.0f;
        agent.speed = 3.0f;

        enemyRenderer = GetComponent<Renderer>();
        originalColor = enemyRenderer.material.color;

        StartCoroutine(FleeRoutine());
        StartCoroutine(ShootAtPlayer());
        StartCoroutine(CheckLineOfSight());
    }

    IEnumerator FleeRoutine()
    {
        while (true)
        {
            if (!isTired)
            {
                StartFleeing();
                yield return new WaitForSeconds(fleeDuration);
                EnterTiredState();
            }
            else
            {
                yield return new WaitForSeconds(restDuration);
                ExitTiredState();
            }
        }
    }

    private void StartFleeing()
    {
        if (player != null)
        {
            Vector3 directionAwayFromPlayer = (transform.position - player.transform.position).normalized;
            Vector3 fleeTarget = transform.position + directionAwayFromPlayer * fleeDistance;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(fleeTarget, out hit, fleeDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                Vector3 alternateTarget = transform.position + (player.transform.position - transform.position).normalized * fleeDistance;
                if (NavMesh.SamplePosition(alternateTarget, out hit, fleeDistance, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }

            Debug.DrawLine(transform.position, agent.destination, Color.magenta, 2.0f);
        }
    }

    private void EnterTiredState()
    {
        isTired = true;
        agent.ResetPath();
        enemyRenderer.material.color = Color.blue;
        canShoot = false;
        Invoke(nameof(EnableShootingWhileTired), shootingInterval * 2);
    }

    private void ExitTiredState()
    {
        isTired = false;
        enemyRenderer.material.color = originalColor;
    }

    IEnumerator ShootAtPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootingInterval);
            if (player != null && IsPlayerInRange() && canShoot)
            {
                FireAtPlayer();
            }
        }
    }

    private void FireAtPlayer()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Vector3 directionToPlayer = (player.transform.position - bulletSpawnPoint.position).normalized;
        bullet.GetComponent<EnemyBullet>().Fire(directionToPlayer);
    }

    private void EnableShootingWhileTired()
    {
        canShoot = true;
    }

    bool IsPlayerInRange()
    {
        return Vector3.Distance(transform.position, player.transform.position) <= detectionRadius;
    }

    IEnumerator CheckLineOfSight()
    {
        while (true)
        {
            yield return new WaitForSeconds(lineOfSightCheckInterval);

            if (player == null)
                yield break;

            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            RaycastHit hit;

            if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRadius, obstacleLayer))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    hasLineOfSight = true;
                    timeWithoutVision = 0.0f;
                    if (!isTired)
                    {
                        agent.ResetPath();
                    }
                }
                else
                {
                    hasLineOfSight = false;
                    timeWithoutVision += lineOfSightCheckInterval;
                    if (timeWithoutVision >= visionDuration && !isTired)
                    {
                        StartFleeing();
                    }
                }
            }
            Debug.DrawRay(transform.position, directionToPlayer * detectionRadius, hasLineOfSight ? Color.green : Color.red);
        }
    }

    void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.transform.position);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
