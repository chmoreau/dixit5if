using UnityEngine;
using System;
using System.Collections;

public class Hand : MonoBehaviour {
    public const int HAND_SIZE = 6;

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
    private Transform m_CardSlotPanel = null;
    [SerializeField]
    private CardSlot[] m_CardSlots = new CardSlot[HAND_SIZE];
    public CardSlot[] CardSlots { get { return m_CardSlots; } }
    [SerializeField]
    private Transform m_ZoomTargetPoint = null;
    [SerializeField]
    private Collider m_PlayCardArea = null;
    [SerializeField]
    private float m_ZoomDuration = 0.3f;
    [SerializeField]
    private float m_PlayDuration = 0.4f;
    
    private int m_SelectedCardIndex = -1;
    private bool m_IsInteractable = false;
    public bool SetInteractable { set { m_IsInteractable = value; } }
    private bool m_IsPlayable = false;
    public bool SetPlayable { set { m_IsPlayable = value; } }
    private IEnumerator m_ZoomCoroutine = null;

    void Update()
    {
        if (m_IsInteractable)
        {
            //if (m_IsDrawing) { return; }
            if (m_ZoomCoroutine != null || m_SelectedCardIndex == -1) { return; }
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Debug.DrawRay(ray.origin, ray.direction * 20, Color.yellow);
#elif UNITY_IOS || UNITY_ANDROID                
            if (Input.touchCount == 1)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
#endif
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 20) && hit.collider.Equals(m_PlayCardArea))
                {
                    if (m_IsPlayable)
                    {
                        PlayCard();
                    }
                }
                else
                {
                    RestoreFocus();
                }
            }
        }
    }

    public void Init(string[] handIds = null, bool isInteractable = false, bool isPlayable = false)
    {
        if (handIds == null)
        {
            handIds = new string[0];
        }
        for (int i = 0; i < HAND_SIZE && i < handIds.Length; i++)
        {
            m_CardSlots[i].Card = GameSessionService.CurrentGameSession.InstantiateCard(handIds[i], GameSessionService.CurrentGameSession.LocalPlayer.UserId, m_CardSlots[i].FaceUpAnchor);
            m_CardSlots[i].Card.transform.SetParent(m_CardSlots[i].transform);
        }

        //m_IsDrawing = false;
        m_IsInteractable = isInteractable;
        m_IsPlayable = isPlayable;
    }

    public void Reset()
    {
        StopAllCoroutines();

        m_SelectedCardIndex = -1;
        for (int i = 0; i < HAND_SIZE; i++)
        {
            if (m_CardSlots[i].Card != null)
            {
                Destroy(m_CardSlots[i].Card.gameObject);
            }
            m_CardSlots[i].transform.SetSiblingIndex(i);
            m_CardSlots[i].gameObject.SetActive(true);
        }
    }

    public void Draw(string[] cardIds)
    {
        StartCoroutine("DrawHand", cardIds);
    }

    //private bool m_IsDrawing = false;
    IEnumerator DrawHand(string[] cardIds)
    {
        m_IsInteractable = false;

        foreach (CardSlot slot in m_CardSlots)
        {
            slot.gameObject.SetActive(true);
        }

        int k = 0;        
        for (int i = 0; i < m_CardSlotPanel.childCount; i++)
        {
            CardSlot slot = m_CardSlotPanel.GetChild(i).GetComponent<CardSlot>();
            if (slot.Card == null)
            {
                GameSessionService.CurrentGameSession.DrawCardFromDeck(cardIds[k++], slot);
                yield return new WaitForSeconds(0.4f);
            }
        }

        m_IsInteractable = true;
    }

    public void FocusOnCard(int cardIndex)
    {
        if (!m_IsInteractable) { return; }
        if (!m_CardSlots[cardIndex].Card) { return; }

        if (m_ZoomCoroutine == null && m_SelectedCardIndex == -1)
        {
            TransformAnimation.AnimationCallback onZoomInStart = () => { };
            TransformAnimation.AnimationCallback onZoomInEnd = () => {
                m_SelectedCardIndex = cardIndex;
                m_ZoomCoroutine = null;
            };
            m_ZoomCoroutine = TransformAnimation.FromToAnimation(m_CardSlots[cardIndex].Card.gameObject, m_CardSlots[cardIndex].FaceUpAnchor, m_ZoomTargetPoint, Vector3.zero, Vector3.zero, m_ZoomDuration, onZoomInStart, onZoomInEnd);
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
            m_ZoomCoroutine = TransformAnimation.FromToAnimation(m_CardSlots[m_SelectedCardIndex].Card.gameObject, m_ZoomTargetPoint, m_CardSlots[m_SelectedCardIndex].FaceUpAnchor, Vector3.zero, Vector3.zero, m_ZoomDuration, onZoomOutStart, onZoomOutEnd);
            StartCoroutine(m_ZoomCoroutine);
        }     
    }

    private void PlayCard()
    {
        CardSlot targetSlot = GameSessionService.CurrentGameSession.AllocateTableSlot();
        if (targetSlot != null)
        {
            //Register card play to game server
            if (!GameSessionService.CurrentGameSession.PlayCard(m_CardSlots[m_SelectedCardIndex].Card.CardId)) { return; }

            m_IsInteractable = false;
            m_IsPlayable = false;
            TransformAnimation.AnimationCallback onPlayStart = () => 
            {
                m_CardSlots[m_SelectedCardIndex].Card.transform.SetParent(null);
                m_CardSlots[m_SelectedCardIndex].gameObject.SetActive(false);
                m_CardSlots[m_SelectedCardIndex].transform.SetAsLastSibling();
            };
            TransformAnimation.AnimationCallback onPlayEnd = () => 
            {
                targetSlot.Card = m_CardSlots[m_SelectedCardIndex].Card;
                targetSlot.Card.transform.SetParent(targetSlot.transform);
                m_CardSlots[m_SelectedCardIndex].Card = null;
                m_SelectedCardIndex = -1;
            };
            IEnumerator playCoroutine = TransformAnimation.FromToAnimation(m_CardSlots[m_SelectedCardIndex].Card.gameObject, m_ZoomTargetPoint, targetSlot.FaceDownAnchor, Vector3.zero, Vector3.zero, m_PlayDuration, onPlayStart, onPlayEnd);
            StartCoroutine(playCoroutine);
        }
        
    }
}
