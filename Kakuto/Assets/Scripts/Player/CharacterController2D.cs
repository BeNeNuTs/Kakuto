using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
    public CharacterControllerConfig m_ControllerConfig;
    [SerializeField] private Transform m_GroundCheck;           // A position marking where to check if the player is grounded.

    const float k_GroundedRadius = .2f;                         // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;                                    // Whether or not the player is grounded.
    private Rigidbody2D m_Rigidbody2D;
    private Vector3 m_Velocity = Vector3.zero;
    private bool m_FacingRight;                                 // For determining which side the player is currently facing.
    private bool m_MovingRight;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    [Header("Events")]
    [Space]

    public BoolEvent OnJumpEvent;
    public BoolEvent OnCrouchEvent;
    public UnityEvent OnDirectionChangedEvent;
    private bool m_wasCrouching = false;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        m_FacingRight = gameObject.CompareTag("Player1");
        m_MovingRight = m_FacingRight;

        if (OnJumpEvent == null)
            OnJumpEvent = new BoolEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();

        if (OnDirectionChangedEvent == null)
            OnDirectionChangedEvent = new UnityEvent();
    }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_ControllerConfig.m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                if (!wasGrounded)
                    OnJumpEvent.Invoke(false);
            }
        }

        if(wasGrounded && !m_Grounded)
        {
            OnJumpEvent.Invoke(true);
        }
    }


    public void Move(float move, bool crouch, bool jump)
    {
        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_ControllerConfig.m_AirControl)
        {
            // If crouching
            if (crouch)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                // Reduce the speed by the crouchSpeed multiplier
                move *= m_ControllerConfig.m_CrouchSpeed;
            }
            else
            {
                if (m_wasCrouching)
                {
                    m_wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }

            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * m_ControllerConfig.m_WalkSpeed * 10.0f, m_Rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_ControllerConfig.m_MovementSmoothing);

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_MovingRight)
            {
                OnDirectionChanged();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && m_MovingRight)
            {
                OnDirectionChanged();
            }
        }
        // If the player should jump...
        if (m_Grounded && jump)
        {
            // Add a vertical force to the player.
            m_Grounded = false;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_ControllerConfig.m_JumpForce));
        }
    }


    private void OnDirectionChanged()
    {
        // Switch the way the player is labelled as moving.
        m_MovingRight = !m_MovingRight;

        OnDirectionChangedEvent.Invoke();
    }

    public void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}