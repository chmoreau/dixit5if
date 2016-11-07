using UnityEngine;

using SocketIO;
using System.Collections.Generic;
using System.Collections;

public class Network : MonoBehaviour
{
    SocketIOComponent socket;
    public GameObject go;
    private IEnumerator update;
    private bool first_turn = true;
    private string storyTeller;
    private string cardPickedByStoryTeller;
    InGameCardModel[] cardModels = null;
    Dictionary<string, DataPair<string, int>> dict = null;

    public string UserName;

    public void Start()
    {

        go = GameObject.Find("SocketIO");
        update = Update_Sockets();
        socket = go.GetComponent<SocketIOComponent>();

        socket.On("GAME_CREATED", GameCreated);
        socket.On("QUEUE_SIZE", QueueSize);
        socket.On("INFO_PLAYERS", InfoPlayers);
        socket.On("START_TURN", StartTurn);
        socket.On("THEME", ReceiveTheme);
        socket.On("CARD_PLAYED", CardPlayed);
        socket.On("REVEAL_CARDS", RevealCards);
        socket.On("CARD_PICKED", CardPicked);
        socket.On("NEW_TURN", NewTurn);
        socket.On("TRICK", Trick);
        socket.On("GAME_OVER", GameOver);

        JSONObject obj = new JSONObject();
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
        first_turn = true;
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

    public void InfoPlayers(SocketIOEvent e)
    {
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
        Dictionary<string, string> data = e.data.ToDictionary();
        string s = data["names"];
        List<string> players = new List<string>();
        string aux = "";
        for(int i=0;i<s.Length;++i)
        {
            if (s[i] != ' ')
            {
                aux = aux + s[i];
            }
            else
            {
                players.Add(aux);
                aux = "";
            }
        }
        GameObject go = GameObject.Find("GameSessionService");
        GameSessionService gameSession = (GameSessionService)go.GetComponent(typeof(GameSessionService));
        List<InGamePlayerModel> others = new List<InGamePlayerModel>();
        cardModels = new InGameCardModel[players.Count];
        dict = new Dictionary<string, DataPair<string, int>>();
        cnt = 0;
        for (int i=0;i<players.Count;++i)
        {
            if(players[i]==UserName)
            {
                gameSession.LocalPlayer = new InGamePlayerModel();
                gameSession.LocalPlayer.Nickname = players[i];
                gameSession.LocalPlayer.UserId = players[i];
                gameSession.LocalPlayer.Score = 0;
                gameSession.LocalPlayer.State = 0;
            } else
            {
                InGamePlayerModel new_player = new InGamePlayerModel();
                new_player.Nickname = players[i];
                new_player.UserId = players[i];
                new_player.Score = 0;
                new_player.State = 0;
                others.Add(new_player);
            }
        }
        gameSession.OtherPlayers = others.ToArray();
       
    }

    public void StartTurn(SocketIOEvent e)
    {
            Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
            Dictionary<string, string> data = e.data.ToDictionary();
            List<string> keyList = new List<string>(data.Keys);
            string narrator = data["narrator"];
            string hand = data["hand"];
            storyTeller = narrator;

            List<string> cards = new List<string>();
            string aux = "";
            for (int i = 0; i < hand.Length; ++i)
            {
                if (hand[i] != ',' && hand[i] != ']')
                {
                    aux = aux + hand[i];
                }
                else
                {
                    cards.Add(aux);
                    aux = "";
                }
            }
            cards.Add(aux);
        if (first_turn)
        {
            MatchMakingView matchmaking = FindObjectOfType<MatchMakingView>();
            matchmaking.StopMatchMaking();
            GameObject go = GameObject.Find("GameSessionService");
            GameSessionService gameSession = (GameSessionService)go.GetComponent(typeof(GameSessionService));
            gameSession.HandIds = cards.ToArray();
            gameSession.CreateSession(narrator);
            first_turn = false;
        } else
        {
            GameSessionService.CurrentGameSession.TranslateToPhase(GameSession.Phase.DrawHand, (object)cards.ToArray());
            GameSessionService.CurrentGameSession.TranslateToPhase(GameSession.Phase.ChooseTheme, (object)narrator);
        }
    }

    public void ReceiveTheme(SocketIOEvent e)
    {
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
        Dictionary<string, string> data = e.data.ToDictionary();
        string theme = data["theme"];
       
        go = GameObject.Find("GameSessionService");
        //GameSessionService gameSession = (GameSessionService)go.GetComponent(typeof(GameSessionService));
        GameSessionService.CurrentGameSession.SetTheme(theme);
        GameSessionService.CurrentGameSession.TranslateToPhase(GameSession.Phase.PlayCard);
    }

    public void CardPlayed(SocketIOEvent e)
    {
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
        Dictionary<string, string> data = e.data.ToDictionary();
        string playerId = data["playerID"];
        go = GameObject.Find("GameSessionService");
        //GameSessionService gameSession = (GameSessionService)go.GetComponent(typeof(GameSessionService));
        GameSessionService.CurrentGameSession.PutOtherPlayerPlayedCard(playerId);
    }

    public void RevealCards(SocketIOEvent e)
    {
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
        Dictionary<string, string> data = e.data.ToDictionary();
        string str = data["cards"];
        Debug.Log(str);
        List<string> cards = new List<string>();
        string aux = "";
        for(int i=0;i<str.Length;++i)
        {
            if(str[i] == ',')
            {
                cards.Add(aux);
                aux = "";
            } else
            {
                aux = aux + str[i];
            }
        }
        for (int i = 0; i < cards.Count; ++i)
            Debug.Log(cards[i]);
        go = GameObject.Find("GameSessionService");
        //GameSessionService gameSession = (GameSessionService)go.GetComponent(typeof(GameSessionService));
        GameSessionService.CurrentGameSession.TranslateToPhase(GameSession.Phase.PickCard, (object)(cards.ToArray()));
    }


    public void CardPicked(SocketIOEvent e)
    {
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
        Dictionary<string, string> data = e.data.ToDictionary();
        string playerId = data["playerID"];
        go = GameObject.Find("GameSessionService");
        //GameSessionService gameSession = (GameSessionService)go.GetComponent(typeof(GameSessionService));
        GameSessionService.CurrentGameSession.MarkOtherPlayerPickCard(playerId);
    }

    public void NewTurn(SocketIOEvent e)
    {
        
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
        /*Dictionary<string, string> data = e.data.ToDictionary();
        string playerID = data["playerID"];
        string score = data["score"];
        go = GameObject.Find("GameSessionService");
        GameSessionService gameSession = (GameSessionService)go.GetComponent(typeof(GameSessionService));
        gameSession.UpdateScore(name,score);
        */

    }

    private int cnt = 0;

    public void Trick(SocketIOEvent e)
    {
        
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
        Dictionary<string, string> data = e.data.ToDictionary();
        string playerID = data["playerID"];
        string cardPlayed = data["cardPlayed"];
        string score = data["score"];
        string cardPicked = "";
        if (data.ContainsKey("cardPicked")) cardPicked = data["cardPicked"];
        else cardPicked = cardPlayed;
        cardModels[cnt] = new InGameCardModel();
        cardModels[cnt].OwnerId = playerID;
        cardModels[cnt].CardId = cardPlayed;

        if (playerID == storyTeller)
        {
            cardModels[cnt].IsThemeCard = true;

        }
        else
        {
            cardModels[cnt].IsThemeCard = false;
        }

            DataPair<string, int> data_pair = new DataPair<string, int>(cardPicked, int.Parse(score));
        dict.Add(playerID, data_pair);
        cnt++;

        if (cnt == cardModels.Length)
        {
            GameSessionService.CurrentGameSession.TranslateToPhase(GameSession.Phase.ShowScore, cardModels, dict);
        }

    }

    public void GameOver(SocketIOEvent e)
    {
        Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));
        GameSessionService.CurrentGameSession.TranslateToPhase(GameSession.Phase.EndSession);
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
        data["playerID"] = UserName;
        socket.Emit("PLAYER_READY", new JSONObject(data));
    }

    public void JoinMatchmaking()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["playerID"] = UserName;
        socket.Emit("JOIN_MATCHMAKING", new JSONObject(data));
    }

    public void ConfirmTheme(string theme)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["theme"] = theme;
        socket.Emit("THEME", new JSONObject(data));
    }

    public void PlayCard(string cardID)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["cardID"] = cardID;
        socket.Emit("PLAY_CARD", new JSONObject(data));
    }

    public void PickCard(string voteID)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["voteID"] = voteID;
        socket.Emit("PICK_CARD", new JSONObject(data));
    }

    public void Disconnect()
    {
        socket.Emit("Disconnect");
    }
}
