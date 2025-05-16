using Google.Protobuf.Protocol;

/*-------------------------------------------------------
			(DomainName)BasePacketHandler

- 자동생성 파일 (수정 X)
- DomainName : 각 도메인 이름 ex) Player, Monster, etc...
	- DomainName은 .proto의 SC_(도메인명)에서 가져온다.
- 컨텐츠 로직등에서 (DomainName)BasePacketHandler 상속받아 함수 구현
	- 자식 클래스명은 반드시 (DomainName)PacketHandler
	- 자식 클래스는 반드시 싱글톤이여야 한다.
--------------------------------------------------------*/

public abstract class ChatBasePacketHandler
{
	public void RegisterPacketHandlers()
	{
		PacketManager.Instance.Register<SC_CHAT_MSG>(PacketId.SC_CHAT_MSG, msg => Handle_SC_CHAT_MSG((SC_CHAT_MSG)msg));
	}

	protected virtual void Handle_SC_CHAT_MSG(SC_CHAT_MSG packet) { }
}

public abstract class PlayerBasePacketHandler
{
	public void RegisterPacketHandlers()
	{
		PacketManager.Instance.Register<SC_PLAYER_CREATE>(PacketId.SC_PLAYER_CREATE, msg => Handle_SC_PLAYER_CREATE((SC_PLAYER_CREATE)msg));
		PacketManager.Instance.Register<SC_PLAYER_MOVE>(PacketId.SC_PLAYER_MOVE, msg => Handle_SC_PLAYER_MOVE((SC_PLAYER_MOVE)msg));
	}

	protected virtual void Handle_SC_PLAYER_CREATE(SC_PLAYER_CREATE packet) { }
	protected virtual void Handle_SC_PLAYER_MOVE(SC_PLAYER_MOVE packet) { }
}

