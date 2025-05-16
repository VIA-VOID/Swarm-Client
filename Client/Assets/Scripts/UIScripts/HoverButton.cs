using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Options")]
    [Range(1f, 2f)]
    public float scaleMultiplier = 1.08f;
    public float tweenTime = 0.12f;

    RectTransform rect;
    Vector3 originalScale;
    Coroutine tweenCo;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
    }

    public void OnPointerEnter(PointerEventData _)
    {
        StartTween(originalScale * scaleMultiplier);
    }

    public void OnPointerExit(PointerEventData _)
    {
        StartTween(originalScale);
    }

    void StartTween(Vector3 target)
    {
        if (tweenCo != null) StopCoroutine(tweenCo);
        tweenCo = StartCoroutine(TweenScale(target));
    }

    IEnumerator TweenScale(Vector3 target)
    {
        Vector3 start = rect.localScale;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / tweenTime;
            rect.localScale = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        rect.localScale = target;
    }
}
