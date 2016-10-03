using UnityEngine;
using System;

[Serializable]
public class DeckModel {
    [SerializeField]
    private string m_DeckId;
    [SerializeField]
    private string[] m_CardIds;

    public string DeckId { get { return m_DeckId; } set { m_DeckId = value; } }
    public string[] CardIds { get { return m_CardIds; } set { m_CardIds = value; } }
}
