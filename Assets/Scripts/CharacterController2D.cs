using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    private bool isGrounded = true;
    private bool isClimbing = false;
    private bool canClimb = false;

    [Header("Climbing Settings")]
    public float climbSpeed = 3f;
    public string climbTag = "Climb";

    [Header("Animation")]
    public Animator animator;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip moveSound;
    public AudioClip jumpSound;
    public AudioClip hurtSound;
    public AudioClip climbSound;
    public AudioClip idleSound;

    private Rigidbody2D rb;
    private bool isHurt = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isHurt) return; // Skip input processing if hurt

        float horizontal = Input.GetAxis("Horizontal");
        HandleMovement(horizontal);

        if (Input.GetButtonDown("Jump") && isGrounded && !isClimbing)
        {
            Jump();
        }

        if (canClimb && Input.GetAxis("Vertical") != 0)
        {
            EnterClimbingState(); // Enter climb state only when vertical input is triggered
        }

        if (isClimbing)
        {
            HandleClimbing();
        }

        UpdateAnimations(horizontal);
    }

    private void HandleMovement(float horizontal)
    {
        if (!isClimbing)
        {
            rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f); // Disable horizontal movement while climbing
        }

        if (horizontal != 0)
        {
            PlaySound(moveSound);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isGrounded = false;
        PlaySound(jumpSound);
        animator.Play("Jump"); // Ensure there is a "Jump" animation in the animator
    }

    private void HandleClimbing()
    {
        float vertical = Input.GetAxis("Vertical");
        rb.velocity = new Vector2(0f, vertical * climbSpeed);

        if (vertical != 0)
        {
            animator.Play("Climb");
            PlaySound(climbSound);
        }
        else
        {
            animator.Play("ClimbIdle"); // Single-frame idle animation while climbing
        }
    }

    private void EnterClimbingState()
    {
        isClimbing = true;
        rb.gravityScale = 0f;
        animator.Play("Climb");
    }

    private void UpdateAnimations(float horizontal)
    {
        if (isClimbing)
        {
            return; // Skip other animations if climbing
        }

        if (horizontal != 0)
        {
            animator.Play("Move");
        }
        else if (isGrounded)
        {
            animator.Play("Idle");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(climbTag))
        {
            canClimb = true; // Enable climbing, but do not start climbing yet
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(climbTag))
        {
            canClimb = false;
            if (isClimbing)
            {
                ExitClimbingState(); // If climbing, exit climb state when leaving climb trigger
            }
        }
    }

    private void ExitClimbingState()
    {
        isClimbing = false;
        rb.gravityScale = 1f; // Restore gravity
        rb.velocity = new Vector2(rb.velocity.x, 0f); // Stop vertical movement
    }

    public void Hurt()
    {
        isHurt = true;
        PlaySound(hurtSound);
        animator.Play("Hurt");
        // Add any hurt effect logic here (e.g., knockback)
        StartCoroutine(RecoverFromHurt());
    }

    private IEnumerator RecoverFromHurt()
    {
        yield return new WaitForSeconds(1f); // Hurt state lasts for 1 second
        isHurt = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource && clip && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}