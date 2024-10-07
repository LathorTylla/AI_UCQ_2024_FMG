using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 100f; // Velocidad de la bala
    private Vector3 direction; // Dirección de la bala
    
    public void Fire(Vector3 targetDirection)
    {
        direction = targetDirection.normalized; // Normalizar la dirección
        // Establecer la velocidad inicial
        GetComponent<Rigidbody>().velocity = direction * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si colisiona con el jugador
        if (other.CompareTag("Player"))
        {
            // Obtener el componente del jugador para aplicar daño
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(10); // Aplica daño al jugador
            }

            // Desactivar la bala
            gameObject.SetActive(false);
        }
        else if (other.CompareTag("Obstacle") || other.CompareTag("Wall"))
        {
            // Lógica al impactar un obstáculo o pared
            gameObject.SetActive(false);
        }
    }
}