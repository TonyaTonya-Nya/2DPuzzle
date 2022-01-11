using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerController : MonoBehaviour
{
    public float speed;

    private bool facingRight = true;
    private Animator animator;

    public Transform groundCheck;

    public LayerMask whatIsGround;

    public Rigidbody2D Rb { get; set; }

    private bool dead = false;

    private float timer=0f;

    public Text T_t;

    void Start()
    {
        timer = 0f;
        Rb = GetComponent<Rigidbody2D>();
        Rb.freezeRotation = true;
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        timer += Time.deltaTime;
   
       
        float minutes = Mathf.Floor(timer / 60);
        float seconds = timer % 60;

        string time_str = minutes + ":" + Mathf.RoundToInt(seconds);
        PlayerData.Instance.clearTime = time_str;
        T_t.text = time_str;

        HandleInput();
    }

    void FixedUpdate()
    {
        // 事件執行中，不可移動
        if (EventExecutor.Instance.IsRunning)
        {
            animator.SetFloat("Speed", 0);
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        if (!dead)
        {
            animator.SetFloat("Speed", Mathf.Abs(horizontal));
            Rb.velocity = new Vector2(horizontal * speed, Rb.velocity.y);
        }

        facingRight = transform.localScale.x > 0 ? true : false;

        if (horizontal > 0 && !facingRight && !dead)
            Flip(horizontal);
        else if (horizontal < 0 && facingRight && !dead)
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