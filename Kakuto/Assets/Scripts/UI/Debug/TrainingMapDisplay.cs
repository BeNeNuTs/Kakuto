using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingMapDisplay : MonoBehaviour
{
    public EPlayer m_Target;

    //Training display
    public Text m_TextInputs;
    public Text m_TextToDisplayInputs;
    public Text m_TextInputsAttack;
    public Text m_TextToDisplayInputsAttack;
    public Text m_TextAttacks;
    public Text m_TextToDisplayAttacks;
    private float m_DislayAttacksTimeStamp;
    private static readonly float s_DisplayAttacksTime = 2.0f;

    private PlayerMovementComponent m_PlayerMovementComponentToDisplay;
    private PlayerAttackComponent m_PlayerAttackComponentToDisplay;
    private PlayerInfoComponent m_PlayerInfoComponentToDisplay;
    private PlayerAttack m_CurrentDisplayAttack;

    private List<GameInput> m_AttackInputs = new List<GameInput>();
    private List<GameInput> m_TriggeredInputs = new List<GameInput>();

    void Awake()
    {
        m_PlayerMovementComponentToDisplay = Utils.FindComponentMatchingWithTag<PlayerMovementComponent>(m_Target.ToString());
        m_PlayerAttackComponentToDisplay = Utils.FindComponentMatchingWithTag<PlayerAttackComponent>(m_Target.ToString());
        m_PlayerInfoComponentToDisplay = Utils.FindComponentMatchingWithTag<PlayerInfoComponent>(m_Target.ToString());
    }

    void LateUpdate()
    {
        bool displayInputsInfo = ScenesConfig.GetDebugSettings().m_DisplayInputsInfo;
        m_TextInputs.enabled = displayInputsInfo;
        m_TextToDisplayInputs.enabled = displayInputsInfo;
        m_TextInputsAttack.enabled = displayInputsInfo;
        m_TextToDisplayInputsAttack.enabled = displayInputsInfo;
        if (displayInputsInfo)
        {
            DisplayInputs();
        }

        bool displayAttacksInfo = ScenesConfig.GetDebugSettings().m_DisplayAttacksInfo;
        m_TextAttacks.enabled = displayAttacksInfo;
        m_TextToDisplayAttacks.enabled = displayAttacksInfo;
        if (displayAttacksInfo)
        {
            DisplayAttack();
        }
    }

    void DisplayInputs()
    {
        if (m_TextToDisplayInputs != null)
        {
            InputManager.GetAttackInputList(m_PlayerInfoComponentToDisplay.GetPlayerIndex(), m_PlayerMovementComponentToDisplay.IsLeftSide(), ref m_AttackInputs);
            m_TriggeredInputs.AddRange(m_AttackInputs);
            if (m_TriggeredInputs.Count > AttackConfig.Instance.m_MaxInputs)
            {
                m_TriggeredInputs.RemoveRange(0, (int)(m_TriggeredInputs.Count - AttackConfig.Instance.m_MaxInputs));
            }

            m_TextToDisplayInputs.text = string.Empty;
            foreach (GameInput input in m_TriggeredInputs)
            {
                m_TextToDisplayInputs.text += input.GetInputString();
            }
        }

        if (m_TextToDisplayInputsAttack != null)
        {
            m_TextToDisplayInputsAttack.text = m_PlayerAttackComponentToDisplay.GetTriggeredInputsString();
        }
    }

    void DisplayAttack()
    {
        if (m_TextToDisplayAttacks != null)
        {
            if (m_PlayerAttackComponentToDisplay.GetCurrentAttack() != null && m_PlayerAttackComponentToDisplay.GetCurrentAttack() != m_CurrentDisplayAttack)
            {
                m_TextToDisplayAttacks.text = m_PlayerAttackComponentToDisplay.GetCurrentAttack().m_Name + " launched !";
                m_DislayAttacksTimeStamp = Time.unscaledTime;
                m_CurrentDisplayAttack = m_PlayerAttackComponentToDisplay.GetCurrentAttack();
            }
            else if (Time.unscaledTime > m_DislayAttacksTimeStamp + s_DisplayAttacksTime)
            {
                ResetDisplayAttack();
            }
        }
    }

    void ResetDisplayAttack()
    {
        if (m_TextToDisplayAttacks != null)
        {
            m_TextToDisplayAttacks.text = "";
            m_DislayAttacksTimeStamp = 0.0f;
            m_CurrentDisplayAttack = null;
        }
    }
}
