using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoginView : MonoBehaviour
{
    [SerializeField]
    private Animator m_MenuAnimator = null;
    [SerializeField]
    private Text m_ErrorDisplay = null;
    private string m_Username = string.Empty;

    public string Username { set { m_Username = value; } }

    // Network.isLoggedIn
    bool isLoggedIn = false;

    void Update()
    {
        m_MenuAnimator.SetBool("isLoggedIn", isLoggedIn);
    }

    public void LogIn()
    {
        if (string.IsNullOrEmpty(m_Username))
        {
            ShowErrMsg("Erreur : nom d'utilisateur vide");
            return;
        }

        GameObject go = GameObject.Find("NetworkService");
        Network network = (Network)go.GetComponent(typeof(Network));
        network.UserName = m_Username;
        isLoggedIn = true;
    }

    public void ShowErrMsg(string error)
    {
        m_ErrorDisplay.text = error;
    }
}
