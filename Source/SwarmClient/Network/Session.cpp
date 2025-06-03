#include "Session.h"
#include "NetworkThread.h"
#include "Sockets.h"
#include "SocketSubsystem.h"
#include "Packet/PacketHandler.h"

/*----------------------------
        FSession
----------------------------*/

FSession::FSession()
    : Socket(nullptr), SocketSubsystem(nullptr), ServerPort(0), IsConnect(false)
{
    SocketSubsystem = ISocketSubsystem::Get(PLATFORM_SOCKETSUBSYSTEM);
}

FSession::~FSession()
{
    // 연결종료
    Disconnect();
}

// 서버 연결
bool FSession::ConnectToServer(const FString& InServerAddress, int32 InServerPort)
{
    if (IsConnect)
    {
        return false;
    }
    ServerAddress = InServerAddress;
    ServerPort = InServerPort;
    // 소켓 생성
    if (CreateSocket() == false)
    {
        return false;
    }
    // 서버 주소 설정
    TSharedRef<FInternetAddr> ServerAddr = SocketSubsystem->CreateInternetAddr();
    bool IsValid = false;
    ServerAddr->SetIp(*ServerAddress, IsValid);
    ServerAddr->SetPort(ServerPort);
    // 서버 연결
    if (IsValid == false || Socket->Connect(*ServerAddr) == false)
    {
        // 소켓 정리
        CleanupSocket();
        return false;
    }
    // 연결 완료
    IsConnect = true;
    // 네트워크 스레드 시작
    StartThreads();
    
    UE_LOG(LogTemp, Log, TEXT("서버 연결 성공: %s:%d"), *ServerAddress, ServerPort);
    
    return true;
}

// 연결 종료
void FSession::Disconnect()
{
    if (IsConnect == false)
    {
        return;
    }
    IsConnect = false;
    // 송/수신 스레드 정지
    StopThreads();
    // 소켓 정리
    CleanupSocket();
    // 수신 큐 비우기
    TArray<uint8> TempPacket;
    while (ReceiveQueue.Dequeue(TempPacket))
    {
    }
}

// 송신큐 삽입
void FSession::RecvPacket(const TArray<uint8>& Packet)
{
    ReceiveQueue.Enqueue(Packet);
}

// 수신된 패킷 처리
void FSession::ProcessRecvPackets()
{
    TArray<uint8> PacketData;
    // 수신된 모든 패킷 처리
    while (ReceiveQueue.Dequeue(PacketData))
    {
        FPacketHandler::HandlePacket(this, PacketData.GetData(), PacketData.Num());
    }
}

// Socket Get
FSocket* FSession::GetSocket() const
{
    return Socket;
}

// 소켓 생성
bool FSession::CreateSocket()
{
    // 소켓 생성
    Socket = SocketSubsystem->CreateSocket(NAME_Stream, TEXT("ClientSession"));
    if (Socket == nullptr)
    {
        return false;
    }
    // 소켓 옵션 설정
    // 블로킹 모드 사용
    // - 송/수신 스레드 각각 사용
    Socket->SetNonBlocking(false);
    // Nagle 알고리즘 비활성화
    Socket->SetNoDelay(true);
    // Linger 즉시 종료
    Socket->SetLinger(true, 0);

    return true;
}

// 소켓 정리
void FSession::CleanupSocket()
{
    if (Socket)
    {
        Socket->Close();
        
        if (SocketSubsystem)
        {
            SocketSubsystem->DestroySocket(Socket);
        }
        Socket = nullptr;
    }
}

// 송/수신 스레드 시작
void FSession::StartThreads()
{
    SendThread = MakeUnique<FSendThread>(AsShared());
    RecvThread = MakeUnique<FRecvThread>(AsShared());
}

// 송/수신 스레드 정지
void FSession::StopThreads()
{
    if (RecvThread)
    {
        RecvThread->Shutdown();
        RecvThread.Reset();
    }
    if (SendThread)
    {
        SendThread->Shutdown();
        SendThread.Reset();
    }
}
