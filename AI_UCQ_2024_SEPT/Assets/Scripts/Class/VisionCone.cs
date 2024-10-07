using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionCone : MonoBehaviour
{
    public float visionRadius = 10f;  // Radio del cono de visi�n
    public float visionAngle = 60f;   // �ngulo del cono de visi�n
    public Color coneColorDetected = Color.red;  // Color cuando detecta
    public Color coneColorIdle = Color.green;    // Color cuando no detecta
    public float rotationSpeed = 30f; // Velocidad de rotaci�n del cono de visi�n
    public float detectionCooldown = 3f; // Tiempo que espera antes de volver a rotar despu�s de perder al jugador
    public float shootingInterval = 1f;  // Intervalo entre disparos
    public Transform bulletSpawnPoint; // Punto de donde se disparan las balas
    public GameObject bulletPrefab;    // Prefab de la bala
    public float maxBulletSpeed = 15f; // Velocidad m�xima de las balas

    private Transform player;          // Referencia al jugador
    private bool targetDetected = false; // Indica si el jugador ha sido detectado
    private bool isRotating = true;     // Indica si la torreta est� rotando
    private float currentAngle = 0f;    // �ngulo actual de la rotaci�n
    private Vector3 predictedPlayerPosition; // Posici�n futura del jugador
    private Rigidbody playerRb;        // Referencia al Rigidbody del jugador

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;  // Obtener al jugador por su tag
        playerRb = player.GetComponent<Rigidbody>();  // Obtener el Rigidbody del jugador
        StartCoroutine(RotateVisionCone());  // Iniciar la rotaci�n del cono de visi�n
    }

    void Update()
    {
        if (targetDetected)
        {
            // Si se detecta al jugador, deja de rotar y dispara hacia su posici�n futura predicha
            StopCoroutine(RotateVisionCone());
            predictedPlayerPosition = CalculateFuturePosition();
            ShootAtPredictedPosition();
        }
        else
        {
            // Si no detecta al jugador, sigue rotando su cono de visi�n
            StartCoroutine(RotateVisionCone());
        }

        DetectPlayerInCone();  // Detectar al jugador dentro del cono de visi�n
    }

    // Rotar el cono de visi�n en X grados cada Y segundos
    IEnumerator RotateVisionCone()
    {
        while (isRotating)
        {
            currentAngle += rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0, currentAngle, 0); // Rotar alrededor del eje Y
            yield return null; // Esperar el siguiente frame
        }
    }

    // Detectar si el jugador est� dentro del cono de visi�n
    void DetectPlayerInCone()
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, visionRadius);

        targetDetected = false;
        foreach (Collider targetCollider in targetsInViewRadius)
        {
            Transform detectedTarget = targetCollider.transform;
            Vector3 directionToTarget = (detectedTarget.position - transform.position).normalized;

            // Calculamos el �ngulo entre la direcci�n del objetivo y la direcci�n de la torreta
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
            float angleToTarget = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

            // Si el �ngulo est� dentro del campo de visi�n (menor o igual que el �ngulo de visi�n)
            if (angleToTarget <= visionAngle / 2 && detectedTarget.CompareTag("Player"))
            {
                player = detectedTarget;  // Guardar la referencia del jugador
                targetDetected = true;    // Marcar que el jugador ha sido detectado
                isRotating = false;       // Detener la rotaci�n
                StartCoroutine(WaitBeforeResumeRotation()); // Reanudar rotaci�n despu�s de X segundos si pierde al jugador
                return;
            }
        }

        player = null;  // Si no detecta ning�n objetivo
    }

    // Esperar un tiempo antes de volver a rotar si pierde al jugador
    IEnumerator WaitBeforeResumeRotation()
    {
        yield return new WaitForSeconds(detectionCooldown);
        isRotating = true;
        StartCoroutine(RotateVisionCone()); // Reiniciar la rotaci�n del cono de visi�n
    }

    // M�todo para disparar hacia la posici�n predicha del jugador
    void ShootAtPredictedPosition()
    {
        if (player != null)
        {
            // Calcular la direcci�n hacia la posici�n futura del jugador
            Vector3 directionToFuturePosition = (predictedPlayerPosition - bulletSpawnPoint.position).normalized;

            // Instanciar la bala y disparar
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
            bullet.GetComponent<Rigidbody>().velocity = directionToFuturePosition * maxBulletSpeed; // Ajusta la velocidad de la bala

            StartCoroutine(ShootingCooldown());  // Iniciar el cooldown de disparo
        }
    }

    // Coroutine para controlar el tiempo entre disparos
    IEnumerator ShootingCooldown()
    {
        yield return new WaitForSeconds(shootingInterval);
    }

    // M�todo que predice la posici�n futura del jugador
    Vector3 CalculateFuturePosition()
    {
        if (playerRb == null)
        {
            Debug.LogError("El jugador no tiene un Rigidbody asignado.");
            return player.transform.position; // Retorna la posici�n actual si no hay Rigidbody
        }

        // Obtener la velocidad del jugador
        Vector3 playerVelocity = playerRb.velocity;

        // Calcular el tiempo de predicci�n basado en la distancia y la velocidad m�xima de la bala
        float timePrediction = CalculatedPredictedPos(maxBulletSpeed, bulletSpawnPoint.position, player.transform.position);

        // Usar la funci�n PredictPos para calcular la posici�n futura del jugador
        return PredictPos(player.transform.position, playerVelocity, timePrediction);
    }

    // M�todo que predice la posici�n futura basada en la velocidad y el tiempo
    Vector3 PredictPos(Vector3 initialPos, Vector3 velocity, float timePrediction)
    {
        return initialPos + velocity * timePrediction; // Retorna la posici�n futura
    }

    // Funci�n que calcula el tiempo de predicci�n basado en la distancia entre el objeto y el objetivo
    float CalculatedPredictedPos(float maxSpeed, Vector3 initialPos, Vector3 targetPos)
    {
        float distance = Vector3.Distance(initialPos, targetPos); // Calcula la distancia entre las posiciones
        return distance / maxSpeed; // Calcula el tiempo de predicci�n basado en la velocidad m�xima
    }

    // Visualizaci�n del cono de visi�n con Gizmos
    void OnDrawGizmos()
    {
        if (player != null)
        {
            // Dibujar una l�nea roja entre el enemigo y el jugador
            Gizmos.color = targetDetected ? coneColorDetected : coneColorIdle;
            Gizmos.DrawLine(transform.position, player.position);

            // Dibujar una l�nea hacia la posici�n futura predicha del jugador
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, predictedPlayerPosition);
            Gizmos.DrawSphere(predictedPlayerPosition, 0.5f);
        }

        // Dibuja un c�rculo para mostrar el radio de detecci�n
        Gizmos.color = coneColorIdle;
        Gizmos.DrawWireSphere(transform.position, visionRadius);
    }
}