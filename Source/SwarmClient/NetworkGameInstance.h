// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Engine/GameInstance.h"
#include "NetworkGameInstance.generated.h"

class FSession;

/*-------------------------------------------------------
				UNetworkGameInstance

- 네트워크 및 세션 관리
--------------------------------------------------------*/
UCLASS()
class SWARMCLIENT_API UNetworkGameInstance : public UGameInstance, public FTickableGameObject
{
	GENERATED_BODY()

public:
	UNetworkGameInstance();
	// 서버 연결
	UFUNCTION(BlueprintCallable, Category = "Network")
	void ConnectToServer(const FString& InAddress, int32 InServerPort);
	// 서버 연결 해제
	UFUNCTION(BlueprintCallable, Category = "Network")
	void DisconnectFromServer();

protected:
	virtual void Tick(float DeltaTime) override;
	virtual TStatId GetStatId() const override;

private:
	TSharedPtr<FSession> GameSession;
	FString ServerAddress;
	int32 ServerPort;
};
