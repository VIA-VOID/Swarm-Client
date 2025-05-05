using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

/*-------------------------------------------------------
				NetworkManager

- 서버 연결 및 패킷 송수신 담당
--------------------------------------------------------*/

public class NetworkManager : MonoBehaviour
{
    #region Singleton
    private static NetworkManager _instance;
    public static NetworkManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("@NetworkManager");
                _instance = go.AddComponent<NetworkManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    #endregion

    private Session _session;
    private PacketManager _packetManager = new PacketManager();
    // 연결 상태 확인
    public bool IsConnected => _session != null && _session.IsConnected();
    // 외부에서 콜백 확장
    public event Action OnConnectCallback;
    public event Action OnDisconnectCallback;
    public event Action<byte[], int> OnRecvCallback;
    public event Action<int> OnSendCallback;

    // 초기화
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 패킷 핸들러 등록
        PacketHandler.RegisterPacketHandlers(_packetManager);
    }

    // 서버에 연결
    public void Connect(string host, int port)
    {
        // 이미 연결된 경우 먼저 종료
        if (IsConnected)
        {
            Disconnect();
        }

        try
        {
            // 세션 생성
            _session = new Session();

            // 외부 확장함수 호출
            _session.OnConnected = () => MainThreadDispatcher.Instance.Enqueue(() =>
            {
                OnConnectCallback?.Invoke();
            });

            _session.OnDisconnected = () => MainThreadDispatcher.Instance.Enqueue(() =>
            {
                OnDisconnectCallback?.Invoke();
            });

            _session.OnRecv = (buffer, size) => MainThreadDispatcher.Instance.Enqueue(() =>
            {
                OnRecvCallback?.Invoke(buffer, size);
                // 수신한 패킷 처리
                _packetManager.OnRecvPacket(new ArraySegment<byte>(buffer, 0, size));
            });

            _session.OnSend = (bytesSent) => MainThreadDispatcher.Instance.Enqueue(() =>
            {
                OnSendCallback?.Invoke(bytesSent);
            });

            // IP 주소 변환
            IPAddress ipAddr = IPAddress.Parse(host);
            IPEndPoint endPoint = new IPEndPoint(ipAddr, port);

            // 연결 시도
            _session.Connect(endPoint);
            Debug.Log($"서버 연결 시도: {host}:{port}");
        }
        catch (Exception e)
        {
            Debug.LogError($"서버 연결 오류: {e.Message}");
        }
    }

    // 패킷 전송
    public void Send<T>(T packet) where T : Google.Protobuf.IMessage
    {
        if (IsConnected == false)
        {
            Debug.LogWarning("서버에 연결되어 있지 않습니다.");
            return;
        }

        try
        {
            // 패킷 직렬화
            ArraySegment<byte> sendBuffer = _packetManager.MakeSendBuffer(packet);
            // 패킷 전송
            _session.Send(sendBuffer.Array, sendBuffer.Offset, sendBuffer.Count);
        }
        catch (Exception e)
        {
            Debug.LogError($"패킷 전송 오류: {e.Message}");
            Disconnect();
        }
    }

    // 서버 연결 종료
    public void Disconnect()
    {
        if (_session != null)
        {
            _session.Close();
            _session = null;
        }
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}


/*-------------------------------------------------------
				NetworkManager

- 비동기 콜백을 메인 스레드에서 실행
--------------------------------------------------------*/
public class MainThreadDispatcher : MonoBehaviour
{
    #region 싱글톤
    private static MainThreadDispatcher _instance;
    public static MainThreadDispatcher Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("@MainThreadDispatcher");
                _instance = go.AddComponent<MainThreadDispatcher>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    #endregion

    // 실행할 작업 큐
    private Queue<Action> _actionQueue = new Queue<Action>();
    private object _lock = new object();

    // 작업 등록
    public void Enqueue(Action action)
    {
        lock (_lock)
        {
            _actionQueue.Enqueue(action);
        }
    }

    // 매 프레임마다 작업 처리
    void Update()
    {
        // 현재 프레임에서 처리할 작업 목록 가져오기
        Action[] actions = null;
        lock (_lock)
        {
            if (_actionQueue.Count > 0)
            {
                actions = new Action[_actionQueue.Count];
                _actionQueue.CopyTo(actions, 0);
                _actionQueue.Clear();
            }
        }

        // 작업 실행
        if (actions != null)
        {
            foreach (var action in actions)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"메인 스레드 작업 실행 오류: {e.Message}\n{e.StackTrace}");
                }
            }
        }
    }
}
