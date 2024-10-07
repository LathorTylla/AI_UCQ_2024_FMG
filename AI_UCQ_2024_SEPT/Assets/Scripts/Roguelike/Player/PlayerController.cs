using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Variables existentes
    public float speed = 5.0f; // Velocidad de movimiento
    public float rotationSpeed = 10.0f; // Velocidad de rotación
    private float horizontalMove; // Movimiento horizontal
    private float verticalMove; // Movimiento vertical
    private CharacterController player;

    // Variables para el disparo
    public GameObject bulletPrefab; // Prefab de la bala
    public Transform bulletSpawnPoint; // Punto desde donde se disparará la bala
    public int poolBulletSize = 1; // Tamaño del pool de balas
    private List<GameObject> bulletPool; // Pool de balas

    // Variables de vida
    public int maxHP = 100; // Vida máxima del jugador
    private int currentHP; // Vida actual del jugador

    // Variables de estado
    private bool isDead = false; // ¿Está muerto el jugador?

    // Variables de invulnerabilidad
    public float invincibilityDuration = 2f; // Duración de la invulnerabilidad en segundos
    private bool isInvincible = false; // ¿Está el jugador actualmente invencible?
    private float invincibilityTimer; // Temporizador para la invulnerabilidad

    // Variables para el efecto de parpadeo
    public float blinkInterval = 0.1f; // Intervalo de parpadeo en segundos
    private Renderer playerRenderer; // Referencia al Renderer para controlar la visibilidad
    private Color originalColor; // Color original del material

    void Start()
    {
        player = GetComponent<CharacterController>();

        // Inicializar el pool de balas
        bulletPool = new List<GameObject>();
        for (int i = 0; i < poolBulletSize; i++)
        {
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false);
            bulletPool.Add(obj);
        }

        // Inicializar HP
        currentHP = maxHP;

        // Obtener el Renderer del jugador para el efecto de parpadeo
        playerRenderer = GetComponentInChildren<Renderer>();
        if (playerRenderer != null)
        {
            originalColor = playerRenderer.material.color;
        }
    }

    void Update()
    {
        // Verificar si el jugador está muerto
        if (isDead)
        {
            return; // Si está muerto, no realizar ninguna acción
        }

        // Movimiento del jugador
        horizontalMove = Input.GetAxis("Horizontal");
        verticalMove = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalMove, 0, verticalMove);

        if (moveDirection.magnitude > 1)
        {
            moveDirection.Normalize();
        }

        player.Move(moveDirection * speed * Time.deltaTime);

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Disparar al presionar la tecla (por ejemplo, tecla "Space")
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }

        // Manejar el temporizador de invulnerabilidad
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                // Asegurarse de que el jugador esté visible al terminar la invulnerabilidad
                SetPlayerVisibility(true);
            }
        }
    }

    void Shoot()
    {
        // Obtener una bala del pool
        GameObject bullet = GetPooledBullet();
        if (bullet != null)
        {
            bullet.transform.position = bulletSpawnPoint.position;
            bullet.transform.rotation = transform.rotation;
            bullet.SetActive(true);
        }
    }

    GameObject GetPooledBullet()
    {
        for (int i = 0; i < bulletPool.Count; i++)
        {
            if (!bulletPool[i].activeInHierarchy)
            {
                return bulletPool[i];
            }
        }
        return null;
    }

    // Método para recibir daño
    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible)
            return;

        currentHP -= damage;
        Debug.Log("Daño recibido.");
        // Iniciar la invulnerabilidad
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        // Iniciar el efecto de parpadeo
        StartCoroutine(Blink());

        // Verificar si la vida llegó a 0 o menos
        if (currentHP <= 0)
        {
            Die();
        }
    }

    // Método que se llama cuando el jugador muere
    void Die()
    {
        isDead = true;

        Debug.Log("El jugador ha muerto.");
        gameObject.SetActive(false); // Desactivar el jugador como ejemplo
    }

    // Corrutina para el efecto de parpadeo
    IEnumerator Blink()
    {
        while (isInvincible)
        {
            // Alternar la visibilidad del jugador
            SetPlayerVisibility(false);
            yield return new WaitForSeconds(blinkInterval);
            SetPlayerVisibility(true);
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    // Método para establecer la visibilidad del jugador
    void SetPlayerVisibility(bool isVisible)
    {
        if (playerRenderer != null)
        {
            playerRenderer.enabled = isVisible;
        }
    }
}
