/********************************************************************
 * *
 * * Copyright (C) 2013-? Corporation All rights reserved.
 * * 作者： BinGoo QQ：315567586 
 * * 请尊重作者劳动成果，请保留以上作者信息，禁止用于商业活动。
 * *
 * * 创建时间：2014-08-05
 * * 说明：自定义枚举文件
 * *
********************************************************************/

namespace SocketHelper.IUser
{
    /// <summary>
    /// 停车位状态
    /// </summary>
    public enum SitState
    {
        None,
        Enter,
        Leave,
        On,
        Out
    }

    /// <summary>
    /// 车辆运动状态
    /// </summary>
    public enum VehicleMotionState
    {
        /// <summary>
        /// 运动
        /// </summary>
        Running = 0x01,

        /// <summary>
        /// 停止
        /// </summary>
        Stop = 0x00,

        /// <summary>
        /// 加速
        /// </summary>
        AddSpeed = 0x02,

        /// <summary>
        /// 减速
        /// </summary>
        DelSpeed = 0x03,
    }

    /// <summary>
    /// 信号灯状态
    /// </summary>
    public enum LedState
    {
        /// <summary>
        /// 绿灯
        /// </summary>
        Green = 0x00,

        /// <summary>
        /// 绿灯
        /// </summary>
        GreenFlash = 0x01,

        /// <summary>
        /// 红灯
        /// </summary>
        Red = 0x02,

        /// <summary>
        /// 黄灯
        /// </summary>
        Yellow = 0x04,

        /// <summary>
        /// 黄闪
        /// </summary>
        YellowFlash = 0x05,

        /// <summary>
        /// 无灯色（灭灯）
        /// </summary>
        None = 0x06
    }
}
