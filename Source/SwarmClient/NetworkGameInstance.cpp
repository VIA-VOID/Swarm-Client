// Fill out your copyright notice in the Description page of Project Settings.


#include "NetworkGameInstance.h"
#include "Network/Session.h"
#include "Packet/PacketHandler.h"

UNetworkGameInstance::UNetworkGameInstance()
	: GameSession(nullptr), ServerAddress(TEXT("127.0.0.1")), ServerPort(7777)
{
	// 패킷 핸들러 초기화
	FPacketHandler::Init();
}

// 서버 연결
void UNetworkGameInstance::ConnectToServer(const FString& InServerAddress, int32 InServerPort)
{
	if (GameSession == nullptr)
	{
		GameSession = MakeShared<FSession>();		
	}
	// 서버 연결 및 네트워크 스레드 시작
	if (GameSession->ConnectToServer(InServerAddress, InServerPort) == false)
	{
		UE_LOG(LogTemp, Error, TEXT("서버 연결 실패: %s:%d"), *InServerAddress, InServerPort);
		GameSession.Reset();
	}
}

// 서버 연결 해제
void UNetworkGameInstance::DisconnectFromServer()
{
	if (GameSession == nullptr)
	{
		return;
	}
	// TODO: 연결 해제 패킷 전송
	// 이후에 Disconnect 실행
	// GameSession->Disconnect();
}

// 매 프레임마다 실행
void UNetworkGameInstance::Tick(float DeltaTime)
{
	if (GameSession == nullptr)
	{
		return;
	}
	// 수신 패킷 핸들링
	GameSession->ProcessRecvPackets();
}

TStatId UNetworkGameInstance::GetStatId() const
{
	RETURN_QUICK_DECLARE_CYCLE_STAT(UNetworkGameInstance, STATGROUP_Tickables);
}
