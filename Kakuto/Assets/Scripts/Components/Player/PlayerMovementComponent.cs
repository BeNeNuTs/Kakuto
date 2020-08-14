using System;
using UnityEngine;

public enum EJumpPhase
{
    None,
    TakeOff,
    Air,
    Landing
}

public enum EMovingDirection
{
    None,
    Forward,
    Backward
}

[RequireComponent(typeof(CharacterController2D))]
public class PlayerMovementComponent : MonoBehaviour
{
    // If new values added to EBlockedReason
    // Please update CanUpdateMovementBlockedStatus method
    enum EBlockedReason
    {
        None,
        PlayAttack,
        EndAttack,
        RequestByAttack,
        Stun,
        StunEnd,
        TimeOver
    }

    private CharacterController2D m_Controller;
    private Animator m_Animator;
    private PlayerAttackComponent m_AttackComponent;
    private PlayerHealthComponent m_HealthComponent;
    private PlayerInfoComponent m_InfoComponent;

    private Transform m_Enemy;
    private bool m_IsLeftSide;

    private float m_HorizontalMoveInput = 0f;
    private bool m_JumpInput = false;
    private bool m_CrouchInput = false;

    private bool m_JumpTakeOffRequested = false;
    private float m_JumpTakeOffDirection = 0f;
    private bool m_TriggerJumpImpulse = false;

    private EPlayerStance m_PlayerStance = EPlayerStance.Stand;
    private EJumpPhase m_JumpPhase = EJumpPhase.None;
    public Action OnLanding;

    private bool m_IsMovementBlocked = false;
#pragma warning disable 414
    private EBlockedReason m_MovementBlockedReason = EBlockedReason.None;
#pragma warning restore 414

    void Awake()
    {
        m_Controller = GetComponent<CharacterController2D>();
        m_Animator = GetComponentInChildren<Animator>();
        m_AttackComponent = GetComponent<PlayerAttackComponent>();
        m_HealthComponent = GetComponent<PlayerHealthComponent>();
        m_InfoComponent = GetComponent<PlayerInfoComponent>();

        RegisterListeners();
    }

    void Start()
    {
        m_Enemy = GameObject.FindGameObjectWithTag(Utils.GetEnemyTag(gameObject)).transform.root;
        m_IsLeftSide = (m_InfoComponent.GetPlayerIndex() == 0) ? true : false;
    }

    void RegisterListeners()
    {
        m_Controller.OnJumpEvent += OnJumping;
        m_Controller.OnDirectionChangedEvent += OnDirectionChanged;

        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StartListening(EPlayerEvent.EndOfAttack, EndOfAttack);
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StartListening(EPlayerEvent.BlockMovement, BlockMovement);
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StartListening(EPlayerEvent.UnblockMovement, UnblockMovement);
        Utils.GetPlayerEventManager<bool>(gameObject).StartListening(EPlayerEvent.StopMovement, OnStopMovement);
        Utils.GetPlayerEventManager<bool>(gameObject).StartListening(EPlayerEvent.TriggerJumpImpulse, OnTriggerJumpImpulse);

        Utils.GetPlayerEventManager<bool>(gameObject).StartListening(EPlayerEvent.StunBegin, OnStunBegin);
        Utils.GetPlayerEventManager<bool>(gameObject).StartListening(EPlayerEvent.StunEnd, OnStunEnd);

        Utils.GetPlayerEventManager<bool>(gameObject).StartListening(EPlayerEvent.EndTurnAround, OnEndTurnAround);

        RoundSubGameManager.OnRoundOver += OnRoundOver;
    }

    void OnDestroy()
    {
        UnregisterListeners();
    }

    void UnregisterListeners()
    {
        m_Controller.OnJumpEvent -= OnJumping;
        m_Controller.OnDirectionChangedEvent -= OnDirectionChanged;

        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StopListening(EPlayerEvent.EndOfAttack, EndOfAttack);
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StopListening(EPlayerEvent.BlockMovement, BlockMovement);
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StopListening(EPlayerEvent.UnblockMovement, UnblockMovement);
        Utils.GetPlayerEventManager<bool>(gameObject).StopListening(EPlayerEvent.StopMovement, OnStopMovement);
        Utils.GetPlayerEventManager<bool>(gameObject).StopListening(EPlayerEvent.TriggerJumpImpulse, OnTriggerJumpImpulse);

        Utils.GetPlayerEventManager<bool>(gameObject).StopListening(EPlayerEvent.StunBegin, OnStunBegin);
        Utils.GetPlayerEventManager<bool>(gameObject).StopListening(EPlayerEvent.StunEnd, OnStunEnd);

        Utils.GetPlayerEventManager<bool>(gameObject).StopListening(EPlayerEvent.EndTurnAround, OnEndTurnAround);

        RoundSubGameManager.OnRoundOver -= OnRoundOver;
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerSide();

        m_HorizontalMoveInput = 0f;
        m_JumpInput = false;
        if (!m_IsMovementBlocked && !m_InfoComponent.GetPlayerSettings().m_IsStatic)
        {
            int playerIndex = m_InfoComponent.GetPlayerIndex();
            m_HorizontalMoveInput = InputManager.GetHorizontalMovement(playerIndex);
            m_JumpInput = InputManager.GetJumpInput(playerIndex);
            m_CrouchInput = InputManager.GetCrouchInput(playerIndex);

            if(IsStanding())
            {
                if(m_JumpInput && !m_JumpTakeOffRequested && !m_TriggerJumpImpulse && m_Controller.CanJump())
                {
                    ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Movement, "Jump take off requested");

                    m_Animator.SetTrigger("TakeOff");
                    m_AttackComponent.SetAttackBlockedByTakeOff(true);
                    m_JumpTakeOffRequested = true;
                    m_JumpTakeOffDirection = m_HorizontalMoveInput;
                }
            }

            m_Animator.SetBool("IsCrouching", m_CrouchInput);
        }

