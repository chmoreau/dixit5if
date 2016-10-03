using UnityEngine;
using System;
using System.Collections;

public class Hand : MonoBehaviour {
    [Serializable]
    private struct HandAnchor
    {
        [SerializeField]
        private Card m_HandCard;
        [SerializeField]
        private Transform m_HandAnchorPoint;

        public Card HandCard { get { return m_HandCard; } set { m_HandCard = value; } }
        public Transform HandAnchorPoint { get { return m_HandAnchorPoint; } set { m_HandAnchorPoint = value; } }
    }

    [SerializeField]
    private HandAnchor[] m_HandAnchors = new HandAnchor[6];
}
