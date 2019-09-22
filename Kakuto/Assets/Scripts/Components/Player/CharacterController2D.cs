using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
    public CharacterControllerConfig m_ControllerConfig;
#pragma warning disable 0649
    [SerializeField] private Collider2D[] m_GroundChecks;       // colliders where to check if the player is grounded.
#pragma warning restore 0649

    static readonly float k_TimeBetweenJumpsTakeOff = .5f;      // Time between jumps take off

    private bool m_Grounded;                                    // Whether or not the player is grounded.
    private float m_LastJumpTakeOffTimeStamp = 0f;              // Last time character jumps take off
    private float m_LastJumpLandingTimeStamp = 0f;              // Last time character jumps landing
    private bool m_CharacterIsJumping = false;                  // True when the AddForce of the jump is added, until jumpLanding event

    private Rigidbody2D m_Rigidbody2D;
    private Vector2 m_Velocity = Vector2.zero;
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

        m_FacingRight = gameObject.CompareTag(Player.Player1);
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

        ContactPoint2D[] contacts = new ContactPoint2D[5];
        foreach (Collider2D groundCheck in m_GroundChecks)
        {
            if(groundCheck.GetContacts(contacts) > 0)
            {
                foreach(ContactPoint2D contact in contacts)
                {
                    if(Mathf.Approximately(contact.normal.y, 1.0f) && contact.collider.CompareTag("Ground"))
                    {
                        m_Grounded = true;
                        if (!wasGrounded)
                        {
                            m_LastJumpLandingTimeStamp = Time.time;
                            m_CharacterIsJumping = false;
                            OnJumpEvent.Invoke(false);
                        }
                        break;
                    }
                }
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
        if ((m_Grounded && !m_CharacterIsJumping) || m_ControllerConfig.m_AirControl)
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
            Vector2 targetVelocity = new Vector2(move * m_ControllerConfig.m_WalkSpeed * 10.0f, m_Rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector2.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_ControllerConfig.m_MovementSmoothing);

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
            float currentTime = Time.time;
            if(currentTime > m_LastJumpTakeOffTimeStamp + k_TimeBetweenJumpsTakeOff &&
               currentTime > m_LastJumpLandingTimeStamp + m_ControllerConfig.m_TimeBetweenJumps)
            {
                // Add a force to the player according to his direction.
                m_LastJumpTakeOffTimeStamp = Time.time;

                StopMovement();

                GetJumpAngleAndForce(move, out float jumpAngleInDegree, out float jumpForce);
                Vector2 jumpForceDirection = GetJumpForceDirection(jumpAngleInDegree, jumpForce);
                m_Rigidbody2D.AddForce(jumpForceDirection);

                m_CharacterIsJumping = true;
            }
        }
    }

    private void GetJumpAngleAndForce(float move, out float jumpAngleInDegree, out float jumpForce)
    {
        // If the input is moving the player right
        if (move > 0)
        {
            //and the player is facing right...
            jumpAngleInDegree = (m_FacingRight) ? m_ControllerConfig.m_JumpForwardAngle : m_ControllerConfig.m_JumpBackwardAngle;
            jumpForce = (m_FacingRight) ? m_ControllerConfig.m_JumpForwardForce : m_ControllerConfig.m_JumpBackwardForce;
        }
        // Otherwise if the input is moving the player left
        else if (move < 0)
        {
            //and the player is facing right...
            jumpAngleInDegree = (m_FacingRight) ? m_ControllerConfig.m_JumpBackwardAngle : m_ControllerConfig.m_JumpForwardAngle;
            jumpForce = (m_FacingRight) ? m_ControllerConfig.m_JumpBackwardForce : m_ControllerConfig.m_JumpForwardForce;
        }
        // If we don't move
        else
        {
            jumpAngleInDegree = 0f;
            jumpForce = m_ControllerConfig.m_JumpOnPlaceForce;
        }
    }

    private Vector2 GetJumpForceDirection(float jumpAngleInDegree, float jumpForce)
    {
        float jumpAngleInRadian = jumpAngleInDegree * Mathf.Deg2Rad;
        Vector2 jumpDirection = new Vector2(Mathf.Sin(jumpAngleInRadian), Mathf.Cos(jumpAngleInRadian));
        if (!m_FacingRight)
        {
            //Flip direction on the X axis as jumpAngle are calibrate for a P1 character (facing right by default)
            jumpDirection.x *= -1f;
        }
        return jumpDirection.normalized * jumpForce;
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

    public void PushBack(float pushForce)
    {
        StopMovement();
        m_Rigidbody2D.AddForce(new Vector2((m_FacingRight) ? -pushForce : pushForce, 0f));
    }

    public void StopMovement()
    {
        m_Velocity = Vector2.zero;
        m_Rigidbody2D.velocity = m_Velocity;
    }

    public bool IsJumping()
    {
        return m_CharacterIsJumping;
    }
}