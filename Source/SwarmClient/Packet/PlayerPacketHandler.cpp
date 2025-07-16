#include "PlayerPacketHandler.h"

#include "Session.h"
#include "Player/SwarmPlayer.h"
#include "Zone/ZoneGameState.h"

// 캐릭터 입장
void FPlayerPacketHandler::Handle_SC_PLAYER_ENTER_GAME(FSessionRef Session, const Protocol::SC_PLAYER_ENTER_GAME& Packet)
{
	// GameState 가져오기
	if (AZoneGameState* ZoneGameState = Cast<AZoneGameState>(World->GetGameState()))
	{
		// 플레이어 입장
		ZoneGameState->EnterPlayer(Packet);
	}
}

// 타 플레이어 스폰
void FPlayerPacketHandler::Handle_SC_PLAYER_SPAWN(FSessionRef Session, const Protocol::SC_PLAYER_SPAWN& Packet)
{
	// GameState 가져오기
	if (AZoneGameState* ZoneGameState = Cast<AZoneGameState>(World->GetGameState()))
	{
		// 플레이어 스폰
		ZoneGameState->SpawnPlayer(Packet);
	}
}

// 플레이어 디스폰
void FPlayerPacketHandler::Handle_SC_PLAYER_DESPAWN(FSessionRef Session, const Protocol::SC_PLAYER_DESPAWN& Packet)
{
	// GameState 가져오기
	if (AZoneGameState* ZoneGameState = Cast<AZoneGameState>(World->GetGameState()))
	{
		// 플레이어 디스폰
		ZoneGameState->DespawnPlayer(Packet.objectid());
	}
}

void FPlayerPacketHandler::Handle_SC_PLAYER_MOVE(FSessionRef Session, const Protocol::SC_PLAYER_MOVE& Packet)
{
	// GameState 가져오기
	if (AZoneGameState* ZoneGameState = Cast<AZoneGameState>(World->GetGameState()))
	{
		ASwarmPlayer* SwarmPlayer = ZoneGameState->FindPlayer(Packet.objectid());
		if (SwarmPlayer == nullptr)
		{
			return;
		}
		// 캐릭터 이동
		SwarmPlayer->MovePlayer(Packet.posinfo());
	}
}
