// Fill out your copyright notice in the Description page of Project Settings.


#include "Config/NavMeshExporter.h"
#include "NavigationSystem.h"
#include "NavMesh/RecastNavMesh.h"

// Sets default values
ANavMeshExporter::ANavMeshExporter()
    : NavMesh(nullptr)
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
    FBox MapBounds = GetMapBounds();
    if (MapBounds.IsValid == false)
    {
        UE_LOG(LogTemp, Error, TEXT("맵 경계가 유효하지 않음"));
        return;
    }

    // 그리드 크기 계산
    int32 GridX = FMath::CeilToInt(MapBounds.GetSize().X / GridSize);
    int32 GridY = FMath::CeilToInt(MapBounds.GetSize().Y / GridSize);

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
    bool IsSaved = SaveToJSON(MapData, GridX, GridY);
    if (IsSaved)
    {
        UE_LOG(LogTemp, Warning, TEXT("맵 데이터 추출 완료!"));
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("파일 저장 실패"));
    }
}

// NavMesh 가져오기
bool ANavMeshExporter::GetNavMesh()
{
    UWorld* World = GetWorld();
    if (World == nullptr)
    {
        return false;
    }

    UNavigationSystemV1* NavSystem = FNavigationSystem::GetCurrent<UNavigationSystemV1>(World);
    if (NavSystem == nullptr)
    {
        return false;
    }

    NavMesh = Cast<ARecastNavMesh>(NavSystem->GetDefaultNavDataInstance());
    return (NavMesh != nullptr);
}

// 맵 경계 가져오기
FBox ANavMeshExporter::GetMapBounds()
{
    // 빈 박스 리턴
    if (NavMesh == nullptr)
    {
        return FBox(ForceInit);
    }

    // 경계 가져오기
    FBox Bounds = NavMesh->GetBounds();

    UE_LOG(LogTemp, Warning, TEXT("[SimpleExtractor] 맵 경계: (%.1f, %.1f) ~ (%.1f, %.1f)"),
        Bounds.Min.X, Bounds.Min.Y, Bounds.Max.X, Bounds.Max.Y);

    return Bounds;
}

// 이동 가능여부
bool ANavMeshExporter::IsWalkable(float X, float Y)
{
    if (NavMesh == nullptr)
    {
        return false;
    }

    // 2D 좌표만 사용
    FVector WorldPos(X, Y, 0.0f);    
    FNavLocation NavLocation;
    FVector SearchExtent(GridSize * 0.5f, GridSize * 0.5f, 10000.0f);
    // navMesh상 위치 투영
    return NavMesh->ProjectPoint(WorldPos, NavLocation, SearchExtent);
}

// json 파일로 저장
bool ANavMeshExporter::SaveToJSON(const TArray<TArray<int32>>& MapData, int32 GridX, int32 GridY)
{
    // JSON 루트 객체 생성
    TSharedPtr<FJsonObject> RootObject = MakeShareable(new FJsonObject);

    // 기본 정보
    RootObject->SetStringField(TEXT("mapName"), GetWorld()->GetMapName());
    RootObject->SetNumberField(TEXT("gridX"), GridX);
    RootObject->SetNumberField(TEXT("gridY"), GridY);
    RootObject->SetNumberField(TEXT("gridSize"), GridSize);
    RootObject->SetNumberField(TEXT("totalCells"), GridX * GridY);
    RootObject->SetStringField(TEXT("exportTime"), FDateTime::Now().ToString());

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
    TSharedRef<TJsonWriter<TCHAR, TPrettyJsonPrintPolicy<TCHAR>>> Writer =
        TJsonWriterFactory<TCHAR, TPrettyJsonPrintPolicy<TCHAR>>::Create(&OutputString);
    FJsonSerializer::Serialize(RootObject.ToSharedRef(), Writer);

    // 파일 경로 생성
    FString FilePath = FPaths::Combine(
        FPaths::ProjectContentDir(),
        TEXT("ServerData"),
        OutputFileName + TEXT(".json")
    );

    // 디렉토리 생성
    FString Directory = FPaths::GetPath(FilePath);
    if (FPaths::DirectoryExists(Directory) == false)
    {
        IFileManager::Get().MakeDirectory(*Directory, true);
    }

    // 파일 저장
    bool IsSaved = FFileHelper::SaveStringToFile(OutputString, *FilePath);
    if (IsSaved)
    {
        int64 FileSize = IFileManager::Get().FileSize(*FilePath);
        UE_LOG(LogTemp, Warning, TEXT("파일 저장 완료: %s (%.2f KB)"), *FilePath, FileSize / 1024.0f);
    }
    return IsSaved;
}
