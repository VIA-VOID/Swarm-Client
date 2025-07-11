// Fill out your copyright notice in the Description page of Project Settings.


#include "Config/NavMeshExporter.h"

#include "GameDefine.h"
#include "NavigationSystem.h"
#include "NavMesh/RecastNavMesh.h"

// Sets default values
ANavMeshExporter::ANavMeshExporter()
    : GridSize(100.0f), OutputFileName(TEXT("MapData")), NavMesh(nullptr)
{ 	
	PrimaryActorTick.bCanEverTick = false;
}

// 맵 데이터 export
void ANavMeshExporter::ExportMapData()
{
    UE_LOG(LogTemp, Log, TEXT("맵 데이터 추출 시작"));

    // NavMesh 가져오기
    if (GetNavMesh() == false)
    {
        UE_LOG(LogTemp, Error, TEXT("NavMesh를 찾을 수 없음"));
        return;
    }

    // 맵 경계 계산
    GetMapBounds();
    if (MapBounds.IsValid == false)
    {
        UE_LOG(LogTemp, Error, TEXT("맵 경계가 유효하지 않음"));
        return;
    }

    // 그리드 크기 계산
    const int32 GridX = FMath::CeilToInt(MapBounds.GetSize().X / GridSize);
    const int32 GridY = FMath::CeilToInt(MapBounds.GetSize().Y / GridSize);

    UE_LOG(LogTemp, Warning, TEXT("맵 크기: %dx%d 그리드"), GridX, GridY);

    // 맵 데이터 생성
    TArray<TArray<int32>> MapData;
    MapData.SetNum(GridY);
    for (int32 Y = 0; Y < GridY; Y++)
    {
        MapData[Y].SetNum(GridX);
    }

    // 각 그리드 체크
    for (int32 Y = 0; Y < GridY; Y++)
    {
        for (int32 X = 0; X < GridX; X++)
        {
            // 월드 좌표
            const float WorldX = MapBounds.Min.X + (X + 0.5f) * GridSize;
            const float WorldY = MapBounds.Min.Y + (Y + 0.5f) * GridSize;

            // 이동 가능 여부 체크
            const bool bWalkable = IsWalkable(WorldX, WorldY);
            MapData[Y][X] = bWalkable ? 1 : 0;
        }
    }

    // JSON 파일로 저장
    if (SaveToJSON(MapData, GridX, GridY))
    {
        UE_LOG(LogTemp, Warning, TEXT("맵 데이터 추출 완료!"));
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("파일 저장 실패"));
    }
}

// Zone 생성
void ANavMeshExporter::CreateZones()
{
    Zones.Empty();
    // 맵 중심점
    const float CenterX = (MapBounds.Min.X + MapBounds.Max.X) * 0.5f;
    const float CenterY = (MapBounds.Min.Y + MapBounds.Max.Y) * 0.5f;
    // Zone 삽입
    AddZone(EZoneType::Town, TEXT("TownZone"),
        FZonePosition(MapBounds.Min.X, CenterY, CenterX, MapBounds.Max.Y));

    AddZone(EZoneType::Pvp, TEXT("PvpZone"),
        FZonePosition(CenterX, CenterY, MapBounds.Max.X, MapBounds.Max.Y));

    AddZone(EZoneType::Pve, TEXT("PveZone"),
        FZonePosition(CenterX, MapBounds.Min.Y, MapBounds.Max.X, CenterY));

    AddZone(EZoneType::Boss, TEXT("BossZone"),
        FZonePosition(MapBounds.Min.X, MapBounds.Min.Y, CenterX, CenterY));
}

void ANavMeshExporter::AddZone(const EZoneType ZoneType, const FString& ZoneName, const FZonePosition& ZonePosition)
{
    const FZoneInfo Zone(ZoneType, ZoneName, ZonePosition);
    Zones.Add(Zone);
}

// NavMesh 가져오기
bool ANavMeshExporter::GetNavMesh()
{
    UWorld* World = GetWorld();
    if (World == nullptr)
    {
        return false;
    }

    const UNavigationSystemV1* NavSystem = FNavigationSystem::GetCurrent<UNavigationSystemV1>(World);
    if (NavSystem == nullptr)
    {
        return false;
    }

    NavMesh = Cast<ARecastNavMesh>(NavSystem->GetDefaultNavDataInstance());
    return (NavMesh != nullptr);
}

// 맵 경계 가져오기
void ANavMeshExporter::GetMapBounds()
{
    if (NavMesh == nullptr)
    {
        MapBounds = FBox(ForceInit);
    }
    MapBounds = NavMesh->GetBounds();
}

