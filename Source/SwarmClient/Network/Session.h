#pragma once

#include "NetworkThread.h"
#include "NetworkDefine.h"
#include "Containers/Deque.h"
#include "Packet/PacketId.h"

class FSendThread;
class FRecvThread;

/*-------------------------------------------------------
					FSession

- 서버와의 연결 관리
- 패킷 송수신 처리
--------------------------------------------------------*/
class SWARMCLIENT_API FSession : public TSharedFromThis<FSession>
{
public:
	FSession();
	~FSession();
	// 서버 연결
	bool ConnectToServer(const FString& InServerAddress, int32 InServerPort);
	// 연결 종료
	void Disconnect();
	// 수신큐 삽입
	void RecvPacket(const TArray<uint8>& Packet);
	// 송신큐 삽입
	template <typename T>
	void SendPacket(const EPacketID PacketId, const T& ProtoPacket);
	// 수신된 패킷 처리
	void ProcessRecvPackets();
	// RTT 업데이트
	void UpdateRoundTripTime(const int32 RoundTripTime);
	// Socket Get
	FSocket* GetSocket() const;
	// RTT 평균값 가져오기
	int32 GetRttAvg() const;

private:
	// 소켓 생성
	bool CreateSocket();
	// 소켓 정리
	void CleanupSocket();
	// 송/수신 스레드 시작
	void StartThreads();
	// 송/수신 스레드 정지
	void StopThreads();
	
private:
	// 소켓
	FSocket* Socket;
	ISocketSubsystem* SocketSubsystem;
    
	// Send, Recv 스레드
	TUniquePtr<FSendThread> SendThread;
	TUniquePtr<FRecvThread> RecvThread;
    
	// 수신 패킷 큐
	TQueue<TArray<uint8>> ReceiveQueue;
    
	// 연결 정보
	FString ServerAddress;
	int32 ServerPort;

	// RTT
	int32 AvgRoundTripTime;
	TDeque<int32> RttDeque;

	// 연결여부
	bool IsConnect;
};

// 송신큐 삽입
template <typename T>
void FSession::SendPacket(const EPacketID PacketId, const T& ProtoPacket)
{
	if (Socket == nullptr || IsConnect == false || SendThread.IsValid() == false)
	{
		return;
	}

	// 송신데이터 조립
	const uint16 PayloadSize = static_cast<uint16>(ProtoPacket.ByteSizeLong());
	const uint16 PacketSize = PayloadSize + sizeof(FPacketHeader);

	// 데이터 직렬화
	TArray<uint8> PayloadBuffer;
	PayloadBuffer.SetNumUninitialized(PayloadSize);
	ProtoPacket.SerializeToArray(PayloadBuffer.GetData(), PayloadSize);

	// 헤더 조립
	FPacketHeader Header;
	Header.PacketId = static_cast<uint16>(PacketId);
	Header.PacketSize = PacketSize;

	// 전체 조립
	TArray<uint8> AllPacket;
	AllPacket.SetNumUninitialized(PacketSize);

	FMemory::Memcpy(AllPacket.GetData(), &Header, sizeof(FPacketHeader));
	FMemory::Memcpy(AllPacket.GetData() + sizeof(FPacketHeader), PayloadBuffer.GetData(), PayloadSize);
	
	// 패킷 송신 요청
	SendThread->RequestSend(AllPacket);
}
