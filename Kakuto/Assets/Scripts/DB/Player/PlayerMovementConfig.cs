using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "Data/Player/PlayerMovementConfig", order = 0)]
public class PlayerMovementConfig : ScriptableObject {

    [Tooltip("The fastest the player can travel in the x axis.")]
    public float m_MaxSpeed = 10f;

    [Tooltip("Amount of force added when the player jumps.")]
    public float m_JumpForce = 400f;

    [Tooltip("Whether or not a player can steer while jumping.")]
    public bool m_AirControl = false;
}