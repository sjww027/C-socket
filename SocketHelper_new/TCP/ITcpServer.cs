/********************************************************************
 * *
 * * Copyright (C) 2013-? Corporation All rights reserved.
 * * 作者： BinGoo QQ：315567586 
 * * 请尊重作者劳动成果，请保留以上作者信息，禁止用于商业活动。
 * *
 * * 创建时间：2014-08-05
 * * 说明：ITcpServer服务端组件
 * *
********************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SocketHelper.Helper;
using SocketHelper.ICommond;
using SocketHelper.ITool;
using SocketHelper.IModels;
using SocketHelper.TClass;

namespace SocketHelper.TCP
{
    public partial class ITcpServer : Component, IServerBase
    {
        

        #region 变量

        // 监听Socket
        private Socket _serverSocket = null;
        // 心跳检测线程
        private Thread _tHeartCheck = null;
        //监听IP，为空时监听本机所有IP
        private string _serverIp = "";
        //监听端口
        private int _serverPort = 5000;
        //网络端点
        private IPEndPoint _ipEndPoint = null;
        //是否开启心跳检测
        private bool _isHeartCheck = true;
        //心跳检测间隔
        private int _checkTime = 3000;
        //是否已启动监听
        private bool _isStartListening = false;
        //客户端列表
        private List<IClient> _clientList;

        #endregion

        #region 属性

        [Description("无IP地址时默认监听本机所有IP")]
        [Category("TCP服务端")]
        public string ServerIp
        {
            get { return _serverIp; }
            set { _serverIp = value; }
        }

        [Description("本机监听端口")]
        [Category("TCP服务端")]
        public int ServerPort
        {
            get { return _serverPort; }
            set { _serverPort = value; }
        }

        [Description("网络端点,IP+PORT")]
        [Category("TCP服务端"), Browsable(false)]
        internal IPEndPoint IpEndPoint
        {
            get
            {
                try
                {
                    IPAddress ipAddress = null;
                    if (ServerIp == "")
                        ipAddress = IPAddress.Any;
                    else
                        ipAddress = IPAddress.Parse(CommonMethod.HostnameToIp(ServerIp));
                    _ipEndPoint = new IPEndPoint(ipAddress, ServerPort);
                }
                catch
                {
                }
                return _ipEndPoint;
            }
        }

        [Description("是否开启心跳检测")]
        [Category("TCP服务端")]
        public bool IsHeartCheck
        {
            get { return _isHeartCheck; }
            set { _isHeartCheck = value; }
        }

        [Description("心跳检测时间,单位：毫秒")]
        [Category("TCP服务端")]
        public int CheckTime
        {
            get { return _checkTime; }
            set { _checkTime = value; }
        }

        [Description("是否已启动监听")]
        [Category("TCP服务端"), Browsable(false)]
        public bool IsStartListening
        {
            get { return _isStartListening; }
            set { _isStartListening = value; }
        }

        [Description("客户端列表")]
        [Category("TCP服务端"), Browsable(false)]
        public List<IClient> ClientSocketList
        {
            get
            {
                if (_clientList == null)
                {
                    _clientList = new List<IClient>();
                }
                return _clientList;
            }
        }

        #endregion

        #region 构造函数

        public ITcpServer()
        {
            InitializeComponent();
        }

        public ITcpServer(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        #endregion

        #region 接口实现方法

        /// <summary>
        /// 开启监听
        /// </summary>
        public void Start()
        {
            if (IsStartListening)
                return;
            StartListen();
        }

        /// <summary>
        /// 启动服务器,没出现异常,则启动成功
        /// </summary>
        private void StartListen()
        {
            IsStartListening = true;
            try
            {
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Bind(IpEndPoint);
                _serverSocket.Listen(10000);
                TcpServerStateInfo(null,string.Format("服务端Ip:{0},端口:{1}已启动监听", ServerIp, ServerPort),
                    SocketState.StartListening);
                TcpServerGetLog(null, LogType.Server,
                    string.Format("服务端Ip:{0},端口:{1}已启动监听", string.IsNullOrEmpty(ServerIp) ? "本机所有IP" : ServerIp,
                        ServerPort));
                _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), _serverSocket);
                if (_tHeartCheck == null)
                {
                    _tHeartCheck = new Thread(new ThreadStart(HeartCheckThread));
                    _tHeartCheck.IsBackground = true;
                    _tHeartCheck.Start();
                }
                IsStartListening = true;
            }
            catch (Exception ex)
            {
                IsStartListening = false;
                TcpServerStateInfo(null,string.Format("服务端Ip:{0},端口:{1}启动监听失败", ServerIp, ServerPort),
                    SocketState.StartListeningError);
                TcpServerGetLog(null, LogType.Server, ex.Message);
            }
        }

        /// <summary>
        /// 停止监听，释放所有资源
        /// </summary>
        public void Stop()
        {
            try
            {

                IsStartListening = false;
                if (_serverSocket != null)
                {
                    _serverSocket.Close();
                }
                _serverSocket = null;

                if (_tHeartCheck != null)
                {
                    _tHeartCheck.Interrupt();
                    _tHeartCheck.Abort();
                    //_tHeartCheck.Join();
                }
                _tHeartCheck = null;
                CloseClient();
                ClientSocketList.Clear();
                TcpServerReturnClientCount(ClientSocketList.Count);
                TcpServerStateInfo(null,string.Format("服务端已停止监听"), SocketState.StopListening);
                TcpServerGetLog(null, LogType.Server, "服务端已停止监听");
            }
            catch (Exception ex)
            {
                TcpServerErrorMsg(string.Format("停止监听时发生错误，错误原因：{0}",ex.Message));
            }
        }

        #endregion

        #region 心跳检测方法模块
        /// <summary>
        /// 心跳检测
        /// </summary>
        private void HeartCheckThread()
        {
            try
            {
                while (IsStartListening)
                {
                    Thread.Sleep(CheckTime);
                    if (!IsHeartCheck)
                    {
                        //如果没有开启心跳检测或者没有启动监听则跳过检测
                        continue;
                    }
                    int i = 0;
                    while (i < ClientSocketList.Count)
                    {
                        Thread.Sleep(1);
                        if (!IsStartListening)
                        {
                            break;
                        }
                        if (ClientSocketList[i] == null)
                        {
                            TcpServerOfflineClient(ClientSocketList[i]);
                            ClientSocketList.RemoveAt(i);
                            TcpServerReturnClientCount(ClientSocketList.Count);
                            continue;
                        }
                       
                        byte[] sendHeartbeatData=new byte[0];
                        if (ClientSocketList[i].ClientInfo.HeartCheckType == HeartCheckType.EncodingString)
                        {
                            if (!string.IsNullOrEmpty(ClientSocketList[i].ClientInfo.Heartbeat))
                            {
                                sendHeartbeatData = Encoding.Default.GetBytes(ClientSocketList[i].ClientInfo.Heartbeat);
                            }
                            //switch (ClientSocketList[i].ClientInfo.Clienttype)
                            //{
                            //    case ClientType.None:
                            //        sendHeartbeatData=Encoding.Default.GetBytes("客户端\r\n");
                            //        break;
                            //    default:
                            //         sendHeartbeatData=Encoding.Default.GetBytes(ClientSocketList[i].ClientInfo.Heartbeat);
                            //        break;
                            //}
                        }
                        else if (ClientSocketList[i].ClientInfo.HeartCheckType == HeartCheckType.HexString)
                        {
                            sendHeartbeatData = ITool.DataToolManager.StringToHexByteArray(ClientSocketList[i].ClientInfo.Heartbeat);
                        }
                        else if (ClientSocketList[i].ClientInfo.HeartCheckType == HeartCheckType.Byte)
                        {
                            sendHeartbeatData = ClientSocketList[i].ClientInfo.HeartbeatByte;
                        }
                        if (sendHeartbeatData.Length>0)
                        {
                            try
                            {
                                if (ClientSocketList[i].ClientStyle == ClientStyle.WebSocket)
                                {
                                    SendToWebClient(ClientSocketList[i], Encoding.Default.GetString(sendHeartbeatData));
                                }
                                else
                                {
                                    SendData(ClientSocketList[i], sendHeartbeatData);
                                }
                                i++;
                            }
                            catch (Exception ex)
                            {
                                TcpServerErrorMsg(string.Format("心跳检测时发生错误，错误原因：{0}", ex.Message));
                                try
                                {
                                    if (ClientSocketList[i].WorkSocket != null)
                                    {
                                        TcpServerOfflineClient(ClientSocketList[i]);
                                        ClientSocketList.Remove(ClientSocketList[i]);
                                        TcpServerReturnClientCount(ClientSocketList.Count);
                                    }
                                    continue;
                                }
                                catch (Exception e)
                                {
                                    TcpServerErrorMsg(string.Format("心跳检测异常处理时发生错误，错误原因：{0}", e.Message));
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }
        /// <summary>
        /// 手动检测
        /// </summary>
        public void HandCheck()
        {
            int i = 0;
            while (i < ClientSocketList.Count)
            {
                Thread.Sleep(1);
                
                if (ClientSocketList[i] == null)
                {
                    TcpServerOfflineClient(ClientSocketList[i]);
                    ClientSocketList.RemoveAt(i);
                    TcpServerReturnClientCount(ClientSocketList.Count);
                    continue;
                }
                string sendLoginMsg = string.IsNullOrEmpty(ClientSocketList[i].ClientInfo.Heartbeat)?"客户端\r\n":ClientSocketList[i].ClientInfo.Heartbeat;
                try
                {
                    SendData(ClientSocketList[i], Encoding.Default.GetBytes(sendLoginMsg));
                    i++;
                }
                catch (Exception ex)
                {
                    TcpServerErrorMsg(string.Format("手动检测时发生错误，错误原因：{0}", ex.Message));
                    try
                    {
                        if (ClientSocketList[i].WorkSocket != null)
                        {
                            TcpServerOfflineClient(ClientSocketList[i]);
                            ClientSocketList.Remove(ClientSocketList[i]);
                            TcpServerReturnClientCount(ClientSocketList.Count);
                        }
                        continue;
                    }
                    catch (Exception e)
                    {
                        TcpServerErrorMsg(string.Format("手动检测异常处理时发生错误，错误原因：{0}", e.Message));
                    }
                }
            }
        }

        #endregion

        #region Socket相关方法

        /// <summary>
        /// 异步监听当有客户端连接后的回调函数
        /// </summary>
        /// <param name="iar"></param>
        private void AcceptCallback(IAsyncResult iar)
        {
            IClient iClient = null;
            try
            {
                if (_serverSocket != null)
                    _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), _serverSocket);
            }
            catch (Exception ex)
            {
                TcpServerErrorMsg(string.Format("监听客户端连接状态时发生错误，错误原因：{0}", ex.Message));
                Stop();
            }
            try
            {
                if (_serverSocket != null)
                {
                    Socket workSocket = _serverSocket.EndAccept(iar);
                    iClient = new IClient(workSocket);
                    ClientSocketList.Add(iClient);
                    TcpServerStateInfo(iClient, string.Format("<{0}：{1}>---上线", iClient.Ip, iClient.Port),
                        SocketState.ClientOnline);

                    TcpServerGetLog(iClient, LogType.Client, "客户端上线"); //记录
                    TcpServerOnlineClient(iClient);
                    TcpServerReturnClientCount(ClientSocketList.Count);
                    iClient.WorkSocket.BeginReceive(iClient.BufferInfo.ReceivedBuffer, 0,
                        iClient.BufferInfo.ReceivedBuffer.Length, 0,
                        new AsyncCallback(ClientReadCallback), iClient);
                    
                }
            }
            catch (Exception ex)
            {
                ShutdownClient(iClient);
                TcpServerOfflineClient(iClient);
                ClientSocketList.Remove(iClient);
                TcpServerReturnClientCount(ClientSocketList.Count);
                TcpServerErrorMsg(string.Format("监听客户端时发生错误，错误原因：{0}", ex.Message));
            }
        }

        /// <summary>
        /// 异步接收数据
        /// </summary>
        /// <param name="ar"></param>
        private void ClientReadCallback(IAsyncResult ar)
        {
            IClient iClient = (IClient) ar.AsyncState;
            try
            {
                int bytesRead = iClient.WorkSocket.EndReceive(ar);
                if (bytesRead > 0)
                {
                    //byte[] haveDate = ReceiveDateOne.DateOneManage(iClient, bytesRead);//接收完成之后对数组进行重置
                    byte[] bytes = new byte[bytesRead];
                    Array.Copy(iClient.BufferInfo.ReceivedBuffer, 0, bytes, 0, bytesRead);
                    string clientRecevieStr = "";
                    #region WebSocket验证
                    //验证websocket握手协议
                    if (iClient != null && iClient.ClientStyle != ClientStyle.WebSocket)
                    {
                        clientRecevieStr = ASCIIEncoding.Default.GetString(iClient.BufferInfo.ReceivedBuffer, 0, bytesRead);
                        //如果客户端不存在则退出检测
                        WebSocketHandShake(iClient, clientRecevieStr, bytes);
                    }
                    if (iClient.ClientStyle == ClientStyle.WebSocket)
                    {
                        string webstr = AnalyticData(bytes, bytes.Length);
                        clientRecevieStr = webstr;
                        bytes = Encoding.Default.GetBytes(clientRecevieStr);
                    }
                    #endregion
                    TcpServerRecevice(iClient,bytes);
                    TcpServerGetLog(iClient, LogType.ReceviedData, DataToolManager.HexByteArrayToString(bytes));
                    iClient.WorkSocket.BeginReceive(iClient.BufferInfo.ReceivedBuffer, 0,
                        iClient.BufferInfo.ReceivedBuffer.Length, 0,
                        new AsyncCallback(ClientReadCallback), iClient);
                }
                else if (bytesRead == 0)
                {
                    //接收数据长度为0时，标示客户单下线
                    if (iClient != null)
                    {
                        ShutdownClient(iClient);
                        TcpServerStateInfo(iClient,string.Format("<{0}：{1}>---下线", iClient.Ip, iClient.Port),
                            SocketState.ClientOnOff);
                        RemoveClient(iClient, "客户端下线");
                        TcpServerOfflineClient(iClient);
                        ClientSocketList.Remove(iClient);
                        TcpServerReturnClientCount(ClientSocketList.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                TcpServerErrorMsg(string.Format("接收客户端数据时发生错误，错误原因：{0}", ex.Message));
                int i = ex.Message.IndexOf("远程主机强迫关闭了一个现有的连接");
                if (iClient != null && i != -1)
                {
                    RemoveClient(iClient, ex.Message);
                }
                iClient = null;
            }
        }

        #endregion

        #region websocket模块

        public void WebSocketHandShake(IClient iClient, string msg, byte[] data)
        {
            #region 网络WebSocket协议握手
            string webkey = GetSecKey(data, data.Length);
            if (!string.IsNullOrEmpty(webkey) && iClient.ClientStyle != ClientStyle.WebSocket)
            {
                byte[] bufferwoshou = PackHandShakeData(webkey);
                if (bufferwoshou.Length > 0)
                {
                    iClient.ClientStyle = ClientStyle.WebSocket;
                    iClient.WorkSocket.Send(bufferwoshou);
                }
            }
            else
            {
            }

            #endregion
        }

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="recBytes"></param>
        /// <param name="recByteLength"></param>
        /// <returns></returns>
        public string AnalyticData(byte[] recBytes, int recByteLength)
        {
            //byte[] bytes = new byte[recByteLength];
            //Array.Copy(recBytes, 0, bytes, 0, recByteLength);
            //return Encoding.Default.GetString(bytes);

            if (recByteLength < 2) { return string.Empty; }

            bool fin = (recBytes[0] & 0x80) == 0x80; // 1bit，1表示最后一帧  
            if (!fin)
            {
                return string.Empty;// 超过一帧暂不处理 
            }

            bool mask_flag = (recBytes[1] & 0x80) == 0x80; // 是否包含掩码  
            if (!mask_flag)
            {
                return string.Empty;// 不包含掩码的暂不处理
            }

            int payload_len = recBytes[1] & 0x7F; // 数据长度  

            byte[] masks = new byte[4];
            byte[] payload_data;

            if (payload_len == 126)
            {
                Array.Copy(recBytes, 4, masks, 0, 4);
                payload_len = (UInt16)(recBytes[2] << 8 | recBytes[3]);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 8, payload_data, 0, payload_len);

            }
            else if (payload_len == 127)
            {
                Array.Copy(recBytes, 10, masks, 0, 4);
                byte[] uInt64Bytes = new byte[8];
                for (int i = 0; i < 8; i++)
                {
                    uInt64Bytes[i] = recBytes[9 - i];
                }
                UInt64 len = BitConverter.ToUInt64(uInt64Bytes, 0);

                payload_data = new byte[len];
                for (UInt64 i = 0; i < len; i++)
                {
                    payload_data[i] = recBytes[i + 14];
                }
            }
            else
            {
                Array.Copy(recBytes, 2, masks, 0, 4);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 6, payload_data, 0, payload_len);

            }

            for (var i = 0; i < payload_len; i++)
            {
                payload_data[i] = (byte)(payload_data[i] ^ masks[i % 4]);
            }

            return Encoding.UTF8.GetString(payload_data);
        }

        /// <summary>
        /// 向客户端发送信息
        /// </summary>
        /// <param name="iClient">客户端</param>
        /// <param name="sendData">发送的数据包</param>
        public void SendToWebClient(IClient iClient, string sendData)
        {
            if (iClient == null)
            {
                return;
            }
            if (!iClient.WorkSocket.Connected)
            {
                return;
            }
            try
            {
                DataFrame dr = new DataFrame(sendData);
                iClient.WorkSocket.Send(dr.GetBytes());

            }
            catch (Exception ex)
            {
                //logger.Log(ex.Message);
            }
        }

        /// <summary>
        /// 打包服务器数据
        /// </summary>
        /// <param name="message">数据</param>
        /// <returns>数据包</returns>
        private byte[] PackData(string message)
        {
            byte[] contentBytes = null;
            byte[] temp = Encoding.UTF8.GetBytes(message);

            if (temp.Length < 126)
            {
                contentBytes = new byte[temp.Length + 2];
                contentBytes[0] = 0x81;
                contentBytes[1] = (byte)temp.Length;
                Array.Copy(temp, 0, contentBytes, 2, temp.Length);
            }
            else if (temp.Length < 0xFFFF)
            {
                contentBytes = new byte[temp.Length + 4];
                contentBytes[0] = 0x81;
                contentBytes[1] = 126;
                contentBytes[2] = (byte)(temp.Length & 0xFF);
                contentBytes[3] = (byte)(temp.Length >> 8 & 0xFF);
                Array.Copy(temp, 0, contentBytes, 4, temp.Length);
            }
            else
            {
                // 暂不处理超长内容  
            }

            return contentBytes;
        }
        /// <summary>
        /// 打包握手信息
        /// </summary>
        /// <param name="secKeyAccept">Sec-WebSocket-Accept</param>
        /// <returns>数据包</returns>
        public byte[] PackHandShakeData(string secKeyAccept)
        {
            var responseBuilder = new StringBuilder();
            responseBuilder.Append("HTTP/1.1 101 Switching Protocols" + Environment.NewLine);
            responseBuilder.Append("Upgrade: websocket" + Environment.NewLine);
            responseBuilder.Append("Connection: Upgrade" + Environment.NewLine);
            responseBuilder.Append("Sec-WebSocket-Accept: " + secKeyAccept + Environment.NewLine + Environment.NewLine);
            //如果把上一行换成下面两行，才是thewebsocketprotocol-17协议，但居然握手不成功，目前仍没弄明白！
            //responseBuilder.Append("Sec-WebSocket-Accept: " + secKeyAccept + Environment.NewLine);
            //responseBuilder.Append("Sec-WebSocket-Protocol: chat" + Environment.NewLine);

            return Encoding.UTF8.GetBytes(responseBuilder.ToString());
        }

        /// <summary>
        /// 生成Sec-WebSocket-Accept
        /// </summary>
        /// <param name="handShakeText">客户端握手信息</param>
        /// <returns>Sec-WebSocket-Accept</returns>
        public string GetSecKeyAccetp(byte[] handShakeBytes, int bytesLength)
        {
            string handShakeText = Encoding.UTF8.GetString(handShakeBytes, 0, bytesLength);
            string key = string.Empty;
            Regex r = new Regex(@"Sec\-WebSocket\-Key:(.*?)\r\n");
            Match m = r.Match(handShakeText);
            if (m.Groups.Count != 0)
            {
                key = Regex.Replace(m.Value, @"Sec\-WebSocket\-Key:(.*?)\r\n", "$1").Trim();
            }
            if (key == "")
            {
                return handShakeText;
            }
            byte[] encryptionString = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")); //这字符串是固定的.不能改
            string str = Convert.ToBase64String(encryptionString);
            return str;
        }
        public string GetSecKey(byte[] handShakeBytes, int bytesLength)
        {
            string handShakeText = Encoding.UTF8.GetString(handShakeBytes, 0, bytesLength);
            string key = string.Empty;
            Regex r = new Regex(@"Sec\-WebSocket\-Key:(.*?)\r\n");
            Match m = r.Match(handShakeText);
            if (m.Groups.Count != 0)
            {
                key = Regex.Replace(m.Value, @"Sec\-WebSocket\-Key:(.*?)\r\n", "$1").Trim();
            }
            if (key == "")
            {
                return string.Empty;
            }
            byte[] encryptionString = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")); //这字符串是固定的.不能改
            string str = Convert.ToBase64String(encryptionString);
            return str;
        }
        #endregion

        #region 组件方法

        internal void CloseClient()
        {
            foreach (IClient iClient in ClientSocketList)
            {
                RemoveClient(iClient, "服务器主动关闭客户端");
            }
        }

        /// <summary>
        /// 关闭相连的scoket以及关联的TcpState,释放所有的资源
        /// </summary>
        /// <param name="iClient">TcpState</param>
        /// <param name="str">原因</param>
        internal void RemoveClient(IClient iClient, string str)
        {
            if (iClient == null)
                return;
            try
            {
                ShutdownClient(iClient);
                iClient.WorkSocket.Close();
                TcpServerOfflineClient(iClient);
                TcpServerGetLog(iClient, LogType.Client, str); //记录
            }
            catch (Exception ex)
            {
                TcpServerErrorMsg(string.Format("释放客户端资源时发生错误，错误原因：{0}", ex.Message));
            }
            
        }
        /// <summary>
        /// 强制断开与某个客户端的连接
        /// </summary>
        /// <param name="iClient"></param>
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

        #region 发送数据
        /// <summary>
        /// 向指定客户端发送字符串数据
        /// </summary>
        /// <param name="iClient"></param>
        /// <param name="msg"></param>
        /// <param name="isHexString"></param>
        public void SendData(IClient iClient, string msg,bool isHexString=false)
        {
            byte[] data;
            if (isHexString)
            {
                data = DataToolManager.StringToHexByteArray(msg);
            }
            else
            {
                data = Encoding.Default.GetBytes(msg);
            }
            SendData(iClient, data);
        }
        /// <summary>
        /// 向指定IP和端口客户端发送字符串数据
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="msg"></param>
        /// <param name="isHexString"></param>
        public void SendData(string ip,int port,string msg,bool isHexString=false)
        {
            IClient iClient=FindIClient(ip, port);
            if (iClient == null)
            {
                return;
            }
            byte[] data;
            if (isHexString)
            {
                data = DataToolManager.StringToHexByteArray(msg);
            }
            else
            {
                data = Encoding.Default.GetBytes(msg);
            }
            SendData(iClient, data);
        }
        /// <summary>
        /// 向指定IP和端口客户端发送数据
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="data"></param>
        public void SendData(string ip, int port, byte[] data)
        {
            IClient iClient = FindIClient(ip, port);
            if (iClient == null)
            {
                return;
            }
            SendData(iClient, data);
        }
        /// <summary>
        /// 向指定客户端发送数据(基础)
        /// </summary>
        /// <param name="iClient">客户端</param>
        /// <param name="data">字节数组数据</param>
        public void SendData(IClient iClient, byte[] data)
        {
            try
            {
                if (iClient != null)
                {
                    //异步发送数据
                    //cModel.ClientSocket.Send(data);
                    iClient.WorkSocket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), iClient);
                    iClient.BufferInfo.SendBuffer = data;
                    TcpServerGetLog(iClient, LogType.SendData, DataToolManager.HexByteArrayToString(data));
                }

            }
            catch (SocketException ex)
            {
                if (iClient == null)
                {
                    ClientSocketList.Remove(iClient);
                    TcpServerReturnClientCount(ClientSocketList.Count);
                    return;
                }
                ShutdownClient(iClient);
                TcpServerOfflineClient(iClient);
                TcpServerErrorMsg(string.Format("向客户端发送数据时发生错误，错误原因：{0}", ex.Message));
                TcpServerGetLog(iClient, LogType.SendData, ex.Message);
                ClientSocketList.Remove(iClient);
                TcpServerReturnClientCount(ClientSocketList.Count);
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
                TcpServerSendDateSuccess(iClient, bytesSent);
                TcpServerGetLog(iClient, LogType.SendDataResult, string.Format("发送成功，字节数：{0}", bytesSent));
            }
            catch (Exception ex)
            {
                TcpServerErrorMsg(string.Format("发送数据后回调时发生错误，错误原因：{0}", ex.Message));
                TcpServerGetLog(iClient, LogType.SendDataResult, string.Format("发送失败，失败原因：{0}", ex.Message));
            }
        }
        #endregion

        #region 查找客户端方法
        /// <summary>
        /// 根据IP,端口查找Socket客户端
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public IClient FindIClient(string ip, int port)
        {
            try
            {
                foreach (IClient iClient in ClientSocketList)
                {
                    if (iClient.Ip.Equals(ip)
                        && iClient.Port.Equals(port))
                    {
                        return iClient;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region 注册事件

        #region OnRecevice接收数据事件

        [Description("接收数据事件")]
        [Category("TcpServer事件")]
        public event EventHandler<TcpServerReceviceaEventArgs> OnRecevice;

        protected virtual void TcpServerRecevice(IClient iClient, byte[] data)
        {
            if (OnRecevice != null)
            {
                CommonMethod.EventInvoket(() => { OnRecevice(this, new TcpServerReceviceaEventArgs(iClient, data)); });
            }
        }

        #endregion

        #region OnErrorMsg返回错误消息事件

        [Description("错误消息")]
        [Category("TcpServer事件")]
        public event EventHandler<TcpServerErrorEventArgs> OnErrorMsg;

        protected virtual void TcpServerErrorMsg(string msg)
        {
            if (OnErrorMsg != null)
            {
                CommonMethod.EventInvoket(() => { OnErrorMsg(this, new TcpServerErrorEventArgs(msg)); });
            }
        }

        #endregion

        #region OnReturnClientCount用户上线下线时更新客户端在线数量事件

        [Description("用户上线下线时更新客户端在线数量事件")]
        [Category("TcpServer事件")]
        public event EventHandler<TcpServerReturnClientCountEventArgs> OnReturnClientCount;

        /// <summary>
        /// 用户上线下线时更新客户端在线数量事件
        /// </summary>
        /// <param name="count"></param>
        protected virtual void TcpServerReturnClientCount(int count)
        {
            if (OnReturnClientCount != null)
            {
                CommonMethod.EventInvoket(() => { OnReturnClientCount(this, new TcpServerReturnClientCountEventArgs(count)); });
            }
        }

        #endregion

        #region OnStateInfo监听状态改变时返回监听状态事件

        [Description("监听状态改变时返回监听状态事件")]
        [Category("TcpServer事件")]
        public event EventHandler<TcpServerStateEventArgs> OnStateInfo;

        protected virtual void TcpServerStateInfo(IClient iClient,string msg, SocketState state)
        {
            if (OnStateInfo != null)
            {
                CommonMethod.EventInvoket(() => { OnStateInfo(this, new TcpServerStateEventArgs(iClient, msg, state)); });
            }
        }

        #endregion

        #region OnAddClient新客户端上线时返回客户端事件

        [Description("新客户端上线时返回客户端事件")]
        [Category("TcpServer事件")]
        public event EventHandler<TcpServerClientEventArgs> OnOnlineClient;

        protected virtual void TcpServerOnlineClient(IClient iclient)
        {
            if (OnOnlineClient != null)
            {
                CommonMethod.EventInvoket(() => { OnOnlineClient(this, new TcpServerClientEventArgs(iclient)); });
            }
        }

        #endregion

        #region OnOfflineClient客户端下线时返回客户端事件

        [Description("客户端下线时返回客户端事件")]
        [Category("TcpServer事件")]
        public event EventHandler<TcpServerClientEventArgs> OnOfflineClient;

        protected virtual void TcpServerOfflineClient(IClient iclient)
        {
            if (OnOfflineClient != null)
            {
                CommonMethod.EventInvoket(() => { OnOfflineClient(this, new TcpServerClientEventArgs(iclient)); });
            }
        }

        #endregion

        #region OnGetLog服务端读写操作时返回日志消息

        [Description("服务端读写操作时返回日志消息")]
        [Category("TcpServer事件")]
        public event EventHandler<TcpServerLogEventArgs> OnGetLog;

        protected virtual void TcpServerGetLog(IClient temp, LogType logType, string logMsg)
        {
            if (OnGetLog != null)
            {
                CommonMethod.EventInvoket(() => { OnGetLog(this, new TcpServerLogEventArgs(temp, logType, logMsg)); });
            }
        }

        #endregion

        #region OnSendDateSuccess发送消息成功时返回成功消息事件

        [Description("发送消息成功时返回成功消息事件")]
        [Category("TcpServer事件")]
        public event EventHandler<TcpServerSendReturnEventArgs> OnSendDateSuccess;

        protected virtual void TcpServerSendDateSuccess(IClient temp, int byteLen)
        {
            if (OnSendDateSuccess != null)
            {
                CommonMethod.EventInvoket(() => { OnSendDateSuccess(this, new TcpServerSendReturnEventArgs(temp, byteLen)); });
            }
        }

        #endregion
        #endregion
    }
}
