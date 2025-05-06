using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class SelectableCharacter : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [LabelText("카메라 이동 지점")]
    [SerializeField] private Transform focusPoint;
    
    [LabelText("Idle 값")]
    [SerializeField] private int idleValue     = 0;
    [LabelText("Selected 값")]
    [SerializeField] private int selectedValue = 1;
    [LabelText("Attack 값")]
    [SerializeField] private int attackValue   = 2;
    [LabelText("Attack 재생 시간")]
    [SerializeField] private float attackDuration = 0.7f;

    private Animator anim;
    private Coroutine attackRoutine;
    private LoginSceneController loginSceneController;
    
    public Action<SelectableCharacter> OnSelected;
    public Transform FocusPoint => focusPoint != null ? focusPoint : transform;

    void Awake()
    {
        anim = GetComponent<Animator>();
        SetIdle();
    }

    void Start()
    {
        loginSceneController = UIManager.Instance.GetSceneController().GetComponent<LoginSceneController>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // if (loginSceneController.CheckDetailUIPanel()) return;
        // OnSelected?.Invoke(this);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // if (loginSceneController.CheckDetailUIPanel()) return;
        // SetOutLine(true);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        SetOutLine(false);
    }
    
    public void SetIdle()
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        anim.SetInteger("animation", idleValue);
    }

    public void PlayAttackThenSelected()
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        attackRoutine = StartCoroutine(AttackThenSelectedRoutine());
    }

    IEnumerator AttackThenSelectedRoutine()
    {
        yield return new WaitForSeconds(1); 
        
        int prevHash = anim.GetCurrentAnimatorStateInfo(0).shortNameHash;

        anim.SetInteger("animation", attackValue);
        
        yield return null;
        while (anim.GetCurrentAnimatorStateInfo(0).shortNameHash == prevHash)
            yield return null;
        
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        anim.SetInteger("animation", selectedValue);
        attackRoutine = null;
    }

    void SetOutLine(bool setOn)
    {
        int targetLayer = LayerMask.NameToLayer(setOn ? "Selectable" : "Default");
        if (targetLayer < 0)
        {
            Debug.LogError("SetOutLine: 'Selectable' 또는 'Default' 레이어가 없습니다!");
            return;
        }
        
        foreach (Transform t in transform.GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.layer = targetLayer;
        }
    }
}
