using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Variables existentes
    public float speed = 5.0f; // Velocidad de movimiento
    public float rotationSpeed = 10.0f; // Velocidad de rotaci�n
    private float horizontalMove; // Movimiento horizontal
    private float verticalMove; // Movimiento vertical
    private CharacterController player;

    // Variables para el disparo
    public GameObject bulletPrefab; // Prefab de la bala
    public Transform bulletSpawnPoint; // Punto desde donde se disparar� la bala
    public int poolBulletSize = 1; // Tama�o del pool de balas
    private List<GameObject> bulletPool; // Pool de balas

    // Variables para la l�nea de punter�a
    public LineRenderer aimLine;  // Asignar en inspector el LineRenderer
    public float aimLineLength = 10f; // Longitud de la l�nea gu�a
    public Color defaultLineColor = Color.white; // Color por defecto de la l�nea
    public Color enemyLineColor = Color.red; // Color cuando detecta un enemigo
    public Color destructibleLineColor = Color.green; // Color cuando detecta un objeto destructible

    // Variables de vida
    public int maxHP = 100; // Vida m�xima del jugador
    private int currentHP; // Vida actual del jugador

    // Variables de estado
    private bool isDead = false; // �Est� muerto el jugador?

    // Variables de invulnerabilidad
    public float invincibilityDuration = 2f; // Duraci�n de la invulnerabilidad en segundos
    private bool isInvincible = false; // �Est� el jugador actualmente invencible?
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

        // Opcional: desactivar la l�nea al inicio (si quieres)
        if (aimLine != null)
        {
            aimLine.positionCount = 0;
        }
    }

    void Update()
    {
        // Verificar si el jugador est� muerto
        if (isDead)
        {
            return; // Si est� muerto, no realizar ninguna acci�n
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

        // Rotaci�n con mouse (apuntar hacia el cursor)
        RotateTowardsMouse();

        // Actualizar l�nea de punter�a con colisiones
        UpdateAimLine();

        // Disparar con "space"
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
                // Asegurarse de que el jugador est� visible al terminar la invulnerabilidad
                SetPlayerVisibility(true);
            }
        }
    }

    // Rota el jugador hacia la posici�n del mouse
    void RotateTowardsMouse()
    {
        // Comprobar si el cursor est� dentro de la pantalla antes de usar ScreenPointToRay
        if (Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width &&
            Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height)
        {
            // Crear un rayo desde la c�mara hacia el punto del mouse
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // Crear un plano horizontal para detectar el suelo
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            // Comprobar si el rayo intersecta con el plano del suelo
            float rayDistance;

            // Si el rayo intersecta con el plano del suelo, calcular la direcci�n de rotaci�n
            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 pointToLook = ray.GetPoint(rayDistance);
                Vector3 direction = pointToLook - transform.position;
                direction.y = 0;

                if (direction.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                }
            }
        }
    }

    // Actualiza la l�nea de punter�a y maneja las colisiones
    void UpdateAimLine()
    {
        if (aimLine == null)
            return;

        Vector3 start = bulletSpawnPoint.position;
        Vector3 direction = transform.forward;

        RaycastHit hit;
        if (Physics.Raycast(start, direction, out hit, aimLineLength))
        {
            // Si la l�nea toca algo, cambiar el color dependiendo del tipo de objeto
            aimLine.positionCount = 2;
            aimLine.SetPosition(0, start);
            aimLine.SetPosition(1, hit.point);

            // Verificar si es un enemigo o un objeto destructible
            if (hit.collider.GetComponent<DestructibleObstacle>() != null)
            {
                aimLine.material.color = destructibleLineColor; // Color para objetos destructibles
            }
            else if (hit.collider.CompareTag("Enemy")) // Asume que los enemigos tienen la etiqueta "Enemy"
            {
                aimLine.material.color = enemyLineColor; // Color para enemigos
            }
            else
            {
                aimLine.material.color = defaultLineColor; // Color por defecto
            }
        }
        else
        {
            // Si no toca nada, dibujar la l�nea normalmente
            aimLine.positionCount = 2;
            aimLine.SetPosition(0, start);
            aimLine.SetPosition(1, start + direction * aimLineLength);
            aimLine.material.color = defaultLineColor; // Color por defecto
        }
    }

    void Shoot()
    {
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

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible)
            return;

        currentHP -= damage;
        Debug.Log("Da�o recibido.");

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        StartCoroutine(Blink());

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        Debug.Log("El jugador ha muerto.");
        gameObject.SetActive(false);
    }

    IEnumerator Blink()
    {
        while (isInvincible)
        {
            SetPlayerVisibility(false);
            yield return new WaitForSeconds(blinkInterval);
            SetPlayerVisibility(true);
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    void SetPlayerVisibility(bool isVisible)
    {
        if (playerRenderer != null)
        {
            playerRenderer.enabled = isVisible;
        }
    }
}
