using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
    public CharacterControllerConfig m_ControllerConfig;
#pragma warning disable 0649
    [SerializeField] private Collider2D m_GroundCheck;          // collider where to check if the player is grounded.
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

    [Separator("Events")]
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

        List<Collider2D> groundCheckColliders = new List<Collider2D>();

        Collider2D[] groundCheckContacts = new Collider2D[5];
        bool hasContacts = (m_GroundCheck.GetContacts(groundCheckContacts) > 0);
        if(!hasContacts)
        {
            Collider2D overlapCollider = Physics2D.OverlapCircle(transform.position, MovementConfig.Instance.m_OverlapCircleRadius, MovementConfig.Instance.m_GroundLayerMask);
            groundCheckColliders.Add(overlapCollider);
        }
        else
        {
            groundCheckColliders.AddRange(groundCheckContacts);
        }

        foreach (Collider2D collider in groundCheckColliders)
        {
            if(collider != null)
            {
                if (Utils.IsInLayerMask(collider.gameObject.layer, MovementConfig.Instance.m_GroundLayerMask))
                {
                    if (!wasGrounded)
                    {
                        m_LastJumpLandingTimeStamp = Time.time;
                        OnJumpEvent.Invoke(false);
                        m_CharacterIsJumping = false;
                    }
                    m_Grounded = true;
                    break;
                }
            }
        }

        if (wasGrounded && !m_Grounded)
        {
            OnJumpEvent.Invoke(true);
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (MovementConfig.Instance.d_DisplayOverlapCircle)
        {
            Gizmos.DrawWireSphere(transform.position, MovementConfig.Instance.m_OverlapCircleRadius);
        }
#endif
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
            Vector2 targetVelocity = new Vector2(move * GetWalkSpeed(move) * 10.0f, m_Rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector2.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_ControllerConfig.m_MovementSmoothing);

            // If the input is moving the player right and the player is facing left...
            if (move > 0f && !m_MovingRight)
            {
                OnDirectionChanged();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0f && m_MovingRight)
            {
                OnDirectionChanged();
            }
        }
        // If the player should jump...
        if (jump && CanJump())
        {
            // Add a force to the player according to his direction.
            m_LastJumpTakeOffTimeStamp = Time.time;

            StopMovement();

            GetJumpAngleAndForce(move, out float jumpAngleInDegree, out float jumpForce);
            Vector2 jumpForceDirection = GetJumpForceDirection(jumpAngleInDegree, jumpForce);
            m_Rigidbody2D.AddForce(jumpForceDirection, ForceMode2D.Impulse);

            m_CharacterIsJumping = true;
        }
    }

    private float GetWalkSpeed(float move)
    {
        if (move > 0f)
        {
            return (m_FacingRight) ? m_ControllerConfig.m_WalkForwardSpeed : m_ControllerConfig.m_WalkBackwardSpeed;
        }
        else if(move < 0f)
        {
            return (m_FacingRight) ? m_ControllerConfig.m_WalkBackwardSpeed : m_ControllerConfig.m_WalkForwardSpeed;
        }
        else
        {
            return 0f;
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

    public void PushForward(float pushForce)
    {
        PushBack(-pushForce);
    }

    public void PushBack(float pushForce)
    {
        StopMovement();
        m_Rigidbody2D.AddForce(new Vector2((m_FacingRight) ? -pushForce : pushForce, 0f), ForceMode2D.Impulse);
    }

    public void StopMovement()
    {
        m_Velocity = Vector2.zero;
        m_Rigidbody2D.velocity = m_Velocity;
    }

    public bool CanJump()
    {
        if(!IsJumping())
        {
            float currentTime = Time.time;
            if (currentTime > m_LastJumpTakeOffTimeStamp + k_TimeBetweenJumpsTakeOff &&
               currentTime > m_LastJumpLandingTimeStamp + m_ControllerConfig.m_TimeBetweenJumps)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsJumping()
    {
        return m_CharacterIsJumping || !m_Grounded;
    }
}