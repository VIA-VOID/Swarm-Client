// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "SwarmPlayer.h"
#include "SwarmMyPlayer.generated.h"

struct FInputActionValue;
/*-------------------------------------------------------
						AMyPlayer

- 클라에 접속된 자신의 캐릭터
--------------------------------------------------------*/
UCLASS()
class SWARMCLIENT_API ASwarmMyPlayer : public ASwarmPlayer
{
	GENERATED_BODY()

public:
	ASwarmMyPlayer();

protected:
	virtual void BeginPlay() override;
	virtual void Tick(float DeltaTime) override;

private:
	/** Camera boom positioning the camera behind the character */
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = Camera, meta = (AllowPrivateAccess = "true"))
	class USpringArmComponent* CameraBoom;
	/** Follow camera */
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = Camera, meta = (AllowPrivateAccess = "true"))
	class UCameraComponent* FollowCamera;
};
