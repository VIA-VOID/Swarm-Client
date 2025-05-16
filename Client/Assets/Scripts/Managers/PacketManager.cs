using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using UnityEngine;

/*-------------------------------------------------------
				PacketInfo

- 패킷 정보
- protobuf 패킷 매핑
--------------------------------------------------------*/
public class PacketInfo
{
    public PacketInfo(PacketId packetId, Type packetType, Func<IMessage> factory, Action<IMessage> handler)
    {
        PacketId = packetId;
        PacketType = packetType;
        Factory = factory;
        Handler = handler;
    }
    // 패킷 ID
    public PacketId PacketId { get; private set; }
    // 패킷 유형
    public Type PacketType { get; private set; }
    // 패킷 생성 팩토리
    public Func<IMessage> Factory { get; private set; }
    // 패킷 처리 핸들러
    public Action<IMessage> Handler { get; private set; }
}

/*-------------------------------------------------------
				PacketManager

- 패킷 생성, 직렬화, 역직렬화, 처리 등 패킷 생명주기 관리자
- Protobuf 메시지와 데이터 간 변환
- 패킷 테이블(Dictionary)를 이용해 패킷별 처리 로직 자동화
- 패킷 정보 등록(Register)은 PacketSystem에서 수행
--------------------------------------------------------*/
public class PacketManager
{
    #region Singleton
    private static PacketManager _instance;
    public static PacketManager Instance => _instance ??= new PacketManager();
    #endregion

    private Dictionary<PacketId, PacketInfo> _packetInfos = new Dictionary<PacketId, PacketInfo>();
    private Dictionary<Type, PacketId> _typeToId = new Dictionary<Type, PacketId>();

    // 패킷 정보 등록
    public void Register<T>(PacketId packetId, Action<IMessage> handler) where T : IMessage, new()
    {
        Type packetType = typeof(T);
        // 빈 팩토리 함수 생성
        Func<IMessage> factory = () => new T();
        // 패킷 정보 객체 생성, 등록
        var packetInfo = new PacketInfo(packetId, packetType, factory, handler);
        _packetInfos.Add(packetId, packetInfo);
        _typeToId.Add(packetType, packetId);
    }

    // 패킷 생성
    public IMessage CreatePacket(PacketId packetId)
    {
        if (_packetInfos.TryGetValue(packetId, out var packetInfo))
        {
            return packetInfo.Factory();
        }
        return null;
    }

    // 패킷 타입 등록
    // 타입으로부터 ID 찾기
    public PacketId GetPacketId<T>() where T : IMessage
    {
        Type packetType = typeof(T);
        if (_typeToId.TryGetValue(packetType, out var packetId))
        {
            return packetId;
        }
        return PacketId.INVALID_ID;
    }

    // 패킷 처리
    public void OnRecvPacket(ArraySegment<byte> buffer)
    {
        if (buffer.Array == null)
        {
            return;
        }
        int offset = buffer.Offset;
        // 패킷 ID 추출
        PacketId packetId = (PacketId)BitConverter.ToUInt16(buffer.Array, offset);
        offset += sizeof(ushort);

        // 패킷 크기 추출
        ushort packetSize = BitConverter.ToUInt16(buffer.Array, offset);
        offset += sizeof(ushort);

        if (_packetInfos.TryGetValue(packetId, out var packetInfo))
        {
            // 패킷 생성
            IMessage packet = packetInfo.Factory();
            // 패킷 파싱
            ArraySegment<byte> dataSegment = new ArraySegment<byte>(
                buffer.Array,
                buffer.Offset + PacketHeader.HeaderSize,
                packetSize - PacketHeader.HeaderSize
            );
            // 데이터 파싱
            packet.MergeFrom(dataSegment.Array, dataSegment.Offset, dataSegment.Count);
            // 핸들러 호출
            packetInfo.Handler(packet);
        }
    }

    // 전송할 패킷 버퍼 생성
    public ArraySegment<byte> MakeSendBuffer<T>(T packet) where T : IMessage
    {
        PacketId packetId = GetPacketId<T>();
        int payloadSize = packet.CalculateSize();
        ushort totalSize = (ushort)(payloadSize + PacketHeader.HeaderSize);

        byte[] buffer = new byte[totalSize];
        int offset = 0;

        // 헤더 작성
        BitConverter.GetBytes((ushort)packetId).CopyTo(buffer, offset);
        offset += sizeof(ushort);
        BitConverter.GetBytes(totalSize).CopyTo(buffer, offset);
        offset += sizeof(ushort);

        // 데이터 직렬화
        using (MemoryStream ms = new MemoryStream(buffer, offset, payloadSize, writable: true))
        {
            CodedOutputStream cos = new CodedOutputStream(ms);
            packet.WriteTo(cos);
            cos.Flush();
        }

        return new ArraySegment<byte>(buffer, 0, totalSize);
    }
}
