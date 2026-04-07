using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using NUnit.Framework;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float jumpForce = 15f;
    private Rigidbody2D rb;
    private bool isGrounded;
    [SerializeField]
    private Transform groundCheck;
    [SerializeField]
    private float groundCheckRadius = 0.2f;
    [SerializeField]
    private LayerMask groundLayer;
    private Animator anim;
    [SerializeField]
    private BoxCollider2D normalCollider;
    [SerializeField]
    private CapsuleCollider2D duckCollider;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        normalCollider.enabled = true;
        duckCollider.enabled = false;
    }
    void Update()
    {
        isGrounded = CheckIfGrounded();
        HandleJump();
        HandleDuck();
        HandleSoundEffects();
    }
    private bool CheckIfGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
    private void HandleJump()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = Vector2.up * jumpForce;
            AudioManager.instance.PlayJumpClip();
        }
    }
    private void HandleDuck()
    {
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            anim.SetBool("isDuck", true);
            normalCollider.enabled = false;
            duckCollider.enabled = true;
        }
        else if (Keyboard.current.sKey.wasReleasedThisFrame)
        {
            anim.SetBool("isDuck", false);
            normalCollider.enabled = true;
            duckCollider.enabled = false;
        }
    }
    private void HandleSoundEffects()
    {
        if (isGrounded && !AudioManager.instance.HasPlayEffectSound())
        {
            AudioManager.instance.PlayTapClip();
            AudioManager.instance.SetHasPlayEffectSound(true);
        }
        else if (!isGrounded)
        {
            AudioManager.instance.SetHasPlayEffectSound(false);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle"))
        {
            AudioManager.instance.PlayHurtClip();
        }
    }
}
