using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float chaseRange = 10f;
    public float returnRangeMultiplier = 1.5f;
    public float raycastDistance = 0.6f;
    public float patrolRange = 3f; //  Jarak acak patroli
    public float patrolWaitTime = 2f; //  Waktu tunggu sebelum pilih tujuan baru
    public LayerMask obstacleLayer;
    public Transform target;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Vector2 startPosition;
    private Vector2 patrolTarget;
    private float patrolTimer;

    private SpriteRenderer spriteRenderer;

    private enum State { Idle, Chasing, Returning }
    private State currentState = State.Idle;

    void Awake()
    {
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

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        float distanceToStart = Vector2.Distance(transform.position, startPosition);
        float distanceToPatrol = Vector2.Distance(transform.position, patrolTarget);

        // === State Transition Logic ===
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
                    // Patroli acak saat idle
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

        // === Arah Gerak Berdasarkan State ===
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

        // === Raycast: Stop kalau ada tembok ===
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

        // === Flip Sprite ===
        if (spriteRenderer && moveDirection.x != 0)
            spriteRenderer.flipX = moveDirection.x > 0;
    }

    void FixedUpdate()
    {
        rb.velocity = moveDirection * moveSpeed;
    }

    // === Fungsi Patroli Acak ===
    void SetRandomPatrolTarget()
    {
        float randX = Random.Range(-patrolRange, patrolRange);
        float randY = Random.Range(-patrolRange, patrolRange);
        patrolTarget = startPosition + new Vector2(randX, randY);
        Debug.Log("Enemy: New patrol target set to " + patrolTarget);
    }
}
