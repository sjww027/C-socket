/********************************************************************
 * *
 * * Copyright (C) 2013-? Corporation All rights reserved.
 * * 作者： BinGoo QQ：315567586 
 * * 请尊重作者劳动成果，请保留以上作者信息，禁止用于商业活动。
 * *
 * * 创建时间：2014-08-05
 * * 说明：ITcpClient客户端组件
 * *
********************************************************************/
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SocketHelper.ICommond;
using SocketHelper.IModels;
using SocketHelper.ITool;

namespace SocketHelper.TCP
{
    public partial class ITcpClient : Component
    {
        #region 构造函数
        public ITcpClient()
        {
            InitializeComponent();
        }

        public ITcpClient(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        } 
        #endregion

        #region 变量

        public IClient Client = null;
        private Thread _startThread = null;
        private int _reConnectCount= 0;//重连计数
        private ConncetType _conncetType = ConncetType.Conncet;
        private bool _isReconnect = true;//是否开启断开重连
        private int _reConnectTime = 3000;//重连间隔时间
        private bool _isStart = false;// 是否启动
        #endregion

        #region 属性
        /// <summary>
        /// 服务端IP
        /// </summary>
        private string _serverip="127.0.0.1";
        [Description("服务端IP")]
        [Category("TCP客户端")]
        public string ServerIp
        {
            set { _serverip = value; }
            get { return _serverip; }
        }
        /// <summary>
        /// 服务端监听端口
        /// </summary>
        private int _serverport=5000;
        [Description("服务端监听端口")]
        [Category("TCP客户端")]
        public int ServerPort
        {
            set { _serverport = value; }
            get { return _serverport; }
        }

        /// <summary>
        /// 网络端点
        /// </summary>
        private IPEndPoint _ipEndPoint = null;
        [Description("网络端点,IP+PORT")]
        [Category("TCP客户端")]
        internal IPEndPoint IpEndPoint
        {
            get
            {
                try
                {
                    IPAddress ipAddress = null;
                    ipAddress = string.IsNullOrEmpty(ServerIp)
                        ? IPAddress.Any
                        : IPAddress.Parse(CommonMethod.HostnameToIp(ServerIp));

                    _ipEndPoint = new IPEndPoint(ipAddress, ServerPort);
                }
                catch
                {
                }
                return _ipEndPoint;
            }
        }
       
        /// <summary>
        /// 是否重连
        /// </summary>
        [Description("是否重连")]
        [Category("TCP客户端")]
        public bool IsReconnection
        {
            set { _isReconnect = value; }
            get { return _isReconnect; }
        }

        /// <summary>
        /// 设置断开重连时间间隔单位（毫秒）（默认3000毫秒）
        /// </summary>
        [Description("设置断开重连时间间隔单位（毫秒）（默认3000毫秒）")]
        [Category("TCP客户端")]
        public int ReConnectionTime
        {
            get { return _reConnectTime; }
            set { _reConnectTime = value; }
        }
        [Description("设置断开重连时间间隔单位（毫秒）（默认3000毫秒）")]
        [Category("TCP客户端"), Browsable(false)]
        public bool IsStart
        {
            get { return _isStart; }
            set { _isStart = value; }
        }
        #endregion

        #region 启动停止方法

        public void StartConnect()
        {
            if (IsStart)
                return;
            _startThread = new Thread(StartThread);
            _startThread.IsBackground = true;
            _startThread.Start();
        }

