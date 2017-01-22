using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TEC.Core.Scheduler.Timers;
using TEC.Core.Sockets.Client;
using Training.SocketCore;

namespace Training.SokcetClient.TimerEvents
{
    public class KeepConnectionTimerEvent : ITimerEvent
    {
        private object connectData = new object();
        public KeepConnectionTimerEvent(SocketClient socketClient, IPEndPoint ipEndPoint)
        {
            if (socketClient == null)
            {
                throw new ArgumentNullException(nameof(socketClient));
            }
            if (ipEndPoint == null)
            {
                throw new ArgumentNullException(nameof(ipEndPoint));
            }
            this.SocketClient = socketClient;
            this.SocketClient.OnConnected += this.SocketClient_OnConnected;
            this.SocketClient.OnDisconnected += this.SocketClient_OnDisconnected;
            this.IPEndPoint = ipEndPoint;
        }
        private void SocketClient_OnDisconnected(object sender, SocketCore.EventArguments.DisconnectedSocketEventArgs e)
        {
            if (this.TokenId.HasValue && e.TokenId == this.TokenId)
            {
                this.TokenId = null;
            }
        }

        private void SocketClient_OnConnected(object sender, ConnectedEventArgs e)
        {
            if (e.ConnectOperationUserToken.Data == this.connectData)
            {
                SocketClient socketClient = sender as SocketClient;
                this.TokenId = e.DataHoldingUserToken.TokenId;
            }
        }
        public void execute()
        {
            if (!this.TokenId.HasValue)
            {
                this.SocketClient.connectToServer(this.IPEndPoint, this.connectData);
            }
        }
        public void Dispose()
        {
            if (this.SocketClient != null)
            {
                this.SocketClient.OnConnected -= this.SocketClient_OnConnected;
                this.SocketClient.OnDisconnected -= this.SocketClient_OnDisconnected;
            }
        }
        private SocketClient SocketClient { set; get; }
        private IPEndPoint IPEndPoint { set; get; }
        public int? TokenId { set; get; }
    }
}
