#pragma once
#include "Struct.pb.h"

/*--------------------------------------------------------
					FWorld3D

- 서버와의 통신시 월드 좌표 연산(double <-> Int)
- 고정 소수점 연산 캡슐화
	- 소수점 최대 3자리, GPos_Revise_Num 수치로 보정
--------------------------------------------------------*/

class FWorld3D
{
public:
	FWorld3D();
	// 서버좌표 받기(GPos_Revise_Num 배수만큼 들어옴)
	FWorld3D(const int32 ServerX, const int32 ServerY, const int32 ServerZ = 0, const int32 ServerYaw = 0);
	// 서버좌표에서 FVector로 변환
	FVector ConvertToVector() const;
	// Protocol::PosInfo의 값으로 멤버 업데이트
	void UpdatePosition(const Protocol::PosInfo& InPosInfo);
	// Yaw 변환
	double GetYaw() const;
	
public:
	int32 X;
	int32 Y;
	int32 Z;
	int32 Yaw;
};
