using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class PlayerMovementComponent : MonoBehaviour
{
    private CharacterController2D m_Controller;
    private Animator m_Animator;

    private float m_HorizontalMoveInput = 0f;
    private bool m_JumpInput = false;
    private bool m_CrouchInput = false;

    private bool m_IsJumping = false;
    private bool m_IsCrouching = false;

    private bool m_IsMovementBlocked = false;

    void Awake()
    {
        m_Controller = GetComponent<CharacterController2D>();
        m_Animator = GetComponentInChildren<Animator>();

        RegisterListeners();
    }

    void RegisterListeners()
    {
        Utils.GetPlayerEventManager<string>(gameObject).StartListening(EPlayerEvent.EndOfAttack, EndOfAttack);
        Utils.GetPlayerEventManager<string>(gameObject).StartListening(EPlayerEvent.UnblockMovement, UnblockMovement);

        Utils.GetPlayerEventManager<float>(gameObject).StartListening(EPlayerEvent.StunBegin, OnStunBegin);
        Utils.GetPlayerEventManager<float>(gameObject).StartListening(EPlayerEvent.StunEnd, OnStunEnd);
    }

    void OnDestroy()
    {
        UnregisterListeners();
    }

    void UnregisterListeners()
    {
        Utils.GetPlayerEventManager<string>(gameObject).StopListening(EPlayerEvent.EndOfAttack, EndOfAttack);
        Utils.GetPlayerEventManager<string>(gameObject).StopListening(EPlayerEvent.UnblockMovement, UnblockMovement);

        Utils.GetPlayerEventManager<float>(gameObject).StopListening(EPlayerEvent.StunBegin, OnStunBegin);
        Utils.GetPlayerEventManager<float>(gameObject).StopListening(EPlayerEvent.StunEnd, OnStunEnd);
    }

    // Update is called once per frame
    void Update()
    {
        if(m_IsMovementBlocked)
        {
            return;
        }

        m_HorizontalMoveInput = Input.GetAxisRaw("Horizontal");

        m_Animator.SetFloat("Speed", Mathf.Abs(m_HorizontalMoveInput));

        if (Input.GetKeyDown("up"))
        {
            m_JumpInput = true;
        }

        m_CrouchInput = Input.GetKey("down");
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
        m_JumpInput = false;
    }

    public bool IsJumping()
    {
        return m_IsJumping;
    }

    public bool IsCrouching()
    {
        return m_IsCrouching;
    }

    public float GetHorizontalMoveInput()
    {
        return m_HorizontalMoveInput;
    }

    public void SetMovementBlockedByAttack(bool isMovementBlockedByAttack)
    {
        m_IsMovementBlocked = isMovementBlockedByAttack;
    }

    void EndOfAttack(string attackName)
    {
        if(m_IsMovementBlocked)
        {
            m_IsMovementBlocked = false;
        }
    }

    void UnblockMovement(string attackName)
    {
        if (m_IsMovementBlocked == false)
        {
            Debug.LogError("Movement was not blocked");
            return;
        }
        m_IsMovementBlocked = false;
    }

    void OnStunBegin(float stunTimeStamp)
    {
        m_IsMovementBlocked = true;
    }

    void OnStunEnd(float stunTimeStamp)
    {
        m_IsMovementBlocked = false;
    }
}