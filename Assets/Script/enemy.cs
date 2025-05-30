using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float chaseRange = 10f;
    public float returnRangeMultiplier = 1.5f;
    public float raycastDistance = 0.6f;
    public LayerMask obstacleLayer;
    public Transform target;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Vector2 startPosition;
    private SpriteRenderer spriteRenderer;

    private enum State { Idle, Chasing, Returning }
    private State currentState = State.Idle;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
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

        // === State Transition Logic ===
        switch (currentState)
        {
            case State.Idle:
                if (distanceToPlayer <= chaseRange)
                {
                    currentState = State.Chasing;
                    Debug.Log("Enemy: Switching to CHASING");
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
                }
                // Tambahan penting: kalau player mendekat saat balik, langsung kejar
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
                moveDirection = Vector2.zero;
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
}
