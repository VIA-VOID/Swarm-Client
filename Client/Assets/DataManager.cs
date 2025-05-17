using Unity.Collections;
using UnityEngine;

public class DataManager : GenericSingleton<DataManager>
{
/*-------------------------------------------------------
				DataManager

- 유저, 캐릭터 정보 관리
--------------------------------------------------------*/
	[ReadOnly, SerializeField] private PlayerData userData;
	
	// 유저 정보 세팅
	public void SetUserData(PlayerData data)
	{
		userData = data;
	}
}
