using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 20f; // Velocidad de la bala
    public float lifetime = 3f; // Tiempo antes de que la bala se desactive
    private float lifeTimer;

    public int damage = 10; // Daño que la bala inflige

    void OnEnable()
    {
        lifeTimer = lifetime;
    }

    void Update()
    {
        // Mover la bala hacia adelante SOLO en el plano XZ
        Vector3 forwardXZ = new Vector3(transform.forward.x, 0, transform.forward.z);
        transform.Translate(forwardXZ.normalized * speed * Time.deltaTime, Space.World);

        // Contador de vida útil
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            // Desactivar la bala en lugar de destruirla
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Verificar si colisiona con un enemigo
        if (other.CompareTag("Enemy"))
        {
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage); // Aplicar daño al enemigo
            }

            // Desactivar la bala
            gameObject.SetActive(false);
        }
        else if (other.CompareTag("Obstacle") || other.CompareTag("Wall"))
        {
            // Desactivar la bala al impactar con un obstáculo o pared
            gameObject.SetActive(false);
        }
    }
}
