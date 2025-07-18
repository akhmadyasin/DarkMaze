using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Health System")]
    public int maxHealth = 100;
    private int currentHealth;
    public Image healthFillImage;
    [SerializeField] private GameObject PanelDie; //  Tambahan untuk panel "Die"

    [Header("Knockback Settings")]
    [SerializeField] private float knockBackTime = 0.2f;
    [SerializeField] private float knockBackThrust = 10f;
    private bool isKnockedBack = false;

    [Header("Key System")]
    public int collectedKeys = 0;
    public int totalKeysRequired = 3;
    public TextMeshProUGUI keyCountText;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public LayerMask enemyLayer;
    public float attackCooldown = 0.4f;
    public float playerDamage = 20f;
    private bool isAttacking = false;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip attackSFX;
    [SerializeField] private AudioClip damageSFX;
    [SerializeField] private AudioClip pickupSFX;
    [SerializeField] private AudioClip runSFX;

    private AudioSource audioSource;
    private AudioSource runAudioSource;

    [SerializeField] private GameObject PanelWin;

    private PlayerController playerControl;
    private Vector2 movement;
    private Rigidbody2D rb;

    private Animator anim;
    public SpriteRenderer sprite;
    private bool facingLeft = false;
    private Vector3 originalAttackPointLocalPos;

    public bool FacingLeft { get { return facingLeft; } set { facingLeft = value; } }

    [SerializeField] private float moveSpeed = 1f;

    private bool isDead = false; //  Tambahan flag mati

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        runAudioSource = gameObject.AddComponent<AudioSource>();
        runAudioSource.clip = runSFX;
        runAudioSource.loop = true;
        runAudioSource.playOnAwake = false;

        rb = GetComponent<Rigidbody2D>();
        playerControl = new PlayerController();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
        UpdateHealthUI();

        playerControl.Movement.Attack.performed += ctx => HandleAttackInput();

        originalAttackPointLocalPos = attackPoint.localPosition;

        addKey(0);
    }

    private void Start()
    {
        // Reset semua flag biar player bisa gerak & serang di level baru
        isDead = false;
        isAttacking = false;
        isKnockedBack = false;
        movement = Vector2.zero;

        Time.timeScale = 1f; // Pastikan tidak freeze

        currentHealth = maxHealth;
        UpdateHealthUI();

        if (runAudioSource != null && runAudioSource.isPlaying)
            runAudioSource.Stop();

        collectedKeys = 0;
        addKey(0);
    }


    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
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
        if (isDead) return; //  Jangan input kalau sudah mati
        PlayerInput();
    }

    private void FixedUpdate()
    {
        if (isKnockedBack || isAttacking || isDead) return;
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
            if (!runAudioSource.isPlaying && runSFX != null)
                runAudioSource.Play();
        }
        else
        {
            anim.SetInteger("state", 0); // Idle
            if (runAudioSource.isPlaying)
                runAudioSource.Stop();
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
            bool newFacingLeft = movement.x < 0f;

            if (facingLeft != newFacingLeft)
            {
                facingLeft = newFacingLeft;
                sprite.flipX = facingLeft;

                Vector3 flippedAttackPos = originalAttackPointLocalPos;
                flippedAttackPos.x *= facingLeft ? -1 : 1;
                attackPoint.localPosition = flippedAttackPos;

                Vector3 flippedScale = attackPoint.localScale;
                flippedScale.x = Mathf.Abs(flippedScale.x) * (facingLeft ? -1 : 1);
                attackPoint.localScale = flippedScale;
            }
        }
    }

    private void HandleAttackInput()
    {
        if (!isAttacking && !isDead)
        {
            PlaySFX(attackSFX);
            if (runAudioSource.isPlaying)
                runAudioSource.Stop();
            StartCoroutine(AttackCoroutine());
        }
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        anim.SetInteger("state", 2); // Attack
        yield return new WaitForSeconds(0.1f);

        Collider2D attackCollider = attackPoint.GetComponent<Collider2D>();
        if (attackCollider != null)
        {
            Collider2D[] hits = new Collider2D[10];
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
        if (isKnockedBack || isDead) return; //  Jangan ambil damage kalau sudah mati
        PlaySFX(damageSFX);

        currentHealth -= damage;
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }

        StartCoroutine(HandleKnockback(direction.normalized));
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

    private void Die()
    {
        isDead = true;
        anim.SetInteger("state", 3); //  Ganti ke animasi mati
        rb.velocity = Vector2.zero;
        movement = Vector2.zero;

        if (runAudioSource.isPlaying)
            runAudioSource.Stop();

        StartCoroutine(ShowDiePanelAfterDelay());
    }

    private IEnumerator ShowDiePanelAfterDelay()
    {
        yield return new WaitForSeconds(0.8f); // waktu untuk animasi mati
        Time.timeScale = 0f;
        if (PanelDie != null)
            PanelDie.SetActive(true);
    }

    private void UpdateHealthUI()
    {
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    public void addKey(int amount)
    {
        collectedKeys += amount;
        if (keyCountText != null)
        {
            keyCountText.text = collectedKeys + " / " + totalKeysRequired;
        }
    }

    public void AttackFromButton()
    {
        HandleAttackInput();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("key"))
        {
            // Tambahkan logika ambil kunci di sini
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
