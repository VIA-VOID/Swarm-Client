using System;
using System.ComponentModel;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : GenericSingleton<UIManager>
{
    [Title("Orientation Roots")]
    private GameObject portraitRoot;
    private GameObject landscapeRoot;

    private CanvasScaler canvasScaler;
    private SceneController sceneController;
    
    private int lastW, lastH;
    
    public static event Action<bool> OnOrientationChanged;
    
    private void Start()
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
            Debug.LogError("UIManager: Canvas 에 SceneController 컴포넌트가 없습니다.");
        }
        else
        {
            portraitRoot = sceneController.GetPanel(true);
            landscapeRoot = sceneController.GetPanel(false);
        }

        ApplyOrientation();
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
        bool isPortrait;
        
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

    public SceneController GetSceneController() => sceneController;
}
