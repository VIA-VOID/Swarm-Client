// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/PlayerController.h"
#include "InputActionValue.h"
#include "SwarmPlayerController.generated.h"

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
	virtual void SetupInputComponent() override;

private:
	// 점프
	void Jump();
	void StopJump();
	// 이동 Input 받기
	void Move(const FInputActionValue& Value);
	// 시점 Input 받기
	void Look(const FInputActionValue& Value);

protected:
	/** MappingContext */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = Input, meta = (AllowPrivateAccess = "true"))
	class UInputMappingContext* DefaultMappingContext;
	// 점프 action
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = Input, meta = (AllowPrivateAccess = "true"))
	class UInputAction* JumpAction;
	// 이동 action
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = Input, meta = (AllowPrivateAccess = "true"))
	UInputAction* MoveAction;
	// 시점 action
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = Input, meta = (AllowPrivateAccess = "true"))
	UInputAction* LookAction;
};