        m_Animator.SetFloat("Speed", Mathf.Abs(m_HorizontalMoveInput));
    }

    public void UpdatePlayerSide()
    {
        // If we're not stunned AND not jumping AND not attacking
        if(!m_HealthComponent.GetStunInfoSubComponent().IsStunned() && !IsJumping() && m_AttackComponent.GetCurrentAttack() == null)
        {
            // If movement is not blocked OR blocked by time over => Then we can update player side
            if (!m_IsMovementBlocked || m_MovementBlockedReason == EBlockedReason.TimeOver)
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
    }

    void OnSideChanged()
    {
        m_Animator.SetBool("TurnAroundRequested", true);
    }

    void OnEndTurnAround(bool dummy)
    {
        m_IsLeftSide = !m_IsLeftSide;
        m_Controller.Flip();
        OnDirectionChanged();

        m_Animator.SetBool("TurnAroundRequested", false);
    }

    void OnTriggerJumpImpulse(bool dummy)
    {
        if(!m_IsMovementBlocked || m_MovementBlockedReason == EBlockedReason.TimeOver)
        {
            m_JumpTakeOffRequested = false;
            m_TriggerJumpImpulse = true;
            m_Animator.ResetTrigger("TakeOff");

            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Movement, "Jump impulse requested after take off");
        }
        else
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Movement, "Jump impulse can't be requested because movement has been blocked, reason : " + m_MovementBlockedReason);
        }
    }

    private void OnJumping(bool isJumping)
    {
        m_Animator.SetBool("IsJumping", isJumping);
        if(isJumping)
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Movement, "On Jumping");

            ChangePlayerStance(EPlayerStance.Jump, EJumpPhase.Air);
            m_AttackComponent.SetAttackBlockedByTakeOff(false);
        }
        else
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Movement, "On Landing");

            ChangePlayerStance(EPlayerStance.Stand, EJumpPhase.Landing);

            OnLanding?.Invoke();
        }
    }

    private void OnDirectionChanged()
    {
        m_Animator.SetFloat("MovingDirection", m_Animator.GetFloat("MovingDirection") * -1.0f);
    }

    public EMovingDirection GetMovingDirection()
    {
        return m_Controller.GetMovingDirection();
    }

    void FixedUpdate()
    {
        if(m_IsMovementBlocked && (m_MovementBlockedReason != EBlockedReason.TimeOver || !m_TriggerJumpImpulse))
        {
            return;
        }

        // Move our character
        m_Controller.Move(m_HorizontalMoveInput * Time.fixedDeltaTime, m_CrouchInput, m_TriggerJumpImpulse, m_JumpTakeOffDirection, m_JumpPhase);
        m_TriggerJumpImpulse = false;
    }

    public void ChangePlayerStance(EPlayerStance newStance, EJumpPhase newJumpPhase)
    {
        if(m_PlayerStance != newStance)
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Movement, "On player stance changed | " + string.Format("{0,-20} {1,-20}", ("Old stance : " + m_PlayerStance), "New stance : " + newStance));
            m_PlayerStance = newStance;
        }

        if(m_JumpPhase != newJumpPhase)
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Movement, "On player jump phase changed | " + string.Format("{0,-20} {1,-20}", ("Old phase : " + m_JumpPhase), "New phase : " + newJumpPhase));
            m_JumpPhase = newJumpPhase;
        }
    }

    public EPlayerStance GetCurrentStance()
    {
        EPlayerStance playerStance = EPlayerStance.Stand;
        if(IsCrouching())
        {
            playerStance = EPlayerStance.Crouch;
        }
        else if(IsJumping())
        {
            playerStance = EPlayerStance.Jump;
        }

        return playerStance;
    }

    public bool IsStanding()
    {
        return m_PlayerStance == EPlayerStance.Stand;
    }

    public bool IsCrouching()
    {
        return m_PlayerStance == EPlayerStance.Crouch && m_CrouchInput;
    }

    public bool IsJumping()
    {
        return m_PlayerStance == EPlayerStance.Jump && m_Controller.IsJumping();
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

    public bool IsFacingRight()
    {
        return m_Controller.IsFacingRight();
    }

    public void PushForward(float pushForce)
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Movement, "On push forward");
        m_Controller.PushForward(pushForce);
    }

    public void PushBack(float pushForce)
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Movement, "On push back");
        m_Controller.PushBack(pushForce);
    }

    public void SetMovementBlockedByAttack(bool isMovementBlockedByAttack)
    {
        SetMovementBlocked(isMovementBlockedByAttack, EBlockedReason.PlayAttack);

        if (!IsJumping())
        {
            OnStopMovement();
        }
    }

    private void OnStopMovement(bool dummyBool = true)
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Movement, "On movement stopped");
        m_Controller.StopMovement();
    }

    void EndOfAttack(EAnimationAttackName attackName)
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Movement, "On end of attack : " + attackName);
        if (m_IsMovementBlocked)
        {
            // We need to check m_AttackComponent.GetCurrentAttack().m_AnimationAttackName AND m_AttackComponent.GetCurrentAttackLogic().GetAnimationAttackName() because for ProjectileAttack, 
            // it can happen that a different animation is triggered due to guard crush property, so the animation attack name is changed at runtime
            if (m_AttackComponent.GetCurrentAttack() == null || 
                m_AttackComponent.GetCurrentAttack().m_AnimationAttackName == attackName ||
                m_AttackComponent.GetCurrentAttackLogic().GetAnimationAttackName() == attackName.ToString())
            {
                SetMovementBlocked(false, EBlockedReason.EndAttack);
            }
        }
    }

    void BlockMovement(EAnimationAttackName attackName)
    {
        if (m_IsMovementBlocked)
        {
            Debug.LogError("Movement was already blocked");
            return;
        }
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Movement, "Block movement requested by : " + attackName);
        SetMovementBlocked(true, EBlockedReason.PlayAttack);
    }

    void UnblockMovement(EAnimationAttackName attackName)
    {
        if (m_IsMovementBlocked == false)
        {
            Debug.LogError("Movement was not blocked");
            return;
        }
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Movement, "Unblock movement requested by : " + attackName);
        SetMovementBlocked(false, EBlockedReason.RequestByAttack);
    }

    void OnStunBegin(bool isStunned = true)
    {
        SetMovementBlocked(true, EBlockedReason.Stun);
    }

    void OnStunEnd(bool isStunned = false)
    {
        SetMovementBlocked(false, EBlockedReason.StunEnd);
    }

    void OnRoundOver()
    {
        SetMovementBlocked(true, EBlockedReason.TimeOver);
        m_Animator.SetBool("IsCrouching", false);
        RoundSubGameManager.OnRoundOver -= OnRoundOver;
    }

    void SetMovementBlocked(bool isMovementBlocked, EBlockedReason reason)
    {
        if(CanUpdateMovementBlockedStatus(isMovementBlocked, reason))
        {
            m_IsMovementBlocked = isMovementBlocked;
            m_MovementBlockedReason = reason;

            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Movement, "Movement has been "+ (isMovementBlocked ? "blocked" : "unblocked") + ", reason : " + reason);

            if (m_IsMovementBlocked)
            {
                m_Animator.ResetTrigger("TakeOff");
                m_JumpTakeOffRequested = false;
                m_JumpTakeOffDirection = 0f;
                m_TriggerJumpImpulse = false;
                m_AttackComponent.SetAttackBlockedByTakeOff(false);
                
                if(m_Controller.IsJumping() && reason == EBlockedReason.Stun)
                {
                    OnStopMovement();
                }
            }
        }
        else
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Movement, "/!\\ Movement has NOT been " + (isMovementBlocked ? "blocked" : "unblocked") + " by reason : " + reason);
        }
    }

    bool CanUpdateMovementBlockedStatus(bool isMovementBlocked, EBlockedReason reason)
    {
        //Block reasons : 
        // PlayAttack, RequestByAttack, Stun, TimeOver

        //Unblock reasons : 
        // EndAttack, RequestByAttack, StunEnd

        if (m_IsMovementBlocked)
        {
            if(m_MovementBlockedReason == EBlockedReason.TimeOver)
            {
                // We can't update the block status if the current block reason is TimeOver
                return false;
            }

            // If we need to unblock movement
            if (!isMovementBlocked)
            {
                switch (m_MovementBlockedReason)
                {
                    // If movement blocked reason is Play/RequestBy Attack, you can unblock the movement with EndAttack or RequestByAttack
                    case EBlockedReason.PlayAttack:
                    case EBlockedReason.RequestByAttack:
                        return reason == EBlockedReason.EndAttack || reason == EBlockedReason.RequestByAttack;

                    // If movement blocked reason is Stun, you can unblock the movement only with a StunEnd reason
                    case EBlockedReason.Stun:
                        return reason == EBlockedReason.StunEnd;

                    default:
                        string errorMsg = "Movement has been blocked with an invalid reason : " + m_MovementBlockedReason + " and trying to unblock with reason : " + reason;
                        Debug.LogError(errorMsg);
                        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Movement, "ERROR : " + errorMsg);
                        return true;
                }
            }
        }

        return true;
    }
}