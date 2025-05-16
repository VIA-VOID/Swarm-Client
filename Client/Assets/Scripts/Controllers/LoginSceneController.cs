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
    [SerializeField, LabelText("씬 전환 연출")] private OrientationObject sceneTransition;

    [Header("Page0 요소")]
    [SerializeField, LabelText("로딩바 이미지")] private OrientationObject loadingBarImage;
    
    [Header("Page1 요소")]
    [SerializeField, LabelText("비디오 패널")] private GameObject videoPanel;
    [SerializeField, LabelText("설정창")] private OrientationObject settingBtn;
    
    [Header("Page2 요소")]

    [Header("Page3 요소")] // 설정 패널
    
    private int currentIndex = -1;     // ‑1이면 아무 것도 선택 안 한 상태

    private SoundManager soundManager;
    private UIManager uiManager;

    private void Start()
    {
        soundManager = SoundManager.Instance;
        uiManager = UIManager.Instance;

        OrientationObject page0 = usingUIList[0];
        ChangePage(page0);
        
        Show(loadingBarImage, () => FillSliderWithTime());
    }
    
    // 로딩 진행 상황따라 그래프 값
    public void FillSlider(float fillAmount)
    {        
        Hide(loadingBarImage);
        //Show(ChangePage());
    }

    public void FillSliderWithTime()
    {
        StartCoroutine(FillRoutine());
    }
    
    IEnumerator FillRoutine()
    {
        var imgPort = loadingBarImage.GetUIObj(true).GetComponent<Image>();
        var imgLand = loadingBarImage.GetUIObj(false).GetComponent<Image>();
        
        imgPort.fillAmount = 0;
        imgLand.fillAmount = 0;
        
        float t = 0;

        // 임시라 3초로 고정
        while (t < 3)
        {
            t += Time.deltaTime;
            imgPort.fillAmount = Mathf.Clamp01(t / 3);
            imgLand.fillAmount = Mathf.Clamp01(t / 3);
            yield return null;
        }

        imgPort.fillAmount = 1;
        imgLand.fillAmount = 1;
        
        soundManager.PlayBGM(0);
        
        Hide(loadingBarImage);

        ChageMainPage();
    }
    
    public void Fade()
    {
        //Hide(touchBG);
        //Hide(mainPanel);

        Show(sceneTransition);
        //Show(sceneObjectParent);

        //StartCoroutine(SceneTransitionRoutine());
    }

    // 로딩 완료후 메인 페이지 전환
    public void ChageMainPage()
    {
        OrientationObject page2 = usingUIList[1];
        
        ChangePage(page2);
        
        videoPanel.SetActive(true);
    }
    
    // 설정창 전환
    public void OpenSettingUI()
    {
        soundManager.PlaySFX(0);
    }
    
    // 캐릭터 선택 화면 전환
    public void OpenCharacterSelectUI()
    {
        videoPanel.SetActive(false);
        
        OrientationObject getUIPanel = usingUIList[2];
        
        soundManager.PlaySFX(0);
        
        //ChangePage(getUIPanel);
        // 우선 임시로 바로 캐릭터 생성 화면으로 넘김
        soundManager.PlayBGM(2, 0.5f, () => uiManager.MoveScene(1));
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
