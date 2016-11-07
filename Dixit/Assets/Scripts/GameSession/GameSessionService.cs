using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

public class GameSessionService : MonoBehaviour
{
    [SerializeField]
    private Animator m_CameraAnimator = null;
    [Header("Test")]
    public string SessionId = null;
    public InGamePlayerModel LocalPlayer = null;
    public InGamePlayerModel[] OtherPlayers = null;
    public string NarratorId = null;
    public GameSession.Phase Phase = GameSession.Phase.InitSession;
    public string[] HandIds = null;
    public string Theme = null;
    public InGameCardModel[] TableCardIds = null;
    public string[] PlayedCardIds = null;
    public string[] VoteResult = null;
    [Header("Results")]
    public InGameCardModel[] CardResults = new InGameCardModel[3];
    public string[] PlayerIds = new string[3];
    public string[] PlayerVotes = new string[3];
    public int[] PlayerScores = new int[3];

    private static GameSession m_CurrentGameSession = null;
    public static GameSession CurrentGameSession
    {
        get
        {
            return m_CurrentGameSession;
        }
    }

    #region Test
    private void DestroySession()
    {
        if (m_CurrentGameSession != null)
        {
            DestroyImmediate(m_CurrentGameSession.gameObject);
        }
    }

    public void TestCreateSession()
    {
        DestroySession();
        CreateSession(NarratorId);
    }

    public void TestToPlayCardPhase()
    {
        m_CurrentGameSession.TranslateToPhase(GameSession.Phase.PlayCard);
    }

    public void TestOtherPlayerPlayCard(string playerId)
    {
        m_CurrentGameSession.PutOtherPlayerPlayedCard(playerId);
    }

    public void TestToPickCardPhase()
    {
        m_CurrentGameSession.TranslateToPhase(GameSession.Phase.PickCard, (object)PlayedCardIds);
    }

    public void TestOtherPlayerPickCard(string playerId)
    {
        m_CurrentGameSession.MarkOtherPlayerPickCard(playerId);
    }

    public void TestToShowScorePhase()
    {
        Dictionary<string, DataPair<string, int>> dic = new Dictionary<string, DataPair<string, int>>();
        for (int i = 0; i < PlayerIds.Length; i++)
        {
            dic.Add(PlayerIds[i], new DataPair<string, int>(PlayerVotes[i], PlayerScores[i]));
        }
        m_CurrentGameSession.TranslateToPhase(GameSession.Phase.ShowScore, (object)CardResults, (object)dic);
    }

    public void TestOtherPlayerReadyForNext(string playerId)
    {
        m_CurrentGameSession.MarkOtherPlayerReadyForNext(playerId);
    }

    public void TestToNexrDrawCardPhase()
    {
        string[] newHands = { "4" };
        string newNarrator = "1";
        m_CurrentGameSession.StartNextRound(newHands, newNarrator);
    }
    #endregion

    public void FetchAllSessionSketchs()
    {
        // todo : network api
        // get ids of all active sessions
        // Network.FetchAllSessionSketchs(localPlayerId) return [](sessionIds, numPlayers, time since creation, etc.)
    }

    private GameSession InstantiateSessionInstance()
    {
        GameSession gameSession = new GameObject("GameSession", typeof(GameSession)).GetComponent<GameSession>();
        gameSession.transform.SetParent(transform);
        return gameSession;
    }

    public void CreateSession(string narratorId)
    {
        if (m_CurrentGameSession != null) { return; }
        // todo : network api
        // get sessionId, otherplayers and initHandIds from the server
        // Network.CreateSession(); return sessionId,[]otherPlayers, []initHandIds
        // Network.LocalPlayer
        string sessionId = SessionId; // test
        InGamePlayerModel localPlayer = LocalPlayer; // test
        InGamePlayerModel[] otherPlayers = OtherPlayers; // test
        string[] initHandIds = HandIds; // test

        StartCoroutine(CreateSessionCoroutine(sessionId, localPlayer, otherPlayers, initHandIds, narratorId));        
    }

    private IEnumerator CreateSessionCoroutine(string sessionId, InGamePlayerModel localPlayer, InGamePlayerModel[] otherPlayers, string[] initHandIds, string narratorId)
    {
        m_CurrentGameSession = InstantiateSessionInstance();
        m_CurrentGameSession.InitSession(sessionId, localPlayer, otherPlayers);
        m_CameraAnimator.SetBool("inGame", true);
        yield return new WaitForSeconds(1);
        m_CurrentGameSession.TranslateToPhase(GameSession.Phase.DrawHand, (object)initHandIds);
        m_CurrentGameSession.TranslateToPhase(GameSession.Phase.ChooseTheme, (object)narratorId);
    }

    public void RestoreSession(string sessionId)
    {
        // todo : network api
        // get all parameters to restore a seesion
        // Network.FetchSession(sessionId)        
        InGamePlayerModel localPlayer = LocalPlayer; // test
        InGamePlayerModel[] otherPlayers = OtherPlayers; // test
        GameSession.Phase currentPhase = (GameSession.Phase)Phase; // test
        string[] handIds = HandIds; // test
        string theme = Theme; // test
        InGameCardModel[] tableCardIds = TableCardIds; // test

        // todo : restoreSessionCoroutine
        m_CurrentGameSession = InstantiateSessionInstance();
        m_CurrentGameSession.InitSession(sessionId, localPlayer, otherPlayers, currentPhase, handIds, theme, tableCardIds, VoteResult);
        m_CameraAnimator.SetBool("inGame", true);
        //m_CurrentGameSession.TranslateToPhase(GameSession.Phase.DrawHand, (object)HandIds);
    }

    public void EndSession()
    {
        if (m_CurrentGameSession == null) { return; }
        StartCoroutine(EndSessionCoroutine());
    }

    private IEnumerator EndSessionCoroutine()
    {
        m_CameraAnimator.SetBool("inGame", false);
        yield return new WaitForSeconds(1);
        DestroyImmediate(m_CurrentGameSession.gameObject);
    }

    public void JoinMatchMaking(int playerNumber)
    {
        // Network.JoinMatchMaking(3)
    }
}
