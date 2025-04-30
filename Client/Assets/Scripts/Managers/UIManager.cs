using System.ComponentModel;
using Sirenix.OdinInspector;
using UnityEngine;

public class UIManager : GenericSingleton<UIManager>
{
    private SceneController sceneController;
    
    private void Start()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("UIManager: Canvas 오브젝트를 찾지 못했습니다.");
            return;
        }
        
        sceneController = canvas.GetComponent<SceneController>();
        if (sceneController == null)
        {
            Debug.LogError("UIManager: Canvas 에 SceneController 컴포넌트가 없습니다.");
        }
    }

    public SceneController GetSceneController() => sceneController;
}
