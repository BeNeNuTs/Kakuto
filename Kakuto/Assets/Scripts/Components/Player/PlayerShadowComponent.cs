using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerShadowComponent : MonoBehaviour
{
    public SpriteRenderer m_PlayerSpriteRenderer;
    public SpriteRenderer m_ShadowSpriteRenderer;
    public AnimationCurve m_AlphaMultiplierFromYDelta;
    
    private float m_InitialShadowAlphaValue;

    private float m_InitialRootYPosition;
    private float m_InitialPlayerSpriteYPosition;

    public bool m_UpdatePosition = true;
    public bool m_UpdateSize = true;

    void Awake()
    {
        m_InitialShadowAlphaValue = m_ShadowSpriteRenderer.color.a;

        m_InitialRootYPosition = transform.root.position.y;
        m_InitialPlayerSpriteYPosition = GetCroppedPlayerSprite().center.y;
    }

    void OnEnable()
    {
        Update();
        m_ShadowSpriteRenderer.enabled = true;
    }

    void OnDisable()
    {
        m_ShadowSpriteRenderer.enabled = false;
    }

    void Update()
    {
        Rect croppedPlayerSprite = GetCroppedPlayerSprite();
        UpdateTransparency(croppedPlayerSprite);
        if(m_UpdatePosition)
        {
            UpdatePosition(croppedPlayerSprite);
        }
        if(m_UpdateSize)
        {
            UpdateSize(croppedPlayerSprite);
        }
    }

    void UpdateTransparency(Rect croppedPlayerSprite)
    {
        float YDelta = croppedPlayerSprite.center.y - m_InitialPlayerSpriteYPosition;
        Color shadowColor = m_ShadowSpriteRenderer.color;
        shadowColor.a = m_InitialShadowAlphaValue * m_AlphaMultiplierFromYDelta.Evaluate(YDelta);
        m_ShadowSpriteRenderer.color = shadowColor;
    }

    void UpdatePosition(Rect croppedPlayerSprite)
    {
        transform.position = new Vector3(croppedPlayerSprite.center.x, m_InitialRootYPosition);
    }

    void UpdateSize(Rect croppedPlayerSprite)
    {
        float playerSpriteWidth = (croppedPlayerSprite.max.x - croppedPlayerSprite.min.x);
        transform.localScale = new Vector3(playerSpriteWidth, transform.lossyScale.y, transform.lossyScale.z);
    }

    Rect GetCroppedPlayerSprite()
    {
        Sprite playerSprite = m_PlayerSpriteRenderer.sprite;
        Rect playerSpriteRect = playerSprite.rect;
        float pixelPerUnit = playerSprite.pixelsPerUnit;

        Rect croppedRect = new Rect(
            (playerSprite.textureRectOffset.x - playerSpriteRect.width / 2f) / pixelPerUnit,
            (playerSprite.textureRectOffset.y - playerSpriteRect.height / 2f) / pixelPerUnit,
            playerSprite.textureRect.width / pixelPerUnit,
            playerSprite.textureRect.height / pixelPerUnit);

        croppedRect.center = m_PlayerSpriteRenderer.transform.TransformPoint(croppedRect.center);
        return croppedRect;
    }
}
