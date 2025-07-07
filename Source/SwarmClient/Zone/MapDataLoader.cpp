#include "MapDataLoader.h"

#include "GameDefine.h"

// 맵 데이터 읽고 파싱
bool FMapDataLoader::LoadMapDataFromFile(const FString& FilePath, FMapData& OutMapData)
{
    // 파일 읽기
    FString JsonString;
    if (ReadFileToString(FilePath, JsonString) == false)
    {
        UE_LOG(LogTemp, Error, TEXT("JSON 파일 읽기 실패: %s"), *FilePath);
        return false;
    }

    // JSON 파싱
    if (ParseJsonToMapData(JsonString, OutMapData) == false)
    {
        UE_LOG(LogTemp, Error, TEXT("JSON 파싱 실패: %s"), *FilePath);
        return false;
    }

    UE_LOG(LogTemp, Warning, TEXT("맵 데이터 로드 완료: %s"), *OutMapData.MapName);
    return true;
}

// 파일 읽기
bool FMapDataLoader::ReadFileToString(const FString& FilePath, FString& OutJsonString)
{
    // 파일 존재 확인
    if (FPlatformFileManager::Get().GetPlatformFile().FileExists(*FilePath) == false)
    {
        return false;
    }

    // 파일 읽기
    if (FFileHelper::LoadFileToString(OutJsonString, *FilePath) == false)
    {
        return false;
    }

    // 파일이 비어있는지 확인
    if (OutJsonString.IsEmpty())
    {
        return false;
    }

    return true;
}

// MapData로 변환
bool FMapDataLoader::ParseJsonToMapData(const FString& JsonString, FMapData& OutMapData)
{
    const TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(JsonString);
    TSharedPtr<FJsonObject> JsonObject;

    // JSON 파싱
    if (FJsonSerializer::Deserialize(Reader, JsonObject) == false || JsonObject.IsValid() == false)
    {
        UE_LOG(LogTemp, Error, TEXT("JSON 파싱 실패"));
        return false;
    }

    // 기본 정보 파싱
    OutMapData.MapName = JsonObject->GetStringField(TEXT("mapName"));
    OutMapData.GridSize = JsonObject->GetIntegerField(TEXT("gridSize"));
    OutMapData.GridX = JsonObject->GetIntegerField(TEXT("gridX"));
    OutMapData.GridY = JsonObject->GetIntegerField(TEXT("gridY"));
    OutMapData.WorldMinX = JsonObject->GetIntegerField(TEXT("worldMinX")) / GPos_Revise_Num;
    OutMapData.WorldMinY = JsonObject->GetIntegerField(TEXT("worldMinY")) / GPos_Revise_Num;
    OutMapData.WorldMaxX = JsonObject->GetIntegerField(TEXT("worldMaxX")) / GPos_Revise_Num;
    OutMapData.WorldMaxY = JsonObject->GetIntegerField(TEXT("worldMaxY")) / GPos_Revise_Num;

    // Zone 정보 파싱
    if (ParseZones(JsonObject, OutMapData.Zones) == false)
    {
        UE_LOG(LogTemp, Error, TEXT("Zone 정보 파싱 실패"));
        return false;
    }

    // 맵 그리드 파싱
    if (ParseMapGrid(JsonObject, OutMapData.MapGrid) == false)
    {
        UE_LOG(LogTemp, Error, TEXT("맵 그리드 파싱 실패"));
        return false;
    }

    return true;
}

// Zone 정보 파싱
bool FMapDataLoader::ParseZones(const TSharedPtr<FJsonObject>& JsonObject, TArray<FZone>& OutZones)
{
    // zones 가져오기
    const TArray<TSharedPtr<FJsonValue>>* ZoneArray;
    if (JsonObject->TryGetArrayField(TEXT("zones"), ZoneArray) == false)
    {
        return true;
    }

    OutZones.Empty();
    OutZones.Reserve(ZoneArray->Num());

    // 각 Zone 정보 파싱
    for (int32 i = 0; i < ZoneArray->Num(); i++)
    {
        TSharedPtr<FJsonObject> ZoneObject = (*ZoneArray)[i]->AsObject();
        if (ZoneObject.IsValid() == false)
        {
            continue;
        }

        FZone Zone;
        
        Zone.ZoneName = ZoneObject->GetStringField(TEXT("zoneName"));
        Zone.ZoneType = static_cast<EZoneType>(ZoneObject->GetIntegerField(TEXT("zoneType")));
        Zone.GridSize = ZoneObject->GetIntegerField(TEXT("gridSize"));

        // 월드 위치 정보 파싱
        TSharedPtr<FJsonObject> WorldPosObject = ZoneObject->GetObjectField(TEXT("worldPos"));
        if (WorldPosObject.IsValid())
        {
            if (ParseZonePosition(WorldPosObject, Zone.WorldPos) == false)
            {
                continue;
            }
        }

        OutZones.Add(Zone);
    }
    
    return true;
}

// 맵 그리드 파싱
bool FMapDataLoader::ParseMapGrid(const TSharedPtr<FJsonObject>& JsonObject, TArray<TArray<int32>>& OutMapGrid)
{
    // mapData 가져오기
    const TArray<TSharedPtr<FJsonValue>>* MapDataArray;
    if (JsonObject->TryGetArrayField(TEXT("mapData"), MapDataArray) == false)
    {
        return false;
    }

    OutMapGrid.Empty();
    OutMapGrid.Reserve(MapDataArray->Num());

    // 각 행 파싱
    for (int32 Y = 0; Y < MapDataArray->Num(); Y++)
    {
        const TArray<TSharedPtr<FJsonValue>>* RowArray;
        if ((*MapDataArray)[Y]->TryGetArray(RowArray) == false)
        {
            return false;
        }

        TArray<int32> Row;
        Row.Reserve(RowArray->Num());
        
        for (int32 X = 0; X < RowArray->Num(); X++)
        {
            int32 CellValue = (*RowArray)[X]->AsNumber();
            Row.Add(CellValue);
        }

        OutMapGrid.Add(Row);
    }

    return true;
}

// Zone 위치 정보 파싱
bool FMapDataLoader::ParseZonePosition(const TSharedPtr<FJsonObject>& JsonObject, FZonePosition& OutPosition)
{
    if (JsonObject.IsValid() == false)
    {
        return false;
    }

    OutPosition.MinX = JsonObject->GetIntegerField(TEXT("minX")) / GPos_Revise_Num;
    OutPosition.MinY = JsonObject->GetIntegerField(TEXT("minY")) / GPos_Revise_Num;
    OutPosition.MaxX = JsonObject->GetIntegerField(TEXT("maxX")) / GPos_Revise_Num;
    OutPosition.MaxY = JsonObject->GetIntegerField(TEXT("maxY")) / GPos_Revise_Num;

    return true;
}