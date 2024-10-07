using UnityEngine;

public class DestructibleObstacle : MonoBehaviour
{
    public int maxHP = 50; // Vida m�xima del obst�culo
    private int currentHP;

    void Start()
    {
        currentHP = maxHP;
    }

    // M�todo para recibir da�o
    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        // Verificar si el obst�culo ha sido destruido
        if (currentHP <= 0)
        {
            DestroyObstacle();
        }
    }

    // Destruir el obst�culo
    void DestroyObstacle()
    {
        // Aqu� puedes a�adir efectos visuales, part�culas, etc.
        Destroy(gameObject); // Destruir el objeto de la escena
    }

    // Detectar colisiones con balas
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            // Si una bala colisiona, aplicar da�o
            BulletController bullet = other.GetComponent<BulletController>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
                bullet.gameObject.SetActive(false); // Desactivar la bala
            }
        }
    }
}
