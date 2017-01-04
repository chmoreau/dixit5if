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
    
    public void Init()
    {

    }

    public void Reset()
    {
        StopAllCoroutines();
    }

	public void DrawCard(string cardId, CardSlot cardSlot)
    {
        Card card = FetchAndInstantiateCard(cardId, null, m_CardSpawnPoint); // GameSessionService.CurrentGameSession.LocalPlayer.UserId
        TransformAnimation.AnimationCallback onDrawStart = () => 
        {
            cardSlot.Card = card;
            cardSlot.Card.transform.SetParent(cardSlot.transform);
        };
        IEnumerator drawCoroutine = TransformAnimation.FromToAnimation(card.gameObject, m_CardSpawnPoint, cardSlot.FaceUpAnchor, Vector3.zero, Vector3.zero, m_DrawCardDuration, onDrawStart, null);
        StartCoroutine(drawCoroutine);
    }

    public Card FetchAndInstantiateCard(string cardId, string ownerId, Transform anchor)
    {
        Card card = Instantiate(m_CardPrefab, anchor.position, anchor.rotation) as Card;
        card.SetOwner(ownerId);
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
