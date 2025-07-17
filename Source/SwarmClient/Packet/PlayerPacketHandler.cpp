#include "PlayerPacketHandler.h"

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
