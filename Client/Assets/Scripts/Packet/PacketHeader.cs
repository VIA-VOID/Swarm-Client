public enum PacketId
{
    INVALID_ID = 0,
	CS_CHAT_MSG = 10001,
	CS_PLAYER_CREATE = 10002,
	CS_PLAYER_MOVE = 10003,
	SC_CHAT_MSG = 30001,
	SC_PLAYER_CREATE = 30002,
	SC_PLAYER_MOVE = 30003,
}

public struct PacketHeader
{
    // 패킷 식별자
    public ushort Id;
    // 헤더를 포함한 전체 패킷 크기
    public ushort Size;
    public static readonly int HeaderSize = sizeof(ushort) + sizeof(ushort);
}
