using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

/*-------------------------------------------------------
				Session

- 서버와의 세션 관리
- 소켓 연결 및 데이터 송수신
--------------------------------------------------------*/
public class Session
{
    // 소켓 및 연결 정보
    private Socket _socket;
    private IPEndPoint _endPoint;

    // 수신 버퍼
    private RecvBuffer _recvBuffer;

    // 연결 상태
    private int _isConnect;

    // 송신 관련
    private object _lock = new object();
    private Queue<SendBuffer> _sendQueue;
    private int _isSending;

    // 소켓 비동기 이벤트
    private SocketAsyncEventArgs _recvArgs;
    private SocketAsyncEventArgs _sendArgs;

    // 외부에서 등록할 수 있는 이벤트 콜백
    public Action OnConnected;
    public Action OnDisconnected;
    public Action<byte[], int> OnRecv;
    public Action<int> OnSend;

    public Session()
    {
        _recvBuffer = new RecvBuffer(ushort.MaxValue);
        _sendQueue = new Queue<SendBuffer>();
        _isSending = 0;
        _isConnect = 0;
        // 이벤트 객체 생성
        _recvArgs = CreateSocketEventArgs();
        _sendArgs = CreateSocketEventArgs();
    }

    // 서버에 연결
    public void Connect(IPEndPoint endPoint)
    {
        _endPoint = endPoint;
        _socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // 소켓옵션 적용
        SetSocketOpt();

        SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs();
        connectArgs.RemoteEndPoint = endPoint;

        EventHandler<SocketAsyncEventArgs> handler = (s, e) =>
        {
            if (e.SocketError == SocketError.Success)
            {
                Interlocked.Exchange(ref _isConnect, 1);
                OnConnected?.Invoke();
                // 수신 등록
                RegisterRecv();
            }
            else
            {
                Debug.LogError($"연결 실패: {e.SocketError}");
                OnDisconnected?.Invoke();
            }
        };

        connectArgs.Completed += handler;

        if (_socket.ConnectAsync(connectArgs) == false)
        {
            // 등록한 handler 호출
            handler(this, connectArgs);
        }
    }

    // 송신 요청
    public void Send(byte[] buffer, int offset, int size)
    {
        if (IsConnected() == false)
        {
            return;
        }

        SendBuffer sendBuffer = new SendBuffer(size + 1);
        sendBuffer.Write(buffer, offset, size);

        lock (_lock)
        {
            _sendQueue.Enqueue(sendBuffer);
            // 송신중이 아니라면
            if (Interlocked.CompareExchange(ref _isSending, 1, 0) == 0)
            {
                // 송신 등록
                RegisterSend();
            }
        }
    }

    // 연결 종료
    public void Close()
    {
        // 0이면 이미 종료된 상태
        if (Interlocked.Exchange(ref _isConnect, 0) == 0)
        {
            return;
        }

        try
        {
            // 소켓 종료
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket = null;

            // 전송 큐 비우기
            lock (_lock)
            {
                _sendQueue.Clear();
                // 송신 플래그 초기화
                Interlocked.Exchange(ref _isSending, 0);
            }

            _recvArgs.Dispose();
            _sendArgs.Dispose();
            // 연결 종료 이벤트 호출
            OnDisconnected?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"연결 종료 실패: {e.Message}");
        }
    }

    // 연결 상태 확인
    public bool IsConnected()
    {
        return Interlocked.CompareExchange(ref _isConnect, 0, 0) == 1;
    }

    // 이벤트 객체 생성
    private SocketAsyncEventArgs CreateSocketEventArgs()
    {
        var args = new SocketAsyncEventArgs();
        args.Completed += OnIoCompleted;
        return args;
    }

    // 소켓 옵션 설정
    private void SetSocketOpt()
    {
        // Nagle 비활성화
        _socket.NoDelay = true;
        // 5초 대기 후 종료
        _socket.LingerState = new LingerOption(true, 5);
    }

    // 수신 등록
    // 데이터 수신 대기
    private void RegisterRecv()
    {
        if (IsConnected() == false)
        {
            return;
        }

        ArraySegment<byte> segment = _recvBuffer.GetWriteSegment();
        _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

        if (_socket.ReceiveAsync(_recvArgs) == false)
        {
            OnIoCompleted(this, _recvArgs);
        }
    }

    // 송신 등록
    private void RegisterSend()
    {
        if (IsConnected() == false)
        {
            return;
        }

        SendBuffer buffer = _sendQueue.Peek();
        ArraySegment<byte> segment = buffer.GetReadSegment();
        _sendArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

        if (_socket.SendAsync(_sendArgs) == false)
        {
            // IO 완료처리
            OnIoCompleted(this, _sendArgs);
        }
    }

    // IO 완료처리
    private void OnIoCompleted(object sender, SocketAsyncEventArgs e)
    {
        switch (e.LastOperation)
        {
            case SocketAsyncOperation.Receive:
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    // 쓰기 완료 Pos 이동
                    _recvBuffer.OnWrite(e.BytesTransferred);
                    // 버퍼에 쌓인 패킷 처리
                    ProcessPacket();
                    // 다음 수신 등록
                    RegisterRecv();
                }
                else
                {
                    Close();
                }
                break;

            case SocketAsyncOperation.Send:
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    lock (_lock)
                    {
                        // 송신완료 버퍼 제거
                        _sendQueue.Dequeue();
                        // 콜백
                        OnSend?.Invoke(e.BytesTransferred);
                        if (_sendQueue.Count > 0)
                        {
                            // 송신 등록
                            RegisterSend();
                        }
                        else
                        {
                            // 송신 플래그 초기화
                            Interlocked.Exchange(ref _isSending, 0);
                        }
                    }
                }
                else
                {
                    Close();
                }
                break;
        }
    }

    // 패킷 처리
    private void ProcessPacket()
    {
        while (true)
        {
            // 헤더 사이즈만큼의 데이터가 있는지 확인
            if (PacketHeader.HeaderSize > _recvBuffer.DataSize)
            {
                break;
            }
            // 패킷 크기 확인
            ArraySegment<byte> buffer = _recvBuffer.GetReadSegment();
            int offset = buffer.Offset;
            // 패킷 ID, Size 추출
            ushort id = BitConverter.ToUInt16(buffer.Array, offset);
            offset += sizeof(ushort);

            ushort packetSize = BitConverter.ToUInt16(buffer.Array, offset);
            offset += sizeof(ushort);
            // 패킷 전체 데이터가 도착했는지 확인
            if (packetSize > _recvBuffer.DataSize)
            {
                break;
            }
            // 패킷 데이터 추출 및 처리
            byte[] packetData = new byte[packetSize];
            _recvBuffer.Read(packetData, 0, packetSize);

            // 패킷 처리 콜백 호출
            OnRecv?.Invoke(packetData, packetSize);
        }
    }


}