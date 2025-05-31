#pragma once
#include "PacketId.h"
#include "Network/NetworkDefine.h"

class FSession;
class FPacketHandler;

using FPacketFunc = TFunction<void(FSession*, BYTE*, uint16)>;
using FPacketClass = TArray<TUniquePtr<FPacketHandler>>;

/*--------------------------------------------------------
					FPacketHandler

- Protobuf 패킷, 패킷함수 관리
- 도메인별로 PacketHandler를 상속받아 핸들러 등록
- PacketHandler.cpp의 Init 구현부 자동생성
--------------------------------------------------------*/

class FPacketHandler
{
public:
	virtual ~FPacketHandler() = default;
	// 파생 클래스들의 테이블 등록, 초기화
	// 자동생성 코드
	static void Init();
	// 함수 테이블 등록
	// 상속받아 구현
	virtual void RegisterHandlers() = 0;
	// 함수 테이블에 등록된 함수 실행 (템플릿 HandlePacket 함수 실행)
	static void HandlePacket(FSession* Session, BYTE* Buffer, uint16 Len);

protected:
	// 함수 테이블에 패킷 등록
	template<typename PacketType, typename HandleFunc>
	static void RegisterPacket(EPacketID PacketId, HandleFunc Handle);
	// 전달받은 RunFunc 함수 실행
	template<typename PacketType, typename RunFunc>
	static void HandlePacket(RunFunc Func, FSession* Session, BYTE* Buffer, uint16 Len);

protected:
	// 함수 테이블
	static FPacketFunc Handlers[UINT16_MAX];
	// 도메인별 핸들러
	static FPacketClass DomainHandlerClasses;
};

// 함수 테이블에 패킷 등록
template<typename PacketType, typename HandleFunc>
void FPacketHandler::RegisterPacket(EPacketID PacketId, HandleFunc Handle)
{
	Handlers[static_cast<uint16>(PacketId)] = [Handle](FSession* Session, BYTE* Buffer, uint16 Len)
		{
			HandlePacket<PacketType>(Handle, Session, Buffer, Len);
		};
}

// 전달받은 RunFunc 함수 실행
template<typename PacketType, typename RunFunc>
void FPacketHandler::HandlePacket(RunFunc Func, FSession* Session, BYTE* Buffer, uint16 Len)
{
	PacketType Packet;
	BYTE* Payload = Buffer + sizeof(FPacketHeader);
	uint16 PayloadSize = Len - sizeof(FPacketHeader);

	if (Packet.ParseFromArray(Payload, PayloadSize) == false)
	{
		UE_LOG(LogTemp, Error, TEXT("Packet ParseFromArray 실패: %s, PacketSize: %d"), typeid(RunFunc).name(), Len);
	}

	Func(Session, Packet);
}
