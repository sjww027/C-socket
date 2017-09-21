/********************************************************************
 * *
 * * Copyright (C) 2013-? Corporation All rights reserved.
 * * 作者： BinGoo QQ：315567586 
 * * 请尊重作者劳动成果，请保留以上作者信息，禁止用于商业活动。
 * *
 * * 创建时间：2014-08-05
 * * 说明：客户端类型管理
 * *
********************************************************************/

using System.ComponentModel;

namespace SocketHelper.TClass
{
    public enum ClientType
    {
        /// <summary>
        /// 小车客户端
        /// </summary>
        [Description("小车客户端")]
        CarType = 0,
        /// <summary>
        /// 轨道列车客户端
        /// 包括（火车、地铁、高铁、动车）
        /// </summary>
        [Description("轨道列车客户端")]
        TrainType = 1,
        /// <summary>
        /// 轨道监控PC客户端
        /// </summary>
        [Description("轨道监控PC客户端")]
        TrainPcType = 2,
        /// <summary>
        /// Led客户端
        /// </summary>
        [Description("信号机客户端")]
        LedType = 3,
        /// <summary>
        /// 监控调度客户端
        /// </summary>
        [Description("监控调度客户端")]
        GpsClientType = 4,
        /// <summary>
        /// 信号机PC客户端
        /// </summary>
        [Description("信号机PC客户端")]
        PcUtcClientType = 5,
        /// <summary>
        ///公交服务客户端
        /// </summary>
        [Description("公交服务客户端")]
        BusServerType = 6,
        /// <summary>
        /// 沙盘建筑背景灯控制客户端
        /// </summary>
        [Description("建筑灯客户端")]
        PcLampClient = 7,
        /// <summary>
        /// 沙盘建筑背景灯客户端
        /// </summary>
        [Description("建筑灯PC客户端")]
        LampClient = 8,
        /// <summary>
        /// 升降杆客户端
        /// </summary>
        [Description("升降杆客户端")]
        MotorClient = 9,
        /// <summary>
        /// 无标记
        /// </summary>
        [Description("未知")]
        None = 10,
        /// <summary>
        /// 华软客户端
        /// </summary>
        [Description("华软客户端")]
        HuaRuan = 11,
        /// <summary>
        /// 电子警察客户端
        /// </summary>
        [Description("电子警察客户端")]
        ElectronicPoliceClient = 12,
        /// <summary>
        /// 硬件地磁客户端
        /// </summary>
        [Description("硬件地磁客户端")]
        Geomagnetic = 13,
        /// <summary>
        /// 网页地磁客户端
        /// </summary>
        [Description("网页地磁客户端")]
        WebGeomagnetic = 14,
        /// <summary>
        /// andorid手机地磁客户端
        /// </summary>
        [Description(" Andorid手机地磁客户端")]
        AndroidGeomagnetic = 15,
        /// <summary>
        ///PC版地磁客户端
        /// </summary>
        [Description("地磁PC客户端")]
        PcGeomagnetic = 16,
        /// <summary>
        ///车辆监控客户端
        /// </summary>
        [Description("车辆监控客户端")]
        CarMonitor = 17,
        /// <summary>
        ///公交站闸机客户端
        /// </summary>
        [Description("公交站闸机客户端")]
        RfidDoor = 18,
        /// <summary>
        ///上海电气客户端
        /// </summary>
        [Description("上海电气客户端")]
        ShangHaiDianQi = 19,
        /// <summary>
        /// 电子警察闪关灯客户端
        /// </summary>
        [Description("电子警察闪关灯客户端")]
        ElectronicPoliceFlashLamp = 20,
        /// <summary>
        /// 停车场收费客户端
        /// </summary>
        [Description("停车场收费客户端")]
        ParkingChargeClient = 21,
        /// <summary>
        /// 新版调度监控客户端
        /// </summary>
        [Description("新版调度监控客户端")]
        DispatchClient = 22
    }
}
