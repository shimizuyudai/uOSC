using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using UtilPack4Unity;
using uOSC;
using System.Linq;

public class OSCManager : MonoBehaviour
{
    [SerializeField]
    string settingFileName;
    public List<uOscClient> Clients
    {
        get;
        private set;
    }

    public List<uOscServer> Servers
    {
        get;
        private set;
    }

    public Action<uOscServer, Message> DataReceveEvent;

    private void Awake()
    {
        Init();   
    }

    [ContextMenu("TestExport")]
    void TestExport()
    {
        var client = new OSCClientSetting();
        client.Id = "Client";
        client.Port = 12000;
        client.Address = "127.0.0.1";

        var server = new OSCServerSetting();
        server.Id = "Server";
        server.Port = 12000;

        var setting = new OSCSetting();
        setting.clients = new OSCClientSetting[] { client};
        setting.servers = new OSCServerSetting[] { server };

        IOHandler.SaveJson(IOHandler.IntoStreamingAssets(settingFileName), setting);
    }

    public void OnDataReceived(uOscServer server, Message message)
    {
        DataReceveEvent?.Invoke(server, message);
    }

    private void Init()
    {
        Clients = new List<uOscClient>();
        Servers = new List<uOscServer>();
        var setting = IOHandler.LoadJson<OSCSetting>(IOHandler.IntoStreamingAssets(settingFileName));
        if (setting == null) return;
        if (setting.servers != null)
        {
            foreach (var serverSetting in setting.servers)
            {
                var go = new GameObject();
                go.transform.SetParent(this.transform, false);
                var component = go.AddComponent<uOscServer>();
                component.Play(serverSetting);
                component.DataReceveEvent += OnDataReceived;
                Servers.Add(component);
            }
        }

        if (setting.clients != null)
        {
            foreach (var clientSetting in setting.clients)
            {
                var go = new GameObject();
                go.transform.SetParent(this.transform, false);
                var component = go.AddComponent<uOscClient>();
                component.Play(clientSetting);
                Clients.Add(component);
            }
        }
    }

    public void Send(string id, Message message)
    {
        var client = Clients.FirstOrDefault(e => e.Setting.Id == id);
        if (client == null) return;
        client.Send(message);
    }

    public void Send(string id, string addressPattern, object[] values)
    {
        var client = Clients.FirstOrDefault(e => e.Setting.Id == id);
        if (client == null) return;
        client.Send(addressPattern, values);
    }


}

[Serializable]
public class OSCClientSetting
{
    public string Id;
    public string Address;
    public int Port;
}

[Serializable]
public class OSCServerSetting
{
    public string Id;
    public int Port;
}
public class OSCSetting
{
    public OSCClientSetting[] clients;
    public OSCServerSetting[] servers;
}
