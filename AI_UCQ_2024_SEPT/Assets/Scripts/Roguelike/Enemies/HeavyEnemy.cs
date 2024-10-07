using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyEnemy : EnemyBase
{
    public float detectionRadius = 10.0f; // Radio de detecci�n del enemigo
    public float maxSpeed = 5.0f; // Velocidad m�xima del enemigo
    public float maxAcceleration = 10.0f; // Aceleraci�n m�xima
    private Vector3 velocity = Vector3.zero; // Velocidad actual del enemigo

    private GameObject player; // Referencia al jugador
    private Rigidbody rb; // Referencia al Rigidbody

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player"); // Buscar el jugador por su tag
        rb = GetComponent<Rigidbody>(); // Obtener el Rigidbody

        // Bloquear la rotaci�n en los ejes X e Z para que no se caiga
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (player == null) return; // Si no hay jugador, salir

        // Verificar si el jugador est� dentro del radio de detecci�n
        if (Vector3.Distance(transform.position, player.transform.position) <= detectionRadius)
        {
            // Aplicar el comportamiento de Seek para perseguir al jugador
            Seek();
        }
    }

    // M�todo que implementa el Seek para perseguir al jugador
    void Seek()
    {
        // Calcular la direcci�n desde el enemigo hacia el jugador usando PuntaMenosCola
        Vector3 directionToPlayer = PuntaMenosCola(player.transform.position, transform.position).normalized;

        // Mantener la direcci�n solo en el plano XZ (ignorar el eje Y)
        directionToPlayer.y = 0;

        // Calcular la nueva aceleraci�n hacia el jugador
        Vector3 desiredVelocity = directionToPlayer * maxSpeed;
        Vector3 steering = desiredVelocity - velocity;
        steering = Vector3.ClampMagnitude(steering, maxAcceleration);

        // Aplicar la aceleraci�n al Rigidbody solo en el plano XZ
        rb.AddForce(new Vector3(steering.x, 0, steering.z), ForceMode.Acceleration);

        // Limitar la velocidad m�xima solo en el plano XZ
        velocity = rb.velocity;
        velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z); // Mantener la velocidad vertical (gravedad)
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        rb.velocity = velocity;
    }

    // M�todo que calcula Punta menos Cola
    public Vector3 PuntaMenosCola(Vector3 Punta, Vector3 Cola)
    {
        return new Vector3(Punta.x - Cola.x, 
                           Punta.y - Cola.y, 
                           Punta.z - Cola.z);
    }

    // Dibujar Gizmos para visualizar el rango de detecci�n
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Visualizar la l�nea hacia el jugador si est� en el rango de detecci�n
        if (player != null && Vector3.Distance(transform.position, player.transform.position) <= detectionRadius)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.transform.position);
        }
    }
}
