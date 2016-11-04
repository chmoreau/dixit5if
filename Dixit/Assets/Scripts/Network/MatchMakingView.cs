using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MatchMakingView : MonoBehaviour {
    [SerializeField]
    private Animator m_MenuAnimator = null;
    [SerializeField]
    private Animator[] m_PlayerIconAnimator = new Animator[6];
    private string m_Username = string.Empty;
    
    public void StartMatchMaking(int expectedPlayers = 3)
    {
        for (int i = 0; i < 6; i++)
        {
            m_PlayerIconAnimator[i].gameObject.SetActive(i < expectedPlayers);            
        }
        ShowMatchMakingProgress();
        m_MenuAnimator.SetTrigger("toMatchMaking");
    }

    public void ShowMatchMakingProgress(int foundPlayers = 1)
    {
        for (int i = 0; i < 6; i++)
        {
            if (m_PlayerIconAnimator[i].gameObject.activeSelf)
            {
                m_PlayerIconAnimator[i].SetBool("isOccupied", i < foundPlayers);
            }
        }
    }
}
