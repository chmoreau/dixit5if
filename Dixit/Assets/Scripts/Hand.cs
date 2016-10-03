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
    private HandAnchor[] m_HandAnchors = new HandAnchor[6];
    public HandAnchor[] HandAnchors { get { return m_HandAnchors; } }
    [SerializeField]
    private Transform m_ZoomTargetPoint = null;
    [SerializeField]
    private float m_ZoomDuration = 0.5f;

    [Header("Test")]
    [SerializeField]
    private Deck m_Deck = null;

    private int m_SelectedCardIndex = -1;

    void Start()
    {
        StartCoroutine("DrawInitHand");
    }

    IEnumerator DrawInitHand()
    {
        for (int i = 0; i < m_HandAnchors.Length; i++)
        {
            m_Deck.Draw(string.Empty, i);
            yield return new WaitForSeconds(0.8f);
        }
    }

    public void FocusOnCard(int cardIndex)
    {
        if (m_SelectedCardIndex == -1)
        {
            m_SelectedCardIndex = cardIndex;
            StartCoroutine("ZoomIn");
        }
    }

    public void RestoreFocus()
    {
        StartCoroutine("ZoomOut");        
    }

    //bool isZoomingIn = false;
    IEnumerator ZoomIn()
    {
        //isZoomingIn = true;
        float timer = m_ZoomDuration;
        while (timer >= 0)
        {
            timer -= Time.deltaTime;
            float process = (m_ZoomDuration - timer) / m_ZoomDuration;
            m_HandAnchors[m_SelectedCardIndex].HandCard.transform.position = Vector3.Lerp(m_HandAnchors[m_SelectedCardIndex].HandAnchorPoint.position, m_ZoomTargetPoint.position, process);
            //card.transform.rotation = Quaternion.Slerp(m_CardSpawnPoint.rotation, m_ZoomTargetPoint.rotation, process);
            yield return new WaitForEndOfFrame();
        }
        //isZoomingIn = false;
    }

    //bool isZoomingO
    IEnumerator ZoomOut()
    {
        float timer = m_ZoomDuration;
        while (timer >= 0)
        {
            timer -= Time.deltaTime;
            float process = (m_ZoomDuration - timer) / m_ZoomDuration;
            m_HandAnchors[m_SelectedCardIndex].HandCard.transform.position = Vector3.Lerp(m_ZoomTargetPoint.position, m_HandAnchors[m_SelectedCardIndex].HandAnchorPoint.position, process);
            //card.transform.rotation = Quaternion.Slerp(m_CardSpawnPoint.rotation, m_ZoomTargetPoint.rotation, process);
            yield return new WaitForEndOfFrame();
        }
        m_SelectedCardIndex = -1;
    }
}
