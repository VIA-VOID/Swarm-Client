// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "ChatDefine.generated.h"


UENUM(BlueprintType)
enum class EMsgType : uint8
{
	None        = 0,
	All		    = 1,
	General     = 2,
	Local       = 3,
	System      = 4,
};

USTRUCT(BlueprintType)
struct FChatMessage
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, EditAnywhere)
	int64 Timestamp;

	UPROPERTY(BlueprintReadWrite, EditAnywhere)
	FString PlayerName;

	UPROPERTY(BlueprintReadWrite, EditAnywhere)
	FString Message;

	UPROPERTY(BlueprintReadWrite, EditAnywhere)
	EMsgType MsgType;

	FChatMessage()
	{
		Timestamp = 0;
		PlayerName = TEXT("");
		Message = TEXT("");
		MsgType = EMsgType::General;
	}

	FChatMessage(const int64 InTimestamp, const FString& InPlayerName, const FString& InMessage, const EMsgType InMsgType)
	{
		Timestamp = InTimestamp;
		PlayerName = InPlayerName;
		Message = InMessage;
		MsgType = InMsgType;
	}
};
