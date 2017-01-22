using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TEC.Core.Sockets.Client;
using TEC.Core.Sockets.Core;
using Training.SocketCore;

namespace Training.SokcetClient
{
    class Program
    {
        private static object connectData = new object();
        public static void Main()
        {
            SocketClient socketClient = new SocketClient();
            socketClient.OnConnected += Program.SocketClient_OnConnected;
            //socketClient.PropertyChanged += SocketClient_PropertyChanged;
            socketClient.connectToServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9527), Program.connectData);
            //socketClient.sendDataAsync(socketClient.TokenId.Value, "123");
            Console.ReadKey();
        }

        private static void SocketClient_OnConnected(object sender, ConnectedEventArgs e)
        {
            if(e.ConnectOperationUserToken.Data == Program.connectData)
            {
                SocketClient socketClient = sender as SocketClient;
                Program.TokenId = e.DataHoldingUserToken.TokenId;
                socketClient.sendDataAsync(e.DataHoldingUserToken.TokenId, "789");
            }
        }

        //private static void SocketClient_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    SocketClient socketClient = sender as SocketClient;
        //    if (e.PropertyName == nameof(socketClient.TokenId) && socketClient.TokenId.HasValue)
        //    {
        //        socketClient.sendDataAsync(socketClient.TokenId.Value, "123");
        //    }
        //}
        private static int? TokenId { set; get; }
    }
}
