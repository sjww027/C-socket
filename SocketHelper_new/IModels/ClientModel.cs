using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using SocketHelper.TClass;

namespace SocketHelper.IModels
{
    /// <summary>
    /// 此类在旧版本AxTcpServer组件中中使用
    /// 客户端类
    /// </summary>
    public class ClientModel
    {
        private byte[] _bufferBackup = null;//备份缓冲区;主要是缓冲区有时候需要增大或缩小的时候用到；
        /// <summary>
        /// 备份缓冲区;主要是缓冲区有时候需要增大或缩小的时候用到；
        /// </summary>
        internal byte[] BufferBackup
        {
            get { return _bufferBackup; }
            set { _bufferBackup = value; }
        }
        /// <summary>
        /// 接受的数据缓存
        /// </summary>
        public string RecevidMsg = "";
        /// <summary>
        /// 缓存数据
        /// </summary>
        public List<byte> RecevidBuffer = new List<byte>();
        /// <summary>
        /// PC嵌套字
        /// </summary>
        public Socket ClientSocket;
        /// <summary>
        /// 客户端类型1
        /// </summary>
        public ClientType Clienttype = ClientType.None;

        /// <summary>
        /// Socket类型（网页版或者PC版）
        /// </summary>
        public ClientStyle ClientStyle = ClientStyle.PcSocket;
        /// <summary>
        /// 客户端编号
        /// </summary>
        public int Id = -1;
        /// <summary>
        /// 若是小车类型，则该标示表示车辆编号，若是信号机类型，则该标示表示信号机编号，其他则不使用该类型
        /// </summary>
        public string ClientNo = "";
        /// <summary>
        /// 如果是小车类型，则该项用于存储车辆运动状态，默认：运动
        /// </summary>
        public VehicleMotionState VehicleMotionState = VehicleMotionState.Running;
        /// <summary>
        /// 判断在停车位上的状态
        /// </summary>
        public SitState SitState = SitState.None;
        public LedState[] SingleState = new LedState[0];
        public LedState[] SidewalkState = new LedState[0];
        /// <summary>
        /// 若是小车类型，则该标示表示小车单前的RFID卡号
        /// </summary>
        public string ClientRfidNo = "";
        /// <summary>
        /// 若是小车类型，则该标示表示小车单前的车道号
        /// </summary>
        public string ClientLaneNo = "";
        /// <summary>
        /// 若是PC客户端类型，则下面两个表示表示用户登陆账号和密码
        /// </summary>
        public string Username = "";
        public string Password = "";
        //存储PC客户端当前需要订阅的车辆
        public List<string> CarNo { get; set; }
        /// <summary>
        /// 如果是闸机，升降杆客户端，（用于存储各个控制端的状态）
        /// </summary>
        public byte[] MotorClientState = new byte[32];
        /// <summary>
        /// 是否登陆
        /// </summary>
        public bool IsLogin { get; set; }
        /// <summary>
        /// 是否授权
        /// </summary>
        public bool IsAuthorization { get; set; }
        /// <summary>
        /// 是否在线
        /// </summary>
        public bool IsOnline = true;
        /// <summary>
        /// 心跳包字符串【如果为空则不发送心跳包】
        /// </summary>
        public string Heartbeat = "";
        public string OrderType = "";
        /// <summary>
        /// 承载客户端Socket的网络流
        /// </summary>
        public NetworkStream NetworkStream { get; set; }
        /// <summary>
        /// 发生异常时不为null.
        /// </summary>
        public Exception Exception { get; set; }
        /// <summary>
        /// 接收缓冲区
        /// </summary>
        public byte[] RecBuffer =new byte[1024];

        /// <summary>
        /// 发送缓冲区
        /// </summary>
        public byte[] SendBuffer { get; set; }

        /// <summary>
        /// 异步接收后包的大小
        /// </summary>
        public int Offset { get; set; }
    }
    

    
}
