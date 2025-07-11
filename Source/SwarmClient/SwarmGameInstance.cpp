// Fill out your copyright notice in the Description page of Project Settings.


#include "SwarmGameInstance.h"

#include "Protocol.pb.h"
#include "Network/Session.h"
#include "Packet/PacketHandler.h"

USwarmGameInstance::USwarmGameInstance()
	: GameSession(nullptr), ServerAddress(TEXT("127.0.0.1")), ServerPort(7777)
{
}

// 서버 연결
void USwarmGameInstance::ConnectToServer(const FString& InAddress, const int32 InServerPort)
{
	if (GameSession == nullptr)
	{
		GameSession = MakeShared<FSession>();
	}
	// 서버 연결 및 네트워크 스레드 시작
	if (GameSession->ConnectToServer(InAddress, InServerPort) == false)
	{
		UE_LOG(LogTemp, Error, TEXT("서버 연결 실패: %s:%d"), *InAddress, InServerPort);
		GameSession.Reset();
		return;
	}
	// 캐릭터 생성 패킷 전송
	// todo: 캐릭터 선택창에서 직업, 캐릭터명 지정 후 넘기게
	// 현재는 선택창이 따로 없기 때문에 하드코딩
	Protocol::CS_PLAYER_ENTER_GAME EnterGame;
	EnterGame.set_playertype(Protocol::PLAYER_TYPE_WARRIOR);
	EnterGame.set_name("PLAYER-01");
	GameSession->SendPacket(EPacketID::CS_PLAYER_ENTER_GAME, EnterGame);
}

// 서버 연결 해제
void USwarmGameInstance::DisconnectFromServer()
{
	if (GameSession == nullptr)
	{
		return;
	}

	GameSession->Disconnect();
}

// 세션 가져오기
FSessionRef USwarmGameInstance::GetGameSession()
{
	return GameSession;
}

// 매 프레임마다 실행
void USwarmGameInstance::Tick(float DeltaTime)
{
	if (GameSession == nullptr)
	{
		return;
	}
	// 수신 패킷 핸들링
	GameSession->ProcessRecvPackets();
}

TStatId USwarmGameInstance::GetStatId() const
{
	RETURN_QUICK_DECLARE_CYCLE_STAT(UNetworkGameInstance, STATGROUP_Tickables);
}

void USwarmGameInstance::OnStart()
{
	Super::OnStart();
	// 패킷 핸들러 초기화
	FPacketHandler::Init(GetWorld());
}
