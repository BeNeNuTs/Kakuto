using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class PlayerAttackEditor : EditorWindow
{
    public PlayerAttackConfig m_PlayerAttackConfig;
    private int m_ViewIndex = 1;

    private static readonly string m_ObjectPath = "Assets/Data/Player/Attack";

    [MenuItem("Window/Player Attack Editor %#e")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(PlayerAttackEditor));
    }

    void OnEnable()
    {
        if (EditorPrefs.HasKey(m_ObjectPath))
        {
            string objectPath = EditorPrefs.GetString(m_ObjectPath);
            m_PlayerAttackConfig = AssetDatabase.LoadAssetAtPath(objectPath, typeof(PlayerAttackConfig)) as PlayerAttackConfig;
        }
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Player Attack Editor", EditorStyles.boldLabel);
        if (m_PlayerAttackConfig != null)
        {
            if (GUILayout.Button("Show Attack List"))
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = m_PlayerAttackConfig;
            }
        }
        if (GUILayout.Button("Open Attack List"))
        {
            OpenItemList();
        }
        if (GUILayout.Button("New Attack List"))
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = m_PlayerAttackConfig;
        }
        GUILayout.EndHorizontal();

        if (m_PlayerAttackConfig == null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("Create New Attack List", GUILayout.ExpandWidth(false)))
            {
                CreateNewItemList();
            }
            if (GUILayout.Button("Open Existing Attack List", GUILayout.ExpandWidth(false)))
            {
                OpenItemList();
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(20);

        if (m_PlayerAttackConfig != null)
        {
            m_PlayerAttackConfig.name = EditorGUILayout.TextField("Config Name", m_PlayerAttackConfig.name as string);

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Space(10);

            if (GUILayout.Button("Prev", GUILayout.ExpandWidth(false)))
            {
                if (m_ViewIndex > 1)
                    m_ViewIndex--;
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Next", GUILayout.ExpandWidth(false)))
            {
                if (m_ViewIndex < m_PlayerAttackConfig.m_AttackList.Count)
                {
                    m_ViewIndex++;
                }
            }

            GUILayout.Space(60);

            if (GUILayout.Button("Add Attack", GUILayout.ExpandWidth(false)))
            {
                AddItem();
            }
            if (GUILayout.Button("Delete Attack", GUILayout.ExpandWidth(false)))
            {
                DeleteItem(m_ViewIndex - 1);
            }

            GUILayout.EndHorizontal();
            if (m_PlayerAttackConfig.m_AttackList == null)
                Debug.LogError("wtf");
            if (m_PlayerAttackConfig.m_AttackList.Count > 0)
            {
                GUILayout.BeginHorizontal();
                m_ViewIndex = Mathf.Clamp(EditorGUILayout.IntField("Current Attack", m_ViewIndex, GUILayout.ExpandWidth(false)), 1, m_PlayerAttackConfig.m_AttackList.Count);
                EditorGUILayout.LabelField("of   " + m_PlayerAttackConfig.m_AttackList.Count.ToString() + "  attacks", "", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                m_PlayerAttackConfig.m_AttackList[m_ViewIndex - 1].m_Name = EditorGUILayout.TextField("Attack name", m_PlayerAttackConfig.m_AttackList[m_ViewIndex - 1].m_Name as string);
                m_PlayerAttackConfig.m_AttackList[m_ViewIndex - 1].m_ShouldBeCrouched = (bool)EditorGUILayout.Toggle("Attack crouch", m_PlayerAttackConfig.m_AttackList[m_ViewIndex - 1].m_ShouldBeCrouched, GUILayout.ExpandWidth(false));
                //m_PlayerAttackConfig.m_AttackList[m_ViewIndex - 1].m_InputList = EditorGUILayout.ObjectField("Attack inputs", m_PlayerAttackConfig.m_AttackList[m_ViewIndex - 1].m_AttackInput, typeof(List<KeyCode>), false) as List<KeyCode>;
                m_PlayerAttackConfig.m_AttackList[m_ViewIndex - 1].m_Damage = EditorGUILayout.IntField("Attack damage", m_PlayerAttackConfig.m_AttackList[m_ViewIndex - 1].m_Damage);

                GUILayout.Space(10);
            }
            else
            {
                GUILayout.Label("This Attack List is Empty.");
            }
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(m_PlayerAttackConfig);
        }
    }

    void CreateNewItemList()
    {
        // There is no overwrite protection here!
        // There is No "Are you sure you want to overwrite your existing object?" if it exists.
        // This should probably get a string from the user to create a new name and pass it ...
        m_ViewIndex = 1;
        m_PlayerAttackConfig = CreatePlayerAttackConfig.Create();
        if (m_PlayerAttackConfig)
        {
            m_PlayerAttackConfig.m_AttackList = new List<PlayerAttack>();
            string relPath = AssetDatabase.GetAssetPath(m_PlayerAttackConfig);
            EditorPrefs.SetString(m_ObjectPath, relPath);
        }
    }

    void OpenItemList()
    {
        string absPath = EditorUtility.OpenFilePanel("Select Player Attack List", "", "");
        if (absPath.StartsWith(Application.dataPath))
        {
            string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
            m_PlayerAttackConfig = AssetDatabase.LoadAssetAtPath(relPath, typeof(PlayerAttackConfig)) as PlayerAttackConfig;
            if (m_PlayerAttackConfig.m_AttackList == null)
                m_PlayerAttackConfig.m_AttackList = new List<PlayerAttack>();
            if (m_PlayerAttackConfig)
            {
                EditorPrefs.SetString(m_ObjectPath, relPath);
            }
        }
    }

    void AddItem()
    {
        PlayerAttack newAttack = new PlayerAttack();
        newAttack.m_Name = "New Attack";
        m_PlayerAttackConfig.m_AttackList.Add(newAttack);
        m_ViewIndex = m_PlayerAttackConfig.m_AttackList.Count;
    }

    void DeleteItem(int index)
    {
        m_PlayerAttackConfig.m_AttackList.RemoveAt(index);
    }
}