using UnityEngine;
using UnityEngine.Events;
using System;

namespace uOSC
{

    public class uOscServer : MonoBehaviour
    {
        //[SerializeField]
        //int port = 3333;

#if NETFX_CORE
    Udp udp_ = new Uwp.Udp();
    Thread thread_ = new Uwp.Thread();
#else
        Udp udp_ = new DotNet.Udp();
        Thread thread_ = new DotNet.Thread();
#endif
        Parser parser_ = new Parser();

        //public class DataReceiveEvent : UnityEvent<Message> { };
        //public DataReceiveEvent onDataReceived { get; private set; }
        public Action<uOscServer, Message> DataReceveEvent;

        [SerializeField]
        private bool standalone;
        [SerializeField]
        private OSCServerSetting setting;
        public OSCServerSetting Setting
        {
            get {
                return setting;
            }
            private set {
                setting = value;
            }
        }

        void Awake()
        {
            //onDataReceived = new DataReceiveEvent();
        }

        void OnEnable()
        {
            if (!standalone) return;
            Play(Setting);
        }

        void OnDisable()
        {
            if (!standalone) return;
            Stop();
        }

        public void Play(OSCServerSetting setting)
        {
            this.Setting = setting;
            udp_.StartServer(setting.Port);
            thread_.Start(UpdateMessage);
        }

        public void Stop()
        {
            thread_.Stop();
            udp_.Stop();
        }

        void Update()
        {
            while (parser_.messageCount > 0)
            {
                var message = parser_.Dequeue();
                //onDataReceived.Invoke(message);
                DataReceveEvent?.Invoke(this, message);
            }
        }

        void UpdateMessage()
        {
            while (udp_.messageCount > 0)
            {
                var buf = udp_.Receive();
                int pos = 0;
                parser_.Parse(buf, ref pos, buf.Length);
            }
        }
    }

}