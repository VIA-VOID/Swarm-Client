#pragma once

//----------------------------------------------------------//
// 스마트포인터
#define SHARED_PTR(name)		using name##Ref = TSharedPtr<class name>;
#define WEAK_PTR(name)			using name##WRef = TWeakPtr<class name>;
#define UNIQUE_PTR(name)		using name##URef = TUniquePtr<class name>;

UNIQUE_PTR(FPacketHandler);

SHARED_PTR(FSession);

WEAK_PTR(FSession);