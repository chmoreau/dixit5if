using UnityEngine;
using System;
using System.Collections;

public class TransformAnimation {
    public delegate void AnimationCallback();

    public static IEnumerator FromToAnimation(GameObject gameObject, Transform fromTransform, Transform toTransform, float duration, AnimationCallback onStartCallback, AnimationCallback onEndCallback)
    {
        float timer = duration;
        if (onStartCallback != null)
        {
            onStartCallback();
        }
        while (timer >= 0)
        {
            timer -= Time.deltaTime;
            float process = (duration - timer) / duration;
            gameObject.transform.position = Vector3.Lerp(fromTransform.position, toTransform.position, process);
            gameObject.transform.rotation = Quaternion.Slerp(fromTransform.rotation, toTransform.rotation, process);
            yield return new WaitForEndOfFrame();
        }
        if (onEndCallback != null)
        {
            onEndCallback();
        }
    }
}
