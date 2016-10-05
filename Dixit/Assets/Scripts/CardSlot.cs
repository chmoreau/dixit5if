using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public class CardSlot : MonoBehaviour {
    [SerializeField]
    private Transform m_FaceUpAnchor = null;
    [SerializeField]
    private Transform m_FaceDownAnchor = null;
    [SerializeField]
    private Card m_Card;
    [SerializeField]
    private Button m_Clickable = null;
    
    public Transform FaceUpAnchor { get { return m_FaceUpAnchor; } }
    public Transform FaceDownAnchor { get { return m_FaceDownAnchor; } }
    public Card Card { get { return m_Card; } set { m_Card = value; } }
    public Button Clickable { get { return m_Clickable; } }
}
