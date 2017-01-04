using UnityEngine;
using System;

[Serializable]
public class InGamePlayerModel
{
    public enum InGameState
    {
        Waiting = 0,
        Done = 1,
        Inactive = 2
    }

    [SerializeField]
    private string m_UserId;
    [SerializeField]
    private string m_Nickname;
    [SerializeField]
    private bool m_IsStoryteller;
    [SerializeField]
    private bool m_IsLocal;
    [SerializeField]
    private InGameState m_State;
    [SerializeField]
    private int m_Score;

    public string UserId { get { return m_UserId; } set { m_UserId = value; } }
    public string Nickname { get { return m_Nickname; } set { m_Nickname = value; } }
    public bool IsStoryteller { get { return m_IsStoryteller; } set { m_IsStoryteller = value; } }
    public bool IsLocal { get { return m_IsLocal; } set { m_IsLocal = value; } }
    public InGameState State { get { return m_State; } set { m_State = value; } }
    public int Score { get { return m_Score; } set { m_Score = value; } }

    public InGamePlayerModel()
    {
        m_UserId = null;
        m_Nickname = string.Empty;
        m_IsStoryteller = false;
        m_IsLocal = false;
        m_State = InGameState.Waiting;
        m_Score = 0;
    }

    public void AddPoints(int points)
    {
        Score += points;
    }
}
