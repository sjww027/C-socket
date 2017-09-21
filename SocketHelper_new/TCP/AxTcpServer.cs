using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SocketHelper.Helper;
using SocketHelper.ICommond;
using SocketHelper.ITool;
using SocketHelper.Models;

namespace SocketHelper
{
    public partial class AxTcpServer : Component
    {

        #region 构造函数
        public AxTcpServer()
        {
            InitializeComponent();
            ClientSocketList = new List<ClientModel>();
            ClientSocketList.Clear();
        }

        public AxTcpServer(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            #region 初始化委托方法
            #endregion
            ClientSocketList = new List<ClientModel>();
            ClientSocketList.Clear();

        }
        #endregion

        #region 变量属性
        /// <summary>
        /// 信号量
        /// </summary>
        private Semaphore semap = new Semaphore(5, 5000);
        /// <summary>
        /// 监听Socket
        /// </summary>
        public Socket ServerSocket;
        /// <summary>
        /// 监听线程
        /// </summary>
        public Thread StartSockst;
        /// <summary>
        /// 心跳检测线程
        /// </summary>
        public Thread HeadCheck;
        /// <summary>
        /// 本机监听IP,默认是本地ip
        /// </summary>
        private string _serverIp = "127.0.0.1";
        [Description("本机监听IP,默认是本地IP"), Browsable(false)]
        [Category("TCP服务端")]
        public string ServerIp
        {
            get { return _serverIp; }
            set { _serverIp = value; }
        }
        //private string _heartbeatPacket = "X";
        ///// <summary>
        ///// 心跳包检测字符串
        ///// </summary>
        //[Description("心跳包检测字符串")]
        //[Category("TCP服务端")]
        //public string HeartbeatPacket
        //{
        //    get { return _heartbeatPacket; }
        //    set { _heartbeatPacket = value; }
        //}

        private bool _isheartCheck = false;
        /// <summary>
        /// 是否开启心跳检测
        /// </summary>
        [Description("是否开启心跳检测")]
        [Category("TCP服务端")]
        public bool IsheartCheck
        {
            get { return _isheartCheck; }
            set { _isheartCheck = value; }
        }

        private int _checkTime = 3000;
        /// <summary>
        /// 心跳检测时间,单位：毫秒
        /// </summary>
        [Description("心跳检测时间,单位：毫秒")]
        [Category("TCP服务端")]
        public int CheckTime
        {
            get { return _checkTime; }
            set { _checkTime = value; }
        }
        /// <summary>
        /// 监听端口
        /// </summary>
        private int _serverPort = 5000;

        [Description("本机监听端口,默认是5000")]
        [Category("TCP服务端")]
        public int ServerPort
        {
            get { return _serverPort; }
            set { _serverPort = value; }
        }
        /// <summary>
        /// 是否已启动监听
        /// </summary>
        public bool IsStartListening = false;
        /// <summary>
        /// 客户端列表
        /// </summary>
        //public List<Socket> ClientSocketList = new List<Socket>();
        public List<ClientModel> ClientSocketList = new List<ClientModel>();

        public bool XinTiao = true;

        #endregion

        #region 方法
        /// <summary>
        /// 心跳检测线程
        /// </summary>
        public void HeadCheckThread()
        {
            try
            {
                while (IsStartListening)
                {
                    Thread.Sleep(CheckTime);
                    if (!_isheartCheck)
                    {
                        continue;
                    }
                    //ForeachCheack();
                    CheackHeart();
                }
            }
            catch
            {
            }
        }

