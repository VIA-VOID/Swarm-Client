// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Character/CharacterObject.h"
#include "MyPlayer.generated.h"

/*-------------------------------------------------------
						AMyPlayer

- 클라에 접속된 자신의 캐릭터
--------------------------------------------------------*/
UCLASS()
class SWARMCLIENT_API AMyPlayer : public ACharacterObject
{
	GENERATED_BODY()

public:
	AMyPlayer();

protected:
	/** Camera boom positioning the camera behind the character */
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = Camera, meta = (AllowPrivateAccess = "true"))
	class USpringArmComponent* CameraBoom;

	/** Follow camera */
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = Camera, meta = (AllowPrivateAccess = "true"))
	class UCameraComponent* FollowCamera;
	
	
private:
	// 플레이어 직업 타입
	Protocol::PlayerType PlayerJob;
	
};
