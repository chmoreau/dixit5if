using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DeckDatabase", menuName = "DeckDatabase")]
public class DeckDatabase : ScriptableObject {
    [SerializeField]
    private CardModel[] m_AllCards = new CardModel[0];
    [SerializeField]
    private List<DeckModel> m_DeckList = new List<DeckModel>();
}
