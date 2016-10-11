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

	public void DrawCard(string cardId, CardSlot cardSlot)
    {
        Card card = FetchAndInstantiateCard(m_CardSpawnPoint, cardId);
        TransformAnimation.AnimationCallback onDrawEnd = () => 
        {
            cardSlot.Card = card;
            cardSlot.Card.transform.SetParent(cardSlot.transform);
        };
        IEnumerator drawCoroutine = TransformAnimation.FromToAnimation(card.gameObject, m_CardSpawnPoint, cardSlot.FaceUpAnchor, Vector3.zero, Vector3.zero, m_DrawCardDuration, null, onDrawEnd);
        StartCoroutine(drawCoroutine);
    }

    public Card FetchAndInstantiateCard(Transform anchor, string cardId = null)
    {
        Card card = Instantiate(m_CardPrefab, anchor.position, anchor.rotation) as Card;
        CardModel model = m_DeckDatabase.FetchCardModel(cardId);
        card.LoadModel(model);
        return card;
    }

    public CardModel FetchCardModel(string cardId)
    {        
        CardModel model = m_DeckDatabase.FetchCardModel(cardId);
        return model;
    }
}
