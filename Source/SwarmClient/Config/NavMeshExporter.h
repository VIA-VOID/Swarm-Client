// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "NavMesh/RecastNavMesh.h"
#include "Zone/ZoneDefine.h"
#include "NavMeshExporter.generated.h"

/*--------------------------------------------------------
					ANavMeshExporter

- 맵 데이터 exporter
- 각 좌표별 이동가능 1, 이동불가 0
- 구역별로 영역(Zone) 분리, json파일로 export
--------------------------------------------------------*/
UCLASS()
class SWARMCLIENT_API ANavMeshExporter : public AActor
{
	GENERATED_BODY()
	
public:
	ANavMeshExporter();
	// 맵 데이터 export
	UFUNCTION(BlueprintCallable, Category = "Map Export")
	void ExportMapData();

private:
	// Zone 생성
	void CreateZones();
	// Zones에 ZoneInfo 추가
	void AddZone(const EZoneType ZoneType, const FString& ZoneName, const FZonePosition& ZonePosition);
	// NavMesh 가져오기
	bool GetNavMesh();
	// 맵 경계 가져오기
	void GetMapBounds();
	// 이동 가능여부
	bool IsWalkable(const float X, const float Y) const;
	// json 파일로 저장
	bool SaveToJSON(const TArray<TArray<int32>>& MapData, const int32 GridX, const int32 GridY);

public:
	float GridSize;
	FString OutputFileName;
	TArray<FZoneInfo> Zones;

private:
	ARecastNavMesh* NavMesh;
	FBox MapBounds;
};
