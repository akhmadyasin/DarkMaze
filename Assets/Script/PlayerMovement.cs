using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Health System")]
    public int maxHealth = 100;
    private int currentHealth;
    public TextMeshProUGUI healthText;

    [Header("Knockback Settings")]
    [SerializeField] private float knockBackTime = 0.2f;
    [SerializeField] private float knockBackThrust = 10f;
    private bool isKnockedBack = false;

    [Header("Key System")]
    public int collectedKeys = 0;
    public int totalKeysRequired = 3;
    public TextMeshProUGUI keyText;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public LayerMask enemyLayer;
    public float attackCooldown = 0.4f;
    public float playerDamage = 20f;
    private bool isAttacking = false;

    [SerializeField] private GameObject PanelWin;

    private PlayerController playerControl;
    private Vector2 movement;
    private Rigidbody2D rb;

    private Animator anim;
    public SpriteRenderer sprite;
    private bool facingLeft = false;

    public bool FacingLeft { get { return facingLeft; } set { facingLeft = value; } }

    [SerializeField] private float moveSpeed = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerControl = new PlayerController();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
        UpdateHealthUI();

        playerControl.Movement.Attack.performed += ctx => HandleAttackInput();
    }

    private void OnEnable()
    {
        playerControl.Enable();
    }

    private void OnDisable()
    {
        playerControl.Disable();
    }

    private void Update()
    {
        PlayerInput();
    }

    private void FixedUpdate()
    {
        if (isKnockedBack || isAttacking) return;

        AdjustPlayerFacingDirection();
        Move();
    }

    private void PlayerInput()
    {
        movement = playerControl.Movement.Move.ReadValue<Vector2>();

        if (isAttacking) return;

        if (movement.magnitude > 0.1f)
        {
            anim.SetInteger("state", 1); // Run
        }
        else
        {
            anim.SetInteger("state", 0); // Idle
        }
    }

    private void Move()
    {
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
    }

    private void AdjustPlayerFacingDirection()
    {
        if (movement.x != 0f)
        {
            sprite.flipX = movement.x < 0f;
            FacingLeft = movement.x < 0f;
        }
        else if (movement.y != 0f)
        {
            sprite.flipX = false;
            FacingLeft = false;
        }
    }

    private void HandleAttackInput()
    {
        if (!isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        anim.SetInteger("state", 2); // Attack animation
        yield return new WaitForSeconds(0.1f); // waktu timing serangan

        Collider2D attackCollider = attackPoint.GetComponent<Collider2D>();
        if (attackCollider != null)
        {
            Collider2D[] hits = new Collider2D[10]; // buffer array
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(enemyLayer);
            filter.useLayerMask = true;

            int count = attackCollider.OverlapCollider(filter, hits);

            for (int i = 0; i < count; i++)
            {
                enemy e = hits[i].GetComponent<enemy>();
                if (e != null)
                {
                    e.TakeDamage((int)playerDamage, transform);
                }
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    public void TakeDamage(int damage, Vector2 direction)
    {
        if (isKnockedBack) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Player Mati");
        }

        StartCoroutine(HandleKnockback(direction.normalized));
        UpdateHealthUI();
    }

    private IEnumerator HandleKnockback(Vector2 direction)
    {
        isKnockedBack = true;
        rb.velocity = Vector2.zero;

        Vector2 force = direction * knockBackThrust * rb.mass;
        rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockBackTime);
        rb.velocity = Vector2.zero;
        isKnockedBack = false;
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = "Health: " + currentHealth;
    }

    public void addKey(int amount)
    {
        collectedKeys += amount;
        if (keyText != null)
        {
            keyText.text = "Keys: " + collectedKeys.ToString() + "/" + totalKeysRequired;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("key"))
        {
            // Implementasi ngambil key bisa kamu tambahkan di sini
        }
    }

    private void OnCollisionEnter2D(Collision2D door)
    {
        if (door.gameObject.CompareTag("door") && collectedKeys >= totalKeysRequired)
        {
            Time.timeScale = 0f;
            if (PanelWin != null)
            {
                PanelWin.SetActive(true);
            }
        }
    }
}
