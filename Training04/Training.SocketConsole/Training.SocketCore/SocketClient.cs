using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TEC.Core.ComponentModel;
using TEC.Core.Sockets.Client;

namespace Training.SocketCore
{

    public class SocketClient : NotifyPropertyChangedBase
    {
        public SocketClient()
        {
            SocketClientSettingCollection socketClientSettingCollection = new SocketClientSettingCollection();
            socketClientSettingCollection[SocketClientSettingEnum.MaxConnections] = 30;
            socketClientSettingCollection[SocketClientSettingEnum.MaxSimultaneousConnectOperation] = 10;
            socketClientSettingCollection[SocketClientSettingEnum.OperationBufferSize] = 25;
            this.SocketClientInternal = new SocketClient<DataHolder>(new Mediator(), socketClientSettingCollection, false);
            this.SocketClientInternal.OnConnected += this.SocketClientInternal_OnConnected;
            this.SocketClientInternal.inital();
        }

        private void SocketClientInternal_OnConnected(object sender, ConnectedEventArgs e)
        {
            this.OnConnected?.Invoke(this, e);
        }

        public void connectToServer(IPEndPoint ipEndPoint, object data)
        {
            this.SocketClientInternal.connectToServer(ipEndPoint, data);
        }
        public Task<int> sendDataAsync(int tokenId, string data)
        {
            return this.SocketClientInternal.sendDataAsync(tokenId,
                System.Text.Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(
                        new SocketCore.SocketsEvent.ReceivedMessageEvent()
                        {
                            EventArgument = data
                        }
                    )
                )
             );
        }
        public event EventHandler<ConnectedEventArgs> OnConnected;
        private SocketClient<DataHolder> SocketClientInternal { set; get; }
       
    }
}
