/********************************************************************
 * *
 * * Copyright (C) 2013-? Corporation All rights reserved.
 * * 作者： BinGoo QQ：315567586 
 * * 请尊重作者劳动成果，请保留以上作者信息，禁止用于商业活动。
 * *
 * * 创建时间：2014-08-05
 * * 说明：
 * *
********************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SocketHelper
{
    /// <summary>
    /// Socket状态枚举
    /// </summary>
    public enum SocketState
    {
        /// <summary>
        /// 正在连接服务端
        /// </summary>
        Connecting = 0,

        /// <summary>
        /// 已连接服务端
        /// </summary>
        Connected = 1,

        /// <summary>
        /// 重新连接服务端
        /// </summary>
        Reconnection = 2,

        /// <summary>
        /// 断开服务端连接
        /// </summary>
        Disconnect = 3,

        /// <summary>
        /// 正在监听
        /// </summary>
        StartListening = 4,
        /// <summary>
        /// 启动监听异常
        /// </summary>
        StartListeningError = 5,
        /// <summary>
        /// 停止监听
        /// </summary>
        StopListening =6,

        /// <summary>
        /// 客户端上线
        /// </summary>
        ClientOnline = 7,

        /// <summary>
        /// 客户端下线
        /// </summary>
        ClientOnOff = 8
    }
    /// <summary>
    /// 错误类型
    /// </summary>
    public enum SocketError
    {

    }

    /// <summary>
    /// 发送接收命令枚举
    /// </summary>
    public enum Command
    {
        
        /// <summary>
        /// 发送请求接收文件
        /// </summary>
        RequestSendFile = 0x000001,
        /// <summary>
        /// 响应发送请求接收文件
        /// </summary>
        ResponeSendFile = 0x100001,

        /// <summary>
        /// 请求发送文件包
        /// </summary>
        RequestSendFilePack = 0x000002,
        /// <summary>
        /// 响应发送文件包
        /// </summary>
        ResponeSendFilePack = 0x100002,

        /// <summary>
        /// 请求取消发送文件包
        /// </summary>
        RequestCancelSendFile = 0x000003,
        /// <summary>
        /// 响应取消发送文件包
        /// </summary>
        ResponeCancelSendFile = 0x100003,

        /// <summary>
        /// 请求取消接收发送文件
        /// </summary>
        RequestCancelReceiveFile = 0x000004,
        /// <summary>
        /// 响应取消接收发送文件
        /// </summary>
        ResponeCancelReceiveFile = 0x100004,
        /// <summary>
        /// 请求发送文本消息
        /// </summary>
        RequestSendTextMSg = 0x000010,
    }
    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MsgType
    {
        /// <summary>
        /// 文本消息
        /// </summary>
        TxtMsg=0,
        /// <summary>
        /// 抖动窗体
        /// </summary>
        Shake= 1,
        /// <summary>
        /// 表情
        /// </summary>
        Face=2,
        /// <summary>
        /// 图片
        /// </summary>
        Pic=3
    }
    public enum ClientStyle
    {
        WebSocket,
        PcSocket
    }
    public enum LogType
    {
        /// <summary>
        /// 系统
        /// </summary>
        [Description("系统日志")]
        System,

        /// <summary>
        /// 服务端
        /// </summary>
        [Description("服务端类型日志")]
        Server,

        /// <summary>
        /// 客户端
        /// </summary>
        [Description("客户端类型日志")]
        Client,

        /// <summary>
        /// 发送数据
        /// </summary>
        [Description("发送数据类型日志")]
        SendData,

        /// <summary>
        /// 接收数据
        /// </summary>
        [Description("接收数据类型日志")]
        ReceviedData,

        /// <summary>
        /// 发送数据返回结果
        /// </summary>
        [Description("发送数据返回结果类型日志")]
        SendDataResult
    }

}
