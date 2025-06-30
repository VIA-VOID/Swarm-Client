// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Character/GameObject.h"
#include "SwarmPlayer.generated.h"

/*--------------------------------------------------------
					ASwarmPlayer
--------------------------------------------------------*/
UCLASS()
class SWARMCLIENT_API ASwarmPlayer : public AGameObject
{
	GENERATED_BODY()

public:
	// 본인의 플레이어인지, 네트워크(타) 플레이어 인지 구분
	bool IsMyPlayer();
	// 플레이어 정보 저장
	void UpdatePlayerInfo(const Protocol::ObjectInfo& InObjectInfo);

protected:
	Protocol::PlayerType PlayerType;
};
