using System;
using System.IO;
using System.Text;
using UnityEngine;

public class PacketTester : MonoBehaviour
{
    void Start()
    {
        Debug.Log("테스트 실행");
        // 테스트용 송신 (서버 역할)
        string originalMsg = "안녕 클라이언트!";
        
        byte[] recvBuffer = MakeHelloPacket(1, 1234, 2, originalMsg);

        // 클라이언트 수신
        ParseHelloPacket(recvBuffer);

        // 수신한 내용과 같은 내용 송신
        // byte[] sendBuffer = MakeHelloPacket(1, 1234, 2, originalMsg);
        //
        // ParseHelloPacket(sendBuffer);
    }

    byte[] MakeHelloPacket(short a, int b, short c, string msg)
    {
        byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
        int helloSize = sizeof(short) + sizeof(int) + sizeof(short) + sizeof(int) + msgBytes.Length;
        int totalSize = sizeof(short) + sizeof(int) + helloSize;

        using (var ms = new MemoryStream(totalSize))
        using (var bw = new BinaryWriter(ms))
        {
            bw.Write((short)100);
            bw.Write(helloSize); 
            
            bw.Write(a);
            bw.Write(b);
            bw.Write(c);
            bw.Write(msgBytes.Length);
            bw.Write(msgBytes);

            return ms.ToArray();
        }
    }

    void ParseHelloPacket(byte[] data)
    {
        using (var ms = new MemoryStream(data))
        using (var br = new BinaryReader(ms))
        {
            // header
            short type = br.ReadInt16();
            int size = br.ReadInt32();

            if (type != 100)
            {
                Debug.LogWarning($"Unknown packet type: {type}");
                return;
            }
            
            short a = br.ReadInt16();
            int b = br.ReadInt32();
            short c = br.ReadInt16();
            int msgSize = br.ReadInt32();
            byte[] msgBytes = br.ReadBytes(msgSize);
            string msg = Encoding.UTF8.GetString(msgBytes);
            
            Debug.Log($"type: {type}");
            Debug.Log($"pkt_hello size: {size}");
            Debug.Log($"a: {a}, b: {b}, c: {c}");
            Debug.Log($"msg size: {msgSize}");
            Debug.Log($"msg: {msg}");
        }
    }
}
