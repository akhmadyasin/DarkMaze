using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float chaseRange = 10f;
    public float returnRangeMultiplier = 1.5f;
    public float raycastDistance = 0.6f;
    public float patrolRange = 3f;
    public float patrolWaitTime = 2f;
    public LayerMask obstacleLayer;
    public Transform target;

    public int damage = 20;
    public float knockbackForce = 5f; // force knockback musuh dan player

    public int maxHealth = 100;
    private int currentHealth;

    public float hitStopDuration = 0.3f; // durasi stun / berhenti

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Vector2 startPosition;
    private Vector2 patrolTarget;
    private float patrolTimer;

    private SpriteRenderer spriteRenderer;

    private bool isKnockedBack = false;

    private enum State { Idle, Chasing, Returning }
    private State currentState = State.Idle;

    void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        patrolTarget = startPosition;
    }

    void Start()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindWithTag("player");
            if (playerObj != null)
                target = playerObj.transform;
        }
    }

    void Update()
    {
        if (target == null) return;

        if (isKnockedBack)
        {
            // Saat knockback, skip AI gerak
            moveDirection = Vector2.zero;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        float distanceToStart = Vector2.Distance(transform.position, startPosition);
        float distanceToPatrol = Vector2.Distance(transform.position, patrolTarget);

        switch (currentState)
        {
            case State.Idle:
                if (distanceToPlayer <= chaseRange)
                {
                    currentState = State.Chasing;
                    Debug.Log("Enemy: Switching to CHASING");
                }
                else
                {
                    patrolTimer -= Time.deltaTime;
                    if (patrolTimer <= 0 || distanceToPatrol < 0.2f)
                    {
                        SetRandomPatrolTarget();
                        patrolTimer = patrolWaitTime;
                    }
                }
                break;

            case State.Chasing:
                if (distanceToPlayer > chaseRange * returnRangeMultiplier)
                {
                    currentState = State.Returning;
                    Debug.Log("Enemy: Player too far, RETURNING");
                }
                break;

            case State.Returning:
                if (distanceToStart <= 0.1f)
                {
                    Debug.Log("Enemy: Back to start position, now IDLE");
                    currentState = State.Idle;
                    patrolTimer = 0f;
                }
                else if (distanceToPlayer <= chaseRange)
                {
                    currentState = State.Chasing;
                    Debug.Log("Enemy: Player came close while returning, CHASING again");
                }
                break;
        }

        switch (currentState)
        {
            case State.Chasing:
                moveDirection = (target.position - transform.position).normalized;
                break;
            case State.Returning:
                moveDirection = (startPosition - (Vector2)transform.position).normalized;
                break;
            case State.Idle:
                moveDirection = (patrolTarget - (Vector2)transform.position).normalized;
                break;
        }

        if (moveDirection != Vector2.zero)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, raycastDistance, obstacleLayer);
            if (hit.collider != null)
            {
                moveDirection = Vector2.zero;
                Debug.Log("Enemy: Obstacle hit, stopping.");
            }

            Debug.DrawRay(transform.position, moveDirection * raycastDistance, Color.red);
        }

        if (spriteRenderer && moveDirection.x != 0)
            spriteRenderer.flipX = moveDirection.x > 0;
    }

    void FixedUpdate()
    {
        if (isKnockedBack)
        {
            // saat knockback, biarkan velocity yg sudah di-set tetap jalan
            return;
        }

        rb.velocity = moveDirection * moveSpeed;
    }

    void SetRandomPatrolTarget()
    {
        float randX = Random.Range(-patrolRange, patrolRange);
        float randY = Random.Range(-patrolRange, patrolRange);
        patrolTarget = startPosition + new Vector2(randX, randY);
        Debug.Log("Enemy: New patrol target set to " + patrolTarget);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerMovement player = collision.collider.GetComponent<PlayerMovement>();
        if (player != null)
        {
            Vector2 knockDir = (player.transform.position - transform.position).normalized;
            player.TakeDamage(damage, knockDir * knockbackForce);  // player kena knockback disini
        }
    }

    // Fungsi ini dipanggil saat enemy kena serangan
    public void TakeDamage(int amount, Transform source)
    {
        currentHealth -= amount;
        Debug.Log("Enemy took " + amount + " damage. Current HP: " + currentHealth);

        // Hit musuh: apply knockback dan stun singkat
        Vector2 knockbackDir = (transform.position - source.position).normalized;
        StartCoroutine(KnockbackCoroutine(knockbackDir, knockbackForce));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator KnockbackCoroutine(Vector2 direction, float force)
    {
        isKnockedBack = true;
        rb.velocity = direction * force;
        yield return new WaitForSeconds(hitStopDuration);
        isKnockedBack = false;
        rb.velocity = Vector2.zero; // reset velocity setelah knockback selesai
    }

    void Die()
    {
        Debug.Log("Enemy died!");
        Destroy(gameObject);
    }
}
