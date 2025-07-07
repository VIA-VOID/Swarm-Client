// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"

// 버퍼 크기 (4KB)
constexpr uint32 G_Buffer_Size = 4096;
// 패킷 버퍼 크기 (512MB)
constexpr uint32 G_Packet_Buffer_Size = 512;

/**
 *	패킷구조
 *	[id][size][protobuf data]
 *	- id: 프로토콜 ID
 *	- size: 패킷 전체 크기(헤더 포함)
 *	- data: 직렬화된 protobuf 데이터
 */
struct FPacketHeader
{
	// 프로토콜 ID
	uint16 PacketId;
	// 패킷 전체 크기 (헤더 포함)
	uint16 PacketSize;
};
