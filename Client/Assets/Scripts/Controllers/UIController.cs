using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class UIController : MonoBehaviour
{
    
    [LabelText("사용할 UI 리스트")]
    public List<GameObject> usingUIList;
    protected Canvas rootCanvas { get; private set; }
    protected bool Initialized { get; private set; }
    
    protected virtual void Awake()
    {
        // 자신 또는 자식에서 Canvas 검색
        rootCanvas = GetComponent<Canvas>() ?? GetComponentInChildren<Canvas>();

        // 게임 시작 직후 바로 Visible 상태라면 미리 초기화
        if (gameObject.activeInHierarchy)
            Initialize();
    }
    
    public virtual void Initialize()
    {
        if (Initialized) return;
        Initialized = true;
    }
    
    public virtual void Show(GameObject gameObject)
    {
        if (!Initialized) Initialize();

        gameObject.SetActive(true);
        
    }

    // Show 직후 실행할 기능 있는 경우
    public virtual void Show(GameObject gameObject, Action action)
    {
        if (!Initialized) Initialize();

        gameObject.SetActive(true);
        
        action();
    }
    
    public virtual void Hide(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }

    public virtual void Hide(GameObject gameObject, Action action)
    {
        action();
        gameObject.SetActive(false);
    }

}
