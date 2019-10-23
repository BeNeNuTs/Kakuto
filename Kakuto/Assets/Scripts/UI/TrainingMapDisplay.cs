using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingMapDisplay : MonoBehaviour
{
    public EPlayer m_Target;

    //Training display
    private Text m_TextToDisplayInputs;
    private Text m_TextToDisplayAttacks;
    private float m_DislayAttacksTimeStamp;
    private static readonly float s_DisplayAttacksTime = 2.0f;

    private PlayerMovementComponent m_PlayerMovementComponentToDisplay;
    private PlayerAttackComponent m_PlayerAttackComponentToDisplay;
    private PlayerAttack m_CurrentDisplayAttack;

    private string m_TriggeredInputs;

    void Awake()
    {
        //Training display
        GameObject inputDisplayer = GameObject.FindGameObjectWithTag("InputDisplayer");
        if (inputDisplayer)
        {
            m_TextToDisplayInputs = inputDisplayer.GetComponent<Text>();
        }

        GameObject attackDisplayer = GameObject.FindGameObjectWithTag("AttackDisplayer");
        if (attackDisplayer)
        {
            m_TextToDisplayAttacks = attackDisplayer.GetComponent<Text>();
        }

        m_PlayerMovementComponentToDisplay = Utils.FindComponentMatchingWithTag<PlayerMovementComponent>(m_Target.ToString());
        m_PlayerAttackComponentToDisplay = Utils.FindComponentMatchingWithTag<PlayerAttackComponent>(m_Target.ToString());
    }

    void LateUpdate()
    {
        //Training display
        DisplayInputs();
        DisplayAttack();
    }

    void DisplayInputs()
    {
        if (m_TextToDisplayInputs != null)
        {
            m_TriggeredInputs += InputManager.GetAttackInputString(m_PlayerMovementComponentToDisplay.GetPlayerIndex(), m_PlayerMovementComponentToDisplay.IsLeftSide());
            if (m_TriggeredInputs.Length > AttackConfig.Instance.m_MaxInputs)
            {
                m_TriggeredInputs = m_TriggeredInputs.Remove(0, (int)(m_TriggeredInputs.Length - AttackConfig.Instance.m_MaxInputs));
            }

            m_TextToDisplayInputs.text = m_TriggeredInputs;
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
