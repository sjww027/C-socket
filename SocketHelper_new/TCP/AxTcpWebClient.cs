using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace SocketHelper.TCP
{
    public partial class AxTcpWebClient : Component
    {
        public AxTcpWebClient()
        {
            InitializeComponent();
        }

        public AxTcpWebClient(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
