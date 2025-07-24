#include "ChatPacketHandler.h"

#include "ZoneGameState.h"
#include "Player/SwarmMyPlayer.h"

// 채팅
void FChatPacketHandler::Handle_SC_CHAT_MSG(FSessionRef Session, const Protocol::SC_CHAT_MSG& Packet)
{
	// GameState 가져오기
	if (AZoneGameState* ZoneGameState = Cast<AZoneGameState>(World->GetGameState()))
	{
		ASwarmMyPlayer* SwarmMyPlayer = Cast<ASwarmMyPlayer>(ZoneGameState->GetMyPlayer());
		if (SwarmMyPlayer == nullptr)
		{
			return;
		}
		SwarmMyPlayer->RecvChatMessage(Packet);
	}
}
