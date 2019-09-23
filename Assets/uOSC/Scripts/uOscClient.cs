using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace uOSC
{

    public class uOscClient : MonoBehaviour
    {
        private const int BufferSize = 8192;
        private const int MaxQueueSize = 100;

        //[SerializeField]
        //string address = "127.0.0.1";

        //[SerializeField]
        //int port = 3333;

        //[SerializeField]
        //private string id;

        [SerializeField]
        private bool standalone;
        [SerializeField]
        OSCClientSetting setting;
        public OSCClientSetting Setting
        {
            get {
                return setting;
            }
            private set {
                this.setting = value;
            }
        }


#if NETFX_CORE
    Udp udp_ = new Uwp.Udp();
    Thread thread_ = new Uwp.Thread();
#else
        Udp udp_ = new DotNet.Udp();
        Thread thread_ = new DotNet.Thread();
#endif
        Queue<object> messages_ = new Queue<object>();
        object lockObject_ = new object();

        void OnEnable()
        {
            if (!standalone) return;
            Play(Setting);
        }

        void OnDisable()
        {
            Stop();
        }

        public void Play(OSCClientSetting setting)
        {
            this.Setting = setting;
            udp_.StartClient(setting.Address, setting.Port);
            thread_.Start(UpdateSend);
        }

        public void Stop()
        {
            thread_.Stop();
            udp_.Stop();
        }

        void UpdateSend()
        {
            while (messages_.Count > 0)
            {
                object message;
                lock (lockObject_)
                {
                    message = messages_.Dequeue();
                }

                using (var stream = new MemoryStream(BufferSize))
                {
                    if (message is Message)
                    {
                        ((Message)message).Write(stream);
                    }
                    else if (message is Bundle)
                    {
                        ((Bundle)message).Write(stream);
                    }
                    else
                    {
                        return;
                    }
                    udp_.Send(Util.GetBuffer(stream), (int)stream.Position);
                }
            }
        }

        void Add(object data)
        {
            lock (lockObject_)
            {
                messages_.Enqueue(data);

                while (messages_.Count > MaxQueueSize)
                {
                    messages_.Dequeue();
                }
            }
        }

        public void Send(string address, params object[] values)
        {
            Send(new Message()
            {
                address = address,
                values = values
            });
        }

        public void Send(Message message)
        {
            Add(message);
        }

        public void Send(Bundle bundle)
        {
            Add(bundle);
        }
    }

}