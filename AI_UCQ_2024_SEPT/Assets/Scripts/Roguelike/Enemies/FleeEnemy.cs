using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeEnemy : EnemyBase
{
    public float fleeDuration = 3.0f; // Tiempo de huida antes de "cansarse"
    public float restDuration = 2.0f; // Tiempo de descanso antes de repetir el ciclo
    public float maxSpeed = 3.0f; // Velocidad máxima del enemigo
    public float maxAcceleration = 10.0f; // Aceleración máxima del enemigo
    public float shootingInterval = 2.0f; // Intervalo entre disparos
    public GameObject bulletPrefab; // Prefab de la bala
    public Transform bulletSpawnPoint; // Punto desde donde se dispara la bala
    public float detectionRadius = 10.0f; // Radio de detección para disparar
    public float rotationSpeed = 5.0f; // Velocidad de rotación hacia el jugador

    private GameObject bullet; // Bala reutilizable
    private Rigidbody rb; // Referencia al Rigidbody
    private GameObject player; // Referencia al jugador
    private Vector3 velocity = Vector3.zero; // Velocidad actual
    private Vector3 predictedPlayerPosition; // Posición futura predicha del jugador

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player"); // Buscar al jugador por su tag
        rb = GetComponent<Rigidbody>(); // Obtener el Rigidbody

        // Configurar restricciones del Rigidbody para evitar que rote de manera incorrecta
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        // Crear la bala pero dejarla desactivada
        bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        bullet.SetActive(false);

        // Iniciar el ciclo de huida y descanso
        StartCoroutine(FleeRoutine());

        // Iniciar el ciclo de disparo
        StartCoroutine(ShootAtPlayer());
    }

    // Coroutine para manejar el ciclo de huida
    IEnumerator FleeRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(Flee()); // Iniciar el comportamiento de huida
            yield return new WaitForSeconds(restDuration); // Descansar durante Y segundos
        }
    }

    // Coroutine para el comportamiento de huida
    private IEnumerator Flee()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fleeDuration)
        {
            if (player != null)
            {
                Escape(); // Llama a la función de huida
                RotateTowardsPlayer(); // Llama a la función de rotación
            }
            elapsedTime += Time.deltaTime; // Incrementar el tiempo transcurrido
            yield return null; // Esperar el siguiente frame
        }
    }

    // Método para hacer "flee" del jugador
    void Escape()
    {
        // Calcular la dirección opuesta al jugador
        Vector3 directionAwayFromPlayer = (transform.position - player.transform.position).normalized;

        // Calcular la aceleración deseada
        Vector3 desiredVelocity = directionAwayFromPlayer * maxSpeed;
        Vector3 steering = desiredVelocity - velocity;
        steering = Vector3.ClampMagnitude(steering, maxAcceleration);

        // Aplicar la aceleración solo en el plano XZ
        rb.AddForce(new Vector3(steering.x, 0, steering.z), ForceMode.Acceleration);

        // Limitar la velocidad máxima solo en el plano XZ
        velocity = rb.velocity;
        velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z); // Mantener la velocidad vertical (gravedad)
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        rb.velocity = velocity;
    }

    // Método para rotar hacia el jugador
    void RotateTowardsPlayer()
    {
        // Calcular la dirección hacia el jugador
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;

        // Calcular la rotación deseada usando LookRotation
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z)); // Solo en XZ

        // Aplicar una rotación suave usando Slerp para que la rotación sea gradual
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    // Coroutine para disparar automáticamente hacia la posición futura predicha del jugador
    IEnumerator ShootAtPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootingInterval); // Esperar el intervalo de disparo
            if (player != null && IsPlayerInRange())
            {
                // Predecir la posición futura del jugador
                predictedPlayerPosition = CalculateFuturePosition();

                // Disparar hacia la posición predicha
                FireAtPredictedPosition();
            }
        }
    }

    // Método para disparar a la posición futura del jugador
    void FireAtPredictedPosition()
    {
        if (bullet.activeSelf) return; // Si la bala está activa, no disparar hasta que se desactive

        // Calcular la dirección hacia la posición futura del jugador
        Vector3 directionToFuturePosition = (predictedPlayerPosition - bulletSpawnPoint.position).normalized;

        // Reactivar y disparar la bala
        bullet.transform.position = bulletSpawnPoint.position;
        bullet.transform.rotation = Quaternion.identity; // Asegúrate de que la rotación sea correcta
        bullet.SetActive(true);

        // Establecer la dirección y velocidad de la bala
        bullet.GetComponent<EnemyBullet >().Fire(directionToFuturePosition); // Usar el nuevo método en el controlador de la bala
    }

    // Método para verificar si el jugador está dentro del rango de detección
    bool IsPlayerInRange()
    {
        return Vector3.Distance(transform.position, player.transform.position) <= detectionRadius;
    }

    // Método que predice la posición futura del jugador
    Vector3 CalculateFuturePosition()
    {
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb == null)
        {
            Debug.LogError("El jugador no tiene un Rigidbody asignado.");
            return player.transform.position; // Retorna la posición actual si no hay Rigidbody
        }

        // Obtener la velocidad del jugador
        Vector3 playerVelocity = playerRb.velocity;

        // Calcular el tiempo de predicción basado en la distancia y la velocidad máxima del enemigo
        float timePrediction = CalculatedPredictedPos(maxSpeed, transform.position, player.transform.position);

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
        float distance = PuntaMenosCola(targetPos, initialPos).magnitude; // Calcula la distancia entre las posiciones
        return distance / maxSpeed; // Calcula el tiempo de predicción basado en la velocidad máxima
    }

    // Método que calcula Punta menos Cola
    public Vector3 PuntaMenosCola(Vector3 Punta, Vector3 Cola)
    {
        return new Vector3(Punta.x - Cola.x, Punta.y - Cola.y, Punta.z - Cola.z);
    }

    // Dibujar Gizmos para visualizar el rango de detección y la posición futura del jugador
    void OnDrawGizmos()
    {
        if (player != null)
        {
            // Dibujar una línea roja entre el enemigo y el jugador
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.transform.position);

            // Dibujar una línea hacia la posición futura predicha del jugador
            if (predictedPlayerPosition != Vector3.zero)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, predictedPlayerPosition);
                Gizmos.DrawSphere(predictedPlayerPosition, 0.5f);
            }
        }

        // Dibuja un círculo para mostrar el radio de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
