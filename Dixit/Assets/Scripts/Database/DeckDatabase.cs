using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DeckDatabase", menuName = "DeckDatabase")]
public class DeckDatabase : ScriptableObject {
    [SerializeField]
    private CardModel m_DefaultCard = null;
    [SerializeField]
    private CardModel[] m_AllCards = new CardModel[0];
    [SerializeField]
    private List<DeckModel> m_DeckList = new List<DeckModel>();
    
    public CardModel DefaultCard { get { return m_DefaultCard; } }

    private Dictionary<string, CardModel> m_CardDictionary = new Dictionary<string, CardModel>();

    void OnEnable()
    {
        m_CardDictionary[m_DefaultCard.CardId] = m_DefaultCard;
        foreach (CardModel card in m_AllCards)
        {
            Debug.Log(card.CardId);
            if (m_CardDictionary.ContainsKey(card.CardId))
            {
                Debug.LogError("Duplicated key found in Deck database : " + card.CardId + ", value will be ignored.");
            } else
            {
                m_CardDictionary[card.CardId] = card;
            }
        }
    }

    public CardModel FetchCardModel(string cardId)
    {
        if (cardId != null && m_CardDictionary.ContainsKey(cardId))
        {
            return m_CardDictionary[cardId];
        }
        else
        {
            return m_DefaultCard;
        }
    }
}
