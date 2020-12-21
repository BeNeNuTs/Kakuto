using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
    public CharacterControllerConfig m_ControllerConfig;
#pragma warning disable 0649
    [SerializeField] private Collider2D m_GroundCheck;          // collider where to check if the player is grounded.
#pragma warning restore 0649

    static readonly float k_TimeBetweenJumpsTakeOff = .5f;      // Time between jumps take off

    private List<Collider2D> m_GroundCheckColliders = new List<Collider2D>();
    private Collider2D[] m_GroundCheckContacts = new Collider2D[1];
    private bool m_Grounded;                                    // Whether or not the player is grounded.
    private float m_LastJumpTakeOffTimeStamp = 0f;              // Last time character jumps take off
    private float m_LastJumpLandingTimeStamp = 0f;              // Last time character jumps landing
    private bool m_CharacterIsJumping = false;                  // True when the AddForce of the jump is added, until jumpLanding event
    private bool m_ShouldUpdateFalling = true;
    private bool m_JumpApexReached = false;
    private float m_MinFallingVelocity = 0f;

    private Rigidbody2D m_Rigidbody2D;
    private Vector2 m_Velocity = Vector2.zero;
    private bool m_FacingRight;                                 // For determining which side the player is currently facing.
    private bool m_MovingRight;

    private PlayerStunInfoSubComponent m_StunInfoSC;

    public Action<bool> OnJumpEvent;
    public Action OnDirectionChangedEvent;
    private bool m_wasCrouching = false;

    private MovementConfig m_MovementConfig;

    private void Awake()
    {
        m_MovementConfig = MovementConfig.Instance;
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        m_FacingRight = gameObject.CompareTag(Player.Player1);
        m_MovingRight = m_FacingRight;
    }

    private void Start()
    {
        m_StunInfoSC = GetComponent<PlayerHealthComponent>().GetStunInfoSubComponent();
    }

    private void FixedUpdate()
    {
        GroundCheck();
        UpdateFalling();
    }

    private void GroundCheck()
    {
        Profiler.BeginSample("CharacterController2D.GroundCheck");

        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        m_GroundCheckColliders.Clear();
        m_GroundCheckContacts[0] = null;
        bool hasContacts = (m_GroundCheck.GetContacts(m_GroundCheckContacts) > 0);
        if (!hasContacts)
        {
            Collider2D overlapCollider = Physics2D.OverlapCircle(transform.position, m_MovementConfig.m_OverlapCircleRadius, m_MovementConfig.m_GroundLayerMask);
            if(overlapCollider != null)
            {
                m_GroundCheckColliders.Add(overlapCollider);
            }
        }
        else if(m_GroundCheckContacts[0] != null)
        {
            m_GroundCheckColliders.Add(m_GroundCheckContacts[0]);
        }

        int groundCheckCollidersCount = m_GroundCheckColliders.Count;
        for (int i = 0; i < groundCheckCollidersCount; i++)
        {
            if (Utils.IsInLayerMask(m_GroundCheckColliders[i].gameObject.layer, m_MovementConfig.m_GroundLayerMask))
            {
                if (!wasGrounded)
                {
                    m_LastJumpLandingTimeStamp = Time.time;
                    OnJumpEvent?.Invoke(false);
                    m_CharacterIsJumping = false;
                    m_JumpApexReached = false;
                }
                m_Grounded = true;
                break;
            }
        }

        if (wasGrounded && !m_Grounded)
        {
            OnJumpEvent?.Invoke(true);
            m_ShouldUpdateFalling = true;
            m_JumpApexReached = false;
        }

        Profiler.EndSample();
    }

    private void UpdateFalling()
    {
        if(m_ShouldUpdateFalling)
        {
            if(!m_CharacterIsJumping || m_StunInfoSC.IsStunned())
            {
                m_ShouldUpdateFalling = false;
                return;
            }

            if(m_JumpApexReached)
            {
                UpdateFallingVelocity();
            }
            else if (m_Rigidbody2D.velocity.y < 0f)
            {
                m_JumpApexReached = true;
                m_MinFallingVelocity = m_Rigidbody2D.velocity.y;
            }
        }
    }

    void UpdateFallingVelocity()
    {
        if (m_Rigidbody2D.velocity.y < m_MinFallingVelocity)
        {
            m_MinFallingVelocity = m_Rigidbody2D.velocity.y;
        }
        else
        {
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_MinFallingVelocity);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (MovementConfig.Instance.m_DEBUG_DisplayOverlapCircle)
        {
            Gizmos.DrawWireSphere(transform.position, MovementConfig.Instance.m_OverlapCircleRadius);
        }
    }

    private void OnGUI()
    {
        if (MovementConfig.Instance.m_DEBUG_DisplayVelocity && m_Rigidbody2D != null)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
            GUI.Label(new Rect(screenPosition.x - 50f, Screen.height - screenPosition.y - (Screen.height / 2f) - 50f, 200f, 30f), "Velocity : " + m_Rigidbody2D.velocity);
        }
    }
