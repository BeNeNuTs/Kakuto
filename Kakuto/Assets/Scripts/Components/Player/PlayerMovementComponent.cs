﻿using System.Collections;
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
    private PlayerAttackComponent m_PlayerAttackComponent;

    private Transform m_Enemy;
    private bool m_IsLeftSide;
    private int m_PlayerIndex;

    private float m_HorizontalMoveInput = 0f;
    private bool m_JumpInput = false;
    private bool m_CrouchInput = false;

    private bool m_IsJumping = false;
    private bool m_IsCrouching = false;

    private bool m_IsMovementBlocked = false;
    private EBlockedReason m_MovementBlockedReason = EBlockedReason.None;

    [Header("Debug")]
    [Space]

    public bool m_DEBUG_IsStatic = false;

    void Awake()
    {
        m_Controller = GetComponent<CharacterController2D>();
        m_Animator = GetComponentInChildren<Animator>();
        m_PlayerAttackComponent = GetComponent<PlayerAttackComponent>();

        m_Enemy = GameObject.FindGameObjectWithTag(Utils.GetEnemyTag(gameObject)).transform.root;
        m_PlayerIndex = gameObject.CompareTag("Player1") ? 0 : 1;
        m_IsLeftSide = (m_PlayerIndex == 0) ? true : false;

        RegisterListeners();
    }

    void RegisterListeners()
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StartListening(EPlayerEvent.EndOfAttack, EndOfAttack);
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StartListening(EPlayerEvent.UnblockMovement, UnblockMovement);

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
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StopListening(EPlayerEvent.UnblockMovement, UnblockMovement);

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

                m_JumpInput = InputManager.GetJumpInput(m_PlayerIndex);
                m_CrouchInput = InputManager.GetCrouchInput(m_PlayerIndex);
            }
        }
    }

    void UpdatePlayerSide()
    {
        // If movement is not blocked AND we're not jumping AND we're not attacking => Then we can update player side
        if(!m_IsMovementBlocked && !IsJumping() && m_PlayerAttackComponent.GetCurrentAttack() == null)
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
        m_IsJumping = isJumping;
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
        return m_IsJumping;
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
        m_IsMovementBlocked = isMovementBlockedByAttack;
        m_MovementBlockedReason = EBlockedReason.PlayAttack;

        if (!IsJumping())
        {
            m_Controller.StopMovement();
        }
    }

    void EndOfAttack(EAnimationAttackName attackName)
    {
        if(m_IsMovementBlocked)
        {
            m_IsMovementBlocked = false;
            m_MovementBlockedReason = EBlockedReason.None;
        }
    }

    void UnblockMovement(EAnimationAttackName attackName)
    {
        if (m_IsMovementBlocked == false)
        {
            Debug.LogError("Movement was not blocked");
            return;
        }
        m_IsMovementBlocked = false;
        m_MovementBlockedReason = EBlockedReason.None;
    }

    void OnStunBegin(float stunTimeStamp)
    {
        m_IsMovementBlocked = true;
        m_MovementBlockedReason = EBlockedReason.Stun;
    }

    void OnStunEnd(float stunTimeStamp)
    {
        m_IsMovementBlocked = false;
        m_MovementBlockedReason = EBlockedReason.None;
    }
}