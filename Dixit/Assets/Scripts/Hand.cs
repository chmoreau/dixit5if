using UnityEngine;
using System;
using System.Collections;

public class Hand : MonoBehaviour {
    [Serializable]
    public struct HandAnchor
    {
        [SerializeField]
        private Card m_HandCard;
        [SerializeField]
        private Transform m_HandAnchorPoint;

        public Card HandCard { get { return m_HandCard; } set { m_HandCard = value; } }
        public Transform HandAnchorPoint { get { return m_HandAnchorPoint; } set { m_HandAnchorPoint = value; } }
    }

    [SerializeField]
    private CardSlot[] m_CardSlots = new CardSlot[6];
    public CardSlot[] CardSlots { get { return m_CardSlots; } }
    [SerializeField]
    private Transform m_ZoomTargetPoint = null;
    [SerializeField]
    private float m_ZoomDuration = 0.5f;
    [SerializeField]
    private float m_PlayDuration = 0.5f;

    [Header("Test")]
    [SerializeField]
    private Deck m_Deck = null;
    [SerializeField]
    private Table m_Table = null;

    private int m_SelectedCardIndex = -1;
    private bool m_IsInteractable = false;
    private IEnumerator m_ZoomCoroutine = null;

    void Start()
    {
        Init();
        StartCoroutine("DrawInitHand");
    }

    public void Init()
    {
        m_IsInteractable = true;
    }

    public void Reset()
    {
        foreach (CardSlot slot in m_CardSlots)
        {
            slot.gameObject.SetActive(true);
        }
    }

    IEnumerator DrawInitHand()
    {
        for (int i = 0; i < m_CardSlots.Length; i++)
        {
            m_Deck.Draw(string.Empty, i);
            yield return new WaitForSeconds(0.8f);
        }
    }

    public void FocusOnCard(int cardIndex)
    {
        if (!m_IsInteractable) { return; }

        if (m_ZoomCoroutine == null && m_SelectedCardIndex == -1)
        {
            TransformAnimation.AnimationCallback onZoomInStart = () => { };
            TransformAnimation.AnimationCallback onZoomInEnd = () => {
                m_SelectedCardIndex = cardIndex;
                m_ZoomCoroutine = null;
            };
            m_ZoomCoroutine = TransformAnimation.FromToAnimation(m_CardSlots[cardIndex].Card.gameObject, m_CardSlots[cardIndex].FaceUpAnchor, m_ZoomTargetPoint, m_ZoomDuration, onZoomInStart, onZoomInEnd);
            StartCoroutine(m_ZoomCoroutine);
        }
    }

    public void RestoreFocus()
    {
        if (!m_IsInteractable) { return; }

        if (m_ZoomCoroutine == null && m_SelectedCardIndex != -1)
        {
            TransformAnimation.AnimationCallback onZoomOutStart = () => {
                m_SelectedCardIndex = -1;
            };
            TransformAnimation.AnimationCallback onZoomOutEnd = () => 
            {
                m_ZoomCoroutine = null;
            };
            m_ZoomCoroutine = TransformAnimation.FromToAnimation(m_CardSlots[m_SelectedCardIndex].Card.gameObject, m_ZoomTargetPoint, m_CardSlots[m_SelectedCardIndex].FaceUpAnchor, m_ZoomDuration, onZoomOutStart, onZoomOutEnd);
            StartCoroutine(m_ZoomCoroutine);
        }     
    }

    public void PlayCard()
    {
        if (!m_IsInteractable) { return; }
        if (m_ZoomCoroutine != null || m_SelectedCardIndex == -1) { return; }

        CardSlot targetSlot = m_Table.AllocateSlot();
        if (targetSlot != null)
        {
            //m_IsInteractable = false;
            TransformAnimation.AnimationCallback onPlayStart = () => 
            {
                m_CardSlots[m_SelectedCardIndex].Card.transform.SetParent(null);
                m_CardSlots[m_SelectedCardIndex].gameObject.SetActive(false);
            };
            TransformAnimation.AnimationCallback onPlayEnd = () => 
            {
                targetSlot.Card = m_CardSlots[m_SelectedCardIndex].Card;
                targetSlot.Card.transform.SetParent(targetSlot.transform);
                m_CardSlots[m_SelectedCardIndex].Card = null;
                m_SelectedCardIndex = -1;
            };
            IEnumerator playCoroutine = TransformAnimation.FromToAnimation(m_CardSlots[m_SelectedCardIndex].Card.gameObject, m_ZoomTargetPoint, targetSlot.FaceDownAnchor, m_PlayDuration, onPlayStart, onPlayEnd);
            StartCoroutine(playCoroutine);
        }
        
    }
}
