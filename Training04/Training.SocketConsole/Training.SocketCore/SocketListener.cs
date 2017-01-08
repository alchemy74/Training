using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TEC.Core.Sockets.Server;

namespace Training.SocketCore
{
    public class SocketListener
    {
        /// <summary>
        /// 初始化 Socket 監聽器
        /// </summary>
        /// <param name="ipEndPoint">用於監聽連線的位址以及通訊埠號</param>
        /// <exception cref="ArgumentNullException">當 <paramref name="ipEndPoint"/> 為 <c lang="C#">null</c> 參考時擲出。</exception>
        public SocketListener(IPEndPoint ipEndPoint)
        {
            if (ipEndPoint == null)
            {
                throw new ArgumentNullException(nameof(ipEndPoint));
            }
            Mediator mediator = new Mediator();
            SocketServerSettingCollection socketSettingCollection = new SocketServerSettingCollection();
            socketSettingCollection[SocketServerSettingEnum.BackLog] = 100;
            socketSettingCollection[SocketServerSettingEnum.LocalEndPoint] = ipEndPoint;
            socketSettingCollection[SocketServerSettingEnum.MaxConnections] = 30;
            socketSettingCollection[SocketServerSettingEnum.MaxSimultaneousAcceptOperation] = 10;
            socketSettingCollection[SocketServerSettingEnum.MaxProcessingOperationCount] = 3000;
            socketSettingCollection[SocketServerSettingEnum.OperationBufferSize] = 25;
            this.SocketListenerInernal = new SocketListener<DataHolder>(mediator, socketSettingCollection, false);
        }
        private SocketListener<DataHolder> SocketListenerInernal { set; get; }
    }
}
