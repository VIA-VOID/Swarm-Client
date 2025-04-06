using System.IO;
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
    void Start()
    {
        string testMessage = "Hello, Server";
        var sendBuffer = SendPacket(1, 2222, 3, testMessage);

        Debug.Log($"[Send] {sendBuffer.Length} bytes");

        // 수신 테스트 (Loopback)
        ReceivePacket(sendBuffer);
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
