using UnityEngine;
using UnityEngine.EventSystems;

public class Outline : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [SerializeField] private RenderingLayerMask outlineLayer;
    [SerializeField] private Activate activate = Activate.OnHover;

    private Renderer[] renderers;  
    private uint originalLayerMask;
    private bool isOutlineActive;

    private enum Activate { OnHover, OnClick }
    
    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);

        originalLayerMask = renderers.Length > 0
            ? renderers[0].renderingLayerMask          // uint
            : 0u;

        SetOutline(false);                             // 시작은 꺼둠
    }

    public void OnPointerEnter(PointerEventData _)  { if (activate == Activate.OnHover) SetOutline(true);  }
    public void OnPointerExit (PointerEventData _)  { if (activate == Activate.OnHover) SetOutline(false); }
    public void OnPointerClick(PointerEventData _)
    {
        if (activate != Activate.OnClick) return;
        isOutlineActive = !isOutlineActive;
        SetOutline(isOutlineActive);
    }
    
    public void SetOutline(bool enable)
    {
        if (renderers == null || renderers.Length == 0) return;   // 안전장치

        uint outlineMask = outlineLayer;      // RenderingLayerMask → uint

        foreach (var rend in renderers)
        {
            rend.renderingLayerMask = enable
                ? originalLayerMask | outlineMask   // 두 마스크 OR
                : originalLayerMask;
        }
    }
}