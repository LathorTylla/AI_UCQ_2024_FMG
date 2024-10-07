using UnityEngine;

public class DestructibleObstacle : MonoBehaviour
{
    public int maxHP = 50; // Vida máxima del obstáculo
    private int currentHP;

    void Start()
    {
        currentHP = maxHP;
    }

    // Método para recibir daño
    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        // Verificar si el obstáculo ha sido destruido
        if (currentHP <= 0)
        {
            DestroyObstacle();
        }
    }

    // Destruir el obstáculo
    void DestroyObstacle()
    {
        // Aquí puedes añadir efectos visuales, partículas, etc.
        Destroy(gameObject); // Destruir el objeto de la escena
    }

    // Detectar colisiones con balas
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            // Si una bala colisiona, aplicar daño
            BulletController bullet = other.GetComponent<BulletController>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
                bullet.gameObject.SetActive(false); // Desactivar la bala
            }
        }
    }
}
