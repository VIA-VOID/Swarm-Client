using Sirenix.OdinInspector;
using UnityEngine;

public class NetWorkManager : MonoBehaviour
{
    private TcpClientSocket _client;

    [LabelText("에코 서버 사용 여부")]
    [SerializeField] private bool isUseEchoServer;
    
    [LabelText("서버 전송 메세지")]
    [InlineButton("SendMessageToServer", SdfIconType.ChatText, "전송")]
    [SerializeField] private string sendText;

    private const string serverIP = "192.168.0.3";
    private const string echoIP = "127.0.0.1";

    void Start()
    {
        _client = new TcpClientSocket();

        string useIp = isUseEchoServer ? echoIP : serverIP;
        
        Debug.Log($"[NetWorkManager] 서버 연결 시도: {useIp}:7777");

        _client.Connect(useIp, 7777); // 서버 주소/포트
    }

    void SendMessageToServer()
    {
        if (_client != null)
        {
            if (!string.IsNullOrEmpty(sendText))
            {
                _client.Send(sendText);
                Debug.Log($"[NetWorkManager] 서버로 전송: {sendText}");
            }
            else
            {
                Debug.LogWarning("[NetWorkManager] 전송할 메시지가 비어 있음!");
            }
        }
        else
        {
            Debug.LogWarning("[NetWorkManager] 클라이언트 연결되지 않음!");
        }
    }
    
    void OnApplicationQuit()
    {
        _client.Disconnect();
    }
}
