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
	FZonePosition(const float InMinX, const float InMinY, const float InMaxX, const float InMaxY)
		: MinX(InMinX), MinY(InMinY), MaxX(InMaxX), MaxY(InMaxY) {}
	
	int32 MinX = 0;
	int32 MinY = 0;
	int32 MaxX = 0;
	int32 MaxY = 0;
};

// Zone 정보
struct FZoneInfo
{
	FZoneInfo()
		: ZoneName(""), ZoneType(0), GridSize(0), WorldPos(0, 0, 0, 0) {}
	
	FZoneInfo(const EZoneType InZoneType, const FString& InZoneName, const FZonePosition ZonePosition)
		: ZoneName(InZoneName), ZoneType(static_cast<uint8>(InZoneType)), WorldPos(ZonePosition) {}
	
	FString ZoneName;
	uint8 ZoneType;
	int32 GridSize;
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
	TArray<FZoneInfo> Zones;
	// 맵 그리드 데이터
	TArray<TArray<int8>> MapGrid;
};
