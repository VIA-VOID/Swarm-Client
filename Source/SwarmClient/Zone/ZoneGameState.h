// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Protocol.pb.h"
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
	// Player 스폰 패킷
	void SpawnPlayer(const Protocol::SC_PLAYER_SPAWN& Packet);
	// 플레이어 찾기
	ASwarmPlayer* FindPlayer(const uint64 PlayerId);
	
private:
	// Object 스폰
	void SpawnObject(const Protocol::ObjectInfo& ObjInfo, const bool IsOwn = false);

	void SpawnPlayer(const Protocol::ObjectInfo& ObjInfo, FActorSpawnParameters& SpawnParams,
		const FVector& SpawnLocation, const FRotator& SpawnRotation, const bool IsOwn = false);

private:
	// Player 정보
	UPROPERTY(EditAnywhere)
	TSubclassOf<ASwarmPlayer> OtherPlayerClass;
	ASwarmMyPlayer* MyPlayer;
	TMap<uint64, ASwarmPlayer*> Players;
};
