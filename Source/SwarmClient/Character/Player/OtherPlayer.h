// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Character/CharacterObject.h"
#include "OtherPlayer.generated.h"

/**
 * 
 */
UCLASS()
class SWARMCLIENT_API AOtherPlayer : public ACharacterObject
{
	GENERATED_BODY()

private:
	// 플레이어 직업 타입
	Protocol::PlayerType PlayerJob;
};
