using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorretaEnemigo : EnemyBase
{
    public float detectionAngle = 45f; // �ngulo del cono de visi�n
    public float detectionDistance = 10f; // Distancia del cono de visi�n
    public float rotationSpeed = 20f; // Velocidad a la que rota el cono de visi�n
    public float rotationInterval = 2.0f; // Tiempo entre rotaciones
    public float shootingInterval = 1.5f; // Intervalo de tiempo entre disparos
    public float detectionCooldown = 3.0f; // Tiempo para volver a rotar despu�s de perder al jugador
    public GameObject bulletPrefab; // Prefab de la bala
    public Transform bulletSpawnPoint; // Punto de disparo
    public Transform turretHead; // La parte que rota de la torreta
    public float bulletSpeed = 20f; // Velocidad de la bala
    public Color coneColorDetected = Color.red;  // Color del cono cuando detecta al jugador
    public Color coneColorIdle = Color.green;  // Color cuando no detecta al jugador
    private GameObject bullet; // Referencia a la bala reutilizable

    private GameObject player; // Referencia al jugador
    private bool isPlayerDetected = false; // Indica si el jugador ha sido detectado
    private bool isRotating = true; // Indica si la torreta est� rotando
    private Vector3 predictedPlayerPosition; // Posici�n futura del jugador

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player"); // Buscar al jugador por su tag

        // Crear la bala pero dejarla desactivada hasta que se dispare
        bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        bullet.SetActive(false); // Desactivamos la bala hasta que sea disparada

        StartCoroutine(RotateTurret()); // Iniciar la rotaci�n del cono de visi�n
    }

    // Coroutine para rotar el cono de visi�n
    IEnumerator RotateTurret()
    {
        while (true)
        {
            while (isRotating)
            {
                turretHead.Rotate(Vector3.up, rotationSpeed * Time.deltaTime); // Rotar la cabeza de la torreta
                DetectPlayer(); // Comprobar si el jugador est� en el cono de visi�n
                yield return null;
            }

            yield return new WaitForSeconds(rotationInterval); // Esperar antes de seguir rotando
        }
    }

    // M�todo para detectar si el jugador est� dentro del cono de visi�n
    void DetectPlayer()
    {
        if (player == null) return;

        // Calcular la direcci�n hacia el jugador
        Vector3 directionToPlayer = (player.transform.position - turretHead.position).normalized;
        float angleToPlayer = Vector3.Angle(turretHead.forward, directionToPlayer);

        // Comprobar si el jugador est� dentro del �ngulo y la distancia de detecci�n
        if (angleToPlayer < detectionAngle / 2 && Vector3.Distance(turretHead.position, player.transform.position) <= detectionDistance)
        {
            // El jugador ha sido detectado
            isPlayerDetected = true;
            isRotating = false; // Detener la rotaci�n
            StartCoroutine(ShootAtPlayer()); // Iniciar el disparo
        }
    }

    // Coroutine para disparar autom�ticamente hacia la posici�n del jugador
    IEnumerator ShootAtPlayer()
    {
        while (isPlayerDetected)
        {
            // Actualizar la posici�n del jugador en cada disparo
            if (IsPlayerInFieldOfView())
            {
                // Recalcular la posici�n del jugador antes de disparar
                FireAtPlayer(player.transform.position);
            }

            yield return new WaitForSeconds(shootingInterval); // Esperar entre disparos

            // Si el jugador se escapa del cono de visi�n, iniciar cooldown para volver a rotar
            if (!IsPlayerInFieldOfView())
            {
                isPlayerDetected = false;
                yield return new WaitForSeconds(detectionCooldown);
                isRotating = true; // Volver a rotar
            }
        }
    }

    // M�todo para disparar hacia la posici�n del jugador reutilizando la misma bala
    void FireAtPlayer(Vector3 playerPosition)
    {
        if (bullet != null && bulletSpawnPoint != null)
        {
            // Reactivar la bala y establecer su posici�n en el punto de disparo
            bullet.transform.position = bulletSpawnPoint.position;
            bullet.transform.rotation = Quaternion.identity;
            bullet.SetActive(true); // Reactivar la bala para su reutilizaci�n

            // Establecer la direcci�n y velocidad de la bala
            Vector3 directionToFire = (playerPosition - bulletSpawnPoint.position).normalized;
            bullet.GetComponent<Rigidbody>().velocity = directionToFire * bulletSpeed;
        }
    }

    // Verificar si el jugador sigue dentro del cono de visi�n
    bool IsPlayerInFieldOfView()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.transform.position - turretHead.position).normalized;
        float angleToPlayer = Vector3.Angle(turretHead.forward, directionToPlayer);

        // Verificar si el jugador a�n est� dentro del �ngulo y la distancia
        return angleToPlayer < detectionAngle / 2 && Vector3.Distance(turretHead.position, player.transform.position) <= detectionDistance;
    }

    // M�todo para calcular la posici�n futura del jugador (similar al enemigo Flee)
    Vector3 CalculateFuturePosition()
    {
        if (player == null) return Vector3.zero;

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb == null) return player.transform.position;

        Vector3 playerVelocity = playerRb.velocity;
        float predictionTime = Vector3.Distance(turretHead.position, player.transform.position) / bulletSpeed;
        return player.transform.position + playerVelocity * predictionTime;
    }

    // Dibujar el cono de visi�n con Gizmos y la predicci�n
    void OnDrawGizmos()
    {
        if (player != null)
        {
            // Dibujar una l�nea roja entre el enemigo y el jugador
            Gizmos.color = isPlayerDetected ? coneColorDetected : coneColorIdle;
            Gizmos.DrawLine(turretHead.position, player.transform.position);

            // Calcular y dibujar la posici�n predicha del jugador
            predictedPlayerPosition = CalculateFuturePosition();
            Gizmos.color = Color.green;
            Gizmos.DrawLine(turretHead.position, predictedPlayerPosition);
            Gizmos.DrawSphere(predictedPlayerPosition, 0.5f);
        }

        // Dibujar el cono de visi�n usando trigonometr�a 
        Gizmos.color = isPlayerDetected ? coneColorDetected : coneColorIdle;

        Vector3 forward = turretHead.forward * detectionDistance;
        float halfAngle = detectionAngle / 2.0f;
        Vector3 leftBoundary = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, halfAngle, 0) * forward;

        Gizmos.DrawRay(turretHead.position, leftBoundary);
        Gizmos.DrawRay(turretHead.position, rightBoundary);
        Gizmos.DrawWireSphere(turretHead.position, detectionDistance);
    }
}
