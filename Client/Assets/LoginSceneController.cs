using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class LoginSceneController : SceneController
{
    [Header("로딩 관련")]
    [SerializeField, LabelText("로딩바 이미지")]   private GameObject loadingBarImage;
    [SerializeField, LabelText("로딩바 대기 시간")] private float duration = 3f;
    [SerializeField, LabelText("로딩 배경 이미지")]  private GameObject touchBG;
    [SerializeField, LabelText("메인 패널")] private GameObject mainPanel;

    [Header("씬 오브젝트")]
    [SerializeField] private GameObject sceneObjectParent;
    [SerializeField] private GameObject sceneTransition;

    // [Header("선택 가능한 캐릭터")]
    // [SerializeField] private List<SelectableCharacter> selectableCharacters;

    [Header("카메라 이동")]
    [SerializeField, LabelText("메인 카메라")] private Camera mainCam;
    [SerializeField, LabelText("카메라 이동 시간")] private float camMoveTime = 1f;
    [SerializeField, LabelText("트랜지션 시작 위치")] private Transform camStartPoint;
    [SerializeField, LabelText("트랜지션 종료 위치")] private Transform camOriginPos;

    [Header("UI 버튼")]
    [SerializeField, LabelText("캐릭터 선택 패널UI")] private GameObject characterSelectUIPanel;
    [SerializeField, LabelText("설정창")] private Button settingBtn;

    private int currentIndex = -1;     // ‑1이면 아무 것도 선택 안 한 상태

    private void Start()
    {
        Show(loadingBarImage, Loading);
        
        SoundManager.Instance.PlayBGM(0);
    }

    void Loading() => StartCoroutine(FillRoutine());

    IEnumerator FillRoutine()
    {
        var img = loadingBarImage.GetComponent<Image>();
        img.fillAmount = 0;
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            img.fillAmount = Mathf.Clamp01(t / duration);
            yield return null;
        }

        img.fillAmount = 1;
        Hide(loadingBarImage);
        //Show(mainPanel);
    }

    public void Fade()
    {
        Hide(touchBG);
        Hide(mainPanel);

        if (camStartPoint != null)
        {
            mainCam.transform.position = camStartPoint.position;
            mainCam.transform.rotation = camStartPoint.rotation;
        }
        
        Show(sceneTransition);
        Show(sceneObjectParent);

        StartCoroutine(SceneTransitionRoutine());
    }

    
    private IEnumerator SceneTransitionRoutine()
    {
        Animator anim = sceneTransition.GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogWarning("sceneTransition에 Animator가 없습니다.");
            yield break;
        }
        
        yield return null;

        var state = anim.GetCurrentAnimatorStateInfo(0);
        float clipLen = state.length / Mathf.Max(anim.speed, 0.0001f);

        mainCam.transform.DOMove(camOriginPos.position, clipLen).SetEase(Ease.InOutSine);
        mainCam.transform.DORotateQuaternion(camOriginPos.rotation, clipLen).SetEase(Ease.InOutSine);
        
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f ||
               anim.IsInTransition(0))
        {
            yield return null;
        }

        sceneTransition.SetActive(false);      
        characterSelectUIPanel.SetActive(true);
    }

    public void OpenSettingUI()
    {
        SoundManager.Instance.PlaySFX(0);
    }

    public void OpenCharacterSelectUI()
    {
        SoundManager.Instance.PlaySFX(0);
    }
}
