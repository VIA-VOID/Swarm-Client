#pragma once
#include "PacketHandler.h"
#include "Protocol/Protocol.pb.h"

class FSession;

/*--------------------------------------------------------
				FPlayerPacketHandler

- 자동생성된 패킷 핸들러 클래스
- Handle_* 함수 구현
--------------------------------------------------------*/

class FPlayerPacketHandler : public FPacketHandler
{
public:
	// 자동생성
	// 함수 테이블 등록
	virtual void RegisterHandlers() override
	{
		RegisterPacket<Protocol::SC_PLAYER_CREATE>(EPacketID::SC_PLAYER_CREATE, Handle_SC_PLAYER_CREATE);
		RegisterPacket<Protocol::SC_PLAYER_MOVE>(EPacketID::SC_PLAYER_MOVE, Handle_SC_PLAYER_MOVE);
	}

private:
	// 자동생성
	static void Handle_SC_PLAYER_CREATE(FSession* Session, Protocol::SC_PLAYER_CREATE& Packet);
	static void Handle_SC_PLAYER_MOVE(FSession* Session, Protocol::SC_PLAYER_MOVE& Packet);
};
