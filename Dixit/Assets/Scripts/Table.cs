using UnityEngine;
using System;
using System.Collections;

public class Table : MonoBehaviour {
    [SerializeField]
    private RectTransform m_CardSlotPanel = null;
    [SerializeField]
    private CardSlot m_CardSlotPrefab = null;
    //[SerializeField]
    //private Transform m_CardSpawnPoint = null;
    [SerializeField]
    private float m_DrawCardDuration = 1.0f;
    [Header("Test")]
    //[SerializeField]
    //private Hand m_Hand = null;
    
    private CardSlot[] m_CardSlots = new CardSlot[0];
    //public CardSlot[] CardSlots { get { return m_CardSlots; } }
    private int m_SlotPointer = 0;

    void Start()
    {
        Init(6);
    }

    public void Init(int slotNumber)
    {
        m_CardSlots = new CardSlot[slotNumber];
        for (int i = 0; i < slotNumber; i++)
        {
            m_CardSlots[i] = Instantiate(m_CardSlotPrefab, m_CardSlotPanel, false) as CardSlot;
        }
        m_SlotPointer = 0;
    }

    public void Reset()
    {

    }

    public CardSlot AllocateSlot()
    {
        if (m_SlotPointer >= 0 && m_SlotPointer < m_CardSlots.Length)
        {
            return m_CardSlots[m_SlotPointer++];
        }
        else
        {
            return null;
        }
    }
}
