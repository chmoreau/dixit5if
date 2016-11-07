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
    public const string INSTRUCTION_CHOOSETHEME_STORYTELLER = "Vous etes le conteur de ce tour, écrivez le thème du tour";
    public const string INSTRUCTION_CHOOSETHEME_OTHERS = "Attendez que le thème soit choisi par le conteur";
    public const string INSTRUCTION_PLAYCARD = "Choisissez une carte qui vous fait penser au thème";
    public const string INSTRUCTION_PICKCARD_STORYTELLER = "Attendez que les autres joueus finissent leur votes";
    public const string INSTRUCTION_PICKCARD_OTHERS = "Votez pour la carte que vous pensez etre celle du conteur";
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
    public InGamePlayerModel GetPlayer(string playerId)
    {
        return m_LocalPlayer.UserId == playerId ? m_LocalPlayer : m_OtherPlayers.FirstOrDefault(p => p.UserId == playerId);
    }

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
                    InitAllPlayersState();
                    HUD.Instruction.text = INSTRUCTION_DRAWHAND;
                    m_CurrentPhase = Phase.DrawHand;
                    DrawHand((string[])args[0]);
                    //HUD.InGamePlayerList.ForceAllViewsUpdate();
                }
                break;
            case Phase.DrawHand :
                if (targetPhase == Phase.ChooseTheme)
                {
                    InitAllPlayersState();
                    m_CurrentPhase = Phase.ChooseTheme;
                    //Hand.SetInteractable = true;
                    SetStoryteller((string)args[0]);
                    HUD.InGamePlayerList.ForceAllViewsUpdate();
                }
                break;
            case Phase.ChooseTheme:
                if (targetPhase == Phase.PlayCard)
                {
                    InitAllPlayersState();
                    HUD.Instruction.text = INSTRUCTION_PLAYCARD;
                    m_CurrentPhase = Phase.PlayCard;
                    Hand.SetPlayable = true;
                }
                break;
            case Phase.PlayCard :
                if (targetPhase == Phase.PickCard)
                {
                    InitAllPlayersState();
                    HUD.Instruction.text = m_LocalPlayer.IsStoryteller ? INSTRUCTION_PICKCARD_STORYTELLER : INSTRUCTION_PICKCARD_OTHERS;
                    Storyteller.State = InGamePlayerModel.InGameState.Done;
                    HUD.InGamePlayerList.ForcePlayerViewUpdate(Storyteller.UserId);
                    m_CurrentPhase = Phase.PickCard;
                    Table.ShuffleAndRevealCards((string[])args[0]);
                }
                break;
            case Phase.PickCard :
                if (targetPhase == Phase.ShowScore)
                {
                    InitAllPlayersState();
                    HUD.Instruction.text = INSTRUCTION_SHOWSCORE;
                    m_CurrentPhase = Phase.ShowScore;
                    // args[0] => InGameCardModel[] cardResults : cardId, ownerId, isThemeCard
                    // args[1] => Dictionary<string, DataPair<string, int>> playerResults : playerId, <votedCardId, playerdeltaScore>
                    ProcessResults((InGameCardModel[])args[0], (Dictionary<string, DataPair<string, int>>)args[1]);
                    HUD.EnableNextButton(true);
                }
                break;
            case Phase.ShowScore :
                if (targetPhase == Phase.DrawHand)
                {
                    InitAllPlayersState();
                    HUD.EnableNextButton(false);
                    Table.Clear();
                    HUD.Instruction.text = INSTRUCTION_DRAWHAND;
                    m_CurrentPhase = Phase.DrawHand;
                    DrawHand((string[])args[0]);
                }
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
        HUD.InGamePlayerList.ForceAllViewsUpdate();
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
                HUD.Instruction.text = localPlayer.IsStoryteller ? INSTRUCTION_PICKCARD_STORYTELLER : INSTRUCTION_PICKCARD_OTHERS;
                Hand.Init(handIds);
                Table.Init(otherPlayers.Length + 1, Storyteller.Nickname, theme, tableCards, true, !localPlayer.IsStoryteller);
                break;
            case Phase.ShowScore:
                HUD.Instruction.text = INSTRUCTION_SHOWSCORE;
                Hand.Init(handIds);
                Table.Init(otherPlayers.Length + 1, Storyteller.Nickname, theme, tableCards, true, false, voteResult);
                break;
        }
    }    

    public void SetStoryteller(string storytellerId)
    {
        if (m_LocalPlayer.UserId == storytellerId)
        {
            HUD.Instruction.text = INSTRUCTION_CHOOSETHEME_STORYTELLER;
            m_LocalPlayer.IsStoryteller = true;
            Table.SetStorytellerName(m_LocalPlayer.Nickname);
            Table.EnableThemeInput();
        }
        else
        {
            m_LocalPlayer.State = InGamePlayerModel.InGameState.Done;
        }
        foreach (InGamePlayerModel otherPlayer in m_OtherPlayers)
        {
            if (otherPlayer.UserId == storytellerId)
            {
                HUD.Instruction.text = INSTRUCTION_CHOOSETHEME_OTHERS;
                otherPlayer.IsStoryteller = true;
                Table.SetStorytellerName(otherPlayer.Nickname);
            }
            else
            {
                otherPlayer.State = InGamePlayerModel.InGameState.Done;
            }
        }
    }

    private void DrawHand(string[] cardIds)
    {
        Hand.Draw(cardIds);
        //Hand.SetInteractable = true;
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
        //network api
        FindObjectOfType<Network>().ConfirmTheme(theme);
        if (true)
        {
            m_LocalPlayer.State = InGamePlayerModel.InGameState.Done;
            HUD.InGamePlayerList.ForcePlayerViewUpdate(m_LocalPlayer.UserId);
        }
        return true;
    }

    public void SetTheme(string theme)
    {
        if (m_CurrentPhase != Phase.ChooseTheme) { return; }

        Table.SetTheme(theme);
        if (Storyteller != m_LocalPlayer)
        {
            UpdateOtherPlayerState(Storyteller.UserId, InGamePlayerModel.InGameState.Done);
        }
    }

    public bool PlayCard(string cardId)
    {
        if (m_CurrentPhase != Phase.PlayCard) { return false; }
        //network api
        FindObjectOfType<Network>().PlayCard(cardId);
        if (true)
        {
            m_LocalPlayer.State = InGamePlayerModel.InGameState.Done;
            HUD.InGamePlayerList.ForcePlayerViewUpdate(m_LocalPlayer.UserId);
        }
        return true;
    }

    public void PutOtherPlayerPlayedCard(string playerId)
    {
        if (m_CurrentPhase != Phase.PlayCard) { return; }

        if (m_OtherPlayers.FirstOrDefault(p => p.UserId == playerId).State == InGamePlayerModel.InGameState.Done) { return; }
        Table.PutOtherPlayerPlayedCard(playerId);
        UpdateOtherPlayerState(playerId, InGamePlayerModel.InGameState.Done);
    }

    public bool PickCard(string cardId)
    {
        if (m_CurrentPhase != Phase.PickCard) { return false; }
        //network api
        FindObjectOfType<Network>().PickCard(cardId);
        if (true)
        {
            m_LocalPlayer.State = InGamePlayerModel.InGameState.Done;
            HUD.InGamePlayerList.ForcePlayerViewUpdate(m_LocalPlayer.UserId);
        }
        return true;
    }

    public void MarkOtherPlayerPickCard(string playerId)
    {
        if (m_CurrentPhase != Phase.PickCard) { return; }
        
        UpdateOtherPlayerState(playerId, InGamePlayerModel.InGameState.Done);
    }

    public void ProcessResults(InGameCardModel[] cardResults, Dictionary<string, DataPair<string, int>> playerResults)
    {
        foreach (InGameCardModel cardResult in cardResults)
        {
            CardSlot slot = Table.CardSlots.FirstOrDefault(s => s.Card.CardId == cardResult.CardId);
            if (slot)
            {
                slot.Card.SetOwner(cardResult.OwnerId);
                slot.ShowOwner();
                slot.EnableHighlight(cardResult.IsThemeCard);
            }
        }
        foreach (KeyValuePair<string, DataPair<string, int>> playerResult in playerResults)
        {
            if (playerResult.Key != m_LocalPlayer.UserId && playerResult.Key != Storyteller.UserId)
            {
                CardSlot slot = Table.CardSlots.FirstOrDefault(s => s.Card.CardId == playerResult.Value.Value1);
                if (slot)
                {
                    slot.AddVoter(playerResult.Key);
                }
            }
            // update scores
            GetPlayer(playerResult.Key).AddPoints(playerResult.Value.Value2);
            HUD.InGamePlayerList.AddScore(playerResult.Key, playerResult.Value.Value2);
        }
    }

    public void ReadyForNextRound()
    {
        if (m_CurrentPhase != Phase.ShowScore) { return; }
        // todo : Network api
        // all players ready before server send next round data!
        // Network.ReadyForNext();

        LocalPlayer.State = InGamePlayerModel.InGameState.Done;
        HUD.InGamePlayerList.ForcePlayerViewUpdate(LocalPlayer.UserId);
    }

    public void MarkOtherPlayerReadyForNext(string playerId)
    {
        if (m_CurrentPhase != Phase.ShowScore) { return; }

        UpdateOtherPlayerState(playerId, InGamePlayerModel.InGameState.Done);
    }

    public void StartNextRound(string[] newHandIds, string newStorytellerId)
    {
        TranslateToPhase(Phase.DrawHand, (object)newHandIds);
        TranslateToPhase(Phase.ChooseTheme, (object)newStorytellerId);
    }

    public void UpdateScore(string playerId, int score)
    {
        GetPlayer(playerId).Score = score;
        HUD.InGamePlayerList.ForcePlayerViewUpdate(playerId);
    }

    public void UpdateOtherPlayerState(string playerId, InGamePlayerModel.InGameState newState)
    {
        InGamePlayerModel player = m_OtherPlayers.First(p => p.UserId == playerId);
        player.State = newState;
        HUD.InGamePlayerList.ForcePlayerViewUpdate(playerId);
    }
}
