#pragma once
#include "PacketHandler.h"
#include "Protocol/Protocol.pb.h"

class FSession;

/*--------------------------------------------------------
				FSystemPacketHandler

- 자동생성된 패킷 핸들러 클래스
- Handle_* 함수 구현
--------------------------------------------------------*/

class FSystemPacketHandler : public FPacketHandler
{
public:
	// 자동생성
	// 함수 테이블 등록
	virtual void RegisterHandlers() override
	{
		RegisterPacket<Protocol::SC_SYSTEM_PING>(EPacketID::SC_SYSTEM_PING, Handle_SC_SYSTEM_PING);
	}

private:
	// 자동생성
	static void Handle_SC_SYSTEM_PING(FSessionRef Session, const Protocol::SC_SYSTEM_PING& Packet);
};
