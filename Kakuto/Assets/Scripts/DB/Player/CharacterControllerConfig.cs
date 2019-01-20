using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "CharacterControllerConfig", menuName = "Data/Player/CharacterControllerConfig", order = 0)]
public class CharacterControllerConfig : ScriptableObject {

    [Tooltip("The fastest the player can travel in the x axis.")]
    public float m_WalkSpeed = 10f;

    [Tooltip("Amount of force added when the player jumps.")]
    public float m_JumpForce = 400f;

    [Range(0, 1)]
    [Tooltip("Amount of maxSpeed applied to crouching movement. 1 = 100%")]
    public float m_CrouchSpeed = .36f;

    [Range(0, .3f)]
    [Tooltip("How much to smooth out the movement")]
    public float m_MovementSmoothing = .05f;

    [Tooltip("Whether or not a player can steer while jumping.")]
    public bool m_AirControl = false;

    [Tooltip("A mask determining what is ground to the character")]
    public LayerMask m_WhatIsGround;
}