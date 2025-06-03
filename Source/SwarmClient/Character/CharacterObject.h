// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Struct.pb.h"
#include "GameFramework/Character.h"
#include "CharacterObject.generated.h"

/*-------------------------------------------------------
				ACharacterObject

- 모든 캐릭터의 공통 항목
- ACharacterObject 상속받아 플레이어, 몬스터 구현
--------------------------------------------------------*/
UCLASS()
class SWARMCLIENT_API ACharacterObject : public ACharacter
{
	GENERATED_BODY()

public:
	ACharacterObject();

protected:
	virtual void BeginPlay() override;
	virtual void Tick(float DeltaTime) override;

protected:
	// 캐릭터 공통 정보
	Protocol::CharacterInfo Info;
};
