using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class LoginSceneUIController : UIController
{
    [LabelText("로딩바 이미지")]
    [SerializeField] private GameObject loadingBarImage;
    
    [LabelText("로딩바 대기 시간")]
    [SerializeField] private float duration = 3f;

    [SerializeField] private GameObject touchBG;
    
    [SerializeField] private Image gameStartImage;
    
    [LabelText("씬 오브젝트")]
    [SerializeField] private GameObject sceneObjectParent;

    [LabelText("씬전환 연출")]
    [SerializeField] private GameObject sceneTransition;
    
    private Renderer targetRend;
    private Material instancedMat; 

    private void Start()
    {
        targetRend = sceneTransition.GetComponent<Renderer>();
        instancedMat = targetRend.material;
        
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

    private static readonly int AlphaID  = Shader.PropertyToID("_Alpha");
    private static readonly int ScrollID = Shader.PropertyToID("_Scroll");

    private void BlinkText()
    {
        gameStartImage.gameObject.SetActive(true);
        
        gameStartImage.DOFade(0.1f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .From(1); // 시작 알파를 max로 강제

        touchBG.GetComponent<Button>().interactable = true;
    }
    
    public void Fade(bool isIn)
    {
        StopAllCoroutines();
        
        StartCoroutine(FadeRoutine(isIn));
    }
    
    private IEnumerator FadeRoutine(bool isIn)
    {
        touchBG.SetActive(false);
        gameStartImage.gameObject.SetActive(false);
        
        sceneTransition.SetActive(true);
        
        float t = 0f;

        // 시작/끝 값 설정
        float startAlpha  = isIn ? 1f : 0f;
        float endAlpha    = isIn ? 0f : 1f;
        float startScroll = isIn ? -1f : 1f;
        float endScroll   = isIn ?  1f : -1f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = t / duration;

            instancedMat.SetFloat(AlphaID,  Mathf.Lerp(startAlpha,  endAlpha,  k));
            instancedMat.SetFloat(ScrollID, Mathf.Lerp(startScroll, endScroll, k));

            yield return null;
        }
        
        instancedMat.SetFloat(AlphaID,  endAlpha);
        instancedMat.SetFloat(ScrollID, endScroll);
        
        sceneTransition.SetActive(false);
    }
}
