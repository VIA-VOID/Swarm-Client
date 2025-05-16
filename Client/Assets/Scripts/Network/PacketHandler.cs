using System;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;

/*-------------------------------------------------------
				PacketHandler

- 자동생성 파일 (수정 X)
- 컨텐츠 로직등에서 PacketHandler를 상속받아, 로직 구현부 구현
--------------------------------------------------------*/
public class PacketHandler
{
	// SC_PLAYER 패킷 처리
	public static void Handle_SC_PLAYER(Google.Protobuf.Protocol.SC_PLAYER packet)
	{
		Debug.Log($"Handle_SC_PLAYER 호출: {packet}");
	}


    // 패킷 핸들러 및 팩토리 등록
    public static void RegisterPacketHandlers(PacketManager packetManager)
    {
        // 패킷 팩토리 등록
        packetManager.RegisterPacketFactories();

        // 패킷 타입 등록
		packetManager.RegisterType<Google.Protobuf.Protocol.SC_PLAYER>(PacketId.SC_PLAYER);

        // 패킷 핸들러 등록
		packetManager.Register(PacketId.SC_PLAYER, msg => Handle_SC_PLAYER((Google.Protobuf.Protocol.SC_PLAYER)msg));
    }
}
