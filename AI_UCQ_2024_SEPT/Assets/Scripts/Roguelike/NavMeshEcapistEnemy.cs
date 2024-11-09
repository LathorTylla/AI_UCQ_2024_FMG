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
    public float rotationSpeed = 5.0f;

    private GameObject player;
    private NavMeshAgent agent;
    private bool isTired = false;
    private bool canShoot = true;
    private Renderer enemyRenderer;
    private Color originalColor;

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
        }
    }

    private void EnterTiredState()
    {
        isTired = true;
        agent.ResetPath();
        enemyRenderer.material.color = Color.blue; // Cambia color para indicar cansancio
        canShoot = false;
        Invoke(nameof(EnableShootingWhileTired), shootingInterval * 2); // Hace que el disparo sea más lento
    }

    private void ExitTiredState()
    {
        isTired = false;
        enemyRenderer.material.color = originalColor; // Regresa al color original
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
