using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class TcpClientSocket
{
    private Socket _clientSocket;
    private const int _bufferSize = 1024;
    private byte[] _recvBuffer = new byte[_bufferSize];

    public void Connect(string serverIP, int serverPort)
    {
        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = IPAddress.Parse(serverIP);
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, serverPort);

        _clientSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), null);
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            _clientSocket.EndConnect(ar);
            Debug.Log("서버에 연결됨");

            // 연결되자마자 hello world 전송
            Send("hello world");

            // 데이터 수신 시작
            Receive();
        }
        catch (Exception ex)
        {
            Debug.Log($"연결 실패: {ex.Message}");
        }
    }

    public void Send(string message)
    {
        Debug.Log($"서버로 메세지 전송요청 : {message}");
        
        byte[] data = Encoding.UTF8.GetBytes(message);
        _clientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
    }

    private void SendCallback(IAsyncResult ar)
    {
        int bytesSent = _clientSocket.EndSend(ar);
        Debug.Log($"보낸 바이트 수: {bytesSent}");
    }

    private void Receive()
    {
        Debug.Log("[TcpClientSocket] 수신 대기 시작");
        _clientSocket.BeginReceive(_recvBuffer, 0, _bufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
    }


    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            int bytesRead = _clientSocket.EndReceive(ar);
            Debug.Log($"[TcpClientSocket] ReceiveCallback 호출됨, bytesRead: {bytesRead}");

            if (bytesRead > 0)
            {
                string receivedData = Encoding.UTF8.GetString(_recvBuffer, 0, bytesRead);
                Debug.Log($"서버로부터 받은 데이터: {receivedData}");
                Receive();
            }
            else
            {
                Debug.LogWarning("[TcpClientSocket] 받은 데이터 없음");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TcpClientSocket] 수신 중 오류: {ex.Message}");
        }
    }

    public void Disconnect()
    {
        if (_clientSocket != null && _clientSocket.Connected)
        {
            _clientSocket.Shutdown(SocketShutdown.Both);
            _clientSocket.Close();
            Debug.Log("서버와의 연결 종료");
        }
    }
}
