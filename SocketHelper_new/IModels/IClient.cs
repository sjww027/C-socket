/********************************************************************
 * *
 * * Copyright (C) 2013-? Corporation All rights reserved.
 * * 作者： BinGoo QQ：315567586 
 * * 请尊重作者劳动成果，请保留以上作者信息，禁止用于商业活动。
 * *
 * * 创建时间：2014-08-05
 * * 说明：客户端信息类，存储客户端的一些基本信息，可定义修改
 * *
********************************************************************/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SocketHelper.IUser;
using SocketHelper.TClass;

namespace SocketHelper.IModels
{
    public class IClient
    {
        public IClient()
        {
            ClientStyle=ClientStyle.PcSocket;
            Username = "";
            Password = "";
            BufferInfo=new BufferInfo();
            ClientInfo=new ClientInfo();
        }
        public IClient(Socket socket)
        {
            WorkSocket = socket;
            if (socket != null)
            {
                Ip = ((IPEndPoint)WorkSocket.RemoteEndPoint).Address.ToString();
                Port = ((IPEndPoint)WorkSocket.RemoteEndPoint).Port;
            }
            ClientStyle = ClientStyle.PcSocket;
            Username = "";
            Password = "";
            BufferInfo = new BufferInfo();
            ClientInfo = new ClientInfo();
        }
        /// <summary>
        /// Socket
        /// </summary>
        public Socket WorkSocket { get; set; }
        /// <summary>
        /// 客户端端口IP
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// 客户端端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// Socket类型（网页版或者PC版）
        /// </summary>
        public ClientStyle ClientStyle { get; set; }
        /// <summary>
        /// 客户端登录账号
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// 客户端登录密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 客户端信息类
        /// </summary>
        public ClientInfo ClientInfo { get; set; }
        /// <summary>
        /// 数据缓存区信息
        /// </summary>
        public BufferInfo BufferInfo { get; set; }
        /// <summary>
        /// 自定义数据
        /// </summary>
        public object CustomData { get; set; }
        /// <summary>
        /// 是否已登录
        /// </summary>
        public bool IsLogin { get; set; }
    }

    public class ClientInfo
    {
        /// <summary>
        /// 心跳检测模式
        /// </summary>
        public HeartCheckType HeartCheckType = HeartCheckType.EncodingString;
        /// <summary>
        /// 心跳包数组数据【如果长度为0为空则不发送心跳包】
        /// </summary>
        public byte[] HeartbeatByte =new byte[0];
        /// <summary>
        /// 心跳包字符串【如果为空则不发送心跳包】
        /// </summary>
        public string Heartbeat = "";
        /// <summary>
        /// 客户端ID
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// 客户端编号
        /// </summary>
        public string ClientNo { get; set; }
        /// <summary>
        /// 客户端类型
        /// </summary>
        public ClientType Clienttype = ClientType.None;
        /// <summary>
        /// 若是小车类型，则该标示表示小车单前的RFID卡号
        /// </summary>
        public string ClientRfidNo = "";
        /// <summary>
        /// 若是小车类型，则该标示表示小车单前的车道号
        /// </summary>
        public string ClientLaneNo = "";
        /// <summary>
        /// 如果是小车类型，则该项用于存储车辆运动状态，默认：运动
        /// </summary>
        public VehicleMotionState VehicleMotionState = VehicleMotionState.Running;
        /// <summary>
        /// 判断在停车位上的状态
        /// </summary>
        public SitState SitState = SitState.None;
        /// <summary>
        /// 如果是信号机类型，用来存储信号灯路口状态
        /// </summary>
        public LedState[] SingleState = new LedState[0];
        /// <summary>
        ///  如果是信号机类型，用来存储信号灯人行道状态
        /// </summary>
        public LedState[] SidewalkState = new LedState[0];
        /// <summary>
        /// 如果是闸机，升降杆客户端，（用于存储各个控制端的状态）
        /// </summary>
        public byte[] MotorClientState = new byte[32];
        /// <summary>
        /// 是否授权
        /// </summary>
        public bool IsAuthorization { get; set; }
        /// <summary>
        /// 指令操作类型
        /// </summary>
        public string OrderType = "";
        /// <summary>
        /// 如果是调度系统客户端，则用来存储PC客户端当前需要订阅的车辆列表
        /// </summary>
        public List<string> CarNo { get; set; }
    }

    public class BufferInfo
    {
        //备份缓冲区
        private byte[] _bufferBackup = null;
        /// <summary>
        /// 备份缓冲区;动态增大或缩小缓冲区的时候用到；
        /// </summary>
        internal byte[] BufferBackup
        {
            get { return _bufferBackup; }
            set { _bufferBackup = value; }
        }

        /// <summary>
        /// 接收缓冲区
        /// </summary>
        public byte[] ReceivedBuffer = new byte[1024];

        /// <summary>
        /// 发送缓冲区
        /// </summary>
        public byte[] SendBuffer = new byte[1024];

        /// <summary>
        /// 接收的字符串信息
        /// </summary>
        public string RecevidMsg = "";
    }

    /// <summary>
    /// 心跳检测模式
    /// </summary>
    public enum HeartCheckType
    {
        /// <summary>
        /// 字符串模式
        /// </summary>
        EncodingString,
        /// <summary>
        /// 十六进制字符串
        /// </summary>
        HexString,
        /// <summary>
        /// byte数组模式
        /// </summary>
        Byte
    }
}
