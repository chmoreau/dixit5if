using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public class CardSlot : MonoBehaviour {
    [SerializeField]
    private Transform m_FaceUpAnchor = null;
    [SerializeField]
    private Transform m_FaceDownAnchor = null;
    [SerializeField]
    private Card m_Card;
    [SerializeField]
    private Button m_Clickable = null;
    [SerializeField]
    private RectTransform m_OwnerView = null;
    [SerializeField]
    private RectTransform[] m_VoterViews = new RectTransform[5];

    public Transform FaceUpAnchor { get { return m_FaceUpAnchor; } }
    public Transform FaceDownAnchor { get { return m_FaceDownAnchor; } }
    public Card Card { get { return m_Card; } set { m_Card = value; } }
    public Button Clickable { get { return m_Clickable; } }

    void OnDestroy()
    {
        if (m_Card != null)
        {
            Destroy(m_Card.gameObject);
        }
    }
    
    public void Clear()
    {
        if (m_Card != null)
        {
            Destroy(m_Card.gameObject);
        }
        HideOwner();
        ClearVoters();
    }

    public void ShowOwner()
    {
        m_OwnerView.gameObject.SetActive(true);
        Text text = m_OwnerView.GetComponentInChildren<Text>();
        text.text = GameSessionService.CurrentGameSession.GetPlayer(m_Card.OwnerId).Nickname;
    }

    public void HideOwner()
    {
        m_OwnerView.gameObject.SetActive(false);
        Text text = m_OwnerView.GetComponentInChildren<Text>();
        text.text = string.Empty;
    }

    public void AddVoter(string voterId)
    {
        for (int i = 0; i < m_VoterViews.Length; i++)
        {
            GameObject voter = m_VoterViews[i].gameObject;
            if (!voter.activeSelf)
            {
                voter.SetActive(true);
                Text text = voter.GetComponentInChildren<Text>();
                text.text = GameSessionService.CurrentGameSession.GetPlayer(voterId).Nickname;
                break;
            }
        }
    }

    public void ClearVoters()
    {
        foreach (RectTransform voterView in m_VoterViews)
        {
            voterView.gameObject.SetActive(false);
            Text text = voterView.GetComponentInChildren<Text>();
            text.text = string.Empty;
        }
    }
}
