#pragma once
#include "PacketHandler.h"
#include "Protocol/Protocol.pb.h"

class FSession;

/*--------------------------------------------------------
				FObjectPacketHandler

- 자동생성된 패킷 핸들러 클래스
- Handle_* 함수 구현
--------------------------------------------------------*/

class FObjectPacketHandler : public FPacketHandler
{
public:
	// 자동생성
	// 함수 테이블 등록
	virtual void RegisterHandlers() override
	{
		RegisterPacket<Protocol::SC_OBJECT_SPAWN>(EPacketID::SC_OBJECT_SPAWN, Handle_SC_OBJECT_SPAWN);
		RegisterPacket<Protocol::SC_OBJECT_DESPAWN>(EPacketID::SC_OBJECT_DESPAWN, Handle_SC_OBJECT_DESPAWN);
	}

private:
	// 자동생성
	static void Handle_SC_OBJECT_SPAWN(FSessionRef Session, const Protocol::SC_OBJECT_SPAWN& Packet);
	static void Handle_SC_OBJECT_DESPAWN(FSessionRef Session, const Protocol::SC_OBJECT_DESPAWN& Packet);
};
