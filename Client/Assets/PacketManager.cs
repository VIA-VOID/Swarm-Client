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
    public int msgLength;
    
    //가변길이는 별도로 처리
}

#endregion

public class PacketManager : MonoBehaviour
{
    private Socket _socket;
    private byte[] _recvBuffer = new byte[4096];
    private PacketHeader sendHeader;
    private PacketMessage sendMessage;
    private string sendMsgText;
    
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
            // NetStream 대신 사용
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
        sendMsgText = message;

        byte[] packet = BuildPacketWithMarshal();
        // 패킷 형식 자체를 보냄
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
                int read = await _socket.ReceiveAsync(_recvBuffer.AsMemory(0, headerSize), SocketFlags.None);
                if (read < headerSize) continue;

                PacketHeader header = ByteToStructure<PacketHeader>(_recvBuffer);

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

    #region 패킷 조립, 파싱

    byte[] BuildPacketWithMarshal()
    {
        byte[] msgBytes = Encoding.UTF8.GetBytes(sendMsgText);
        sendMessage.msgLength = msgBytes.Length;

        sendHeader.type = 100;
        sendHeader.size = Marshal.SizeOf<PacketMessage>() + msgBytes.Length;
        
        int totalSize = Marshal.SizeOf<PacketHeader>() + sendHeader.size;
        byte[] buffer = new byte[totalSize];

        int offset = 0;

        // 헤더 사이즈 부터 메세지 길이만큼
        byte[] headerBytes = StructureToBytes(sendHeader);
        Buffer.BlockCopy(headerBytes, 0, buffer, offset, headerBytes.Length);
        offset += headerBytes.Length;

        byte[] fixedBytes = StructureToBytes(sendMessage);
        Buffer.BlockCopy(fixedBytes, 0, buffer, offset, fixedBytes.Length);
        offset += fixedBytes.Length;

        Buffer.BlockCopy(msgBytes, 0, buffer, offset, msgBytes.Length);

        return buffer;
    }

    void ReceivePacket(byte[] data)
    {
        int offset = 0;

        PacketHeader header = ByteToStructure<PacketHeader>(data, ref offset);
        if (header.type != 100)
        {
            Debug.LogWarning("알 수 없는 패킷 타입");
            return;
        }

        PacketMessage fixedPart = ByteToStructure<PacketMessage>(data, ref offset);
        string msg = Encoding.UTF8.GetString(data, offset, fixedPart.msgLength);

        Debug.Log($"[Recv] a: {fixedPart.a}, b: {fixedPart.b}, c: {fixedPart.c}, msgSize: {fixedPart.msgLength}, msg: {msg}");
    }

    // 구조체 바이트로 변경
    static byte[] StructureToBytes<T>(T structure) where T : struct
    {
        // 변경시 마샬 적용
        int size = Marshal.SizeOf<T>();
        byte[] bytes = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(structure, ptr, false);
        Marshal.Copy(ptr, bytes, 0, size);
        Marshal.FreeHGlobal(ptr);
        return bytes;
    }

    // 바이트를 구조체로
    static T ByteToStructure<T>(byte[] data, ref int offset) where T : struct
    {
        // 마찬가지로 마샬 적용
        int size = Marshal.SizeOf<T>();
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(data, offset, ptr, size);
        T result = Marshal.PtrToStructure<T>(ptr);
        Marshal.FreeHGlobal(ptr);
        offset += size;
        return result;
    }

    static T ByteToStructure<T>(byte[] data) where T : struct
    {
        int offset = 0;
        return ByteToStructure<T>(data, ref offset);
    }
    
    #endregion
}


