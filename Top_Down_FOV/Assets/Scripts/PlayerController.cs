using System;
using Skallu.Utils.Tools;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private static PlayerController _self;
    public static PlayerController self => _self;
    
    [SerializeField] private float movementSpeed = 10;

    private Vector2 mousePosition;
    private Rigidbody2D rb;
    private ParticleSystem particleSystem;
    private Animator animator;
    
    private const string animationIdle = "player_idle";
    private const string animationWalk = "player_walk";

    private void Awake()
    {
        if (_self != null && _self != this)
            Destroy(gameObject);
        else
        {
            _self = this;

            rb = GetComponent<Rigidbody2D>();
            particleSystem = GetComponent<ParticleSystem>();
            animator = GetComponent<Animator>();
        }
    }

    private void Start()
    {
        animator.Play(animationIdle);
        particleSystem.Stop();
    }

    private void FixedUpdate()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        MovePlayer();
        RotatePlayer();
    }

    // moves player character based on input
    private void MovePlayer()
    {
        var playerMovement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if ((playerMovement.x < 0f || playerMovement.x > 0f) || (playerMovement.y < 0 || playerMovement.y > 0))
        {
            // sets walking animation
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationWalk))
                animator.Play(animationWalk);
            
            rb.velocity = playerMovement * (movementSpeed * Time.deltaTime);
        }
        else
        {
            // sets idle animation
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationIdle))
                animator.Play(animationIdle);
            
            rb.velocity = Vector2.zero;
        }

        rb.angularVelocity = 0f;
    }

    // rotates player character to mouse position
    private void RotatePlayer()
    {
        var angle = Mathf.Atan2((mousePosition.y - transform.position.y), (mousePosition.x - transform.position.x)) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Bullet"))
            particleSystem.Play();

        if (col.gameObject.CompareTag("Wall"))
            rb.angularVelocity = 0f;
    }
    
}