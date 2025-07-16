// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/PlayerController.h"
#include "InputActionValue.h"
#include "Struct.pb.h"
#include "SwarmPlayerController.generated.h"

class ASwarmMyPlayer;

/*-------------------------------------------------------
				AMyPlayerController

- Player의 입력, 카메라 및 행동 관리
- UI 시스템과 연동
--------------------------------------------------------*/
UCLASS()
class SWARMCLIENT_API ASwarmPlayerController : public APlayerController
{
	GENERATED_BODY()

public:
	ASwarmPlayerController();

protected:
	virtual void BeginPlay() override;
	virtual void Tick(float DeltaTime) override;
	virtual void OnPossess(APawn* InPawn) override;
	virtual void SetupInputComponent() override;

private:
	// 이동 Input 받기
	void Move(const FInputActionValue& Value);
	// 시점 Input 받기
	void Look(const FInputActionValue& Value);
	// 이동 패킷 전송
	void SendMovePacket(const Protocol::PosInfo& InPosInfo);

protected:
	/** MappingContext */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = Input, meta = (AllowPrivateAccess = "true"))
	class UInputMappingContext* DefaultMappingContext;
	// 이동 action
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = Input, meta = (AllowPrivateAccess = "true"))
	class UInputAction* MoveAction;
	// 시점 action
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = Input, meta = (AllowPrivateAccess = "true"))
	UInputAction* LookAction;

private:
	// 캐릭터
	ASwarmMyPlayer* Player;
	// 이동관련
	float SendDelayTime;
	FVector2D LastMovement;
};
