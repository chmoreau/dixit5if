using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

public class InGamePlayer : MonoBehaviour {
    [Serializable]
    private class StateView
    {
        public InGamePlayerModel.InGameState State = 0;
        public string Text = null;
        public Color Color = default(Color);
    }

    [Serializable]
    public class StringEvent : UnityEvent<string> { };
    [Serializable]
    public class ColorEvent : UnityEvent<Color> { };
    [Serializable]
    public class BoolEvent : UnityEvent<bool> { };

    [SerializeField]
    private Animator m_Animator = null;
    [SerializeField]
    private Text m_DeltaScore = null;
    [SerializeField]
    private StateView[] m_StateViews = null;

    [Header("Events")]
    public StringEvent onNicknameUpdate = new StringEvent();
    public StringEvent onStateTextUpdate = new StringEvent();
    public ColorEvent onStateColorUpdate = new ColorEvent();
    public StringEvent onScoreUpdate = new StringEvent();
    public BoolEvent onIsStoryteller = new BoolEvent();

    private InGamePlayerModel m_PlayerModel = null;
    public InGamePlayerModel PlayerModel { get { return m_PlayerModel; } }

    public void LoadModel(InGamePlayerModel model)
    {
        m_PlayerModel = model;
        UpdateView();
    }

    public void UpdateView()
    {
        onNicknameUpdate.Invoke(m_PlayerModel.Nickname);
        StateView stateView = m_StateViews.First(s => s.State == m_PlayerModel.State);
        onStateColorUpdate.Invoke(stateView.Color);
        onStateTextUpdate.Invoke(stateView.Text);
        onScoreUpdate.Invoke(m_PlayerModel.Score.ToString());
        onIsStoryteller.Invoke(m_PlayerModel.IsStoryteller);
    }

    public void ResetDeltaScore()
    {
        m_DeltaScore.text = "";
    }

    public void AddScore(int deltaScore)
    {
        if (deltaScore > 0)
        {
            //m_PlayerModel.AddPoints(deltaScore);
            m_DeltaScore.text = deltaScore.ToString();
            m_Animator.SetTrigger("scoreUp");
            onScoreUpdate.Invoke(m_PlayerModel.Score.ToString());
        }
    }
}
