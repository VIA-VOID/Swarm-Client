#pragma once

// Zone 타입
enum class EZoneType : uint8
{
	Town = 0,
	Pvp = 1,
	Pve = 2,
	Boss = 3
};

// Zone 위치 정보
struct FZonePosition
{
	int32 MinX = 0;
	int32 MinY = 0;
	int32 MaxX = 0;
	int32 MaxY = 0;
};

// Zone 정보
struct FZone
{
	FString ZoneName;
	EZoneType ZoneType;
	int32 GridSize = 0;
	FZonePosition WorldPos;
};

// 맵 데이터
struct FMapData
{
	FString MapName;
	int32 GridSize = 0;
	int32 GridX = 0;
	int32 GridY = 0;
	int32 WorldMinX = 0;
	int32 WorldMinY = 0;
	int32 WorldMaxX = 0;
	int32 WorldMaxY = 0;
	TArray<FZone> Zones;
	// 맵 그리드 데이터
	TArray<TArray<int32>> MapGrid;
};
