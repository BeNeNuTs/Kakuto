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

    void Awake()
    {
        m_Controller = GetComponent<CharacterController2D>();
        m_Animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        m_HorizontalMoveInput = Input.GetAxisRaw("Horizontal");

        m_Animator.SetFloat("Speed", Mathf.Abs(m_HorizontalMoveInput));

        if (Input.GetKeyDown("up"))
        {
            m_JumpInput = true;
        }

        if (Input.GetKeyDown("down"))
        {
            m_CrouchInput = true;
        }
        else if (Input.GetKeyUp("down"))
        {
            m_CrouchInput = false;
        }

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

    public void OnFlipping()
    {
        m_Animator.SetFloat("WalkingSpeed", m_Animator.GetFloat("WalkingSpeed") * -1.0f);
    }

    void FixedUpdate()
    {
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
}