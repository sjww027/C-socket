/********************************************************************
 * *
 * * Copyright (C) 2013-? Corporation All rights reserved.
 * * 作者： BinGoo QQ：315567586 
 * * 请尊重作者劳动成果，请保留以上作者信息，禁止用于商业活动。
 * *
 * * 创建时间：2014-08-05
 * * 说明：数据工具管理类
 * *
********************************************************************/
using System;
using System.Text;

namespace SocketHelper.ITool
{
    public class DataToolManager
    {
        /// <summary>
        /// 十六进制字符串转为字节数组
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] StringToHexByteArray(string s)
        {
            try
            {
                s = s.Replace(" ", "");
                if ((s.Length % 2) != 0)
                    s += " ";
                byte[] returnBytes = new byte[s.Length / 2];
                for (int i = 0; i < returnBytes.Length; i++)
                    returnBytes[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);
                return returnBytes;
            }
            catch
            {
                return new byte[0];
            }
        }
        /// <summary>
        /// 字节数组转为十六进制字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="intervalChar"></param>
        /// <returns></returns>
        public static string HexByteArrayToString(byte[] data, char intervalChar=' ')
        {
            try
            {
                StringBuilder sb = new StringBuilder(data.Length * 3);
                foreach (byte b in data)
                {
                    sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, intervalChar));
                }
                return sb.ToString().ToUpper();//将得到的字符全部以字母大写形式输出
            }
            catch
            {
                return "";
            }
        }
    }
}
