using ET;
using System;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    public Button button;
    public string address = "127.0.0.1";
    public const int port = 10002;
    TextMeshProUGUI text;
    NetKcpComponent NetKcpComponent;
    Session session;

    private void Awake()
    {
        text = button.GetComponentInChildren<TextMeshProUGUI>();
        NetKcpComponent = GetComponent<NetKcpComponent>();
    }
    private void Start()
    {
        button.onClick.AddListener(OnButtonClick);
        text.text = "Connect";
    }

    bool isConnected => session != null && !session.IsDisposed;
    private async void OnButtonClick()
    {
        if (isConnected)
        {
            text.text = "Connect";
            session.Dispose();
            session = null;
        }
        else
        {
            var host = $"{address}:{port}";
            var result = await LoginAsync(host);
            text.text = result ? "Connected" : "Try again";
        }
    }

    public async ETTask<bool> LoginAsync(string address)
    {
        bool isconnected = true;
        try
        {
            // 创建一个ETModel层的Session
            R2C_Login r2CLogin;
            Session forgate = null;
            forgate = NetKcpComponent.Create(NetworkHelper.ToIPEndPoint(address));
            r2CLogin = (R2C_Login)await forgate.Call(new C2R_Login());
            forgate?.Dispose();
            // 创建一个gate Session,并且保存到SessionComponent中
            session = NetKcpComponent.Create(NetworkHelper.ToIPEndPoint(r2CLogin.Address));
            session.ping = new ET.Ping(session);
            G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await session.Call(new C2G_LoginGate() { Key = r2CLogin.Key, GateId = r2CLogin.GateId });
            Debug.Log("登陆gate成功!");

            // 登录 map 服务器
            // 进入地图
            var request = new C2G_EnterMap() ;
            G2C_EnterMap map = await session.Call(request) as G2C_EnterMap;
            Debug.Log($"进入地图成功：  Net_id = {map.MyId}");
        }
        catch (Exception e)
        {
            isconnected = false;
            Debug.LogError($"登陆失败 - {e}");
        }
        return isconnected;
    }
}
