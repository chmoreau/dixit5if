using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MatchMakingView : MonoBehaviour
{
    private const string TIME_FORMAT = "{0}:{1}:{2}";

    [SerializeField]
    private Animator m_MenuAnimator = null;
    [SerializeField]
    private Text m_TickingDisplay = null;
    [SerializeField]
    private Animator[] m_PlayerIconAnimator = new Animator[6];
    private string m_Username = string.Empty;
    bool stopSearching = false;

    private IEnumerator m_TickCoroutine = null;

    public void StartMatchMaking(int expectedPlayers = 3)
    {
        JoinMatchMaking();

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

    private IEnumerator Tick()
    {
        float timer = 0f;
        while (true)
        {
            int hours = (int)timer / 3600;
            int minutes = ((int)timer % 3600) / 60;
            int seconds = (int)timer % 60;
            m_TickingDisplay.text = string.Format(TIME_FORMAT, hours.ToString("0"), minutes.ToString("00"), seconds.ToString("00"));
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
    }

    public void JoinMatchMaking()
    {
        GameObject go = GameObject.Find("NetworkService");
        Network network = (Network)go.GetComponent(typeof(Network));
        network.JoinMatchmaking();
        m_TickCoroutine = Tick();
        StartCoroutine(m_TickCoroutine);
    }

    public void StopMatchMaking()
    {
        stopSearching = true;
        GameObject go = GameObject.Find("NetworkService");
        Network network = (Network)go.GetComponent(typeof(Network));
        network.Disconnect();
        StopCoroutine(m_TickCoroutine);
        m_TickCoroutine = null;
    }
}
