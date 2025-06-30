// Fill out your copyright notice in the Description page of Project Settings.


#include "Character/Player/SwarmPlayerController.h"
#include "EnhancedInputComponent.h"
#include "EnhancedInputSubsystems.h"
#include "SwarmMyPlayer.h"

ASwarmPlayerController::ASwarmPlayerController()
{
	PrimaryActorTick.bCanEverTick = true;
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

void ASwarmPlayerController::SetupInputComponent()
{
	Super::SetupInputComponent();

	if (UEnhancedInputComponent* EnhancedInputComponent = Cast<UEnhancedInputComponent>(InputComponent))
	{
		// 점프		
		EnhancedInputComponent->BindAction(JumpAction, ETriggerEvent::Started, this, &ASwarmPlayerController::Jump);
		EnhancedInputComponent->BindAction(JumpAction, ETriggerEvent::Completed, this, &ASwarmPlayerController::StopJump);
		// 이동
		EnhancedInputComponent->BindAction(MoveAction, ETriggerEvent::Triggered, this, &ASwarmPlayerController::Move);
		// 시점
		EnhancedInputComponent->BindAction(LookAction, ETriggerEvent::Triggered, this, &ASwarmPlayerController::Look);
	}
}

// 점프
void ASwarmPlayerController::Jump()
{
	if (ASwarmMyPlayer* SwarmMyPlayer = Cast<ASwarmMyPlayer>(GetPawn()))
	{
		SwarmMyPlayer->Jump();
	}
}

// 점프 중지
void ASwarmPlayerController::StopJump()
{
	if (ASwarmMyPlayer* SwarmMyPlayer = Cast<ASwarmMyPlayer>(GetPawn()))
	{
		SwarmMyPlayer->StopJumping();
	}
}

// 이동 Input 받기
void ASwarmPlayerController::Move(const FInputActionValue& Value)
{
	const FVector2D MovementVector = Value.Get<FVector2D>();

	if (ACharacter* ControlledCharacter = GetCharacter())
	{
		const FRotator Rotation = GetControlRotation();
		const FRotator YawRotation(0, Rotation.Yaw, 0);

		const FVector ForwardDirection = FRotationMatrix(YawRotation).GetUnitAxis(EAxis::X);
		const FVector RightDirection = FRotationMatrix(YawRotation).GetUnitAxis(EAxis::Y);

		ControlledCharacter->AddMovementInput(ForwardDirection, MovementVector.Y);
		ControlledCharacter->AddMovementInput(RightDirection, MovementVector.X);
	}
}

// 시점 Input 받기
void ASwarmPlayerController::Look(const FInputActionValue& Value)
{
	const FVector2D LookAxis = Value.Get<FVector2D>();

	AddYawInput(LookAxis.X);
	AddPitchInput(LookAxis.Y);
}
