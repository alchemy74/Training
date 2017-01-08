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
        private static SocketClientSettingCollection socketSettingCollection = new SocketClientSettingCollection();
        public static void Main()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9527);
            socketSettingCollection[SocketClientSettingEnum.MaxConnections] = 30;
            socketSettingCollection[SocketClientSettingEnum.MaxSimultaneousConnectOperation] = 10;
            socketSettingCollection[SocketClientSettingEnum.OperationBufferSize] = 25;
            SocketClient<DataHolder> socketClient = new SocketClient<DataHolder>(new Mediator(), Program.socketSettingCollection, false);
            socketClient.OnConnected += Program.SocketClient_OnConnected;
            socketClient.OnClosingSocket += Program.SocketClient_OnClosingSocket;
            socketClient.OnConnectError += Program.SocketClient_OnConnectError;
            socketClient.OnDataReceived += Program.SocketClient_OnDataReceived;
            socketClient.OnReceivedMessage += Program.SocketClient_OnReceivedMessage;
            socketClient.OnReceiveEmptyData += Program.SocketClient_OnReceiveEmptyData;
            socketClient.OnReceiveError += Program.SocketClient_OnReceiveError;
            socketClient.OnDataSent += Program.SocketClient_OnDataSent;
            socketClient.OnSendCompleted += Program.SocketClient_OnSendCompleted;
            socketClient.OnSendError += Program.SocketClient_OnSendError;
            socketClient.OnStartingReceive += Program.SocketClient_OnStartingReceive;
            socketClient.OnStartingSend += Program.SocketClient_OnStartingSend;
            ((System.Collections.Specialized.INotifyCollectionChanged)socketClient.ReadOnlyConnectedTokenIdCollection).CollectionChanged += Program.Program_CollectionChanged;
            socketClient.inital();
            socketClient.connectToServer(remoteEndPoint, null);
            while (true)
            {
                string data = Console.ReadLine();
                socketClient.ReadOnlyConnectedTokenIdCollection.ToList().ForEach(tokenId =>
                {
                    socketClient.sendDataAsync(tokenId, System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new SocketCore.SocketsEvent.AuthenticateEvent()
                    {
                        EventArgument = data
                    })));
                    ////傳送訊息至伺服器
                    //Console.WriteLine($"已佇列傳送資料至 Token: {tokenId}，發送的訊息 Token 為:{socketClient.sendDataAsync(tokenId, System.Text.Encoding.UTF8.GetBytes(data)).Result}");
                });
            }
        }
        private static void SocketClient_OnSendError(object sender, SocketErrorEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 傳送作業發生錯誤，Token ID: {e.UserToken.TokenId}，原因: {e.SocketError.ToString()}");
        }
        private static void SocketClient_OnDataSent(object sender, DataHoldingEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 已傳送完整訊息，Token ID: {e.DataHoldingUserToken.TokenId}，訊息: { System.Text.Encoding.UTF8.GetString(e.DataHoldingUserToken.DataHolder.Data)}");
        }
        private static void SocketClient_OnSendCompleted(object sender, DataHoldingEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- Send 操作已完成，Token ID: {e.DataHoldingUserToken.TokenId}");
        }
        private static void SocketClient_OnReceiveError(object sender, SocketErrorEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 接收作業發生錯誤，Token ID: {e.UserToken.TokenId}，原因: {e.SocketError.ToString()}");
        }
        private static void SocketClient_OnReceiveEmptyData(object sender, DataHoldingEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 接收到空白資料，Token ID: {e.DataHoldingUserToken.TokenId}");
        }
        private static void SocketClient_OnReceivedMessage(object sender, DataHoldingEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 已完成一次 Receive 作業，目前結果:  {(e.DataHoldingUserToken.DataHolder.Data == null ? "尚未建立或已傳送完畢" : System.Text.Encoding.UTF8.GetString(e.DataHoldingUserToken.DataHolder.Data))}");
        }
        private static void SocketClient_OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 已接收到一次完整訊息，資料: {(e.DataHolder.Data == null ? "尚未建立" : System.Text.Encoding.UTF8.GetString(e.DataHolder.Data))}，來源:{e.RemoteEndPoint}");
        }
        private static void SocketClient_OnConnectError(object sender, SocketErrorEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 客戶端 Socket 發生錯誤，Token ID:\"{e.UserToken.TokenId}\"，訊息: {e.SocketError.ToString()}");
        }
        private static void SocketClient_OnClosingSocket(object sender, DataHoldingEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 客戶端正在關閉 Socket ，Token ID:\"{e.DataHoldingUserToken.TokenId}\"");
        }
        private static void SocketClient_OnStartingSend(object sender, DataHoldingEventArgs e)
        {
            DataHolder dataHolder = e.DataHoldingUserToken.DataHolder as DataHolder;
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 以 Token ID: {e.DataHoldingUserToken.TokenId} 準備送資料給伺服器端({e.DataHoldingUserToken.RemoteEndpoint})。");
            Console.WriteLine($"   資料:{System.Text.Encoding.UTF8.GetString(dataHolder.OriginalDataToSend) }");
            Console.WriteLine($"   (已處理)資料:{String.Join(" ", dataHolder.Data) }");
        }
        private static void SocketClient_OnStartingReceive(object sender, DataHoldingEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 以 Token ID: {e.DataHoldingUserToken.TokenId} 準備接受伺服器端({e.DataHoldingUserToken.RemoteEndpoint})的資料。");
        }
        private static void Program_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            System.Collections.ObjectModel.ReadOnlyObservableCollection<int> connectedTokenIdCollection = sender as System.Collections.ObjectModel.ReadOnlyObservableCollection<int>;
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 已連接的連線集合發生變更，目前共管理\"{connectedTokenIdCollection.Count}\"個 Token");
            Console.WriteLine($"   Token: {String.Join(",", connectedTokenIdCollection)}");
        }
        private static void SocketClient_OnConnected(object sender, ConnectedEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 已連接的至伺服器，Token ID:\"{e.DataHoldingUserToken.TokenId}\"");
        }
    }
}
