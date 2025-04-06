using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

#region Packet Structs

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PacketHeader
{
    public short type;
    public int size; // body size
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PacketMessage
{
    public short a;
    public int b;
    public short c;
    public int size; // msg 길이
    public string msg; // 문자열은 따로 처리
}

#endregion

public class PacketManager : MonoBehaviour
{
    private Socket _socket;
    private byte[] _recvBuffer = new byte[4096];
    private PacketHeader sendHeader;
    private PacketMessage sendMessage;

    private NetworkBufferManager _bufferManager = new NetworkBufferManager();

    public string serverIp = "127.0.0.1";
    public int serverPort = 5000;

    [InlineButton("OnClickSendMessage", SdfIconType.Messenger, "서버로 메시지 전송")]
    [SerializeField] private string message;

    async void Start()
    {
        await ConnectToServer();
    }

    void Update()
    {
        // 메인 스레드에서 수신 패킷 처리
        while (_bufferManager.TryDequeue(out var packet))
        {
            ReceivePacket(packet);
        }
    }

    async Task ConnectToServer()
    {
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await _socket.ConnectAsync(IPAddress.Parse(serverIp), serverPort);
            Debug.Log($"[Socket] 연결됨: {serverIp}:{serverPort}");
            _ = ReceiveLoop();
        }
        catch (Exception e)
        {
            Debug.LogError($"서버 연결 실패: {e.Message}");
        }
    }


    public void OnClickSendMessage()
    {
        _ = SendMessageAsync();
    }

    async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        sendMessage.a = 1;
        sendMessage.b = 1234;
        sendMessage.c = 2;
        sendMessage.msg = message;

        byte[] packet = BuildPacket();
        await _socket.SendAsync(packet, SocketFlags.None);
        Debug.Log($"[Send] {packet.Length} bytes 보냄");
    }

    async Task ReceiveLoop()
    {
        int headerSize = Marshal.SizeOf<PacketHeader>();

        while (true)
        {
            try
            {
                //헤더 수신
                int read = await _socket.ReceiveAsync(_recvBuffer.AsMemory(0, headerSize), SocketFlags.None);
                if (read < headerSize) continue;

                PacketHeader header;
                using (var br = new BinaryReader(new MemoryStream(_recvBuffer, 0, headerSize)))
                {
                    header.type = br.ReadInt16();
                    header.size = br.ReadInt32();
                }

                // 바디 수신
                int bodySize = header.size;
                int offset = 0;
                while (offset < bodySize)
                {
                    int r = await _socket.ReceiveAsync(_recvBuffer.AsMemory(headerSize + offset, bodySize - offset), SocketFlags.None);
                    if (r == 0) break;
                    offset += r;
                }

                byte[] fullPacket = new byte[headerSize + bodySize];
                Array.Copy(_recvBuffer, fullPacket, fullPacket.Length);
                _bufferManager.EnqueueBuffer(fullPacket);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ReceiveLoop] 오류: {e.Message}");
                break;
            }
        }
    }

    #region 패킷 조립 & 파싱

    byte[] BuildPacket()
    {
        byte[] msgBytes = Encoding.UTF8.GetBytes(sendMessage.msg);
        sendMessage.size = msgBytes.Length;

        sendHeader.type = 100;
        sendHeader.size = sizeof(short) + sizeof(int) + sizeof(short) + msgBytes.Length;

        using (var ms = new MemoryStream())
        using (var bw = new BinaryWriter(ms))
        {
            bw.Write(sendHeader.type);
            bw.Write(sendHeader.size);
            bw.Write(sendMessage.a);
            bw.Write(sendMessage.b);
            bw.Write(sendMessage.c);
            bw.Write(sendMessage.size);
            bw.Write(msgBytes);
            return ms.ToArray();
        }
    }

    void ReceivePacket(byte[] data)
    {
        using (var ms = new MemoryStream(data))
        using (var br = new BinaryReader(ms))
        {
            PacketHeader header = new PacketHeader
            {
                type = br.ReadInt16(),
                size = br.ReadInt32()
            };

            if (header.type != 100)
            {
                Debug.LogWarning("알 수 없는 패킷 타입");
                return;
            }

            PacketMessage msg = new PacketMessage
            {
                a = br.ReadInt16(),
                b = br.ReadInt32(),
                c = br.ReadInt16(),
                size = br.ReadInt32()
            };

            byte[] msgBytes = br.ReadBytes(msg.size);
            msg.msg = Encoding.UTF8.GetString(msgBytes);

            Debug.Log($"[Recv] a: {msg.a}, b: {msg.b}, c: {msg.c}, msgSize: {msg.size}, msg: {msg.msg}");
        }
    }

    #endregion
}
