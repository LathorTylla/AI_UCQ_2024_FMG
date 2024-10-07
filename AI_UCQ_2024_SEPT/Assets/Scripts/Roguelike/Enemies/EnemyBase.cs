using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public int maxHP = 100; // Vida m�xima del enemigo
    protected int currentHP; // Vida actual del enemigo
    public int damageToPlayer = 10; // Da�o que el enemigo inflige al jugador cuando lo toca

    // Inicializaci�n de la vida del enemigo
    protected virtual void Start()
    {
        currentHP = maxHP;
    }

    // M�todo para recibir da�o
    public virtual void TakeDamage(int damage)
    {
        currentHP -= damage;

        // Verificar si la vida llega a 0 o menos
        if (currentHP <= 0)
        {
            Die();
        }
    }

    // M�todo para destruir al enemigo
    protected virtual void Die()
    {
        Debug.Log(gameObject.name + " ha muerto.");
        Destroy(gameObject); // Destruye el enemigo
    }

    // Detectar colisiones con balas usando "OnTriggerEnter"
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            BulletController bullet = other.GetComponent<BulletController>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage); // Aplicar da�o al enemigo
                bullet.gameObject.SetActive(false); // Desactivar la bala
            }
        }
    }

    // Detectar colisiones con el jugador
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Obtener el script del jugador para aplicar da�o
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damageToPlayer);
            }
        }
    }
}
