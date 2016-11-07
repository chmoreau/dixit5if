using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GameSession : MonoBehaviour
{
    public enum Phase
    {
        InitSession = -1,
        DrawHand = 0,
        ChooseTheme = 1,
        PlayCard = 2,
        PickCard = 3,
        ShowScore = 4
    }

    public const string INSTRUCTION_INITSESSION = "Attendez l'initalisation de la partie du jeu";
    public const string INSTRUCTION_DRAWHAND = "Piocher les cartes jusqu'à une main de six cartes";
    public const string INSTRUCTION_CHOOSETHEME_STORYTELLER = "PhaseVous etes le conteur de ce tour, écrivez le thème du tour";
    public const string INSTRUCTION_CHOOSETHEME_OTHERS = "Attendez que le thème soit choisi par le conteur";
    public const string INSTRUCTION_PLAYCARD = "Choisissez une carte qui vous fait penser au thème";
    public const string INSTRUCTION_PICKCARD = "Votez pour la carte que vous pensez etre celle du conteur";
    public const string INSTRUCTION_SHOWSCORE = "Voyez le résultat du vote et le décompte des points";

    public Deck Deck = null;
    public Hand Hand = null;
    public Table Table = null;
    public HUD HUD = null;

    private InGamePlayerModel m_LocalPlayer = null;
    public InGamePlayerModel LocalPlayer
    {
        get
        {
            return m_LocalPlayer;
        }
    }
    private InGamePlayerModel[] m_OtherPlayers = null;
    private InGamePlayerModel Storyteller
    {
        get
        {
            return m_LocalPlayer.IsStoryteller ? m_LocalPlayer : m_OtherPlayers.FirstOrDefault(p => p.IsStoryteller);
        }
    }

    private string m_GameSessionId = null;
    private Phase m_CurrentPhase = Phase.InitSession;

    void Awake()
    {
        Deck = FindObjectOfType<Deck>();
        Hand = FindObjectOfType<Hand>();
        Table = FindObjectOfType<Table>();
        HUD = FindObjectOfType<HUD>();
    }


    void OnDestroy()
    {
        if (Deck != null) { Deck.Reset(); }
        if (Hand != null) { Hand.Reset(); }
        if (Table != null) { Table.Reset(); }
        if (HUD != null) { HUD.Reset(); }
    }

    public void TranslateToPhase(Phase targetPhase, params object[] args)
    {
        switch (m_CurrentPhase)
        {
            case Phase.InitSession :
                if (targetPhase == Phase.DrawHand)
                {
                    HUD.Instruction.text = INSTRUCTION_DRAWHAND;
                    m_CurrentPhase = Phase.DrawHand;
                    DrawHand((string[])args[0]);
                }
                break;
            case Phase.DrawHand :
                if (targetPhase == Phase.ChooseTheme)
                {
                    string storyTeller = (string)args[0];
                    if (storyTeller == m_LocalPlayer.Nickname)
                        HUD.Instruction.text = INSTRUCTION_CHOOSETHEME_STORYTELLER;
                    else
                        HUD.Instruction.text = INSTRUCTION_CHOOSETHEME_OTHERS;
                    m_CurrentPhase = Phase.ChooseTheme;
                    SetStoryteller((string)args[0]);
                }
                break;
            case Phase.ChooseTheme:
                if (targetPhase == Phase.PlayCard)
                {
                    HUD.Instruction.text = INSTRUCTION_PLAYCARD;
                    m_CurrentPhase = Phase.PlayCard;
                    Hand.SetPlayable = true;
                    InitAllPlayersState();
                }
                break;
            case Phase.PlayCard :
                break;
            case Phase.PickCard :
                break;
            case Phase.ShowScore :
                break;
        }
    }

    private void InitAllPlayersState()
    {
        m_LocalPlayer.State = InGamePlayerModel.InGameState.Waiting;
        foreach (InGamePlayerModel otherPlayer in m_OtherPlayers)
        {
            otherPlayer.State = InGamePlayerModel.InGameState.Waiting;
        }
    }

    public void InitSession(string sessionId, InGamePlayerModel localPlayer, InGamePlayerModel[] otherPlayers, Phase currentPhase = Phase.InitSession, string[] handIds = null, string theme = null, InGameCardModel[] tableCards = null, string[] voteResult = null)
    {
        m_GameSessionId = sessionId;
        m_LocalPlayer = localPlayer;
        m_OtherPlayers = otherPlayers;
        Deck.Init();
        HUD.Init();
        HUD.InGamePlayerList.LoadPlayers(localPlayer, otherPlayers);
        m_CurrentPhase = currentPhase;
        switch (m_CurrentPhase)
        {
            case Phase.InitSession :
                HUD.Instruction.text = INSTRUCTION_INITSESSION;
                Hand.Init();
                Table.Init(otherPlayers.Length + 1);
                break;
            case Phase.DrawHand :
                HUD.Instruction.text = INSTRUCTION_DRAWHAND;
                Hand.Init(handIds, true);
                Table.Init(otherPlayers.Length + 1);
                break;
            case Phase.ChooseTheme :
                HUD.Instruction.text = localPlayer.IsStoryteller ? INSTRUCTION_CHOOSETHEME_STORYTELLER : INSTRUCTION_CHOOSETHEME_OTHERS;
                Hand.Init(handIds, true);
                Table.Init(otherPlayers.Length + 1, Storyteller.Nickname);
                if (localPlayer.IsStoryteller)
                {
                    Table.EnableThemeInput();
                }
                break;
            case Phase.PlayCard :
                HUD.Instruction.text = INSTRUCTION_PLAYCARD;
                Hand.Init(handIds, true, localPlayer.State == InGamePlayerModel.InGameState.Waiting);
                int k = localPlayer.State == InGamePlayerModel.InGameState.Done ? 1 : 0;
                foreach (InGamePlayerModel otherPlayer in otherPlayers)
                {
                    k += otherPlayer.State == InGamePlayerModel.InGameState.Done ? 1 : 0;
                }
                Table.Init(otherPlayers.Length + 1, Storyteller.Nickname, theme, new InGameCardModel[k], false);
                break;
            case Phase.PickCard:
                HUD.Instruction.text = INSTRUCTION_PICKCARD;
                Hand.Init(handIds);
                Table.Init(otherPlayers.Length + 1, Storyteller.Nickname, theme, tableCards, true, true);
                break;
            case Phase.ShowScore:
                HUD.Instruction.text = INSTRUCTION_SHOWSCORE;
                Hand.Init(handIds);
                Table.Init(otherPlayers.Length + 1, Storyteller.Nickname, theme, tableCards, true, false, voteResult);
                break;
        }
    }    

    public void SetStoryteller(string storytellerId, string theme="")
    {
        if (m_LocalPlayer.UserId == storytellerId)
        {
            HUD.Instruction.text = INSTRUCTION_CHOOSETHEME_STORYTELLER;
            m_LocalPlayer.IsStoryteller = true;
            
            m_LocalPlayer.State = InGamePlayerModel.InGameState.Waiting;
            Table.SetStorytellerName(m_LocalPlayer.Nickname);
            Table.EnableThemeInput();
        } else
        {
            foreach (InGamePlayerModel otherPlayer in m_OtherPlayers)
            {
                if (otherPlayer.UserId == storytellerId)
                {
                    HUD.Instruction.text = INSTRUCTION_CHOOSETHEME_OTHERS;
                    otherPlayer.IsStoryteller = true;
                    otherPlayer.State = InGamePlayerModel.InGameState.Waiting;
                    Table.SetTheme(theme);
                    Table.SetStorytellerName(otherPlayer.Nickname);
                       
                }
                else
                {
                    otherPlayer.State = InGamePlayerModel.InGameState.Done;
                }
            }
        }
    }

    private void DrawHand(string[] cardIds)
    {
        Hand.Draw(cardIds);
        Hand.SetInteractable = true;
    }

    public void DrawCardFromDeck(string cardId, CardSlot targetSlot)
    {
        Deck.DrawCard(cardId, targetSlot);
    }

    public Card InstantiateCard(string cardId, string ownerId, Transform anchor)
    {
        return Deck.FetchAndInstantiateCard(cardId, ownerId, anchor);
    }

    public CardSlot AllocateTableSlot()
    {
        return Table.AllocateSlot();
    }

    public bool ConfirmTheme(string theme)
    {
        if (!m_LocalPlayer.IsStoryteller || m_CurrentPhase != Phase.ChooseTheme) { return false; }
        Debug.Log(theme);
        GameObject go = GameObject.Find("NetworkService");

        Network network = (Network)go.GetComponent(typeof(Network));
        network.ConfirmTheme(theme);
        return true;
    }

    public bool PlayCard(string cardId)
    {
        if (m_CurrentPhase != Phase.PlayCard) { return false; }
        //todo : network api
        //return Network.PlayCard(currentSessionId, localPlayerId, cardId);
        if (true)
        {
            m_LocalPlayer.State = InGamePlayerModel.InGameState.Done;
            HUD.InGamePlayerList.ForcePlayerViewUpdate(m_LocalPlayer.UserId);
        }
        return true;
    }

    public void UpdateOtherPlayerState(string playerId, InGamePlayerModel.InGameState newState)
    {
        InGamePlayerModel player = m_OtherPlayers.First(p => p.UserId == playerId);
        player.State = newState;
        HUD.InGamePlayerList.ForcePlayerViewUpdate(playerId);
    }
}
