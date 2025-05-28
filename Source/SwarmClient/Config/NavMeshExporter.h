// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "NavMesh/RecastNavMesh.h"
#include "NavMeshExporter.generated.h"

UCLASS()
class SWARMCLIENT_API ANavMeshExporter : public AActor
{
	GENERATED_BODY()
	
public:
	// Sets default values for this actor's properties
	ANavMeshExporter();
	// 맵 데이터 export
	UFUNCTION(BlueprintCallable, Category = "Map Export")
	void ExportMapData();

private:
	// NavMesh 가져오기
	bool GetNavMesh();
	// 맵 경계 가져오기
	FBox GetMapBounds();
	// 이동 가능여부
	bool IsWalkable(float X, float Y);
	// json 파일로 저장
	bool SaveToJSON(const TArray<TArray<int32>>& MapData, int32 GridX, int32 GridY);

public:
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Settings")
	float GridSize = 100.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Settings")
	FString OutputFileName = TEXT("MapData");

private:
	ARecastNavMesh* NavMesh;
};
