using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;

    private bool facingRight = true;
    private Animator animator;

    public Transform groundCheck;

    public LayerMask whatIsGround;

    public Rigidbody2D Rb { get; set; }

    bool dead = false;
    bool attack = false;

    void Start()
    {
        Rb = GetComponent<Rigidbody2D>();
        Rb.freezeRotation = true;
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        // 事件執行中，不可移動
        if (EventExcutor.Instance.IsRunning)
        {
            animator.SetFloat("Speed", 0);
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        if (!dead && !attack)
        {
            animator.SetFloat("Speed", Mathf.Abs(horizontal));
            Rb.velocity = new Vector2(horizontal * speed, Rb.velocity.y);
        }

        facingRight = transform.localScale.x > 0 ? true : false;

        if (horizontal > 0 && !facingRight && !dead && !attack)
            Flip(horizontal);
        else if (horizontal < 0 && facingRight && !dead && !attack)
            Flip(horizontal);
    }

    private void HandleInput()
    {
        
    }

    private void Flip(float horizontal)
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}