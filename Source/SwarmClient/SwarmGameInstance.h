// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameMacro.h"
#include "Engine/GameInstance.h"
#include "SwarmGameInstance.generated.h"

class FSession;

/*-------------------------------------------------------
				UNetworkGameInstance

- 네트워크 및 세션 관리
--------------------------------------------------------*/
UCLASS()
class SWARMCLIENT_API USwarmGameInstance : public UGameInstance, public FTickableGameObject
{
	GENERATED_BODY()

public:
	USwarmGameInstance();
	// 서버 연결
	UFUNCTION(BlueprintCallable, Category = "Network")
	void ConnectToServer(const FString& InAddress, const int32 InServerPort);
	// 서버 연결 해제
	UFUNCTION(BlueprintCallable, Category = "Network")
	void DisconnectFromServer();
	// 세션 가져오기
	FSessionRef GetGameSession();

protected:
	virtual void Tick(float DeltaTime) override;
	virtual TStatId GetStatId() const override;
	virtual void OnStart() override;

private:
	FSessionRef GameSession;
	FString ServerAddress;
	int32 ServerPort;
};
