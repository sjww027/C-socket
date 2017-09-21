using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace SocketHelper
{
    /// <summary>
    /// ���ͽ����ļ�������
    /// </summary>
	public class FileTansfersContainer : Panel
	{
		private IFileTransfersItemText _fileTransfersItemText;
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IFileTransfersItemText FileTransfersItemText
		{
			get
			{
				if (this._fileTransfersItemText == null)
				{
					this._fileTransfersItemText = new FileTransfersItemText();
				}
				return this._fileTransfersItemText;
			}
			set
			{
				this._fileTransfersItemText = value;
				foreach (FileTransfersItem item in base.Controls)
				{
					item.FileTransfersText = this._fileTransfersItemText;
				}
			}
		}
		public FileTansfersContainer()
		{
			this.AutoScroll = true;
		}

        private bool _isAutomaticShowHide = true;
        [Description("�Ƿ����Զ���ʾ���ؿؼ�\r\n(trueΪ�������н����ļ������ļ�ʱ�Զ���ʾ���ؿؼ�)")]
        public bool IsAutomaticShowHide {
            get { return _isAutomaticShowHide; }
            set { _isAutomaticShowHide = value; }
        }

        public FileTransfersItem AddItem(string text, string fileName, Image image, long fileSize, FileTransfersItemStyle style)
		{
			FileTransfersItem item = new FileTransfersItem();
			item.Text = text;
			item.FileName = fileName;
			item.Image = image;
			item.FileSize = fileSize;
			item.Style = style;
			item.FileTransfersText = this.FileTransfersItemText;
			item.Dock = DockStyle.Top;
			base.SuspendLayout();
			base.Controls.Add(item);
			item.BringToFront();
			base.ResumeLayout(true);
            if (IsAutomaticShowHide)
            {
                base.Visible = base.Controls.Count > 0 ? true : false;
            }
			return item;
		}
        /// <summary>
        /// ���FileTransfersItem�ļ���
        /// </summary>
        /// <param name="md5">���ƣ�MD5У��ֵ��</param>
        /// <param name="typetext">�ļ����ͣ������ļ�/�����ļ���</param>
        /// <param name="fileName">�ļ�·��</param>
        /// <param name="image">�ļ�ͼ��</param>
        /// <param name="fileSize">�ļ���С</param>
        /// <param name="style">�ļ�״̬������鿴��FileTransfersItemStyle��</param>
        /// <returns></returns>
		public FileTransfersItem AddItem(string md5, string typetext, string fileName, Image image, long fileSize, FileTransfersItemStyle style)
		{
			FileTransfersItem item = new FileTransfersItem();
            item.Name = md5;
            item.Text = typetext;
			item.FileName = fileName;
			item.Image = image;
			item.FileSize = fileSize;
			item.Style = style;
			item.FileTransfersText = this.FileTransfersItemText;
			item.Dock = DockStyle.Top;
			base.SuspendLayout();
			base.Controls.Add(item);
            item.BringToFront();
			base.ResumeLayout(true);
            if (IsAutomaticShowHide)
            {
                base.Visible = base.Controls.Count > 0 ? true : false;
            }
			return item;
		}
        /// <summary>
        /// �Ƴ���ӦFileTransfersItem
        /// </summary>
        /// <param name="item">FileTransfersItem</param>
		public void RemoveItem(FileTransfersItem item)
		{
			base.Controls.Remove(item);
            if (IsAutomaticShowHide)
            {
                base.Visible = base.Controls.Count > 0 ? true : false;
            }
		}
        /// <summary>
        /// ����MD5��ֵ�Ƴ���Ӧ�ؼ�
        /// </summary>
        /// <param name="md5"></param>
		public void RemoveItem(string md5)
		{
            base.Controls.RemoveByKey(md5);
            if (IsAutomaticShowHide)
            {
                base.Visible = base.Controls.Count > 0 ? true : false;
            }
		}
        
		public void RemoveItem(Predicate<FileTransfersItem> match)
		{
			FileTransfersItem itemRemove = null;
			foreach (FileTransfersItem item in base.Controls)
			{
				if (match(item))
				{
					itemRemove = item;
				}
			}
			base.Controls.Remove(itemRemove);
            if (IsAutomaticShowHide)
            {
                base.Visible = base.Controls.Count > 0 ? true : false;
            }
		}
        /// <summary>
        /// ����MD5У��ֵ����FileTransfersItem
        /// </summary>
        /// <param name="md5"></param>
        /// <returns></returns>
		public FileTransfersItem Search(string md5)
		{
            return base.Controls[md5] as FileTransfersItem;
		}
		public FileTransfersItem Search(Predicate<FileTransfersItem> match)
		{
			FileTransfersItem result;
			foreach (FileTransfersItem item in base.Controls)
			{
				if (match(item))
				{
					result = item;
					return result;
				}
			}
			result = null;
			return result;
		}
	}
}
