using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

/*-------------------------------------------------------
				LoginSceneController

- LoginScene UI 관리
--------------------------------------------------------*/
public class LoginSceneController : SceneController
{
    [Header("로딩 관련")]
    [SerializeField, LabelText("로딩바 이미지")] private OrientationObject loadingBarImage;
    [SerializeField, LabelText("로고 이미지")]  private OrientationObject logoImage;
    [SerializeField, LabelText("메인 패널")] private OrientationObject gameStartButton;

    [Header("씬 오브젝트")]
    [SerializeField, LabelText("씬오브젝트")] private OrientationObject sceneObjectParent;
    [SerializeField, LabelText("메인 패널")] private OrientationObject sceneTransition;

    [Header("Page0 요소")]
    //[SerializeField, LabelText("")]
    
    [Header("Page1 요소")]
    [SerializeField, LabelText("비디오 패널")] private GameObject videoPanel;
    
    [Header("Page2 요소")]
    
    [Header("UI 버튼")]
    [SerializeField, LabelText("캐릭터 선택 패널UI")] private OrientationObject characterSelectUIPanel;
    [SerializeField, LabelText("설정창")] private Button settingBtn;

    private int currentIndex = -1;     // ‑1이면 아무 것도 선택 안 한 상태

    private SoundManager soundManager;
    private UIManager uiManager;

    private void Start()
    {
        Show(loadingBarImage, () => FillSlider(1));
        
        SoundManager.Instance.PlayBGM(0);
    }
    
    // 로딩 진행 상황따라 그래프 값
    public void FillSlider(float fillAmount)
    {        
        Hide(loadingBarImage);
        //Show(ChangePage());
    }

    public void Fade()
    {
        //Hide(touchBG);
        //Hide(mainPanel);

        Show(sceneTransition);
        Show(sceneObjectParent);

        //StartCoroutine(SceneTransitionRoutine());
    }

    // 로딩 완료후 메인 페이지 전환
    public void ChageMainPage()
    {
        videoPanel.SetActive(true);
    }
    
    // 설정창 전환
    public void OpenSettingUI()
    {
        SoundManager.Instance.PlaySFX(0);
    }
    
    // 캐릭터 선택 화면 전환
    public void OpenCharacterSelectUI()
    {
        videoPanel.SetActive(false);
        
        OrientationObject getUIPanel = usingUIList[1];
        
        SoundManager.Instance.PlaySFX(0);
        
        ChangePage(getUIPanel);
    }
    
    public void MoveCharacterSelectScene()
    {
        
    }

    public void MoveWorldScene()
    {
        
    }

    public void MoveDungeonScene()
    {
        
    }
}
