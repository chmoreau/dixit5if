using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MatchMakingView : MonoBehaviour
{
    [SerializeField]
    private Animator m_MenuAnimator = null;
    [SerializeField]
    private Animator[] m_PlayerIconAnimator = new Animator[6];
    private string m_Username = string.Empty;
    bool Searching = true;

    [SerializeField]
    private Text timerText;
    private int timer;

    public void StartMatchMaking(int expectedPlayers = 3)
    {
        JoinMatchMaking();
        // set up the timer
        timerText = GameObject.Find("Time").GetComponent<Text>();
        timer = 0;
        StartCoroutine(UpdateTimer());
        for (int i = 0; i < 6; i++)
        {
            m_PlayerIconAnimator[i].gameObject.SetActive(i < expectedPlayers);
        }
        ShowMatchMakingProgress();
        m_MenuAnimator.SetTrigger("toMatchMaking");
    }

    public void ShowMatchMakingProgress(int foundPlayers = 0)
    {
        for (int i = 0; i < 6; i++)
        {
            if (m_PlayerIconAnimator[i].gameObject.activeSelf)
            {
                m_PlayerIconAnimator[i].SetBool("isOccupied", i < foundPlayers);
            }
        }
    }

    void update()
    {
    }

    public void JoinMatchMaking()
    {
 
        GameObject go = GameObject.Find("NetworkService");
        Network network = (Network)go.GetComponent(typeof(Network));
        network.JoinMatchmaking();
    }

    public void StopMatchMaking()
    {
        Searching = false;
        GameObject go = GameObject.Find("NetworkService");
        Network network = (Network)go.GetComponent(typeof(Network));
        network.Disconnect();
        m_MenuAnimator.SetTrigger("toMainMenu");
    }

    private IEnumerator UpdateTimer()
    {
        while(Searching)
        {
            timerText.text = (++timer).ToString();
            yield return new WaitForSeconds(1);
        }
    }
}
