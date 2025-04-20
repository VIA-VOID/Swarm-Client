using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class LoginSceneController : SceneController
{
    [LabelText("로딩바 이미지")]
    [SerializeField] private GameObject loadingBarImage;
    
    [LabelText("로딩바 대기 시간")]
    [SerializeField] private float duration = 3f;

    [LabelText("로딩 배경 이미지")]
    [SerializeField] private GameObject touchBG;
    
    [LabelText("로딩 완료 이미지")]
    [SerializeField] private Image gameStartImage;
    
    [LabelText("씬 오브젝트")]
    [SerializeField] private GameObject sceneObjectParent;

    [LabelText("씬전환 연출")]
    [SerializeField] private GameObject sceneTransition;

    [LabelText("선택 가능한 캐릭터 리스트")]
    [SerializeField] private List<SelectableCharacter> selectableCharacters;
    
    private void Start()
    {
        Show(loadingBarImage, Loading);
    }

    void Loading()
    {
        StartCoroutine(FillRoutine());
    }

    private IEnumerator FillRoutine()
    {
        Image targetImage = loadingBarImage.GetComponent<Image>();
        
        float time = 0f;
        targetImage.fillAmount = 0f;   // 시작값

        while (time < duration)
        {
            time += Time.deltaTime;
            targetImage.fillAmount = Mathf.Clamp01(time / duration);
            yield return null;
        }

        targetImage.fillAmount = 1f;
        Hide(loadingBarImage, () => BlinkText() );
    }

    private void BlinkText()
    {
        Show(gameStartImage.gameObject, () =>
        {
            gameStartImage.DOFade(0.1f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .From(1); // 시작 알파를 max로 강제

            touchBG.GetComponent<Button>().interactable = true;
        });
    }

    public void Fade()
    {
        Hide(touchBG);
        
        Hide(gameStartImage.gameObject);

        Show(sceneTransition);
        
        Show(sceneObjectParent);
    }
}
