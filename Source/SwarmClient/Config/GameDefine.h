#pragma once

// RTT 측정 최대 크기
constexpr uint8 GRtt_History_Size = 100;
// 이동패킷 전송 간격 (기본 초당 5회)
constexpr float GMove_Packet_Interval = 0.2f;
// 이동 방향에서 이동 가능거리인지 판별할 거리
constexpr float GCan_Move_Distance = 120.f;