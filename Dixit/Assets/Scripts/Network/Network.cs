using UnityEngine;

using SocketIO;
using System.Collections.Generic;
using System.Collections;

public class Network : MonoBehaviour
{
    SocketIOComponent socket;
    public GameObject go;
    private IEnumerator update;

    public string UserName;

    public void Start()
    {

        go = GameObject.Find("SocketIO");
        update = Update_Sockets();
        socket = go.GetComponent<SocketIOComponent>();

        socket.On("GAME_CREATED", GameCreated);
        socket.On("QUEUE_SIZE", QueueSize);
        socket.On("START_TURN", StartTurn);
        socket.On("THEME", ReceiveTheme);
        socket.On("CARD_PLAYED", CardPlayed);
        socket.On("REVEAL_CARDS", RevealCards);
        socket.On("CARD_PICKED", CardPicked);
        socket.On("NEW_TURN", NewTurn);
        socket.On("TRICK", Trick);
        socket.On("GAME_OVER", GameOver);

        JSONObject obj = new JSONObject();
        StartCoroutine(update);
    }

    public void GameCreated(SocketIOEvent e)
    {
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
        PlayerReady();
        Dictionary<string, string> data = e.data.ToDictionary();
        string gameID = data["gameID"];
        GameObject go = GameObject.Find("GameSessionService");
        GameSessionService gameSession = (GameSessionService)go.GetComponent(typeof(GameSessionService));
        gameSession.SessionId = gameID;
        gameSession.CreateSession();
    }

    public void QueueSize(SocketIOEvent e)
    {
        GameObject go = GameObject.Find("MatchMakingView");
        MatchMakingView matchmaking = (MatchMakingView)go.GetComponent(typeof(MatchMakingView));
        Dictionary<string, string> data = e.data.ToDictionary();
        string queueLength = data["queueLength"];
        matchmaking.ShowMatchMakingProgress(int.Parse(queueLength));
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
    }

    public void StartTurn(SocketIOEvent e)
    {
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
    }

    public void ReceiveTheme(SocketIOEvent e)
    {
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
    }

    public void CardPlayed(SocketIOEvent e)
    {
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
    }

    public void RevealCards(SocketIOEvent e)
    {
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
    }


    public void CardPicked(SocketIOEvent e)
    {
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
    }

    public void NewTurn(SocketIOEvent e)
    {
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
    }

    public void Trick(SocketIOEvent e)
    {
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
    }

    public void GameOver(SocketIOEvent e)
    {
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
    }

    IEnumerator Update_Sockets()
    {
        while (true)
        {
            bool boolean = socket.IsConnected;
            if (boolean == true)
            {

            }
            yield return new WaitForSeconds(4);
        }

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void PlayerReady()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["name"] = UserName;
        socket.Emit("PLAYER_READY", new JSONObject(data));
    }

    public void JoinMatchmaking()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["playerId"] = UserName;
        socket.Emit("JOIN_MATCHMAKING", new JSONObject(data));
    }

    public void ConfirmTheme(string theme)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["THEME"] = theme;
        socket.Emit("THEME", new JSONObject(data));
    }

    public void PlayCard(string cardID)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["cardID"] = cardID;
        socket.Emit("PLAYCARD", new JSONObject(data));
    }

    public void PickCard(string cardID)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["cardId"] = cardID;
        socket.Emit("", new JSONObject(data));
    }

    public void Disconnect()
    {
        socket.Emit("Disconnect");
    }
}
