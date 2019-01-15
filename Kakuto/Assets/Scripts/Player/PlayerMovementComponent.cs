using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementComponent : MonoBehaviour {

    public PlayerMovementConfig m_MovementConfig;
    public Transform m_GroundCheck;

    LayerMask m_WhatIsGround;                           // A mask determining what is ground to the character.
    const float k_GroundedRadius = .2f;                 // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;                            // Whether or not the player is grounded.
    private bool m_Jump;                                // Whether or not the player is jumping.
    private bool m_Crouch;                              // Wheter or not the player is crouching.
    private float m_Move;                               // Horizontal movement
    private Animator m_Anim;                            // Reference to the player's animator component.
    private Rigidbody2D m_Rigidbody2D;

    private void Awake()
    {
        // Setting up references.
        m_WhatIsGround = LayerMask.GetMask("Ground");
        m_Anim = GetComponentInChildren<Animator>();
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        m_Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
                m_Grounded = true;
        }
        m_Anim.SetBool("Ground", m_Grounded);

        // Set the vertical animation
        //m_Anim.SetFloat("vSpeed", m_Rigidbody2D.velocity.y);

        Move();
        m_Jump = false;
    }

    private void Update()
    {
        m_Crouch = Input.GetKey("down");
        m_Move = Input.GetAxisRaw("Horizontal");

        if (!m_Jump)
        {
            // Read the jump input in Update so button presses aren't missed.
            m_Jump = Input.GetKeyDown("up");
        }
    }


    public void Move()
    {
        // Set whether or not the character is crouching in the animator
        m_Anim.SetBool("Crouch", m_Crouch);

        if(m_Crouch && m_Grounded)
        {
            //Stop the character on place
            m_Anim.SetFloat("Speed", 0.0f);
            m_Rigidbody2D.velocity = Vector2.zero;
        }
        else
        {
            //only control the player if grounded or airControl is turned on
            if (m_Grounded || m_MovementConfig.m_AirControl)
            {
                // The Speed animator parameter is set to the absolute value of the horizontal input.
                m_Anim.SetFloat("Speed", m_Move);

                // Move the character
                m_Rigidbody2D.velocity = new Vector2(m_Move * m_MovementConfig.m_MaxSpeed, m_Rigidbody2D.velocity.y);
            }

            // If the player should jump...
            if (m_Grounded && m_Jump && m_Anim.GetBool("Ground"))
            {
                // Add a vertical force to the player.
                m_Grounded = false;
                m_Anim.SetBool("Ground", false);
                m_Rigidbody2D.AddForce(new Vector2(0f, m_MovementConfig.m_JumpForce));
            }
        }
    }

    public bool IsCrouched()
    {
        return m_Crouch;
    }

    public bool IsInTheAir()
    {
        return m_Grounded == false;
    }
}
