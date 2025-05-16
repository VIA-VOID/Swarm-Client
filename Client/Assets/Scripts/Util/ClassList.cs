

using System;
using Sirenix.OdinInspector;
using UnityEngine;

/*-------------------------------------------------------
				모든 클래스 List
--------------------------------------------------------*/

//- 가로패널 세로패널 UI 동시 조작을 위한 스크립트
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

// 플레이어 데이터
[Serializable]
public class PlayerData
{
	public string id; // 구분값 (임시 )
	public CharacterAppearanceData appearance; // 외형 값
	public EquipmentData equipment; // 장비
	public CharacterStats Stats; // 스테이터스

	public Vector2 position; // 위치
	public Quaternion rotation; // 보는 방향

	public PlayerData()
	{
		appearance = new CharacterAppearanceData();
		equipment = new EquipmentData();
		Stats = new CharacterStats();
	}
}

// 캐릭터 외형 커스터 마이징 데이터
[Serializable]
public class CharacterAppearanceData
{
	public int sexId;           // 0: 남성, 1: 여성
	public int hairStyleId;		// 헤어 스타일
	public int hairColorId;		// 헤어 컬러
	public int skinColorId;		// 피부 컬러
	public int eyeColorId;		// 눈 컬러
	public int facialHairId;    // 남성 전용
	public int bustSizeId;      // 여성 전용
	public int faceEmotionId;	// 표정
}

// 장비 착용 정보
[Serializable]
public class EquipmentData
{
	// 무기, 모자, 상의, 장갑, 바지, 신발
	public string weaponId;
	public string hatId;
	public string topId;
	public string glovesId;
	public string bottomId;
	public string shoesId;
}

// 캐릭터 스테이터스
[Serializable]
public class CharacterStats
{
	// 기본 능력치 (힘, 민첩, 지능, 솜씨, 행운)
	public int strength;
	public int dexterity;
	public int intelligence;
	public int vitality;
	public int luck;

	// 전투 능력치  (공격력, 방어력, 치명타율)
	public float attackPower;
	public float defense;
	public float critRate;

	// 세부 능력치 (스킬 위력, 연타, 공격속도, 이동속도, 체력 리젠, 궁극기)  
	public float skillPower;
	public float comboBonus;
	public float attackSpeed;
	public float moveSpeed;
	public float regen;
	public float specialGauge;
}
