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
    public GameSession.Phase Phase = GameSession.Phase.InitSession;
    public string[] HandIds = null;
    public string Theme = null;
    public InGameCardModel[] TableCardIds = null;
    public string[] VoteResult = null;

    private static GameSession m_CurrentGameSession = null;
    public static GameSession CurrentGameSession
    {
        get
        {
            return m_CurrentGameSession;
        }
    }

    private bool m_SessionDelayFlag = false;

    #region Test
    private void DestroySession()
    {
        if (m_CurrentGameSession != null)
        {
            DestroyImmediate(m_CurrentGameSession.gameObject);
        }
    }


    public void TestPlayPhase()
    {
        DestroySession();

        m_CurrentGameSession = InstantiateSessionInstance();
        LocalPlayer.State = InGamePlayerModel.InGameState.Waiting;
        m_CurrentGameSession.InitSession(SessionId, LocalPlayer, OtherPlayers, GameSession.Phase.PlayCard, HandIds, "test_theme_abc");
    }

    public void TestUpdatePlayerState(string playerId)//, InGamePlayerModel.InGameState state = InGamePlayerModel.InGameState.Done)
    {
        m_CurrentGameSession.UpdateOtherPlayerState(playerId, InGamePlayerModel.InGameState.Done);
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

    public void CreateSession( string storyTeller)
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

        StartCoroutine(CreateSessionCoroutine(sessionId, localPlayer, otherPlayers, initHandIds, storyTeller));        
    }

    private IEnumerator CreateSessionCoroutine(string sessionId, InGamePlayerModel localPlayer, InGamePlayerModel[] otherPlayers, string[] initHandIds, string storyTeller)
    {
        m_CurrentGameSession = InstantiateSessionInstance();
        m_CurrentGameSession.InitSession(sessionId, localPlayer, otherPlayers);
        m_CameraAnimator.SetBool("inGame", true);
        yield return new WaitForSeconds(1);
        m_CurrentGameSession.TranslateToPhase(GameSession.Phase.DrawHand, (object)initHandIds);
        yield return new WaitForSeconds(6);
        m_CurrentGameSession.TranslateToPhase(GameSession.Phase.ChooseTheme, storyTeller);
    }

    public void StoryTeller(string storyTeller)
    {
        m_CurrentGameSession.SetStoryteller(storyTeller);
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

}