#endif

    public void Move(float move, bool crouch, bool jump, float jumpTakeOffDirection, EJumpPhase jumpPhase)
    {
        // If the player should jump...
        if (jump && CanJump())
        {
            // Add a force to the player according to his direction.
            m_LastJumpTakeOffTimeStamp = Time.time;

            StopMovement();

            GetJumpAngleAndForce(move, jumpTakeOffDirection, out float jumpAngleInDegree, out float jumpForce);
            Vector2 jumpForceDirection = GetJumpForceDirection(jumpAngleInDegree, jumpForce);
            m_Rigidbody2D.AddForce(jumpForceDirection, ForceMode2D.Impulse);

            m_CharacterIsJumping = true;
        }

        //only control the player if grounded or airControl is turned on
        if ((m_Grounded && !m_CharacterIsJumping) || m_ControllerConfig.m_AirControl)
        {
            if(m_Grounded && !m_CharacterIsJumping)
            {
                // If crouching
                if (crouch)
                {
                    if (!m_wasCrouching)
                    {
                        m_wasCrouching = true;
                    }

                    // Reduce the speed by the crouchSpeed multiplier
                    move *= m_ControllerConfig.m_CrouchSpeed;
                }
                else
                {
                    if (m_wasCrouching)
                    {
                        m_wasCrouching = false;
                    }
                }
            }
            
            // Move the character by finding the target velocity
            Vector2 targetVelocity = new Vector2(move * GetWalkSpeed(move, jumpPhase) * 10.0f, m_Rigidbody2D.velocity.y);
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
    }

    private float GetWalkSpeed(float move, EJumpPhase jumpPhase)
    {
        if(jumpPhase != EJumpPhase.TakeOff && jumpPhase != EJumpPhase.Landing)
        {
            if (move > 0f)
            {
                return (m_FacingRight) ? m_ControllerConfig.m_WalkForwardSpeed : m_ControllerConfig.m_WalkBackwardSpeed;
            }
            else if (move < 0f)
            {
                return (m_FacingRight) ? m_ControllerConfig.m_WalkBackwardSpeed : m_ControllerConfig.m_WalkForwardSpeed;
            }
        }

        return 0f;
    }

    private void GetJumpAngleAndForce(float move, float jumpTakeOffDirection, out float jumpAngleInDegree, out float jumpForce)
    {
        float jumpDirection = jumpTakeOffDirection;

        // if move direction is at the opposite of jumpTakeOffDirection, then choose the more recent value
        if ((move > 0f && jumpTakeOffDirection < 0f) || (move < 0f && jumpTakeOffDirection > 0f) || (jumpTakeOffDirection == 0f && move != 0f))
        {
            jumpDirection = move;
        }

        // If the input is moving the player right
        if (jumpDirection > 0f)
        {
            //and the player is facing right...
            jumpAngleInDegree = (m_FacingRight) ? m_ControllerConfig.m_JumpForwardAngle : m_ControllerConfig.m_JumpBackwardAngle;
            jumpForce = (m_FacingRight) ? m_ControllerConfig.m_JumpForwardForce : m_ControllerConfig.m_JumpBackwardForce;
        }
        // Otherwise if the input is moving the player left
        else if (jumpDirection < 0f)
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

        OnDirectionChangedEvent?.Invoke();
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
        m_ShouldUpdateFalling = false;
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

    public bool IsJumpApexReached()
    {
        return m_JumpApexReached;
    }

    public EMovingDirection GetMovingDirection()
    {
        if(!IsJumping() && (m_Rigidbody2D.velocity.x > 0f || m_Rigidbody2D.velocity.x < 0f))
        {
            if(m_FacingRight)
            {
                return (m_MovingRight) ? EMovingDirection.Forward : EMovingDirection.Backward;
            }
            else
            {
                return (m_MovingRight) ? EMovingDirection.Backward : EMovingDirection.Forward;
            }
        }

        return EMovingDirection.None;
    }

    public bool IsFacingRight()
    {
        return m_FacingRight;
    }
}