

using System;
using Sirenix.OdinInspector;
using UnityEngine;

/*-------------------------------------------------------
				OrientationObject

- 가로패널 세로패널 UI 동시 조작을 위한 스크립트
--------------------------------------------------------*/

[Serializable]
public class OrientationObject
{
	[LabelText("UI 타입")] public UIType uiType;
	[LabelText("가로")]public GameObject portraitUIObj;
	[LabelText("세로")]public GameObject landScapeUIObj;
	//[SerializeField, LabelText("이동 속도")] private float moveDuration = 0.5f;

	// 현재 사용중인 UI 상태에 따라 오브젝트 리턴
	public GameObject GetUIObj(bool isPortrait)
	{
		return isPortrait ? portraitUIObj : landScapeUIObj;
	}

	// UI OnOff 관리
	public void SetUIActive(bool isOn)
	{
		portraitUIObj.SetActive(isOn);
		landScapeUIObj.SetActive(isOn);
	}
}