        /// <summary>
        /// 启动客户端基础的一个线程
        /// </summary>
        private void StartThread()
        {
            if (_conncetType == ConncetType.ReConncet && IsReconnection) //如果是重连的延迟N秒
            {
                Thread.Sleep(ReConnectionTime);
                TcpClientStateInfo(string.Format("正在第{0}次重连", _reConnectCount), SocketState.Reconnection);
            }
            else
            {
                TcpClientStateInfo("正在连接服务器... ...", SocketState.Connecting);
            }
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.SendTimeout = 1000;
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
                socket.BeginConnect(IpEndPoint, new AsyncCallback(AcceptCallback), socket);
            }
            catch (Exception ex)
            {
                TcpClientErrorMsg(string.Format("连接服务器失败，错误原因：{0}", ex.Message));
                Reconnect();
            }
        }
        /// <summary>
        /// 当连接服务器之后的回调函数
        /// </summary>
        /// <param name="ar">TcpClient</param>
        private void AcceptCallback(IAsyncResult ar)
        {
            
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                socket.EndConnect(ar);

                Client = new IClient(socket);
                Client.WorkSocket.BeginReceive(Client.BufferInfo.ReceivedBuffer, 0, Client.BufferInfo.ReceivedBuffer.Length, 0, new AsyncCallback(ReadCallback), Client);
                _conncetType = ConncetType.Conncet;
                TcpClientStateInfo(string.Format("已连接服务器"), SocketState.Connected);
                _reConnectCount = 0;
            }
            catch (SocketException ex)
            {
                string msg = ex.Message;
                if (ex.NativeErrorCode.Equals(10060))
                {
                    //无法连接目标主机
                    msg = string.Format("{0} 无法连接: error code {1}!", "", ex.NativeErrorCode);
                }
                else if (ex.NativeErrorCode.Equals(10061))
                {
                    msg = string.Format("{0} 主动拒绝正在重连: error code {1}!", "", ex.NativeErrorCode);
                }
                else if (ex.NativeErrorCode.Equals(10053))
                {
                    //读写时主机断开
                    msg = string.Format("{0} 主动断开连接: error code {1}! ", "", ex.NativeErrorCode);
                }
                else
                {
                    //其他错误
                    msg = string.Format("Disconnected: error code {0}!", ex.NativeErrorCode);
                }
                
                TcpClientErrorMsg(string.Format("连接服务器失败，错误原因：{0}", msg));
                Reconnect();
            }
        }
        #endregion

        #region  登录篇
        /// <summary>
        /// 重连模块
        /// </summary>
        private void Reconnect()
        {
            if (_conncetType == ConncetType.Conncet)
            {
                TcpClientStateInfo(string.Format("已断开服务器{0}", IsReconnection ? "，准备重连" : ""), SocketState.Disconnect);
            }
            if (!IsReconnection)
            {
                return;
            }
            _reConnectCount++;//每重连一次重连的次数加1
            if (Client != null)
            {
                Client.WorkSocket.Close();
                Client = null;
            }
            if (_conncetType == ConncetType.Conncet)
            {
                _conncetType = ConncetType.ReConncet;

                //CommonMethod.EventInvoket(() => { ReconnectionStart(); });
            }
            _isStart = false;
            StartConnect();
            
        }
        #endregion

        #region 发送数据
        public void SendData(byte[] data)
        {
            try
            {
                if (Client.WorkSocket != null)
                {
                    //异步发送数据
                    //cModel.ClientSocket.Send(data);
                    Client.WorkSocket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), Client);
                }

            }
            catch (SocketException ex)
            {
                TcpClientErrorMsg(string.Format("向服务端户端发送数据时发生错误，错误原因：{0}", ex.Message));
            }
        }
        /// <summary>
        /// 发送完数据之后的回调函数
        /// </summary>
        /// <param name="ar">Clicent</param>
        private void SendCallback(IAsyncResult ar)
        {
            IClient iClient = (IClient)ar.AsyncState;
            if (iClient == null)
                return;
            Socket handler = iClient.WorkSocket;
            try
            {
                int bytesSent = handler.EndSend(ar);
            }
            catch (Exception ex)
            {
                TcpClientErrorMsg(string.Format("发送数据后回调时发生错误，错误原因：{0}", ex.Message));
            }
        }
        #endregion

        #region 接收数据
        /// <summary>
        /// 当接收到数据之后的回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ReadCallback(IAsyncResult ar)
        {
            if (Client == null)
                return;
            Socket handler = Client.WorkSocket;
            try
            {
                int bytesRead = handler.EndReceive(ar);
                if (bytesRead > 0)
                {
                    byte[] bytes = new byte[bytesRead];
                    Array.Copy(Client.BufferInfo.ReceivedBuffer, 0, bytes, 0, bytesRead);
                    TcpClientRecevice(bytes);
                    handler.BeginReceive(Client.BufferInfo.ReceivedBuffer, 0, Client.BufferInfo.ReceivedBuffer.Length,
                        0, new AsyncCallback(ReadCallback), Client);
                   
                }
                else
                {
                    Reconnect();
                }
            }
            catch (Exception ex)
            {
                TcpClientErrorMsg(string.Format("接收数据失败，错误原因：{0}", ex.Message));
                Reconnect();
            }
        }
        #endregion

        #region  断开篇
        /// <summary>
        /// 关闭相连的scoket以及关联的StateObject,释放所有的资源
        /// </summary>
        public void StopConnect()
        {
            IsReconnection = false;
            if (Client != null)
            {
                ShutdownClient(Client);
                Client.WorkSocket.Close();
            }
            _conncetType = ConncetType.Conncet;
            _reConnectCount= 0;//前面三个初始化
        }
        public void ShutdownClient(IClient iClient)
        {
            try
            {
                iClient.WorkSocket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }
        }
        #endregion

        #region 事件
        #region OnRecevice接收数据事件
        public delegate void Tcp(string msg, byte[] data);
        [Description("接收数据事件")]
        [Category("TcpClient事件")]
        public event EventHandler<TcpClientReceviceEventArgs> OnRecevice;
        protected virtual void TcpClientRecevice(byte[] data)
        {
            if (OnRecevice != null)
                CommonMethod.EventInvoket(() => { OnRecevice(this,new TcpClientReceviceEventArgs(data)); }); 
                
        }
        #endregion

        #region OnErrorMsg返回错误消息事件
        [Description("返回错误消息事件")]
        [Category("TcpClient事件")]
        public event EventHandler<TcpClientErrorEventArgs> OnErrorMsg;
        protected virtual void TcpClientErrorMsg(string msg)
        {
            if (OnErrorMsg != null)
                CommonMethod.EventInvoket(() => { OnErrorMsg(this,new TcpClientErrorEventArgs(msg)); }); 
        }
        #endregion

        #region OnStateInfo连接状态改变时返回连接状态事件
        [Description("连接状态改变时返回连接状态事件")]
        [Category("TcpClient事件")]
        public event EventHandler<TcpClientStateEventArgs> OnStateInfo;
        protected virtual void TcpClientStateInfo(string msg, SocketState state)
        {
            if (OnStateInfo != null)
                CommonMethod.EventInvoket(() => { OnStateInfo(this, new TcpClientStateEventArgs(msg, state)); });
        }
        #endregion
        #endregion
    }

    public enum ConncetType
    {
        Conncet,
        ReConncet,
        DisConncet
    }
}
