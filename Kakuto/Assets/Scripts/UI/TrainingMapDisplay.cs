using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingMapDisplay : MonoBehaviour
{
    public PlayerAttackComponent m_PlayerAttackToDisplay;

    //Training display
    private Text m_TextToDisplayInputs;
    private Text m_TextToDisplayAttacks;
    private float m_DislayAttacksTimeStamp;
    private static readonly float s_DisplayAttacksTime = 2.0f;

    private PlayerAttack m_CurrentDisplayAttack;

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
            m_TextToDisplayInputs.text = m_PlayerAttackToDisplay.GetTriggeredInputString();
        }
    }

    void DisplayAttack()
    {
        if (m_TextToDisplayAttacks != null)
        {
            if (m_PlayerAttackToDisplay.GetCurrentAttack() != null && m_PlayerAttackToDisplay.GetCurrentAttack() != m_CurrentDisplayAttack)
            {
                m_TextToDisplayAttacks.text = m_PlayerAttackToDisplay.GetCurrentAttack().m_Name + " launched !";
                m_DislayAttacksTimeStamp = Time.unscaledTime;
                m_CurrentDisplayAttack = m_PlayerAttackToDisplay.GetCurrentAttack();
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
