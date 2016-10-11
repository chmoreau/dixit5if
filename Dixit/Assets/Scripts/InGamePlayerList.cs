using UnityEngine;
using System.Collections.Generic;

public class InGamePlayerList : MonoBehaviour {
    [SerializeField]
    private Transform m_InGamePlayerListAnchor = null;
    [SerializeField]
    private InGamePlayer m_InGamePlayerPrefab = null;

    private Dictionary<string, InGamePlayer> m_PlayerList = new Dictionary<string, InGamePlayer>();
    public Dictionary<string, InGamePlayer> PlayerList { get { return m_PlayerList; } }

    public void Init()
    {
        Reset();
    }

    public void Reset()
    {
        foreach (InGamePlayer player in m_PlayerList.Values)
        {
            Destroy(player.gameObject);
        }
        m_PlayerList.Clear();
    }

    public void ChangeListVisibility()
    {
        m_InGamePlayerListAnchor.gameObject.SetActive(!m_InGamePlayerListAnchor.gameObject.activeSelf);
    }

    public void LoadPlayers(InGamePlayerModel local, InGamePlayerModel[] others)
    {
        InGamePlayer localPlayer = Instantiate(m_InGamePlayerPrefab, m_InGamePlayerListAnchor, false) as InGamePlayer;
        localPlayer.LoadModel(local);
        m_PlayerList.Add(local.UserId, localPlayer);
        foreach (InGamePlayerModel other in others)
        {
            InGamePlayer otherPlayer = Instantiate(m_InGamePlayerPrefab, m_InGamePlayerListAnchor, false) as InGamePlayer;
            otherPlayer.LoadModel(other);
            m_PlayerList.Add(other.UserId, otherPlayer);
        }
    }

    public void ForcePlayerViewUpdate(string playerId)
    {
        m_PlayerList[playerId].UpdateView();
    }
}
