// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameMacro.h"
#include "Protocol.pb.h"
#include "ZoneDefine.h"
#include "GameFramework/GameStateBase.h"
#include "ZoneGameState.generated.h"

class AGameObject;
class ASwarmPlayer;
class ASwarmMyPlayer;

/*--------------------------------------------------------
					AZoneGameState

- Zone 상태 관리
--------------------------------------------------------*/
UCLASS()
class SWARMCLIENT_API AZoneGameState : public AGameStateBase
{
	GENERATED_BODY()

public:
	AZoneGameState();

	// 플레이어 입장 처리 패킷
	void EnterPlayer(const Protocol::SC_PLAYER_ENTER_GAME& Packet);
	// Object 스폰 패킷
	void SpawnObject(const Protocol::SC_OBJECT_SPAWN& Packet);
	// Object 디스폰
	void DespawnObject(const uint64 ObjectId);
	// 월드 좌표 그리드 좌표로 변환
	FVector2D MakeGridIndex(const FVector& WorldPosition) const;
	// 이동 가능여부
	bool CanGo(const FVector& WorldPosition) const;
	// Object 찾기
	AGameObject* FindObject(const uint64 ObjectId);
	ASwarmPlayer* FindPlayer(const uint64 ObjectId);
	
	
private:
	// Object 스폰
	void SpawnObject(const Protocol::ObjectInfo& ObjInfo, const bool IsOwn = false);
	// Player 스폰
	void SpawnPlayer(const Protocol::ObjectInfo& ObjInfo, FActorSpawnParameters& SpawnParams,
		const FVector& SpawnLocation, const FRotator& SpawnRotation, const bool IsOwn = false);

private:
	// Player 정보
	UPROPERTY(EditAnywhere)
	TSubclassOf<ASwarmPlayer> OtherPlayerClass;
	ASwarmMyPlayer* MyPlayer;
	TMap<uint64, AGameObject*> Objects;
	
	// 맵데이터
	FMapData MapData;
};
