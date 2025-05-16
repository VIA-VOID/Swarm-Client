using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/*-------------------------------------------------------
				SceneController

- 각 씬의 씬 컨트롤러 부모 클래스
--------------------------------------------------------*/

[Serializable]
public abstract class SceneController : MonoBehaviour
{
    [SerializeField, LabelText("세로 UI 패널")] private GameObject portraitPanel;
    [SerializeField, LabelText("가로 UI 패널")] private GameObject landScapePanel;
    
    [LabelText("사용할 UI 리스트")]
    public List<OrientationObject> usingUIList;

    private OrientationObject currentPage;
    
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

    // 가로 세로 판별
    public GameObject GetPanel(bool isPortrait)
    {
        return isPortrait ? portraitPanel : landScapePanel;
    }
    
    // 초기화
    public virtual void Initialize()
    {
        if (Initialized) return;
        Initialized = true;
    }

    // 페이지 변경
    public virtual void ChangePage(OrientationObject newPage)
    {
        if (!Initialized) Initialize();

        // 같은 페이지로 전환하려고 하면 무시 (선택)
        if (currentPage == newPage) return;

        // 이전 페이지 숨김
        if (currentPage != null)
            Hide(currentPage);

        // 새 페이지 표시
        Show(newPage);
        currentPage = newPage;
    }
    
    public virtual void Show(OrientationObject uiObj)
    {
        if (!Initialized) Initialize();

        uiObj.SetUIActive(true);
    }

    // Show 직후 실행할 기능 있는 경우
    public virtual void Show(OrientationObject uiObj, Action action)
    {
        if (!Initialized) Initialize();

        uiObj.SetUIActive(true);
        
        action();
    }
    
    public virtual void Hide(OrientationObject uiObj)
    {
        uiObj.SetUIActive(false);
    }

    // Hide 직전 실행할 기능이 있는 경우
    public virtual void Hide(OrientationObject uiObj, Action action)
    {
        action();
        uiObj.SetUIActive(false);
    }

}
