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
