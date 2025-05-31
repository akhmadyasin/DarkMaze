using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    public static string havekey = "n";
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
        if (other.tag == "key")
        {
            havekey = "y";
        }
    }
    private void OnCollisionEnter2D(Collision2D door)
    {
        if ((door.gameObject.tag == "door") && (havekey == "y"))
        {
            Time.timeScale = 0f; // Pause game
            if (PanelWin != null)
            {
                PanelWin.SetActive(true); // Tampilkan panel menang
            }
        }
    }
}
