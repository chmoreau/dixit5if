using UnityEngine;
using System;

[Serializable]
public class InGameCardModel
{
    [SerializeField]
    private string m_CardId;
    [SerializeField]
    private string m_OwnerId;
    [SerializeField]
    private bool m_IsThemeCard;

    public string CardId { get { return m_CardId; } set { m_CardId = value; } }
    public string OwnerId { get { return m_OwnerId; } set { m_OwnerId = value; } }
    public bool IsThemeCard { get { return m_IsThemeCard; } set { m_IsThemeCard = value; } }

    public InGameCardModel()
    {
        m_CardId = null;
        m_OwnerId = null;
        m_IsThemeCard = false;
    }
}
