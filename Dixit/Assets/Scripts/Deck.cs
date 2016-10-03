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
        IEnumerator drawCoroutine = DrawCardToHand(cardId, handIndex);
        StartCoroutine(drawCoroutine);
    }

    IEnumerator DrawCardToHand(string cardId, int handIndex)
    {
        Card card = Instantiate(m_CardPrefab, m_CardSpawnPoint.position, m_CardSpawnPoint.rotation) as Card;        
        float timer = m_DrawCardDuration;
        while (timer >= 0)
        {
            timer -= Time.deltaTime;
            float process = (m_DrawCardDuration - timer) / m_DrawCardDuration;
            card.transform.position = Vector3.Lerp(m_CardSpawnPoint.position, m_Hand.HandAnchors[handIndex].HandAnchorPoint.position, process);
            card.transform.rotation = Quaternion.Slerp(m_CardSpawnPoint.rotation, m_Hand.HandAnchors[handIndex].HandAnchorPoint.rotation, process);
            yield return new WaitForEndOfFrame();
        }
        m_Hand.HandAnchors[handIndex].HandCard = card;
    }
}
