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
		RegisterPacket<Protocol::SC_PLAYER_ENTER_GAME>(EPacketID::SC_PLAYER_ENTER_GAME, Handle_SC_PLAYER_ENTER_GAME);
		RegisterPacket<Protocol::SC_PLAYER_MOVE>(EPacketID::SC_PLAYER_MOVE, Handle_SC_PLAYER_MOVE);
	}

private:
	// 자동생성
	static void Handle_SC_PLAYER_ENTER_GAME(FSessionRef Session, const Protocol::SC_PLAYER_ENTER_GAME& Packet);
	static void Handle_SC_PLAYER_MOVE(FSessionRef Session, const Protocol::SC_PLAYER_MOVE& Packet);
};
