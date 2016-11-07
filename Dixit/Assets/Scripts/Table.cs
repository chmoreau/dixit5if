using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class Table : MonoBehaviour {
    [SerializeField]
    private RectTransform m_CardSlotPanel = null;
    [SerializeField]
    private CardSlot m_CardSlotPrefab = null;
    [SerializeField]
    private Text m_Theme = null;
    [SerializeField]
    private GameObject m_ThemeInputPanel = null;
    [SerializeField]
    private Text m_StorytellerName = null;
    [SerializeField]
    private Collider m_PickCardArea = null;
    [Header("Animations")]
    [SerializeField]
    private float m_ShuffleDuration = 0.4f;
    [SerializeField]
    private float m_ShuffleInterval = 0.3f;
    [SerializeField]
    private float m_ShufflePause = 0.1f;
    [SerializeField]
    private float m_DisplayDuration = 1.2f;
    [SerializeField]
    private float m_DisplayInterval = 0.9f;
    [SerializeField]
    private float m_ReturnDuration = 0.6f;
    [SerializeField]
    private float m_ZoomDuration = 0.4f;
    [Header("Anchors")]
    [SerializeField]
    private Transform m_ShuffleSpotPoint = null;
    [SerializeField]
    private Transform m_DisplayTargetPoint = null;
    
    private CardSlot[] m_CardSlots = new CardSlot[0];
    //public CardSlot[] CardSlots { get { return m_CardSlots; } }
    private int m_SlotPointer = 0;

    private int m_SelectedCardIndex = -1;
    private bool m_IsInteractable = false;
    public bool SetInteractable { set { m_IsInteractable = value; } }
    private IEnumerator m_ZoomCoroutine = null;

    void Update()
    {
        if (m_IsInteractable)
        {
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
                LayerMask mask = (1 << LayerMask.NameToLayer("Pick"));
                if (Physics.Raycast(ray, out hit, 20, mask) && hit.collider.Equals(m_PickCardArea))
                {
                    PickCard();
                }
                RestoreFocus();
            }
        }
    }

    public void Init(int slotNumber, string storytellerName = null, string theme = null, InGameCardModel[] tableCards = null, bool isFaceUp = false, bool isInteractable = false, string[] voteResult = null)
    {
        m_Theme.text = theme;
        m_StorytellerName.text = storytellerName;

        m_CardSlots = new CardSlot[slotNumber];
        if (tableCards == null)
        {
            tableCards = new InGameCardModel[0];
        }
        for (int i = 0; i < slotNumber; i++)
        {
            m_CardSlots[i] = Instantiate(m_CardSlotPrefab, m_CardSlotPanel, false) as CardSlot;
            if (i < tableCards.Length)
            {
                m_CardSlots[i].Card = GameSessionService.CurrentGameSession.InstantiateCard(tableCards[i].CardId, tableCards[i].OwnerId, isFaceUp ? m_CardSlots[i].FaceUpAnchor : m_CardSlots[i].FaceDownAnchor);
                m_CardSlots[i].Card.transform.SetParent(m_CardSlots[i].transform);

                if (tableCards[i].IsThemeCard)
                {
                    // todo highlight theme card
                }
                //todo show votes
            }
            int k = i;
            m_CardSlots[i].Clickable.onClick.AddListener(() => { FocusOnCard(k); });            
        }
        m_SlotPointer = tableCards.Length;

        m_IsInteractable = isInteractable;
    }

    public void Reset()
    {
        StopAllCoroutines();

        m_SelectedCardIndex = -1;
        m_ThemeInputPanel.SetActive(false);
        m_Theme.text = null;
        m_StorytellerName.text = null;
        foreach (CardSlot slot in m_CardSlots)
        {
            Destroy(slot.gameObject);
        }
    }

    public void Clear()
    {
        StopAllCoroutines();

        m_SelectedCardIndex = -1;
        m_ThemeInputPanel.SetActive(false);
        m_Theme.text = null;
        m_StorytellerName.text = null;
        foreach (CardSlot slot in m_CardSlots)
        {
            slot.Clear();
        }
        m_SlotPointer = 0;
        m_IsInteractable = false;
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

    public void SetStorytellerName(string name)
    {
        m_StorytellerName.text = name;
    }

    public void ConfirmTheme()
    {
        string theme = m_ThemeInputPanel.GetComponentInChildren<InputField>().text;
        if (string.IsNullOrEmpty(theme)) { return; }
        if (GameSessionService.CurrentGameSession.ConfirmTheme(theme))
        {
            m_ThemeInputPanel.SetActive(false);
            SetTheme(theme);
        }
    }

    public void SetTheme(string theme)
    {
        m_Theme.text = theme;
    }
    
    public void EnableThemeInput()
    {
        m_ThemeInputPanel.SetActive(true);
    }    

    public void PutOtherPlayerPlayedCard(string playerId)
    {
        CardSlot slot = AllocateSlot();
        if (slot)
        {
            slot.Card = GameSessionService.CurrentGameSession.InstantiateCard(null, playerId, slot.FaceDownAnchor);
            slot.Card.transform.SetParent(slot.transform);
        }
    }

    public void ShuffleAndRevealCards(string[] cardIds)
    {
        StartCoroutine(ShuffleAndDisplayCards(cardIds));
    }

    public IEnumerator ShuffleAndDisplayCards(string[] cardIds)
    {
        for (int i = 0; i < cardIds.Length; i++)
        {
            m_CardSlots[i].Card.LoadModel(GameSessionService.CurrentGameSession.Deck.FetchCardModel(cardIds[i]));
            IEnumerator shuffleCoroutine = TransformAnimation.FromToAnimation(m_CardSlots[i].Card.gameObject, m_CardSlots[i].FaceDownAnchor, m_ShuffleSpotPoint, Vector3.zero, Vector3.zero, m_ShuffleDuration, null, null);
            StartCoroutine(shuffleCoroutine);
            yield return new WaitForSeconds(m_ShuffleInterval);
        }
        yield return new WaitForSeconds(m_ShufflePause);
        for (int i = 0; i < cardIds.Length; i++)
        {
            TransformAnimation.AnimationCallback onDisplayEnd = CreateReturnCoroutine(i);
            IEnumerator displayCoroutine = TransformAnimation.FromToAnimation(m_CardSlots[i].Card.gameObject, m_ShuffleSpotPoint, m_DisplayTargetPoint, Vector3.zero, Vector3.zero, m_DisplayDuration, null, onDisplayEnd);
            StartCoroutine(displayCoroutine);
            yield return new WaitForSeconds(m_DisplayInterval);
        }

        m_IsInteractable = !GameSessionService.CurrentGameSession.LocalPlayer.IsStoryteller;
    }

    // Out of the coroutine body to avoid the scope issue of local variables :(
    private TransformAnimation.AnimationCallback CreateReturnCoroutine(int slotIndex)
    {
        return () =>
        {
            IEnumerator returnCoroutine = TransformAnimation.FromToAnimation(m_CardSlots[slotIndex].Card.gameObject, m_DisplayTargetPoint, m_CardSlots[slotIndex].FaceUpAnchor, Vector3.zero, Vector3.zero, m_ReturnDuration, null, null);
            StartCoroutine(returnCoroutine);
        };
    }

    public void FocusOnCard(int slotIndex)
    {
        if (!m_IsInteractable) { return; }
        if (!m_CardSlots[slotIndex].Card) { return; }

        if (m_ZoomCoroutine == null && m_SelectedCardIndex == -1)
        {
            TransformAnimation.AnimationCallback onZoomInStart = () => { };
            TransformAnimation.AnimationCallback onZoomInEnd = () => {
                m_SelectedCardIndex = slotIndex;
                m_ZoomCoroutine = null;
            };
            m_ZoomCoroutine = TransformAnimation.FromToAnimation(m_CardSlots[slotIndex].Card.gameObject, m_CardSlots[slotIndex].FaceUpAnchor, m_DisplayTargetPoint, Vector3.zero, Vector3.zero, m_ZoomDuration, onZoomInStart, onZoomInEnd);
            StartCoroutine(m_ZoomCoroutine);
        }
    }

    public void RestoreFocus()
    {
        //if (!m_IsInteractable) { return; }

        if (m_ZoomCoroutine == null && m_SelectedCardIndex != -1)
        {
            TransformAnimation.AnimationCallback onZoomOutStart = () => {
                m_SelectedCardIndex = -1;
            };
            TransformAnimation.AnimationCallback onZoomOutEnd = () =>
            {
                m_ZoomCoroutine = null;
            };
            m_ZoomCoroutine = TransformAnimation.FromToAnimation(m_CardSlots[m_SelectedCardIndex].Card.gameObject, m_DisplayTargetPoint, m_CardSlots[m_SelectedCardIndex].FaceUpAnchor, Vector3.zero, Vector3.zero, m_ZoomDuration, onZoomOutStart, onZoomOutEnd);
            StartCoroutine(m_ZoomCoroutine);
        }
    }

    private void PickCard()
    {
        //Register card play to game server
        if (!GameSessionService.CurrentGameSession.PickCard(m_CardSlots[m_SelectedCardIndex].Card.CardId)) { return; }

        m_IsInteractable = false;
        m_CardSlots[m_SelectedCardIndex].AddVoter(GameSessionService.CurrentGameSession.LocalPlayer.UserId);
    }
}
