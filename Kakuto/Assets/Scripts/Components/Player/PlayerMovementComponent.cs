using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class PlayerMovementComponent : MonoBehaviour
{
    enum EBlockedReason
    {
        None,
        PlayAttack,
        Stun
    }

    private CharacterController2D m_Controller;
    private Animator m_Animator;
    private PlayerAttackComponent m_AttackComponent;

    private Transform m_Enemy;
    private bool m_IsLeftSide;
    private int m_PlayerIndex;

    private float m_HorizontalMoveInput = 0f;

    private bool m_WantToJump = false;
    private bool m_JumpInput = false;
    private float m_TimeToWaitBeforeJumping = 0f;

    private bool m_CrouchInput = false;

    private bool m_IsCrouching = false;

    private bool m_IsMovementBlocked = false;
#pragma warning disable 414
    private EBlockedReason m_MovementBlockedReason = EBlockedReason.None;
#pragma warning restore 414

    [Header("Debug")]
    [Space]

    public bool m_DEBUG_IsStatic = false;

    void Awake()
    {
        m_Controller = GetComponent<CharacterController2D>();
        m_Animator = GetComponentInChildren<Animator>();
        m_AttackComponent = GetComponent<PlayerAttackComponent>();

        m_Enemy = GameObject.FindGameObjectWithTag(Utils.GetEnemyTag(gameObject)).transform.root;
        m_PlayerIndex = gameObject.CompareTag(Player.Player1) ? 0 : 1;
        m_IsLeftSide = (m_PlayerIndex == 0) ? true : false;

        RegisterListeners();
    }

    void RegisterListeners()
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StartListening(EPlayerEvent.EndOfAttack, EndOfAttack);
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StartListening(EPlayerEvent.BlockMovement, BlockMovement);
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StartListening(EPlayerEvent.UnblockMovement, UnblockMovement);
        Utils.GetPlayerEventManager<bool>(gameObject).StartListening(EPlayerEvent.StopMovement, OnStopMovement);

        Utils.GetPlayerEventManager<float>(gameObject).StartListening(EPlayerEvent.StunBegin, OnStunBegin);
        Utils.GetPlayerEventManager<float>(gameObject).StartListening(EPlayerEvent.StunEnd, OnStunEnd);
    }

    void OnDestroy()
    {
        UnregisterListeners();
    }

    void UnregisterListeners()
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StopListening(EPlayerEvent.EndOfAttack, EndOfAttack);
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StopListening(EPlayerEvent.BlockMovement, BlockMovement);
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StopListening(EPlayerEvent.UnblockMovement, UnblockMovement);
        Utils.GetPlayerEventManager<bool>(gameObject).StopListening(EPlayerEvent.StopMovement, OnStopMovement);

        Utils.GetPlayerEventManager<float>(gameObject).StopListening(EPlayerEvent.StunBegin, OnStunBegin);
        Utils.GetPlayerEventManager<float>(gameObject).StopListening(EPlayerEvent.StunEnd, OnStunEnd);
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerSide();

        if (!m_IsMovementBlocked)
        {
            if (!m_DEBUG_IsStatic)
            {
                m_HorizontalMoveInput = InputManager.GetHorizontalMovement(m_PlayerIndex);
                m_Animator.SetFloat("Speed", Mathf.Abs(m_HorizontalMoveInput));

                if(!m_WantToJump)
                {
                    m_JumpInput = false;
                    if(InputManager.GetJumpInput(m_PlayerIndex))
                    {
                        m_WantToJump = true;
                        m_TimeToWaitBeforeJumping = MovementConfig.Instance.TimeToWaitBeforeJumping;
                    }
                }

                m_CrouchInput = InputManager.GetCrouchInput(m_PlayerIndex);
            }
        }
    }

    void LateUpdate()
    {
        if(m_WantToJump && m_TimeToWaitBeforeJumping > 0f)
        {
            m_TimeToWaitBeforeJumping -= Time.deltaTime;
            if(m_TimeToWaitBeforeJumping <= 0)
            {
                m_JumpInput = true;
                m_WantToJump = false;
            }
        }
    }

    void UpdatePlayerSide()
    {
        // If movement is not blocked AND we're not jumping AND we're not attacking => Then we can update player side
        if(!m_IsMovementBlocked && !IsJumping() && m_AttackComponent.GetCurrentAttack() == null)
        {
            if (m_IsLeftSide)
            {
                if (m_Enemy.position.x < transform.position.x)
                {
                    OnSideChanged();
                }
            }
            else
            {
                if (m_Enemy.position.x > transform.position.x)
                {
                    OnSideChanged();
                }
            }
        }   
    }

    void OnSideChanged()
    {
        m_IsLeftSide = !m_IsLeftSide;
        m_Controller.Flip();
        OnDirectionChanged();
    }

    public void OnJumping(bool isJumping)
    {
        m_Animator.SetBool("IsJumping", isJumping);
    }

    public void OnCrouching(bool isCrouching)
    {
        m_IsCrouching = isCrouching;
        m_Animator.SetBool("IsCrouching", isCrouching);
    }

    public void OnDirectionChanged()
    {
        m_Animator.SetFloat("MovingDirection", m_Animator.GetFloat("MovingDirection") * -1.0f);
    }

    void FixedUpdate()
    {
        if(m_IsMovementBlocked)
        {
            return;
        }

        // Move our character
        m_Controller.Move(m_HorizontalMoveInput * Time.fixedDeltaTime, m_CrouchInput, m_JumpInput);
    }

    public EPlayerStance GetCurrentStance()
    {
        EPlayerStance currentStance = EPlayerStance.Stand;
        if (IsCrouching())
        {
            currentStance = EPlayerStance.Crouch;
        }
        else if(IsJumping())
        {
            currentStance = EPlayerStance.Jump;
        }

        return currentStance;
    }

    public bool IsStanding()
    {
        return !IsCrouching() && !IsJumping();
    }

    public bool IsCrouching()
    {
        return m_IsCrouching;
    }

    public bool IsJumping()
    {
        return m_Controller.IsJumping();
    }

    public bool IsMovingBack()
    {
        if(m_IsLeftSide)
        {
            return m_HorizontalMoveInput < 0.0f;
        }
        else
        {
            return m_HorizontalMoveInput > 0.0f;
        }
    }

    public bool IsLeftSide()
    {
        return m_IsLeftSide;
    }

    public int GetPlayerIndex()
    {
        return m_PlayerIndex;
    }

    public void PushBack(float pushForce)
    {
        m_Controller.PushBack(pushForce);
    }

    public void SetMovementBlockedByAttack(bool isMovementBlockedByAttack)
    {
        SetMovementBlocked(isMovementBlockedByAttack, EBlockedReason.PlayAttack);

        if (!IsJumping())
        {
            m_Controller.StopMovement();
        }
    }

    private void OnStopMovement(bool dummyBool)
    {
        m_Controller.StopMovement();
    }

    void EndOfAttack(EAnimationAttackName attackName)
    {
        if(m_IsMovementBlocked)
        {
            SetMovementBlocked(false, EBlockedReason.None);
        }
    }

    void BlockMovement(EAnimationAttackName attackName)
    {
        if (m_IsMovementBlocked)
        {
            Debug.LogError("Movement was already blocked");
            return;
        }
        SetMovementBlocked(true, EBlockedReason.PlayAttack);
    }

    void UnblockMovement(EAnimationAttackName attackName)
    {
        if (m_IsMovementBlocked == false)
        {
            Debug.LogError("Movement was not blocked");
            return;
        }
        SetMovementBlocked(false, EBlockedReason.None);
    }

    void OnStunBegin(float stunTimeStamp)
    {
        SetMovementBlocked(true, EBlockedReason.Stun);
    }

    void OnStunEnd(float stunTimeStamp)
    {
        SetMovementBlocked(false, EBlockedReason.None);
    }

    void SetMovementBlocked(bool isMovementBlocked, EBlockedReason reason)
    {
        m_IsMovementBlocked = isMovementBlocked;
        m_MovementBlockedReason = reason;

        if(isMovementBlocked)
        {
            m_WantToJump = false;
        }
    }
}