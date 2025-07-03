// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Struct.pb.h"
#include "GameFramework/Character.h"
#include "Zone/World3D.h"
#include "GameObject.generated.h"

class AZoneGameState;
/*-------------------------------------------------------
					AGameObject

- 모든 오브젝트 공통 항목
- AGameObject 상속받아 플레이어, 몬스터 등 구현
--------------------------------------------------------*/
UCLASS()
class SWARMCLIENT_API AGameObject : public ACharacter
{
	GENERATED_BODY()

public:
	AGameObject();

	// Object 정보 업데이트
	void UpdateObjectInfo(const Protocol::ObjectInfo& InObjInfo);
	// 위치 업데이트
	void UpdatePosition(const Protocol::PosInfo& InPosInfo);
	// Player인지 확인
	bool IsPlayer() const;
	// Monster인지 확인
	bool IsMonster() const;

protected:
	virtual void BeginPlay() override;
	virtual void Tick(float DeltaTime) override;

protected:
	// 고유 ID
	uint64 ObjectId;
	// 캐릭터 타입
	Protocol::ObjectType Type;
	// 위치 및 방향 정보
	Protocol::PosInfo PosInfo;
	FWorld3D World3D;
	// 스탯 정보
	Protocol::StatInfo StatInfo;
	// 이름
	std::string Name;
};
