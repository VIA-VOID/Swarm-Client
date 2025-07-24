#include "SystemPacketHandler.h"
#include "Network/Session.h"
#include "GameMacro.h"

// 서버의 PING
void FSystemPacketHandler::Handle_SC_SYSTEM_PING(FSessionRef Session, const Protocol::SC_SYSTEM_PING& Packet)
{
	const int64 ServerTime = Packet.currenttime();
	const int64 ClientTime = std::chrono::duration_cast<std::chrono::milliseconds>(
		std::chrono::system_clock::now().time_since_epoch()).count();
	const int32 DiffTime = static_cast<int32>(ClientTime - ServerTime);

	// RTT 업데이트
	Session->UpdateRoundTripTime(FMath::Abs(DiffTime));

	// PONG 패킷 전송
	Protocol::CS_SYSTEM_PONG PongPkt;
	PongPkt.set_currenttime(ClientTime);

	Session->SendPacket(EPacketID::CS_SYSTEM_PONG, PongPkt);
}
