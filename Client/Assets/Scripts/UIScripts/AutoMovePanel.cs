using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class AutoMovePanel : MonoBehaviour
{
    [SerializeField, LabelText("세로 패널 위치")] private Transform portraitTarget;
    [SerializeField, LabelText("가로 패널 위치")] private Transform landscapeTarget;
    [SerializeField, LabelText("이동 속도")] private float moveDuration = 0.5f;
    
    private Camera renderCamera;
    private RectTransform rectTransform;
    private Tween moveTween;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (renderCamera == null)
            renderCamera = Camera.main;
    }

    private void OnEnable()
    {
        UIManager.OnOrientationChanged += HandleOrientationChanged;
    }

    private void OnDisable()
    {
        UIManager.OnOrientationChanged -= HandleOrientationChanged;
        moveTween?.Kill();
    }

    private void HandleOrientationChanged(bool isPortrait)
    {
        if (!portraitTarget || !landscapeTarget) return;

        moveTween?.Kill();

        Transform target = isPortrait ? portraitTarget : landscapeTarget;

        Vector2 localPoint;
        RectTransform parentRect = rectTransform.parent as RectTransform;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            RectTransformUtility.WorldToScreenPoint(renderCamera, target.position),
            renderCamera,
            out localPoint
        );

        moveTween = rectTransform.DOAnchorPos(localPoint, moveDuration)
            .SetEase(Ease.OutCubic);
    }
}
