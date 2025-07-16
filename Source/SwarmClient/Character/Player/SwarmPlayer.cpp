// Fill out your copyright notice in the Description page of Project Settings.


#include "Character/Player/SwarmPlayer.h"

#include "SwarmMyPlayer.h"
#include "GameFramework/CharacterMovementComponent.h"

ASwarmPlayer::ASwarmPlayer()
{
	if (IsMyPlayer() == false)
	{
		GetCharacterMovement()->bRunPhysicsWithNoController = true;	
	}
}

// Player 이동처리
void ASwarmPlayer::MovePlayer(const Protocol::PosInfo& InPosInfo)
{
	if (IsMyPlayer())
	{
		// 강제이동시킴
		// 서버에서 허용범위 밖으로 벗어났을때만 패킷 보냄
		const FVector TargetLocation(InPosInfo.x(), InPosInfo.y(), InPosInfo.z());
		SetActorLocation(TargetLocation);
	}
	else
	{
		// 위치 업데이트
		UpdatePosition(InPosInfo);	
	}
}

// 본인의 플레이어인지, 네트워크(타) 플레이어 인지 구분
bool ASwarmPlayer::IsMyPlayer()
{
	return Cast<ASwarmMyPlayer>(this) != nullptr;
}

// 플레이어 정보 저장
void ASwarmPlayer::UpdatePlayerInfo(const Protocol::ObjectInfo& InObjectInfo)
{
	UpdateObjectInfo(InObjectInfo);
	PlayerType = InObjectInfo.playertype();
}

void ASwarmPlayer::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	// 타 플레이어 위치 이동
	if (IsMyPlayer() == false)
	{
		// 캐릭터 회전
		const FRotator TargetRotator(0, PosInfo.yaw(), 0);
		const FRotator NewRotation = FMath::RInterpTo(GetActorRotation(), TargetRotator, DeltaTime, 6.f);
		SetActorRotation(NewRotation);
		
		// 캐릭터 이동
		const FVector CurrentLocation = GetActorLocation();
		const FVector TargetLocation(PosInfo.x(), PosInfo.y(), PosInfo.z());
		const FVector Direction = (TargetLocation - CurrentLocation).GetSafeNormal();

		// 목표 거리에 가까워 질때까지 이동
		const float Distance = FVector::Dist(CurrentLocation, TargetLocation);
		if (Distance > 5.f)
		{
			AddMovementInput(Direction);
		}
		else
		{
			// 가까운 거리에서는 바로 멈춤
			GetCharacterMovement()->Velocity = FVector::ZeroVector;
		}
	}
}
