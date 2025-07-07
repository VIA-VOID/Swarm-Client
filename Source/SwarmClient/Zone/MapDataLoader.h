#pragma once
#include "ZoneDefine.h"

/*-------------------------------------------------------
				FMapDataLoader

- 맵 (좌표)데이터를 읽고 파싱
--------------------------------------------------------*/
class FMapDataLoader
{
public:
	// 맵 데이터 읽고 파싱
	static bool LoadMapDataFromFile(const FString& FilePath, FMapData& OutMapData);

private:
	// 파일 읽기
	static bool ReadFileToString(const FString& FilePath, FString& OutJsonString);
	// MapData로 변환
	static bool ParseJsonToMapData(const FString& JsonString, FMapData& OutMapData);
	// Zone 정보 파싱
	static bool ParseZones(const TSharedPtr<FJsonObject>& JsonObject, TArray<FZone>& OutZones);
	// 맵 그리드 파싱
	static bool ParseMapGrid(const TSharedPtr<FJsonObject>& JsonObject, TArray<TArray<int32>>& OutMapGrid);
	// Zone 위치 정보 파싱
	static bool ParseZonePosition(const TSharedPtr<FJsonObject>& JsonObject, FZonePosition& OutPosition);
};
