// Fill out your copyright notice in the Description page of Project Settings.


#include "Zone/ZoneGameState.h"

#include "MapDataLoader.h"
#include "Struct.pb.h"
#include "Kismet/GameplayStatics.h"
#include "Player/SwarmMyPlayer.h"
#include "Player/SwarmPlayerController.h"


AZoneGameState::AZoneGameState()
{
	// 맵 데이터 읽어오기
	const FString Dir = FPaths::Combine(FPaths::ProjectContentDir(), TEXT("ServerData"), TEXT("MapData.json"));
	FMapDataLoader::LoadMapDataFromFile(Dir, MapData);
}

// 플레이어 입장 처리 패킷
void AZoneGameState::EnterPlayer(const Protocol::SC_PLAYER_ENTER_GAME& Packet)
{
	const Protocol::ObjectInfo ObjInfo = Packet.objectinfo();
	
	// Object 스폰
	SpawnObject(ObjInfo, true);
}

// Object 스폰 패킷
void AZoneGameState::SpawnObject(const Protocol::SC_OBJECT_SPAWN& Packet)
{
	const Protocol::ObjectInfo ObjInfo = Packet.objectinfo();

	// Object 스폰
	SpawnObject(ObjInfo, false);
}

// Object 디스폰
void AZoneGameState::DespawnObject(const uint64 ObjectId)
{
	// Object 디스폰
	AGameObject* Object = FindObject(ObjectId);
	if (Object == nullptr)
	{
		return;
	}
	
	Objects.Remove(ObjectId);
	Object->Destroy();
}

// Object 찾기
AGameObject* AZoneGameState::FindObject(const uint64 ObjectId)
{
	if (const auto FindObject = Objects.Find(ObjectId))
	{
		return *FindObject;
	}
	return nullptr;
}

ASwarmPlayer* AZoneGameState::FindPlayer(const uint64 ObjectId)
{
	if (const auto FindObject = Objects.Find(ObjectId))
	{
		if ((*FindObject)->IsPlayer())
		{
			return Cast<ASwarmPlayer>(*FindObject);	
		}		
	}
	return nullptr;
}

// Object 스폰
void AZoneGameState::SpawnObject(const Protocol::ObjectInfo& ObjInfo, const bool IsOwn/* = false*/)
{
	const Protocol::PosInfo PosInfo = ObjInfo.posinfo();
	const Protocol::ObjectType ObjType = ObjInfo.type();
	const FVector SpawnVector(PosInfo.x(), PosInfo.y(), PosInfo.z());
	
	// 스폰
	const FVector SpawnLocation(SpawnVector);
	const FRotator SpawnRotation = FRotator(0, PosInfo.yaw(), 0);

	FActorSpawnParameters SpawnParams;
	SpawnParams.SpawnCollisionHandlingOverride = ESpawnActorCollisionHandlingMethod::AlwaysSpawn;
	
	// type에 따라 분기
	if (ObjType == Protocol::OBJECT_TYPE_PLAYER)
	{
		// player 스폰
		SpawnPlayer(ObjInfo, SpawnParams, SpawnLocation, SpawnRotation, IsOwn);
	}
	else if (ObjType == Protocol::OBJECT_TYPE_MONSTER)
	{
		// Monster 스폰
	}
}

// Player 스폰
void AZoneGameState::SpawnPlayer(const Protocol::ObjectInfo& ObjInfo, FActorSpawnParameters& SpawnParams,
                                 const FVector& SpawnLocation, const FRotator& SpawnRotation, const bool IsOwn/* = false*/)
{
	UWorld* World = GetWorld();
	if (World == nullptr)
	{
		return;
	}
	AGameModeBase* GameMode = UGameplayStatics::GetGameMode(World);
	if (GameMode == nullptr)
	{
		return;
	}
	// 중복 처리 체크
	const uint64 ObjectId = ObjInfo.objectid();
	if (FindObject(ObjectId) != nullptr)
	{
		return;
	}
	// 본인 캐릭터 스폰
	if (IsOwn)
	{
		ASwarmPlayerController* PlayerController = Cast<ASwarmPlayerController>(UGameplayStatics::GetPlayerController(World, 0));
		if (PlayerController == nullptr)
		{
			return;
		}

		SpawnParams.Owner = PlayerController;
		
		const TSubclassOf<APawn> DefaultPawnClass = GameMode->GetDefaultPawnClassForController(PlayerController);
		if (ASwarmMyPlayer* SwarmMyPlayer = World->SpawnActor<ASwarmMyPlayer>(DefaultPawnClass, SpawnLocation, SpawnRotation, SpawnParams))
		{
			// 플레이어 정보 업데이트
			SwarmMyPlayer->UpdatePlayerInfo(ObjInfo);
			// 컨트롤러 빙의
			PlayerController->Possess(SwarmMyPlayer);
		
			MyPlayer = SwarmMyPlayer;
			Objects.Add(ObjectId, Cast<ASwarmPlayer>(SwarmMyPlayer));
		}
	}
	else
	{
		// 타 사용자 캐릭터 스폰
		if (ASwarmPlayer* OtherPlayer = World->SpawnActor<ASwarmPlayer>(OtherPlayerClass, SpawnLocation, SpawnRotation, SpawnParams))
		{
			// 플레이어 정보 업데이트
			OtherPlayer->UpdatePlayerInfo(ObjInfo);
			
			Objects.Add(ObjectId, OtherPlayer);
		}
	}
}
