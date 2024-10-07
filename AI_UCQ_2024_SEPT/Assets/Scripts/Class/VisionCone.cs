using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionCone : MonoBehaviour
{
    public float visionRadius = 10f;  // Radio del cono de visión
    public float visionAngle = 60f;   // Ángulo del cono de visión
    public Color coneColorDetected = Color.red;  // Color cuando detecta
    public Color coneColorIdle = Color.green;    // Color cuando no detecta
    public float rotationSpeed = 30f; // Velocidad de rotación del cono de visión
    public float detectionCooldown = 3f; // Tiempo que espera antes de volver a rotar después de perder al jugador
    public float shootingInterval = 1f;  // Intervalo entre disparos
    public Transform bulletSpawnPoint; // Punto de donde se disparan las balas
    public GameObject bulletPrefab;    // Prefab de la bala
    public float maxBulletSpeed = 15f; // Velocidad máxima de las balas

    private Transform player;          // Referencia al jugador
    private bool targetDetected = false; // Indica si el jugador ha sido detectado
    private bool isRotating = true;     // Indica si la torreta está rotando
    private float currentAngle = 0f;    // Ángulo actual de la rotación
    private Vector3 predictedPlayerPosition; // Posición futura del jugador
    private Rigidbody playerRb;        // Referencia al Rigidbody del jugador

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;  // Obtener al jugador por su tag
        playerRb = player.GetComponent<Rigidbody>();  // Obtener el Rigidbody del jugador
        StartCoroutine(RotateVisionCone());  // Iniciar la rotación del cono de visión
    }

    void Update()
    {
        if (targetDetected)
        {
            // Si se detecta al jugador, deja de rotar y dispara hacia su posición futura predicha
            StopCoroutine(RotateVisionCone());
            predictedPlayerPosition = CalculateFuturePosition();
            ShootAtPredictedPosition();
        }
        else
        {
            // Si no detecta al jugador, sigue rotando su cono de visión
            StartCoroutine(RotateVisionCone());
        }

        DetectPlayerInCone();  // Detectar al jugador dentro del cono de visión
    }

    // Rotar el cono de visión en X grados cada Y segundos
    IEnumerator RotateVisionCone()
    {
        while (isRotating)
        {
            currentAngle += rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0, currentAngle, 0); // Rotar alrededor del eje Y
            yield return null; // Esperar el siguiente frame
        }
    }

    // Detectar si el jugador está dentro del cono de visión
    void DetectPlayerInCone()
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, visionRadius);

        targetDetected = false;
        foreach (Collider targetCollider in targetsInViewRadius)
        {
            Transform detectedTarget = targetCollider.transform;
            Vector3 directionToTarget = (detectedTarget.position - transform.position).normalized;

            // Calculamos el ángulo entre la dirección del objetivo y la dirección de la torreta
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
            float angleToTarget = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

            // Si el ángulo está dentro del campo de visión (menor o igual que el ángulo de visión)
            if (angleToTarget <= visionAngle / 2 && detectedTarget.CompareTag("Player"))
            {
                player = detectedTarget;  // Guardar la referencia del jugador
                targetDetected = true;    // Marcar que el jugador ha sido detectado
                isRotating = false;       // Detener la rotación
                StartCoroutine(WaitBeforeResumeRotation()); // Reanudar rotación después de X segundos si pierde al jugador
                return;
            }
        }

        player = null;  // Si no detecta ningún objetivo
    }

    // Esperar un tiempo antes de volver a rotar si pierde al jugador
    IEnumerator WaitBeforeResumeRotation()
    {
        yield return new WaitForSeconds(detectionCooldown);
        isRotating = true;
        StartCoroutine(RotateVisionCone()); // Reiniciar la rotación del cono de visión
    }

    // Método para disparar hacia la posición predicha del jugador
    void ShootAtPredictedPosition()
    {
        if (player != null)
        {
            // Calcular la dirección hacia la posición futura del jugador
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

    // Método que predice la posición futura del jugador
    Vector3 CalculateFuturePosition()
    {
        if (playerRb == null)
        {
            Debug.LogError("El jugador no tiene un Rigidbody asignado.");
            return player.transform.position; // Retorna la posición actual si no hay Rigidbody
        }

        // Obtener la velocidad del jugador
        Vector3 playerVelocity = playerRb.velocity;

        // Calcular el tiempo de predicción basado en la distancia y la velocidad máxima de la bala
        float timePrediction = CalculatedPredictedPos(maxBulletSpeed, bulletSpawnPoint.position, player.transform.position);

        // Usar la función PredictPos para calcular la posición futura del jugador
        return PredictPos(player.transform.position, playerVelocity, timePrediction);
    }

    // Método que predice la posición futura basada en la velocidad y el tiempo
    Vector3 PredictPos(Vector3 initialPos, Vector3 velocity, float timePrediction)
    {
        return initialPos + velocity * timePrediction; // Retorna la posición futura
    }

    // Función que calcula el tiempo de predicción basado en la distancia entre el objeto y el objetivo
    float CalculatedPredictedPos(float maxSpeed, Vector3 initialPos, Vector3 targetPos)
    {
        float distance = Vector3.Distance(initialPos, targetPos); // Calcula la distancia entre las posiciones
        return distance / maxSpeed; // Calcula el tiempo de predicción basado en la velocidad máxima
    }

    // Visualización del cono de visión con Gizmos
    void OnDrawGizmos()
    {
        if (player != null)
        {
            // Dibujar una línea roja entre el enemigo y el jugador
            Gizmos.color = targetDetected ? coneColorDetected : coneColorIdle;
            Gizmos.DrawLine(transform.position, player.position);

            // Dibujar una línea hacia la posición futura predicha del jugador
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, predictedPlayerPosition);
            Gizmos.DrawSphere(predictedPlayerPosition, 0.5f);
        }

        // Dibuja un círculo para mostrar el radio de detección
        Gizmos.color = coneColorIdle;
        Gizmos.DrawWireSphere(transform.position, visionRadius);
    }
}