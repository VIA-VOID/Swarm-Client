// Fill out your copyright notice in the Description page of Project Settings.


#include "GameObject.h"

#include "Components/CapsuleComponent.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Zone/ZoneGameState.h"

AGameObject::AGameObject()
{
	PrimaryActorTick.bCanEverTick = true;
	// Set size for collision capsule
	GetCapsuleComponent()->InitCapsuleSize(42.f, 96.0f);

	// Don't rotate when the controller rotates. Let that just affect the camera.
	bUseControllerRotationPitch = false;
	bUseControllerRotationYaw = false;
	bUseControllerRotationRoll = false;
	
	// Configure character movement
	GetCharacterMovement()->bOrientRotationToMovement = true; // Character moves in the direction of input...	
	GetCharacterMovement()->RotationRate = FRotator(0.0f, 500.0f, 0.0f); // ...at this rotation rate

	// Note: For faster iteration times these variables, and many more, can be tweaked in the Character Blueprint
	// instead of recompiling to adjust them
	GetCharacterMovement()->JumpZVelocity = 420.f;
	GetCharacterMovement()->AirControl = 0.35f;
	GetCharacterMovement()->MaxWalkSpeed = 500.f;
	GetCharacterMovement()->MinAnalogWalkSpeed = 20.f;
	GetCharacterMovement()->BrakingDecelerationWalking = 2000.f;
	GetCharacterMovement()->BrakingDecelerationFalling = 1500.0f;
}

// Object 정보 업데이트
void AGameObject::UpdateObjectInfo(const Protocol::ObjectInfo& InObjInfo)
{
	Type = InObjInfo.type();
	StatInfo.CopyFrom(InObjInfo.statinfo());
	Name = InObjInfo.name();
	UpdatePosition(InObjInfo.posinfo());
}

void AGameObject::BeginPlay()
{
	Super::BeginPlay();
	
}

void AGameObject::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);
}

// 위치 업데이트
void AGameObject::UpdatePosition(const Protocol::PosInfo& InPosInfo)
{
	PosInfo.CopyFrom(InPosInfo);
	World3D.UpdatePosition(InPosInfo);
}

// Player인지 확인
bool AGameObject::IsPlayer() const
{
	return Type == Protocol::ObjectType::OBJECT_TYPE_PLAYER;
}

// Monster인지 확인
bool AGameObject::IsMonster() const
{
	return Type == Protocol::ObjectType::OBJECT_TYPE_MONSTER;
}
