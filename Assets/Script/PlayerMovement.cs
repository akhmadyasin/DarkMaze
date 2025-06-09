using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


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

    public void addKey(int amount)
    {
        collectedKeys += amount;
        if (keyText != null)
        {
            keyText.text = "Keys: " + collectedKeys.ToString() + "/" + totalKeysRequired;
        }
    }



    public void TakeDamage(int damage, Vector2 direction)
    {
        if (isKnockedBack) return; // Jangan stack knockback

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Player Mati");
        }

        StartCoroutine(HandleKnockback(direction.normalized));
        UpdateHealthUI();
    }


    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = "Health: " + currentHealth;
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

    public bool FacingLeft { get { return facingLeft; } set { facingLeft = value; } }

    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private GameObject PanelWin;

    private PlayerController playerControl;
    private Vector2 movement;
    private Rigidbody2D rb;

    private Animator anim;
    public SpriteRenderer sprite;
    private bool facingLeft = false;

    private void Awake()
    {
        playerControl = new PlayerController();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
        UpdateHealthUI();
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
        AdjustPlayerFacingDirection();
        Move();

        if (isKnockedBack) return;
    }

    private void PlayerInput()
    {
        movement = playerControl.Movement.Move.ReadValue<Vector2>();

        // Threshold untuk mencegah flicker (bolak-balik Idle-Run)
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
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("key"))
        {}
    }

    private void OnCollisionEnter2D(Collision2D door)
    {
        if (door.gameObject.CompareTag("door") && collectedKeys >= totalKeysRequired)
        {
            Time.timeScale = 0f; // Pause game
            if (PanelWin != null)
            {
                PanelWin.SetActive(true); // Tampilkan panel menang
            }
        }
    }
}
