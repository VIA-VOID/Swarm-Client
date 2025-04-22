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
    [SerializeField, LabelText("로딩 완료 이미지")]  private Image      gameStartImage;

    [Header("씬 오브젝트")]
    [SerializeField] private GameObject sceneObjectParent;
    [SerializeField] private GameObject sceneTransition;

    [Header("선택 가능한 캐릭터")]
    [SerializeField] private List<SelectableCharacter> selectableCharacters;

    [Header("카메라 이동")]
    [SerializeField] private Camera mainCam;
    [SerializeField] private float camMoveTime = 1.2f;
    
    [LabelText("트랜지션 시작 위치")]
    [SerializeField] private Transform camStartPoint;
    
    [LabelText("트랜지션 종료 위치")]
    [SerializeField] private Transform camOriginPos;

    [Header("UI 버튼")]
    [SerializeField] private GameObject characterSelectUIPanel;
    [SerializeField] private Button leftArrowBtn;
    [SerializeField] private Button rightArrowBtn;
    [SerializeField] private Button backBtn;

    private int currentIndex = -1;     // ‑1이면 아무 것도 선택 안 한 상태

    private void Start()
    {
        foreach (var ch in selectableCharacters)
            ch.OnSelected = SelectCharacter;

        UpdateArrowInteractable();

        Show(loadingBarImage, Loading);
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
        Hide(loadingBarImage, BlinkText);
    }

    void BlinkText()
    {
        Show(gameStartImage.gameObject, () =>
        {
            gameStartImage.DOFade(0.1f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).From(1);
            touchBG.GetComponent<Button>().interactable = true;
        });
    }

    public void Fade()
    {
        Hide(touchBG);
        Hide(gameStartImage.gameObject);

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
    
    void SelectCharacter(SelectableCharacter ch)
    {
        // 이전 선택 해제
        if (currentIndex != -1)
            selectableCharacters[currentIndex].SetIdle();

        // 새 선택
        currentIndex = selectableCharacters.IndexOf(ch);
        ch.PlayAttackThenSelected();

        MoveCameraTo(ch.FocusPoint);
        UpdateArrowInteractable();
        backBtn.gameObject.SetActive(true);
    }

    public void PrevCharacter()
    {
        if (currentIndex <= 0) return;

        selectableCharacters[currentIndex].SetIdle();
        currentIndex--;
        selectableCharacters[currentIndex].PlayAttackThenSelected();

        MoveCameraTo(selectableCharacters[currentIndex].FocusPoint);
        UpdateArrowInteractable();
    }

    public void NextCharacter()
    {
        if (currentIndex >= selectableCharacters.Count - 1) return;

        selectableCharacters[currentIndex].SetIdle();
        currentIndex++;
        selectableCharacters[currentIndex].PlayAttackThenSelected();

        MoveCameraTo(selectableCharacters[currentIndex].FocusPoint);
        UpdateArrowInteractable();
    }

    public void BackToOrigin()
    {
        if (currentIndex != -1)
            selectableCharacters[currentIndex].SetIdle();  // 모두 Idle로

        currentIndex = -1;

        mainCam.transform.DOMove(camOriginPos.position, camMoveTime).SetEase(Ease.InOutSine);
        mainCam.transform.DORotateQuaternion(camOriginPos.rotation, camMoveTime).SetEase(Ease.InOutSine);

        UpdateArrowInteractable();
        backBtn.gameObject.SetActive(false);
    }

    void MoveCameraTo(Transform target)
    {
        mainCam.transform.DOMove(target.position,  camMoveTime).SetEase(Ease.InOutSine);
        mainCam.transform.DOLookAt(target.position, camMoveTime).SetEase(Ease.InOutSine);
    }

    void UpdateArrowInteractable()
    {
        bool hasSelection = currentIndex != -1;
        leftArrowBtn.interactable  = hasSelection && currentIndex > 0;
        rightArrowBtn.interactable = hasSelection && currentIndex < selectableCharacters.Count - 1;
    }
}
