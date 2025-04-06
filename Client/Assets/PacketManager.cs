using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public struct PacketHeader
{
    public short type;
    public int size;
}

public struct PacketMessage
{
    public short a;
    public int b;
    public short c;
    public int size; // msg 길이
    public string msg; // 가변 메시지
}

public class PacketManager : MonoBehaviour
{
    private TcpClient _client;
    private NetworkStream _stream;
    private byte[] _recvBuffer = new byte[4096];
    
    public string serverIp = "127.0.0.1";
    public int serverPort = 5000;
    
    async void Start()
    {
        await ConnectToServer();
    }
    
    async Task ConnectToServer()
    {
        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(serverIp, serverPort);
            _stream = _client.GetStream();
            Debug.Log($"서버에 연결됨: {serverIp}:{serverPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"서버 연결 실패: {e.Message}");
        }
    }
    
    async Task SendToServer(byte[] data)
    {
        if (_stream == null)
        {
            Debug.LogError("네트워크 스트림이 없습니다.");
            return;
        }

        try
        {
            await _stream.WriteAsync(data, 0, data.Length);
            Debug.Log($"[Send] {data.Length} bytes 보냄");
        }
        catch (Exception e)
        {
            Debug.LogError($"보내기 실패: {e.Message}");
        }
    }
    
    async Task ReceiveFromServer()
    {
        if (_stream == null)
        {
            Debug.LogError("네트워크 스트림이 없습니다.");
            return;
        }

        try
        {
            int bytesRead = await _stream.ReadAsync(_recvBuffer, 0, _recvBuffer.Length);
            if (bytesRead > 0)
            {
                byte[] actualData = new byte[bytesRead];
                Array.Copy(_recvBuffer, actualData, bytesRead);
                OnReceive(actualData);
            }
            else
            {
                Debug.Log("서버로부터 받은 데이터 없음");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"수신 실패: {e.Message}");
        }
    }
    
    void OnReceive(byte[] recvBuffer)
    {
        ReceivePacket(recvBuffer);
    }
    
    public static byte[] SendPacket(short a, int b, short c, string msg)
    {
        byte[] msgBytes = System.Text.Encoding.UTF8.GetBytes(msg);
        int helloSize = sizeof(short) + sizeof(int) + sizeof(short) + sizeof(int) + msgBytes.Length;
        int totalSize = sizeof(short) + sizeof(int) + helloSize;

        using (var ms = new MemoryStream(totalSize))
        using (var bw = new BinaryWriter(ms))
        {
            // Header
            bw.Write((short)100); // type
            bw.Write(helloSize);  // size (pkt_hello 전체 크기)

            // pkt_hello
            bw.Write(a);
            bw.Write(b);
            bw.Write(c);
            bw.Write(msgBytes.Length);
            bw.Write(msgBytes);

            return ms.ToArray();
        }
    }
    
    public static void ReceivePacket(byte[] data)
    {
        using (var ms = new MemoryStream(data))
        using (var br = new BinaryReader(ms))
        {
            PacketHeader header = new PacketHeader();
            header.type = br.ReadInt16();
            header.size = br.ReadInt32();

            if (header.type != 100)
            {
                Debug.LogWarning("Unknown packetMessage type");
                return;
            }

            PacketMessage packetMessage = new PacketMessage();
            packetMessage.a = br.ReadInt16();
            packetMessage.b = br.ReadInt32();
            packetMessage.c = br.ReadInt16();
            packetMessage.size = br.ReadInt32();

            byte[] msgBytes = br.ReadBytes(packetMessage.size);
            packetMessage.msg = System.Text.Encoding.UTF8.GetString(msgBytes);

            Debug.Log($"[Recv] a: {packetMessage.a}, b: {packetMessage.b}, c: {packetMessage.c}, msgSize: {packetMessage.size}, msg: {packetMessage.msg}");
        }
    }
}
