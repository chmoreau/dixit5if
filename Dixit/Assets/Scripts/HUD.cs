using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD : MonoBehaviour {
    public Text Instruction = null;
    public InGamePlayerList InGamePlayerList = null;
    public GameObject OptionsContent = null;
    public Button NextButton = null;

    public void Init()
    {
        //Reset();
        InGamePlayerList.Init();
        InGamePlayerList.SetVisibile();
        Instruction.text = GameSession.INSTRUCTION_INITSESSION;
        EnableNextButton(false);
        NextButton.onClick.AddListener(() => { GameSessionService.CurrentGameSession.ReadyForNextRound(); });
    }

    public void Reset()
    {
        InGamePlayerList.Reset();
        Instruction.text = GameSession.INSTRUCTION_INITSESSION;
    }

    public void ChangePlayerListVisibility()
    {
        InGamePlayerList.ChangeListVisibility();
    }

    public void ChangeOptionsVisibility()
    {
        OptionsContent.SetActive(!OptionsContent.activeSelf);
    }

    public void EnableNextButton(bool isEnabled)
    {
        NextButton.interactable = isEnabled;
        NextButton.gameObject.SetActive(isEnabled);
    }

    public void SetNextButtonInteractable(bool isInteractable)
    {
        NextButton.interactable = isInteractable;
    }
}
