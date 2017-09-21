﻿using System;
using System.IO;

namespace SocketHelper
{
    /// <summary>
    /// 文件管理类
    /// </summary>
    public class SendFileManager : IDisposable
    {
        #region 变量

        private FileStream _fileStream;
        private long _partCount;
        private long _length;
        private int _partSize = 1024 * 20;
        private string _fileName;
        private string _md5;
        private object _tag;
        private Stream _safeStream;

        #endregion

        #region 构造函数

        public SendFileManager(string fileName)
        {
            _fileName = fileName;
            Create(fileName);
        }

        public SendFileManager(string fileName, int partSize)
        {
            _fileName = fileName;
            _partSize = partSize;
            Create(fileName);
        }

        #endregion

        #region 属性

        public long PartCount
        {
            get { return _partCount; }
            set { _partCount = value; }
        }

        public long Length
        {
            get { return _length; }
        }

        public int PartSize
        {
            get { return _partSize; }
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public string Name
        {
            get { return new FileInfo(_fileName).Name; }
        }

        public string MD5
        {
            get { return _md5; }
        }

        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        internal Stream FileStream
        {
            get { return _safeStream; }
        }

        #endregion

        #region 方法
        /// <summary>
        /// 创建初始化文件管理类
        /// </summary>
        /// <param name="fileName">文件路径</param>
        private void Create(string fileName)
        {
            _fileStream = new FileStream(
                fileName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                _partSize * 10,
                true);
            _safeStream = Stream.Synchronized(_fileStream);
            _length = _fileStream.Length;
            _partCount = _length / _partSize;
            if (_length % _partSize != 0)
            {
                _partCount++;
            }
            _md5 = MD5Helper.CretaeMD5(_fileStream);
        }
        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="index"></param>
        public void Read(int index)
        {
            int size = _partSize;
            if (Length - _partSize * index < _partSize)
            {
                size = (int)(Length - _partSize * index);
            }
            byte[] buffer = new byte[size];
            ReadFileObject obj = new ReadFileObject(index, buffer);
            FileStream.Position = index * _partSize;
            FileStream.BeginRead(
                buffer,
                0,
                size,
                new AsyncCallback(EndRead),
                obj);
        }

        /// <summary>
        /// 结束读取文件
        /// </summary>
        /// <param name="result"></param>
        private void EndRead(IAsyncResult result)
        {
            if (FileStream == null)
            {
                return;
            }
            int length = FileStream.EndRead(result);
            ReadFileObject state = (ReadFileObject)result.AsyncState;
            int index = state.Index;
            byte[] buffer = state.Buffer;
            ReadFileBufferEventArgs e = null;
            if (length < _partSize)
            {
                byte[] realBuffer = new byte[length];
                Buffer.BlockCopy(buffer, 0, realBuffer, 0, length);
                e = new ReadFileBufferEventArgs(index, realBuffer);
            }
            else
            {
                e = new ReadFileBufferEventArgs(index, buffer);
            }
            OnReadFileBuffer(e);
        }

        #endregion

        #region 事件
        /// <summary>
        /// 读取文件事件
        /// </summary>
        public event ReadFileBufferEventHandler ReadFileBuffer;
        /// <summary>
        /// 读取文件方法
        /// </summary>
        /// <param name="e"></param>
        protected void OnReadFileBuffer(ReadFileBufferEventArgs e)
        {
            if (ReadFileBuffer != null)
            {
                ReadFileBuffer(this, e);
            }
        }
        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            if (_fileStream != null)
            {
                _safeStream.Flush();
                _safeStream.Close();
                _safeStream.Dispose();
                _safeStream = null;
                _fileStream = null;
            }
        }

        #endregion
    }
}