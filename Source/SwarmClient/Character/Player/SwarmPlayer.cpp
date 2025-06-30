// Fill out your copyright notice in the Description page of Project Settings.


#include "Character/Player/SwarmPlayer.h"

#include "SwarmMyPlayer.h"

// 본인의 플레이어인지, 네트워크(타) 플레이어 인지 구분
bool ASwarmPlayer::IsMyPlayer()
{
	return Cast<ASwarmMyPlayer>(this) != nullptr;
}

// 플레이어 정보 저장
void ASwarmPlayer::UpdatePlayerInfo(const Protocol::ObjectInfo& InObjectInfo)
{
	UpdateObjectInfo(InObjectInfo);
	PlayerType = InObjectInfo.playertype();
}
