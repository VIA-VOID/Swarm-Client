#include "NetworkThread.h"
#include "NetworkDefine.h"
#include "Session.h"
#include "Sockets.h"

/*----------------------------
		FNetworkThread
----------------------------*/
FNetworkThread::FNetworkThread(const FSessionRef& InSession)
	: IsRunning(false), OwnerSession(InSession)
{
}

void FNetworkThread::Stop()
{
	IsRunning = false;
	FRunnable::Stop();
}

// 스레드 종료
void FNetworkThread::Shutdown()
{
	IsRunning = false;
	if (Thread)
	{
		Thread->WaitForCompletion();
		Thread.Reset();
	}
}

/*----------------------------
		FSendThread
----------------------------*/
FSendThread::FSendThread(const FSessionRef& InSession)
	: FNetworkThread(InSession)
{
	// 스레드 생성
	Thread.Reset(FRunnableThread::Create(this, TEXT("FSendThread")));
}

FSendThread::~FSendThread()
{
	FNetworkThread::Shutdown();
	// 남은 패킷 정리
	TArray<uint8> TempPacket;
	while (SendQueue.Dequeue(TempPacket))
	{
	}
}

uint32 FSendThread::Run()
{
	IsRunning = true;
	UE_LOG(LogTemp, Log, TEXT("FSendThread 시작"));
	// 패킷 버퍼
	TArray<uint8> PacketBuffer;
	PacketBuffer.Reserve(PACKET_BUFFER_SIZE);
	
	while (IsRunning)
	{
		// 패킷 가져오기 전에 비우기
		PacketBuffer.Reset();
		// 큐에서 패킷 가져오기
		if (SendQueue.Dequeue(PacketBuffer))
		{
			if (auto Session = OwnerSession.Pin())
			{
				// 소켓 연결상태 확인
				FSocket* Socket = Session->GetSocket();
				if (Socket && Socket->GetConnectionState() == SCS_Connected)
				{
					// 패킷 전송
					int32 BytesSent = 0;
					if (Socket->Send(PacketBuffer.GetData(), PacketBuffer.Num(), BytesSent) == false)
					{
						UE_LOG(LogTemp, Error, TEXT("패킷 송신 실패"));
						IsRunning = false;
						break;
					}
				}
				else
				{
					UE_LOG(LogTemp, Warning, TEXT("소켓이 연결되지 않음"));
					break;
				}
			}
			else
			{
				// 세션 소멸
				UE_LOG(LogTemp, Warning, TEXT("Session 소멸, FSendThread 종료"));
				IsRunning = false;
				break;
			}
		}
		else
		{
			// 큐가 비었으면 잠시대기
			FPlatformProcess::Sleep(0.005f);
		}
	}
    
	UE_LOG(LogTemp, Log, TEXT("FSendThread 종료"));
	return 0;
}

// 패킷 송신 요청
// queue에 데이터 삽입
void FSendThread::RequestSend(const TArray<uint8>& PacketData)
{
	SendQueue.Enqueue(PacketData);
}

/*----------------------------
		FRecvThread
----------------------------*/
FRecvThread::FRecvThread(const FSessionRef& InSession)
    : FNetworkThread(InSession)
{
	// 스레드 생성
	Thread.Reset(FRunnableThread::Create(this, TEXT("FRecvThread")));
	// 버퍼 reserve
    RecvBuffer.SetNumUninitialized(BUFFER_SIZE);
}

FRecvThread::~FRecvThread()
{
	FNetworkThread::Shutdown();
}

uint32 FRecvThread::Run()
{
    IsRunning = true;
	// 패킷 조립 버퍼
    TArray<uint8> PacketBuffer;
	PacketBuffer.Reserve(PACKET_BUFFER_SIZE);
	
    UE_LOG(LogTemp, Log, TEXT("FRecvThread 시작"));

	while (IsRunning)
    {
        auto Session = OwnerSession.Pin();
        if (Session == nullptr)
        {
            UE_LOG(LogTemp, Warning, TEXT("Session 소멸, FRecvThread 종료"));
        	IsRunning = false;
            break;
        }
		// 소켓 연결상태 확인
        FSocket* Socket = Session->GetSocket();
        if (Socket == nullptr || Socket->GetConnectionState() != SCS_Connected)
        {
        	// 대기 후 재확인
            FPlatformProcess::Sleep(0.01f);
            continue;
        }
        // 소켓에서 데이터 수신
        int32 BytesReceived = 0;
        if (Socket->Recv(RecvBuffer.GetData(), RecvBuffer.Num(), BytesReceived))
        {
            if (BytesReceived > 0)
            {
                // 조립 버퍼에 추가
                PacketBuffer.Append(RecvBuffer.GetData(), BytesReceived);
                // 패킷 조립
                while (PacketBuffer.Num() >= sizeof(FPacketHeader))
                {
                    // 헤더 읽기
                    FPacketHeader Header;
                    FMemory::Memcpy(&Header, PacketBuffer.GetData(), sizeof(FPacketHeader));
                    // 패킷 크기 검증
                    if (Header.PacketSize > MAX_PACKET_SIZE || sizeof(FPacketHeader) > Header.PacketSize)
                    {
                        UE_LOG(LogTemp, Error, TEXT("잘못된 패킷 크기: %d"), Header.PacketSize);
                        break;
                    }
                    // 패킷 전체 도착 확인
                    if (PacketBuffer.Num() >= Header.PacketSize)
                    {
                        // 전체 패킷 추출
                        TArray<uint8> AllPacket;
                    	AllPacket.SetNumUninitialized(Header.PacketSize);
                    	FMemory::Memcpy(AllPacket.GetData(), PacketBuffer.GetData(), Header.PacketSize);
                    	// 수신큐 삽입
                        Session->RecvPacket(AllPacket);
                        // 조립 버퍼 데이터 제거
                        PacketBuffer.RemoveAt(0, Header.PacketSize);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        else
        {
            UE_LOG(LogTemp, Error, TEXT("소켓 수신 실패"));
        	IsRunning = false;
            break;
        }
		FPlatformProcess::Sleep(0.005f);
    }
    
    UE_LOG(LogTemp, Log, TEXT("FRecvThread 종료"));
    return 0;
}
