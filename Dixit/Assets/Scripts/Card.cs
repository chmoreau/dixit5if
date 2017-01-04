using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer m_FrontMeshRender = null;
    [Header("Model")]
    [SerializeField]
    private string m_OwnerId = null;
    public string OwnerId { get { return m_OwnerId; } }
    [SerializeField]
    private CardModel m_CardModel = null;
    public string CardId { get { return m_CardModel != null ? m_CardModel.CardId : null; } }

    public void SetOwner(string ownerId)
    {
        m_OwnerId = ownerId;
    }

    public void LoadModel(CardModel model)
    {
        m_CardModel = model;
        if (m_CardModel != null)
        {
            m_FrontMeshRender.material.mainTexture = m_CardModel.CardImage;
        }
    }
}
