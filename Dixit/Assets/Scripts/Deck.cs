using UnityEngine;
using System;
using System.Collections;

public class Deck : MonoBehaviour {
    [SerializeField]
    private DeckDatabase m_DeckDatabase = null;
    [SerializeField]
    private Card m_CardPrefab = null;
    [SerializeField]
    private Transform m_CardSpawnPoint = null;
    [SerializeField]
    private float m_DrawCardDuration = 1.0f;
    [Header("Test")]
    [SerializeField]
    private Hand m_Hand = null;

    void Start()
    {
        //Draw(string.Empty);
    }

	public void Draw(string cardId, int handIndex)
    {
        Card card = Instantiate(m_CardPrefab, m_CardSpawnPoint.position, m_CardSpawnPoint.rotation) as Card;
        TransformAnimation.AnimationCallback onDrawEnd = () => 
        {
            m_Hand.CardSlots[handIndex].Card = card;
            m_Hand.CardSlots[handIndex].Card.transform.SetParent(m_Hand.CardSlots[handIndex].transform);
        };
        IEnumerator drawCoroutine = TransformAnimation.FromToAnimation(card.gameObject, m_CardSpawnPoint, m_Hand.CardSlots[handIndex].FaceUpAnchor, m_DrawCardDuration, null, onDrawEnd);
        StartCoroutine(drawCoroutine);
    }
}
