using UnityEngine;

/*-------------------------------------------------------
				GameNetwork

- 모든 네트워크 관련 클래스에서 상속받아서 구현
- 자식클래스에서 구현만 해두면 메인스레드에서 실행
--------------------------------------------------------*/

public abstract class GameNetwork : MonoBehaviour
{
    protected virtual void Awake()
    {
        // 콜백 등록
        NetworkManager.Instance.OnConnectCallback += OnConnected;
        NetworkManager.Instance.OnDisconnectCallback += OnDisconnected;
        NetworkManager.Instance.OnRecvCallback += OnRecv;
        NetworkManager.Instance.OnSendCallback += OnSend;
    }

    protected virtual void OnDestroy()
    {
        // 콜백 제거
        NetworkManager.Instance.OnConnectCallback -= OnConnected;
        NetworkManager.Instance.OnDisconnectCallback -= OnDisconnected;
        NetworkManager.Instance.OnRecvCallback -= OnRecv;
        NetworkManager.Instance.OnSendCallback -= OnSend;
    }

    // 상속받아서 내용 추가
    protected abstract void OnConnected();
    protected abstract void OnDisconnected();
    protected abstract void OnRecv(byte[] buffer, int size);
    protected abstract void OnSend(int size);
}
