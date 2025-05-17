using System;
using System.ComponentModel;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*-------------------------------------------------------
				UIManager

- 팝업 관리 및 UI 연출, 애니메이션 관리
- 씬 이동, 현재 사용 캔버스 탐색 및 화면비 감지
--------------------------------------------------------*/

public class UIManager : GenericSingleton<UIManager>
{
    [Title("Orientation Roots")]
    private GameObject portraitRoot;
    private GameObject landscapeRoot;

    private CanvasScaler canvasScaler;
    private SceneController sceneController;
    
    private int lastW, lastH;
        
    private bool isPortrait;
    public static event Action<bool> OnOrientationChanged;
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

// 씬이 완전히 로드되면 호출됨
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ReconnectSceneReferences(); // → 새 Canvas와 SceneController 다시 찾기
        ApplyOrientation();         // → 방향 UI 다시 세팅
    }
    
    private void ReconnectSceneReferences()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("UIManager: Canvas 오브젝트를 찾지 못했습니다.");
            return;
        }

        canvasScaler = canvas.GetComponent<CanvasScaler>();
        if (!canvasScaler)
            canvasScaler = canvas.gameObject.AddComponent<CanvasScaler>();

        sceneController = canvas.GetComponent<SceneController>();
        if (sceneController == null)
        {
            Debug.LogError("UIManager: Canvas에 SceneController가 없습니다.");
            return;
        }

        portraitRoot = sceneController.GetPanel(true);
        landscapeRoot = sceneController.GetPanel(false);
    }
    
    private void Update()
    {
#if UNITY_ANDROID || UNITY_IOS
        // 모바일: Orientation 변화 감지
        var dev = Input.deviceOrientation;
        if (dev == DeviceOrientation.Portrait || dev == DeviceOrientation.PortraitUpsideDown ||
            dev == DeviceOrientation.LandscapeLeft || dev == DeviceOrientation.LandscapeRight)
            ApplyOrientation();
#else
        // PC / WebGL: 창 크기 변화 감지
        if (Screen.width != lastW || Screen.height != lastH)
            ApplyOrientation();
#endif
    }

    private void ApplyOrientation()
    {
#if UNITY_ANDROID || UNITY_IOS
        var o = Screen.orientation;
        isPortrait = (o == ScreenOrientation.Portrait || o == ScreenOrientation.PortraitUpsideDown);
#else
        isPortrait = Screen.width < Screen.height;   // 가로/세로 비율 판정
        
        string debugMSG = isPortrait ? "세로" : "가로";
        Debug.Log(debugMSG + "로 화면비 변경");
#endif

        if (portraitRoot)   portraitRoot.SetActive(isPortrait);
        if (landscapeRoot)  landscapeRoot.SetActive(!isPortrait);

        if (canvasScaler)
        {
            canvasScaler.referenceResolution = isPortrait
                ? new Vector2(1080, 1920)      // 세로 기준
                : new Vector2(1920, 1080);     // 가로 기준
        }
        
        lastW = Screen.width;
        lastH = Screen.height;
        
        OnOrientationChanged?.Invoke(isPortrait);
    }

    // 가로 세로 판별 여부 리턴
    public bool GetIsPortrait()
    {
        return isPortrait;
    }
    
    // 씬 이동
    public void MoveScene(int index)
    {
        SceneManager.LoadScene(index);
    }
    
    public SceneController GetSceneController() => sceneController;
}
