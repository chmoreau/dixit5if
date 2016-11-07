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
    private Coroutine currentCoroutine = null;

    [SerializeField]
    private Text timerText;

    public void StartMatchMaking(int expectedPlayers = 3)
    {
        JoinMatchMaking();
        // set up the timer
        timerText = GameObject.Find("Time").GetComponent<Text>();
        
        if(currentCoroutine!=null)
            StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(UpdateTimer(0));
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

    private IEnumerator UpdateTimer(int timer)
    {
        Searching = true;
        while(Searching)
        {
            int number = timer;
            int ms = timer % 10;
            timer /= 10;
            int sec = timer % 60;
            timer /= 60;
            int min = timer % 60;
            string minutes = (min).ToString();
            string seconds = (sec).ToString();
            if (seconds.Length == 1) seconds = '0' + seconds;
            if (minutes.Length == 1) minutes = '0' + minutes;
            timerText.text = minutes + ":" + seconds + ":" + (ms).ToString();
            timer = number+1;
            
            yield return new WaitForSeconds((float)0.1);
        }
    }
}
