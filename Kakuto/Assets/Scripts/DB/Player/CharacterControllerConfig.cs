using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "CharacterControllerConfig", menuName = "Data/Player/CharacterControllerConfig", order = 0)]
public class CharacterControllerConfig : ScriptableObject {

    [Tooltip("The fastest the player can travel in the x axis when walking forward.")]
    public float m_WalkForwardSpeed = 10f;

    [Tooltip("The fastest the player can travel in the x axis when walking backward.")]
    public float m_WalkBackwardSpeed = 10f;

    [Tooltip("Amount of force added when the player jumps on place.")]
    public float m_JumpOnPlaceForce = 500f;

    [Tooltip("Amount of force added when the player jumps backward.")]
    public float m_JumpBackwardForce = 500f;

    [Tooltip("Angle of the jump when player jump backward")]
    public float m_JumpBackwardAngle = -10f;

    [Tooltip("Amount of force added when the player jumps forward.")]
    public float m_JumpForwardForce = 500f;

    [Tooltip("Angle of the jump when player jump forward")]
    public float m_JumpForwardAngle = 10f;

    [Tooltip("Time between 2 jumps (in sec).")]
    public float m_TimeBetweenJumps = .1f;

    [Range(0, 1)]
    [Tooltip("Amount of maxSpeed applied to crouching movement. 1 = 100%")]
    public float m_CrouchSpeed = .36f;

    [Range(0, .3f)]
    [Tooltip("How much to smooth out the movement")]
    public float m_MovementSmoothing = .05f;

    [Tooltip("Whether or not a player can steer while jumping.")]
    public bool m_AirControl = false;
}