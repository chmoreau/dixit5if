using UnityEngine;
using System;

[Serializable]
public class CardModel {
    [SerializeField]
    private string m_CardId;
    [SerializeField]
    private Texture2D m_CardImage;

    public string CardId { get { return m_CardId; } set { m_CardId = value; } }
    public Texture2D CardImage { get { return m_CardImage; } set { m_CardImage = value; } }
}
