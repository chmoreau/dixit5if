using UnityEngine;
using System.Collections;

public class FrameRate : MonoBehaviour {
    void Awake()
    {
#if UNITY_IOS || UNITY_ANDROID
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
#endif
    }
}