// 이동 가능여부
bool ANavMeshExporter::IsWalkable(const float X, const float Y) const
{
    if (NavMesh == nullptr)
    {
        return false;
    }

    // 2D 좌표만 사용
    const FVector WorldPos(X, Y, 0.0f);    
    FNavLocation NavLocation;
    const FVector SearchExtent(GridSize * 0.5f, GridSize * 0.5f, 10000.0f);
    // navMesh상 위치 투영
    return NavMesh->ProjectPoint(WorldPos, NavLocation, SearchExtent);
}

// json 파일로 저장
bool ANavMeshExporter::SaveToJSON(const TArray<TArray<int32>>& MapData, const int32 GridX, const int32 GridY)
{
    // Zone 생성
    CreateZones();
    // JSON 루트 객체 생성
    const TSharedPtr<FJsonObject> RootObject = MakeShareable(new FJsonObject);
    // 기본 정보 추가
    RootObject->SetStringField(TEXT("mapName"), GetWorld()->GetMapName());
    RootObject->SetNumberField(TEXT("gridSize"), GridSize);
    RootObject->SetNumberField(TEXT("gridX"), GridX);
    RootObject->SetNumberField(TEXT("gridY"), GridY);
    RootObject->SetNumberField(TEXT("worldMinX"), MapBounds.Min.X);
    RootObject->SetNumberField(TEXT("worldMinY"), MapBounds.Min.Y);
    RootObject->SetNumberField(TEXT("worldMaxX"), MapBounds.Max.X);
    RootObject->SetNumberField(TEXT("worldMaxY"), MapBounds.Max.Y);
    RootObject->SetNumberField(TEXT("totalCells"), GridX * GridY);
    RootObject->SetStringField(TEXT("exportTime"), FDateTime::Now().ToString());
    // Zone 정보 추가
    TArray<TSharedPtr<FJsonValue>> ZoneArray;
    for (const FZoneInfo& Zone : Zones)
    {
        const TSharedPtr<FJsonObject> ZoneObject = MakeShareable(new FJsonObject);
        ZoneObject->SetNumberField(TEXT("zoneType"), Zone.ZoneType);
        ZoneObject->SetStringField(TEXT("zoneName"), Zone.ZoneName);
        
        // 월드 좌표
        TSharedPtr<FJsonObject> WorldObject = MakeShareable(new FJsonObject);
        WorldObject->SetNumberField(TEXT("minX"), Zone.WorldPos.MinX);
        WorldObject->SetNumberField(TEXT("minY"), Zone.WorldPos.MinY);
        WorldObject->SetNumberField(TEXT("maxX"), Zone.WorldPos.MaxX);
        WorldObject->SetNumberField(TEXT("maxY"), Zone.WorldPos.MaxY);
        ZoneObject->SetObjectField(TEXT("worldPos"), WorldObject);

        ZoneArray.Add(MakeShareable(new FJsonValueObject(ZoneObject)));
    }
    RootObject->SetArrayField(TEXT("zones"), ZoneArray);

    // 맵 데이터 저장
    TArray<TSharedPtr<FJsonValue>> MapRows;
    for (int32 Y = 0; Y < GridY; Y++)
    {
        TArray<TSharedPtr<FJsonValue>> MapRow;
        for (int32 X = 0; X < GridX; X++)
        {
            MapRow.Add(MakeShareable(new FJsonValueNumber(MapData[Y][X])));
        }
        MapRows.Add(MakeShareable(new FJsonValueArray(MapRow)));
    }
    RootObject->SetArrayField(TEXT("mapData"), MapRows);

    // JSON 문자열로 변환
    FString OutputString;
    const TSharedRef<TJsonWriter<TCHAR, TPrettyJsonPrintPolicy<TCHAR>>> Writer =
        TJsonWriterFactory<TCHAR, TPrettyJsonPrintPolicy<TCHAR>>::Create(&OutputString);
    FJsonSerializer::Serialize(RootObject.ToSharedRef(), Writer);

    // 파일 경로 생성
    const FString FilePath = FPaths::Combine(
        FPaths::ProjectContentDir(),
        TEXT("ServerData"),
        OutputFileName + TEXT(".json")
    );

    // 디렉토리 생성
    if (const FString Directory = FPaths::GetPath(FilePath); FPaths::DirectoryExists(Directory) == false)
    {
        IFileManager::Get().MakeDirectory(*Directory, true);
    }

    // 파일 저장
    const bool IsSaved = FFileHelper::SaveStringToFile(OutputString, *FilePath);
    if (IsSaved)
    {
        const int64 FileSize = IFileManager::Get().FileSize(*FilePath);
        UE_LOG(LogTemp, Warning, TEXT("파일 저장 완료: %s (%.2f KB)"), *FilePath, FileSize / 1024.0f);
    }
    return IsSaved;
}
