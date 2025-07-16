// Fill out your copyright notice in the Description page of Project Settings.


#include "Character/Player/SwarmPlayerController.h"

#include "EnhancedInputComponent.h"
#include "EnhancedInputSubsystems.h"
#include "GameDefine.h"
#include "GameMacro.h"
#include "Protocol.pb.h"
#include "Session.h"
#include "SwarmGameInstance.h"
#include "SwarmMyPlayer.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Kismet/KismetMathLibrary.h"

ASwarmPlayerController::ASwarmPlayerController()
{
	PrimaryActorTick.bCanEverTick = true;

	SendDelayTime = 0.f;
	LastMovement = FVector2D::ZeroVector;
}

void ASwarmPlayerController::BeginPlay()
{
	Super::BeginPlay();

	if (IsLocalController())
	{
		// MappingContext 등록
		if (UEnhancedInputLocalPlayerSubsystem* Subsystem = ULocalPlayer::GetSubsystem<UEnhancedInputLocalPlayerSubsystem>(GetLocalPlayer()))
		{
			Subsystem->AddMappingContext(DefaultMappingContext, 0);
		}
	}
}

void ASwarmPlayerController::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);
}

void ASwarmPlayerController::OnPossess(APawn* InPawn)
{
	Super::OnPossess(InPawn);

	Player = Cast<ASwarmMyPlayer>(InPawn);
}

void ASwarmPlayerController::SetupInputComponent()
{
	Super::SetupInputComponent();

	if (UEnhancedInputComponent* EnhancedInputComponent = Cast<UEnhancedInputComponent>(InputComponent))
	{
		// 이동
		EnhancedInputComponent->BindAction(MoveAction, ETriggerEvent::Triggered, this, &ASwarmPlayerController::Move);
		// 시점
		EnhancedInputComponent->BindAction(LookAction, ETriggerEvent::Triggered, this, &ASwarmPlayerController::Look);
	}
}

// 이동 Input 받기
void ASwarmPlayerController::Move(const FInputActionValue& Value)
{
	const FVector2D MovementVector = Value.Get<FVector2D>();
	const FRotator Rotation = GetControlRotation();
	const FRotator YawRotation(0, Rotation.Yaw, 0);
	const FVector ForwardDirection = FRotationMatrix(YawRotation).GetUnitAxis(EAxis::X);
	const FVector RightDirection = FRotationMatrix(YawRotation).GetUnitAxis(EAxis::Y);

	// 캐릭터 이동
	GetCharacter()->AddMovementInput(ForwardDirection, MovementVector.Y);
	GetCharacter()->AddMovementInput(RightDirection, MovementVector.X);
	
	const FVector MoveDirection = ForwardDirection * MovementVector.Y + RightDirection * MovementVector.X;
	const FVector CurrentLocation = Player->GetActorLocation();
	const FRotator Rotator = UKismetMathLibrary::FindLookAtRotation(CurrentLocation, CurrentLocation + MoveDirection);

	// 캐릭터 위치 업데이트
	Protocol::PosInfo PlayerPos;
	PlayerPos.set_x(CurrentLocation.X);
	PlayerPos.set_y(CurrentLocation.Y);
	PlayerPos.set_z(CurrentLocation.Z);
	PlayerPos.set_yaw(Rotator.Yaw);
	Player->UpdatePosition(PlayerPos);

	bool ForceSend = false;
	if (LastMovement != MovementVector)
	{
		ForceSend = true;
		LastMovement = MovementVector;
	}

	// 일정시간 간격으로 이동 패킷 전송
	// - 방향바뀔땐 바로 보냄
	SendDelayTime += GetWorld()->GetDeltaSeconds();
	if (SendDelayTime > GMove_Packet_Interval || ForceSend)
	{
		SendMovePacket(PlayerPos);
		SendDelayTime = 0.f;
	}
}

// 시점 Input 받기
void ASwarmPlayerController::Look(const FInputActionValue& Value)
{
	const FVector2D LookAxis = Value.Get<FVector2D>();

	AddYawInput(LookAxis.X);
	AddPitchInput(LookAxis.Y);
}

// 이동 패킷 전송
void ASwarmPlayerController::SendMovePacket(const Protocol::PosInfo& InPosInfo)
{
	// 이동방향 벡터
	const FVector CurrentAcceleration = Player->GetCharacterMovement()->GetCurrentAcceleration();
	const FVector WorldMoveDirection = CurrentAcceleration.GetSafeNormal();
	
	// 이동 패킷 전송
	Protocol::CS_PLAYER_MOVE MovePkt;
	Protocol::PosInfo PlayerPos;
	MovePkt.mutable_posinfo()->CopyFrom(InPosInfo);
	MovePkt.mutable_movevector()->set_x(WorldMoveDirection.X);
	MovePkt.mutable_movevector()->set_y(WorldMoveDirection.Y);

	SEND_PACKET(EPacketID::CS_PLAYER_MOVE, MovePkt);
}
