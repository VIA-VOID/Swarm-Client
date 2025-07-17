#include "ObjectPacketHandler.h"

#include "ZoneGameState.h"

void FObjectPacketHandler::Handle_SC_OBJECT_SPAWN(FSessionRef Session, const Protocol::SC_OBJECT_SPAWN& Packet)
{
	// GameState 가져오기
	if (AZoneGameState* ZoneGameState = Cast<AZoneGameState>(World->GetGameState()))
	{
		// 플레이어 스폰
		ZoneGameState->SpawnObject(Packet);
	}
}

void FObjectPacketHandler::Handle_SC_OBJECT_DESPAWN(FSessionRef Session, const Protocol::SC_OBJECT_DESPAWN& Packet)
{
	// GameState 가져오기
	if (AZoneGameState* ZoneGameState = Cast<AZoneGameState>(World->GetGameState()))
	{
		// 플레이어 디스폰
		ZoneGameState->DespawnObject(Packet.objectid());
	}
}
