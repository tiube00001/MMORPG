using MMORPG.Common.Network;
using MMORPG.Common.Tool;
using Google.Protobuf;
using QFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MMORPG.Game;
using MMORPG.Tool;
using PimDeWitte.UnityMainThreadDispatcher;
using Serilog;
using UnityEngine;

namespace MMORPG.System
{
    public interface INetworkSystem : ISystem
    {
        public delegate void ReceivedEventHandler<in TMessage>(TMessage response) where TMessage : class, IMessage;

        public Task ConnectAsync();
        public void Close();

        public void SendToServer(IMessage msg);
        public Task<T> ReceiveAsync<T>() where T : class, IMessage;
        public IUnRegister Receive<TMessage>(ReceivedEventHandler<TMessage> onReceived, bool inUnityThread = true) where TMessage : class, IMessage;
        public Task StartAsync();
    }
    public class NetConfig
    {
        public int port;
        public string ip;
    }
    public class NetworkSystem : AbstractSystem, INetworkSystem
    {
        private NetSession _session;
        private Dictionary<Type, Delegate> _messageHandlers = new();

        ////TODO 高水位处理
        private LinkedList<IMessage> _messageList = new();
        private static readonly int MaxMessageCount = 1024;

        public async Task<T> ReceiveAsync<T>() where T : class, IMessage
        {
            while (true)
            {
                IMessage msg = null;
                lock (_messageList)
                {
                    var node = _messageList.FindIf(msg => { return msg.GetType() == typeof(T); });
                    if (node != null)
                    {
                        msg = node.Value;
                        //UnityEngine.Debug.Log(typeof(T));
                        _messageList.Remove(node);
                    }
                }
                if (msg == null)
                {
                    await Task.Delay(100);
                    continue;
                }
                var res = msg as T;
                Debug.Assert(res != null);
                return res;
            }
        }

        public IUnRegister Receive<TMessage>(INetworkSystem.ReceivedEventHandler<TMessage> onReceived, bool inUnityThread = true) where TMessage : class, IMessage
        {
            var received = onReceived;
            if (inUnityThread)
            {
                received = msg =>
                    UnityMainThreadDispatcher.Instance().Enqueue(() => onReceived(msg));
            }
            var type = typeof(TMessage);
            _messageHandlers.TryAdd(type, null);
            _messageHandlers[type] = (_messageHandlers[type] as INetworkSystem.ReceivedEventHandler<TMessage>) + received;
            return new CustomUnRegister(() => UnReceive(received));
        }

        public void SendToServer(IMessage msg)
        {
            _session.Send(msg);
        }

        public Task StartAsync()
        {
            _session.PacketReceived += OnPacketReceived;
            return _session.StartAsync();
        }

        private void OnPacketReceived(object sender, PacketReceivedEventArgs e)
        {
            var msgType = e.Packet.Message.GetType();

            if (_messageHandlers.TryGetValue(msgType, out var handlers))
            {
                handlers?.DynamicInvoke(new object[] { e.Packet.Message });
            }
            else
            {
                lock (_messageList)
                {
                    if (_messageList.Count >= MaxMessageCount)
                    {
                        throw new Exception("数据队列中有过多数据未处理");
                    }

                    _messageList.AddLast(e.Packet.Message);
                }
            }
        }

        public async Task ConnectAsync()
        {
            Socket socket;
            var box = this.GetSystem<IBoxSystem>();
            while (true)
            {
                // 显示旋转加载框
                box.ShowSpinner("连接服务器中......");
                try
                {
                    string filePath = Path.Combine(Application.streamingAssetsPath, "NetConfig.txt");
                    string fileContent  = File.ReadAllText(filePath);
                    NetConfig netConfig = JsonUtility.FromJson<NetConfig>(fileContent);
                    IPAddress ServerIpAddress = IPAddress.Parse(netConfig.ip);
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    await socket.ConnectAsync(ServerIpAddress, netConfig.port);
                    box.CloseSpinner();
                    break;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"连接服务器时出现错误:{ex.Message}");
                    box.CloseSpinner();
                    await box.ShowMessageAsync("错误", $"连接服务器失败:{ex}", "重新连接");
                    continue;
                }
            }
            _session = new NetSession(socket);
        }

        protected override void OnDeinit()
        {
            Close();
            _messageHandlers.Clear();
            lock (_messageList)
            {
                _messageList.Clear();
            }
            _session = null;
        }

        protected override void OnInit()
        {
        }

        private void UnReceive<TMessage>(INetworkSystem.ReceivedEventHandler<TMessage> onReceived) where TMessage : class, IMessage
        {
            var type = typeof(TMessage);
            if (_messageHandlers.ContainsKey(type))
            {
                _messageHandlers[type] = (_messageHandlers[type] as INetworkSystem.ReceivedEventHandler<TMessage>) - onReceived;
            }
        }

        public void Close()
        {
            _session.Close();
        }
    }
}
