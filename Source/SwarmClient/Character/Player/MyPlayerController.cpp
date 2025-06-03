// Fill out your copyright notice in the Description page of Project Settings.


#include "Character/Player/MyPlayerController.h"
#include "EnhancedInputComponent.h"
#include "EnhancedInputSubsystems.h"
#include "MyPlayer.h"

void AMyPlayerController::BeginPlay()
{
	Super::BeginPlay();

	if (IsLocalController())
	{
		// MappingContext 등록
		if (UEnhancedInputLocalPlayerSubsystem* Subsystem = ULocalPlayer::GetSubsystem<UEnhancedInputLocalPlayerSubsystem>(GetLocalPlayer()))
		{
			Subsystem->AddMappingContext(InputMappingContext, 0);
		}
		// MyPlayer 연결
		if (AMyPlayer* MyPlayer = Cast<AMyPlayer>(GetPawn()))
		{
			
		}
	}
}

void AMyPlayerController::SetupInputComponent()
{
	Super::SetupInputComponent();

	if (UEnhancedInputComponent* Input = Cast<UEnhancedInputComponent>(InputComponent))
	{
		Input->BindAction(MoveAction, ETriggerEvent::Triggered, this, &AMyPlayerController::Move);
		Input->BindAction(LookAction, ETriggerEvent::Triggered, this, &AMyPlayerController::Look);
		Input->BindAction(JumpAction, ETriggerEvent::Started, this, &AMyPlayerController::Jump);
		Input->BindAction(JumpAction, ETriggerEvent::Completed, this, &AMyPlayerController::StopJump);
	}
}

void AMyPlayerController::Move(const FInputActionValue& Value)
{
}

void AMyPlayerController::Look(const FInputActionValue& Value)
{
	FVector2D LookAxis = Value.Get<FVector2D>();
	AddYawInput(LookAxis.X);
	AddPitchInput(LookAxis.Y);
}

void AMyPlayerController::Jump()
{
	if (AMyPlayer* MyPlayer = Cast<AMyPlayer>(GetPawn()))
	{
		MyPlayer->Jump();
	}
}

void AMyPlayerController::StopJump()
{
	if (AMyPlayer* MyPlayer = Cast<AMyPlayer>(GetPawn()))
	{
		MyPlayer->StopJumping();
	}
}
