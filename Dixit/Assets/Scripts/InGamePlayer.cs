using UnityEngine;
using UnityEngine.Events;
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

    [SerializeField]
    private StateView[] m_StateViews = null;

    [Header("Events")]
    public StringEvent onNicknameUpdate = new StringEvent();
    public StringEvent onStateTextUpdate = new StringEvent();
    public ColorEvent onStateColorUpdate = new ColorEvent();
    public StringEvent onScoreUpdate = new StringEvent(); 

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
        //StateView stateView = new StateView(); 
        onStateColorUpdate.Invoke(stateView.Color);
        onStateTextUpdate.Invoke(stateView.Text);
        onScoreUpdate.Invoke(m_PlayerModel.Score.ToString());
    }
}
