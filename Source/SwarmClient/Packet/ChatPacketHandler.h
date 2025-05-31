#pragma once
#include "PacketHandler.h"
#include "Protocol/Protocol.pb.h"

class FSession;

/*--------------------------------------------------------
				FChatPacketHandler

- 자동생성된 패킷 핸들러 클래스
- Handle_* 함수 구현
--------------------------------------------------------*/

class FChatPacketHandler : public FPacketHandler
{
public:
	// 자동생성
	// 함수 테이블 등록
	virtual void RegisterHandlers() override
	{
		RegisterPacket<Protocol::SC_CHAT_MSG>(EPacketID::SC_CHAT_MSG, Handle_SC_CHAT_MSG);
	}

private:
	// 자동생성
	static void Handle_SC_CHAT_MSG(FSession* Session, Protocol::SC_CHAT_MSG& Packet);
};
