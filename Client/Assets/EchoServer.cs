using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class EchoServer : MonoBehaviour
{
    private TcpListener _listener;
    private Thread _listenThread;
    private bool _isRunning = false;

    public int port = 7777;

    void Start()
    {
        StartServer();
    }

    void OnApplicationQuit()
    {
        StopServer();
    }

    public void StartServer()
    {
        _isRunning = true;
        _listenThread = new Thread(ListenForClients);
        _listenThread.IsBackground = true;
        _listenThread.Start();
        Debug.Log("Echo 서버 시작됨");
    }

    public void StopServer()
    {
        _isRunning = false;
        _listener?.Stop();
        _listenThread?.Abort();
        Debug.Log("Echo 서버 종료됨");
    }

    private void ListenForClients()
    {
        try
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();

            Debug.Log("클라이언트 대기 중...");

            while (_isRunning)
            {
                TcpClient client = _listener.AcceptTcpClient(); // 블로킹 방식
                Debug.Log("클라이언트 연결됨");

                Thread clientThread = new Thread(HandleClientComm);
                clientThread.IsBackground = true;
                clientThread.Start(client);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"서버 오류: {e.Message}");
        }
    }

    private void HandleClientComm(object clientObj)
    {
        TcpClient client = (TcpClient)clientObj;
        NetworkStream clientStream = client.GetStream();
        byte[] message = new byte[1024];

        try
        {
            while (client.Connected)
            {
                int bytesRead = clientStream.Read(message, 0, message.Length);
                if (bytesRead == 0)
                    break;

                string received = Encoding.UTF8.GetString(message, 0, bytesRead);
                Debug.Log($"받은 메시지: {received}");

                byte[] response = Encoding.UTF8.GetBytes("echo: " + received);
                clientStream.Write(response, 0, response.Length);
                Debug.Log("응답 전송 완료");
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"클라이언트 처리 오류: {ex.Message}");
        }

        client.Close();
    }
}
