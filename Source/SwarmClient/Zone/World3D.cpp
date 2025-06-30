#include "World3D.h"

#include "GameDefine.h"

FWorld3D::FWorld3D()
	: X(0), Y(0), Z(0), Yaw(0)
{
}

FWorld3D::FWorld3D(const int32 ServerX, const int32 ServerY, const int32 ServerZ/* = 0*/, const int32 ServerYaw/* = 0*/)
	: X(ServerX), Y(ServerY), Z(ServerZ), Yaw(ServerYaw)
{
}

// 서버좌표에서 FVector로 변환
FVector FWorld3D::ConvertToVector() const
{
	const double WorldX = X / GPos_Revise_Num;
	const double WorldY = Y / GPos_Revise_Num;
	const double WorldZ = Z / GPos_Revise_Num;
	
	return FVector(WorldX, WorldY, WorldZ);
}

void FWorld3D::UpdatePosition(const Protocol::PosInfo& InPosInfo)
{
	X = InPosInfo.x();
	Y = InPosInfo.y();
	Z = InPosInfo.z();
	Yaw = InPosInfo.yaw();
}

// Yaw 변환
double FWorld3D::GetYaw() const
{
	return Yaw / GPos_Revise_Num;
}