        public void CheackHeart()
        {
            int i = 0;
            while (i < ClientSocketList.Count)
            {
                if (ClientSocketList[i] == null)
                {
                    ClientSocketList.RemoveAt(i);
                    continue;
                }
                if (ClientSocketList[i].ClientStyle == ClientStyle.WebSocket)
                {
                    continue;
                }
                //else if ((int)(DateTime.Now - ClientSocketList[i].HeartTime).TotalSeconds > HeartTime * 4)//4次没有收到失去联系
                //{
                //    RemoveClient(ClientSocketList[i]);
                //    continue;
                //}
                string sendLoginMsg = "";
                switch (ClientSocketList[i].Clienttype)
                {
                    //case ClientType.GpsClientType:
                    //    sendLoginMsg = "&Login&0&&";
                    //    break;
                    case ClientType.None:
                        sendLoginMsg = "客户端\r\n";
                        break;
                    default: sendLoginMsg = ClientSocketList[i].Heartbeat;
                        break;
                }
                if (sendLoginMsg != "")
                {
                    try
                    {
                        SendData(ClientSocketList[i], Encoding.Default.GetBytes(sendLoginMsg));
                        //ClientSocketList[i].ClientSocket.Send(Encoding.Default.GetBytes(sendLoginMsg));
                    }
                    catch
                    {
                        try
                        {
                            ClientSocketList[i].ClientSocket.Shutdown(SocketShutdown.Both);
                            ClientSocketList[i].ClientSocket.Close();
                            OnTcpServerOfflineClientEnterHead(ClientSocketList[i]);
                            ClientSocketList.Remove(ClientSocketList[i]);
                            OnTcpServerReturnClientCountEnterHead(ClientSocketList.Count);
                        }
                        catch
                        {
                        }
                    }
                }
                i++;
            }
        }
        /// <summary>
        /// 心跳检测方法
        /// </summary>
        public void ForeachCheack()
        {
            try
            {
                foreach (var cs in ClientSocketList)
                {
                    if (cs.ClientStyle == ClientStyle.WebSocket)
                    {
                        continue;
                    }
                    string sendLoginMsg = "";
                    switch (cs.Clienttype)
                    {
                        //case ClientType.GpsClientType:
                        //    sendLoginMsg = "&Login&0&&";
                        //    break;
                        case ClientType.None:
                            sendLoginMsg = "客户端\r\n";
                            break;
                        default: sendLoginMsg = cs.Heartbeat;
                            break;
                    }
                    if (sendLoginMsg != "")
                    {
                        try
                        {
                            cs.ClientSocket.Send(Encoding.Default.GetBytes(sendLoginMsg));
                        }
                        catch
                        {
                            try
                            {
                                cs.ClientSocket.Shutdown(SocketShutdown.Both);
                                OnTcpServerOfflineClientEnterHead(cs);
                                ClientSocketList.Remove(cs);
                                OnTcpServerReturnClientCountEnterHead(ClientSocketList.Count);
                                ForeachCheack();
                            }
                            catch
                            {
                            }
                        }
                    }
                    ////电子警察客户端，信号机PC控制端，轨道PC客户端，地磁PC客户端，地磁Androdi客户端，不用发送心跳包
                    //if (cs.Clienttype != ClientType.PcUtcClientType 
                    //    &&cs.Clienttype != ClientType.PcLampClient
                    //    && cs.Clienttype != ClientType.TrainPcType 
                    //    && cs.Clienttype != ClientType.PcGeomagnetic 
                    //    && cs.Clienttype != ClientType.AndroidGeomagnetic 
                    //    && cs.Clienttype != ClientType.ElectronicPoliceClient 
                    //    && cs.Clienttype!=ClientType.None)
                    //{
                    //    try
                    //    {
                    //        if (cs.Clienttype == ClientType.GpsClientType)
                    //        {
                    //            string sendLoginMsg = "&Login&0&&";
                    //            byte[] sendBytes = Encoding.UTF8.GetBytes(sendLoginMsg);
                    //            cs.ClientSocket.Send(sendBytes);
                    //        }
                    //        else if (cs.Clienttype != ClientType.PcLampClient && cs.Clienttype != ClientType.PcUtcClientType)
                    //        {
                    //            cs.ClientSocket.Send(Encoding.Default.GetBytes(_heartbeatPacket));
                    //        }
                    //    }
                    //    catch
                    //    {
                    //        try
                    //        {
                    //            cs.ClientSocket.Shutdown(SocketShutdown.Both);
                    //            OnTcpServerOfflineClientEnterHead(cs);
                    //            ClientSocketList.Remove(cs);
                    //            OnTcpServerReturnClientCountEnterHead(ClientSocketList.Count);
                    //            ForeachCheack();
                    //        }
                    //        catch
                    //        {
                    //        }
                    //    }
                    //}
                }
            }
            catch
            {
            }
        }
        private void RemoveClient(ClientModel clientModel)
        {
            if (clientModel == null)
                return;
            clientModel.ClientSocket.Close();
            OnTcpServerOfflineClientEnterHead(clientModel);
            ClientSocketList.Remove(clientModel);
        }
        /// <summary>
        /// 开始监听
        /// </summary>
        public void Start()
        {
            try
            {
                //若已开始监听，则不在开启线程监听，直至关闭监听后才能再次开启监听
                if (IsStartListening)
                    return;
                //启动线程打开监听
                StartSockst = new Thread(new ThreadStart(StartSocketListening));
                StartSockst.Start();

            }
            catch (SocketException ex)
            {
                OnTcpServerErrorMsgEnterHead(ex.Message);
            }
        }
        /// <summary>
        /// 关闭监听
        /// </summary>
        public void Stop()
        {
            try
            {
                IsStartListening = false;
                ShutdownClient();
                if (ServerSocket != null)
                {
                    ServerSocket.Close();
                }
                if (StartSockst != null)
                {
                    StartSockst.Interrupt();
                    if (StartSockst.IsAlive)
                    {
                        StartSockst.Abort();
                    }
                    StartSockst = null;
                }
                if (HeadCheck != null)
                {
                    HeadCheck.Interrupt();
                    HeadCheck.Abort();

                    if (HeadCheck.IsAlive)
                    {
                        HeadCheck.Abort();
                    }
                    HeadCheck = null;
                }
                OnTcpServerStateInfoEnterHead(string.Format("服务端Ip:{0},端口:{1}已停止监听", ServerIp, ServerPort), SocketState.StopListening);
                //foreach (var cmodel in ClientSocketList)
                //{
                //    cmodel.ClientSocket.Shutdown(SocketShutdown.Both);
                //    //ClientSocketList.Remove(ClientSocketList[i]);
                //    OnTcpServerOfflineClientEnterHead(cmodel);
                //    //Thread.Sleep(10);
                //}
                //ShutdownClient();
                //for (int i = 0; i < ClientSocketList.Count; i++)
                //{
                //    ClientSocketList[i].ClientSocket.Shutdown(SocketShutdown.Both);
                //    //ClientSocketList.Remove(ClientSocketList[i]);
                //    OnTcpServerOfflineClientEnterHead(ClientSocketList[i]);
                //    Thread.Sleep(10);
                //}
                ClientSocketList.Clear();
                OnTcpServerReturnClientCountEnterHead(ClientSocketList.Count);
                GC.Collect();

            }
            catch (SocketException ex)
            {
            }
        }
        public void ShutdownClient()
        {
            if (ClientSocketList.Count > 0)
            {
                foreach (var cmodel in ClientSocketList)
                {
                    cmodel.ClientSocket.Shutdown(SocketShutdown.Both);
                    //ClientSocketList.Remove(ClientSocketList[i]);
                    OnTcpServerOfflineClientEnterHead(cmodel);
                    ClientSocketList.Remove(cmodel);
                    break;
                    //Thread.Sleep(10);
                }
            }
            else
            {
                return;
            }
            ShutdownClient();
        }
        /// <summary>
        /// 开始监听
        /// </summary>
        public void StartSocketListening()
        {
            try
            {

                //获取本机IP:
                //string strip = //Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString();

                //ServerIp = strip;
                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //绑定监听
                ServerSocket.Bind(new IPEndPoint(IPAddress.Any, ServerPort));
                //ServerSocket.Bind(new IPEndPoint(ServerIp=="."?IPAddress.Any:IPAddress.Parse(ServerIp), ServerPort));
                ServerSocket.Listen(10000);
                //标记准备就绪，开始监听
                IsStartListening = true;
                OnTcpServerStateInfoEnterHead(string.Format("服务端Ip:{0},端口:{1}已启动监听", ServerIp, ServerPort), SocketState.StartListening);
                //启动心跳检测线程
                if (HeadCheck == null)
                {
                    HeadCheck = new Thread(new ThreadStart(HeadCheckThread));
                    HeadCheck.Start();
                }
                while (IsStartListening)
                {
                    //阻塞挂起直至有客户端连接
                    Socket clientSocket = ServerSocket.Accept();
                    ClientModel clietnModel = new ClientModel();
                    try
                    {
                        //Thread.Sleep(10);
                        //添加客户端用户
                        clietnModel = new ClientModel() { ClientSocket = clientSocket };
                        ClientSocketList.Add(clietnModel);
                        string ip = ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString();
                        string port = ((IPEndPoint)clientSocket.RemoteEndPoint).Port.ToString();
                        OnTcpServerStateInfoEnterHead("<" + ip + "：" + port + ">---上线", SocketState.ClientOnline);
                        OnTcpServerOnlineClientEnterHead(clietnModel);
                        OnTcpServerReturnClientCountEnterHead(ClientSocketList.Count);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ClientSocketCallBack), clientSocket);
                    }
                    catch (Exception ex)
                    {
                        clientSocket.Shutdown(SocketShutdown.Both);
                        OnTcpServerOfflineClientEnterHead(clietnModel);
                        ClientSocketList.Remove(clietnModel);
                        OnTcpServerReturnClientCountEnterHead(ClientSocketList.Count);
                        OnTcpServerErrorMsgEnterHead(ex.Message);
                        //DelegateHelper.TcpServerErrorMsg("网络通讯异常，异常原因：" + ex.Message);
                    }
                }

            }
            catch (Exception ex)
            {
                //其他错误原因
                OnTcpServerErrorMsgEnterHead(ex.Message);
            }
        }
        /// <summary>
        /// 线程池回调
        /// </summary>
        /// <param name="obj"></param>

        public void ClientSocketCallBack(Object obj)
        {

            Socket temp = (Socket)obj;


            #region 承载socket的客户端
            string ip = ((IPEndPoint)temp.RemoteEndPoint).Address.ToString();
            string port = ((IPEndPoint)temp.RemoteEndPoint).Port.ToString();
            ClientModel ctemp = ResoultSocket(ip, int.Parse(port));
            byte[] receivebyte = new byte[1024];
            #endregion
            while (IsStartListening)
            {
                Thread.Sleep(10);
                //MemoryStream mStream = new MemoryStream();
                //mStream.Position = 0;
                try
                {
                    //可自定心跳包数据
                    //temp.Send(System.Text.Encoding.Default.GetBytes("&conn&")); //心跳检测socket连接
                    int bytelen = temp.Receive(receivebyte);

                    
                    if (bytelen > 0)
                    {
                        byte[] bytes = new byte[bytelen];
                        Array.Copy(receivebyte, 0, bytes, 0, bytelen);
                        //OnTcpServerReceviceByte(temp, bytes);
                        //接收客户端数据
                        string clientRecevieStr = ASCIIEncoding.Default.GetString(receivebyte, 0, bytelen);
                        //验证websocket握手协议
                        if (ctemp != null && ctemp.ClientStyle != ClientStyle.WebSocket)
                        {
                            //如果客户端不存在则退出检测
                            WebSocketHandShake(ctemp, clientRecevieStr, bytes);
                        }
                        if (ctemp.ClientStyle == ClientStyle.WebSocket)
                        {
                            string webstr = AxTcpServer.AnalyticData(bytes, bytes.Length);
                            clientRecevieStr = webstr;
                            bytes = Encoding.Default.GetBytes(clientRecevieStr);
                        }
                        OnTcpServerRecevice(temp, clientRecevieStr, bytes);
                        OnGetLogEnterHead(ctemp, LogType.ReceviedData, DataTool.HexByteArrayToString(bytes));

                        //DelegateHelper.TcpServerReceive(temp, clientRecevieStr);
                    }
                    else if (bytelen == 0)
                    {
                        //接收到数据时数据长度一定是>0，若为0则表示客户端断线
                        //string ip = ((IPEndPoint)temp.RemoteEndPoint).Address.ToString();
                        //string port = ((IPEndPoint)temp.RemoteEndPoint).Port.ToString();
                        //ClientModel ctemp = ResoultSocket(ip, int.Parse(port));
                        if (ctemp != null)
                        {
                            ctemp.ClientSocket.Shutdown(SocketShutdown.Both);
                            OnTcpServerOfflineClientEnterHead(ctemp);
                            ClientSocketList.Remove(ctemp);
                            OnTcpServerStateInfoEnterHead("<" + ip + "：" + port + ">---下线",
                                SocketState.ClientOnOff);
                            OnTcpServerOfflineClientEnterHead(ctemp);
                            OnTcpServerReturnClientCountEnterHead(ClientSocketList.Count);
                        }
                        try
                        {
                            temp.Shutdown(SocketShutdown.Both);
                        }
                        catch
                        {
                        }
                        break;
                    }
                }
                catch
                {
                    try
                    {
                        if (ctemp != null)
                        {
                            ctemp.ClientSocket.Shutdown(SocketShutdown.Both);
                            OnTcpServerOfflineClientEnterHead(ctemp);
                            ClientSocketList.Remove(ctemp);
                            OnTcpServerStateInfoEnterHead("<" + ip + "：" + port + ">---下线",
                                SocketState.ClientOnOff);
                            OnTcpServerOfflineClientEnterHead(ctemp);
                            OnTcpServerReturnClientCountEnterHead(ClientSocketList.Count);
                        }
                    }
                    catch
                    {
                    }
                    break;
                }
            }
        }


        #region websocket模块

        public void WebSocketHandShake(ClientModel cModel, string msg, byte[] data)
        {
            #region 网络WebSocket协议握手
            string webkey = AxTcpServer.GetSecKey(data, data.Length);
            if (!string.IsNullOrEmpty(webkey) && cModel.ClientStyle != ClientStyle.WebSocket)
            {
                byte[] bufferwoshou = AxTcpServer.PackHandShakeData(webkey);
                if (bufferwoshou.Length > 0)
                {
                    cModel.ClientStyle = ClientStyle.WebSocket;
                    cModel.ClientSocket.Send(bufferwoshou);
                }
            }
            else
            {
                //if (cModel.ClientStyle == ClientStyle.WebSocket)
                //{
                //    string webstr = AxTcpServer.AnalyticData(data, data.Length);
                //    msg = webstr;
                //    data = Encoding.Default.GetBytes(msg);
                //}
            }

            #endregion
        }

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="recBytes"></param>
        /// <param name="recByteLength"></param>
        /// <returns></returns>
        public static string AnalyticData(byte[] recBytes, int recByteLength)
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
        /// <param name="client">客户端</param>
        /// <param name="sendData">发送的数据包</param>
        public void SendToWebClient(ClientModel client, string sendData)
        {
            if (client == null)
            {
                return;
            }
            if (!client.ClientSocket.Connected)
            {
                return;
            }
            try
            {
                DataFrame dr = new DataFrame(sendData);
                client.ClientSocket.Send(dr.GetBytes());

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
        private static byte[] PackData(string message)
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
        public static byte[] PackHandShakeData(string secKeyAccept)
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
        public static string GetSecKeyAccetp(byte[] handShakeBytes, int bytesLength)
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
        public static string GetSecKey(byte[] handShakeBytes, int bytesLength)
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
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="strData"></param>
        public void SendData(string ip, int port, string strData)
        {
            ClientModel cModel = ResoultSocket(ip, port);
            try
            {

                if (cModel != null)
                    cModel.ClientSocket.Send((System.Text.Encoding.Default.GetBytes(strData)));

            }
            catch (SocketException ex)
            {

                if (cModel != null)
                    cModel.IsOnline = false;
                //cModel.ClientSocket.Shutdown(SocketShutdown.Both);
                //ClientSocketList.Remove(cModel);
                //OnTcpServerOfflineClientEnterHead(cModel);

                //OnTcpServerReturnClientCountEnterHead(ClientSocketList.Count);
                OnTcpServerErrorMsgEnterHead(ex.Message);
            }
        }
        /// <summary>
        /// 向指定客户端发送数据
        /// </summary>
        /// <param name="cModel">客户端</param>
        /// <param name="strData">字符串数据，ANSI编码</param>
        public void SendData(ClientModel cModel, string strData)
        {
            try
            {
                if (cModel != null)
                    cModel.ClientSocket.Send((System.Text.Encoding.Default.GetBytes(strData)));

            }
            catch (SocketException ex)
            {
                if (cModel != null)
                    cModel.IsOnline = false;
                //if (cModel != null)
                //    cModel.ClientSocket.Shutdown(SocketShutdown.Both);
                //ClientSocketList.Remove(cModel);
                //OnTcpServerOfflineClientEnterHead(cModel);
                //OnTcpServerReturnClientCountEnterHead(ClientSocketList.Count);
                OnTcpServerErrorMsgEnterHead(ex.Message);
            }
        }
        /// <summary>
        /// 向指定客户端发送数据
        /// </summary>
        /// <param name="cModel">客户端</param>
        /// <param name="data">字节数组数据</param>
        public void SendData(ClientModel cModel, byte[] data)
        {
            try
            {
                if (cModel != null)
                {
                    cModel.ClientSocket.Send(data);
                    OnGetLogEnterHead(cModel, LogType.SendData, DataTool.HexByteArrayToString(data));
                }

            }
            catch (SocketException ex)
            {
                if (cModel == null)
                {
                    ClientSocketList.Remove(cModel);
                    return;
                }
                cModel.IsOnline = false;
                cModel.ClientSocket.Shutdown(SocketShutdown.Both);
                OnTcpServerOfflineClientEnterHead(cModel);
                OnTcpServerErrorMsgEnterHead(ex.Message);
                OnGetLogEnterHead(cModel, LogType.SendData, ex.Message);
                ClientSocketList.Remove(cModel);
            }
        }
        /// <summary>
        /// 根据IP,端口查找Socket客户端
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public ClientModel ResoultSocket(string ip, int port)
        {
            ClientModel sk = null;
            try
            {
                foreach (ClientModel cm in ClientSocketList)
                {
                    if (((IPEndPoint)cm.ClientSocket.RemoteEndPoint).Address.ToString().Equals(ip)
                        && port == ((IPEndPoint)cm.ClientSocket.RemoteEndPoint).Port)
                    {
                        sk = cm;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                OnTcpServerErrorMsgEnterHead(ex.Message);
            }
            return sk;
        }
        public ClientModel ResoultSocket(string rfidNo)
        {
            ClientModel sk = null;
            try
            {
                foreach (ClientModel cm in ClientSocketList)
                {
                    if (cm.ClientRfidNo == rfidNo)
                    {
                        sk = cm;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                OnTcpServerErrorMsgEnterHead(ex.Message);
            }
            return sk;
        }
        #endregion

        #region 事件
        #region OnRecevice接收数据事件
        [Description("接收数据事件")]
        [Category("TcpServer事件")]
        public event TcpServerEventHandler<Socket, string, byte[]> OnRecevice;
        protected virtual void OnTcpServerRecevice(Socket temp, string msg, byte[] data)
        {
            if (OnRecevice != null)
            {
                CommonMethod.EventInvoket(() => { OnRecevice(temp, msg, data); });
            }
        }
        //public delegate void ReceviceByteEventHandler(Socket temp, byte[] dataBytes);
        //[Description("接收原始Byte数组数据事件")]
        //[Category("TcpServer事件")]
        //public event ReceviceByteEventHandler OnReceviceByte;
        //protected virtual void OnTcpServerReceviceByte(Socket temp, byte[] dataBytes)
        //{
        //    if (OnReceviceByte != null)
        //    {
        //        CommonMethod.EventInvoket(() => { OnReceviceByte(temp, dataBytes); }); 
        //    }
        //}
        #endregion

        #region OnErrorMsg返回错误消息事件
        [Description("错误消息")]
        [Category("TcpServer事件")]
        public event TcpServerEventHandler<string> OnErrorMsg;
        protected virtual void OnTcpServerErrorMsgEnterHead(string msg)
        {
            if (OnErrorMsg != null)
            {
                CommonMethod.EventInvoket(() => { OnErrorMsg(msg); });
            }
        }
        #endregion

        #region OnReturnClientCount用户上线下线时更新客户端在线数量事件
        [Description("用户上线下线时更新客户端在线数量事件")]
        [Category("TcpServer事件")]
        public event TcpServerEventHandler<int> OnReturnClientCount;
        /// <summary>
        /// 用户上线下线时更新客户端在线数量事件
        /// </summary>
        /// <param name="count"></param>
        protected virtual void OnTcpServerReturnClientCountEnterHead(int count)
        {
            if (OnReturnClientCount != null)
            {
                CommonMethod.EventInvoket(() => { OnReturnClientCount(count); });  
            }
        }
        #endregion

        #region OnStateInfo监听状态改变时返回监听状态事件
        [Description("监听状态改变时返回监听状态事件")]
        [Category("TcpServer事件")]
        public event TcpServerEventHandler<string, SocketState> OnStateInfo;
        protected virtual void OnTcpServerStateInfoEnterHead(string msg, SocketState state)
        {
            if (OnStateInfo != null)
            {
                CommonMethod.EventInvoket(() => { OnStateInfo(msg, state); });
            }
        }
        #endregion

        #region OnAddClient新客户端上线时返回客户端事件
        [Description("新客户端上线时返回客户端事件")]
        [Category("TcpServer事件")]
        public event TcpServerEventHandler<ClientModel> OnOnlineClient;
        protected virtual void OnTcpServerOnlineClientEnterHead(ClientModel temp)
        {
            if (OnOnlineClient != null)
            {
                CommonMethod.EventInvoket(() => { OnOnlineClient(temp); });
            }
        }
        #endregion

        #region OnOfflineClient客户端下线时返回客户端事件
        [Description("客户端下线时返回客户端事件")]
        [Category("TcpServer事件")]
        public event TcpServerEventHandler<ClientModel> OnOfflineClient;
        protected virtual void OnTcpServerOfflineClientEnterHead(ClientModel temp)
        {
            if (OnOfflineClient != null)
            {
                CommonMethod.EventInvoket(() =>
                {
                    OnOfflineClient(temp);
                });
            }
        }
        #endregion

        #region OnLog服务端读写操作时返回日志消息
        [Description("服务端读写操作时返回日志消息")]
        [Category("TcpServer事件")]
        public event TcpServerEventHandler<ClientModel, LogType, string> OnGetLog;
        protected virtual void OnGetLogEnterHead(ClientModel temp, LogType logType, string logMsg)
        {
            if (OnGetLog != null)
            {
                CommonMethod.EventInvoket(() =>
                {
                    OnGetLog(temp, logType, logMsg);
                });
            }
        }
        #endregion

        #endregion
    }
}
