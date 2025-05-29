#pragma once

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
	bool ConnectToServer(const FString& InServerAddress, int32 InPort);
	// 연결 종료
	void Disconnect();
	// 송신큐 삽입
	void RecvPacket(const TArray<uint8>& Packet);
	// 수신큐 삽입
	template <typename T>
	void SendPacket(const T& ProtoPacket);
	// 게임 스레드에서 호출됨
	// 수신된 패킷 처리
	void ProcessRecvPackets();
	// Socket Get
	FSocket* GetSocket() const;

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
	uint16 ServerPort;

	// 연결여부
	bool IsConnect;
};

// 수신큐 삽입
template <typename T>
void FSession::SendPacket(const T& ProtoPacket)
{
	if (Socket == nullptr || IsConnect == false)
	{
		return;
	}
	ReceiveQueue.Enqueue(ProtoPacket);
}
