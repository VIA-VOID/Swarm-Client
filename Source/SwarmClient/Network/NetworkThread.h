#pragma once
#include "Macro.h"

class FSession;

/*-------------------------------------------------------
					FNetworkThread

- 네트워크 패킷 스레드
- FNetworkThread를 상속받아 송/수신 스레드 구현
--------------------------------------------------------*/
class SWARMCLIENT_API FNetworkThread : public FRunnable
{
public:
	FNetworkThread(const FSessionRef& InSession);
	virtual ~FNetworkThread() override = default;
	virtual uint32 Run() override = 0;
	virtual void Stop() override;
	// 스레드 종료
	virtual void Shutdown();

protected:
	bool IsRunning;
	FSessionWRef OwnerSession;
	TUniquePtr<FRunnableThread> Thread;
};

/*-------------------------------------------------------
					FSendThread

- 네트워크 패킷 송신 스레드
--------------------------------------------------------*/
class SWARMCLIENT_API FSendThread : public FNetworkThread
{
public:
	FSendThread(const FSessionRef& InSession);
	virtual ~FSendThread() override;
	// 스레드 실행
	// blocking Send 호출
	virtual uint32 Run() override;
	// 패킷 송신 요청
	// queue에 데이터 삽입
	void RequestSend(const TArray<uint8>& PacketData);

private:
	TQueue<TArray<uint8>> SendQueue;
};

/*-------------------------------------------------------
					FRecvThread

- 네트워크 패킷 수신 스레드
--------------------------------------------------------*/
class SWARMCLIENT_API FRecvThread : public FNetworkThread
{
public:
	FRecvThread(const FSessionRef& InSession);
	virtual ~FRecvThread() override;
	// 스레드 실행
	// blocking Recv 호출
	virtual uint32 Run() override;
	
private:
	TArray<uint8> RecvBuffer;
};
