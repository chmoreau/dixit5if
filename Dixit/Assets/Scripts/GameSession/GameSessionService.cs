using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GameSessionService : MonoBehaviour
{
    [Header("Test")]
    [SerializeField]
    private string SessionId = null;
    [SerializeField]
    private InGamePlayerModel LocalPlayer = null;
    [SerializeField]
    private InGamePlayerModel[] OtherPlayers = null;
    [SerializeField]
    private string[] HandIds = null;

    private static GameSession m_CurrentGameSession = null;
    public static GameSession CurrentGameSession
    {
        get
        {
            return m_CurrentGameSession;
        }

    }

    void OnDestroy()
    {
        if (m_CurrentGameSession != null)
        {
            Destroy(m_CurrentGameSession.gameObject);

        }
    }

    #region Test
    public void TestCreate()
    {
        CreateNewSession();
    }

    public void TestPlay()
    {
        //m_CurrentGameSession.TranslateToPhase(GameSession.Phase.PlayCard);

        m_CurrentGameSession = new GameObject("GameSession", typeof(GameSession)).GetComponent<GameSession>();
        m_CurrentGameSession.transform.SetParent(transform);
        LocalPlayer.State = InGamePlayerModel.InGameState.Waiting;
        m_CurrentGameSession.InitSession(SessionId, LocalPlayer, OtherPlayers, GameSession.Phase.PlayCard, HandIds, "test_theme_wtf");
    }

    public void TestUpdatePlayerState(string playerId)//, InGamePlayerModel.InGameState state = InGamePlayerModel.InGameState.Done)
    {
        m_CurrentGameSession.UpdateOtherPlayerState(playerId, InGamePlayerModel.InGameState.Done);
    }
    #endregion

    public void CreateNewSession()
    {
        if (m_CurrentGameSession != null) { return; }
        // todo : network api
        // get sessionId, otherplayers and initHandIds from the server
        m_CurrentGameSession = new GameObject("GameSession", typeof(GameSession)).GetComponent<GameSession>();
        m_CurrentGameSession.transform.SetParent(transform);
        m_CurrentGameSession.InitSession(SessionId, LocalPlayer, OtherPlayers);
        m_CurrentGameSession.TranslateToPhase(GameSession.Phase.DrawHand, (object)HandIds);
    }
}
