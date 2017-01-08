using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TEC.Core.Sockets.Core;
using TEC.Core.Sockets.Server;
using Training.SocketCore;
using Training.SocketCore.SocketsEvent;

namespace Training.SocketConsole
{
    class Program
    {
        private static SocketServerSettingCollection socketSettingCollection = new SocketServerSettingCollection();
        public static void Main()
        {
            try
            {
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 9527);
                WriteInfoToConsole(localEndPoint);
                Mediator mediator = new Mediator();
                Program.socketSettingCollection[SocketServerSettingEnum.BackLog] = 100;
                Program.socketSettingCollection[SocketServerSettingEnum.LocalEndPoint] = localEndPoint;
                Program.socketSettingCollection[SocketServerSettingEnum.MaxConnections] = 30;
                Program.socketSettingCollection[SocketServerSettingEnum.MaxSimultaneousAcceptOperation] = 10;
                Program.socketSettingCollection[SocketServerSettingEnum.MaxProcessingOperationCount] = 3000;
                Program.socketSettingCollection[SocketServerSettingEnum.OperationBufferSize] = 25;
                SocketListener<DataHolder> socketListener = new SocketListener<DataHolder>(mediator, Program.socketSettingCollection, false);
                socketListener.OnAccepted += Program.SocketListener_OnAccepted;
                socketListener.OnBadAccept += Program.SocketListener_OnBadAccept;
                socketListener.OnClosingClientSocket += Program.SocketListener_OnClosingClientSocket;
                socketListener.OnDataReceived += Program.SocketListener_OnDataReceived;
                socketListener.OnReceivedMessage += Program.SocketListener_OnReceivedMessage;
                socketListener.OnReceiveEmptyData += Program.SocketListener_OnReceiveEmptyData;
                socketListener.OnReceiveError += Program.SocketListener_OnReceiveError;
                socketListener.OnDataSent += Program.SocketListener_OnDataSent;
                socketListener.OnSendCompleted += Program.SocketListener_OnSendCompleted;
                socketListener.OnSendError += Program.SocketListener_OnSendError;
                socketListener.OnStartingAccept += Program.SocketListener_OnStartingAccept;
                socketListener.OnStartingReceive += Program.SocketListener_OnStartingReceive;
                socketListener.OnStartingSend += Program.SocketListener_OnStartingSend;
                ((System.Collections.Specialized.INotifyCollectionChanged)socketListener.ReadOnlyConnectedTokenIdCollection).CollectionChanged += Program.SocketListenerTest_CollectionChanged;
                socketListener.inital();
                while (true)
                {
                    string data = Console.ReadLine();
                    if (String.Compare(data, "D", true) == 0)
                    {
                        //斷線
                        socketListener.ReadOnlyConnectedTokenIdCollection.AsParallel().ForAll(tokenId =>
                        {
                            socketListener.disconnect(tokenId);
                        });
                        continue;
                    }
                    socketListener.ReadOnlyConnectedTokenIdCollection.AsParallel().ForAll(tokenId =>
                    {
                        //傳送訊息至客戶端
                        Console.WriteLine($"已佇列發送訊息，發送的訊息 Token 為: {socketListener.sendDataAsync(tokenId, System.Text.Encoding.UTF8.GetBytes(data)).Result}");
                    });
                    Console.WriteLine("已傳送完成");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private static void SocketListenerTest_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            System.Collections.ObjectModel.ReadOnlyObservableCollection<int> connectedTokenIdCollection = sender as System.Collections.ObjectModel.ReadOnlyObservableCollection<int>;
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 已連接的客戶端發生變更，目前共管理\"{connectedTokenIdCollection.Count}\"個 Token");
            Console.WriteLine($"   Token: {String.Join(",", connectedTokenIdCollection)}");
        }
        private static void SocketListener_OnStartingSend(object sender, DataHoldingEventArgs e)
        {
            DataHolder dataHolder = e.DataHoldingUserToken.DataHolder as DataHolder;
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 以 Token ID: {e.DataHoldingUserToken.TokenId} 準備送資料給客戶端({e.DataHoldingUserToken.RemoteEndpoint})。");
            Console.WriteLine($"   資料:{System.Text.Encoding.UTF8.GetString(dataHolder.OriginalDataToSend) }");
            Console.WriteLine($"   (已處理)資料:{String.Join(" ", dataHolder.Data) }");
        }
        private static void SocketListener_OnStartingReceive(object sender, DataHoldingEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 以 Token ID: {e.DataHoldingUserToken.TokenId} 準備接受客戶端({e.DataHoldingUserToken.RemoteEndpoint})的資料。");
        }
        private static void SocketListener_OnStartingAccept(object sender, AcceptingEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 以 Accept Token ID: {e.AcceptOperationUserToken.TokenId} 的接收通訊端準備接受客戶端的 Accept 作業。");
        }
        private static void SocketListener_OnSendError(object sender, SocketErrorEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 傳送作業發生錯誤，Token ID: {e.UserToken.TokenId}，原因: {e.SocketError.ToString()}");
        }
        private static void SocketListener_OnDataSent(object sender, DataHoldingEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 已傳送完整訊息，Token ID: {e.DataHoldingUserToken.TokenId}，訊息: { System.Text.Encoding.UTF8.GetString(e.DataHoldingUserToken.DataHolder.Data)}");
        }
        private static void SocketListener_OnSendCompleted(object sender, DataHoldingEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- Send 操作已完成，Token ID: {e.DataHoldingUserToken.TokenId}");
        }
        private static void SocketListener_OnReceiveError(object sender, SocketErrorEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 接收作業發生錯誤，Token ID: {e.UserToken.TokenId}，原因: {e.SocketError.ToString()}");
        }
        private static void SocketListener_OnReceiveEmptyData(object sender, DataHoldingEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 接收到空白資料，Token ID: {e.DataHoldingUserToken.TokenId}");
        }
        private static void SocketListener_OnReceivedMessage(object sender, DataHoldingEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 已完成一次 Receive 作業，目前結果:  {(e.DataHoldingUserToken.DataHolder.Data == null ? "尚未建立或已傳送完畢" : Encoding.UTF8.GetString(e.DataHoldingUserToken.DataHolder.Data))}");
        }
        private static void SocketListener_OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            JObject jObject = JObject.Parse(System.Text.Encoding.UTF8.GetString(e.DataHolder.Data));
            string args = jObject.SelectToken("EventArgument").ToString();
            ReceivedMessageEventHandler receivedMessageEventHandler = new ReceivedMessageEventHandler();
            receivedMessageEventHandler.handleEvent(sender as SocketListener<DataHolder>, e.TokenId, args);
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 已接收到一次完整訊息，資料: {(e.DataHolder.Data == null ? "尚未建立" : System.Text.Encoding.UTF8.GetString(e.DataHolder.Data))}，來源:{e.RemoteEndPoint}");
        }
        private static void SocketListener_OnClosingClientSocket(object sender, DataHoldingEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 已關閉連線，Token ID: {e.DataHoldingUserToken.TokenId}");
        }
        private static void SocketListener_OnBadAccept(object sender, SocketErrorEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- Accept 發生錯誤，Token ID: {e.UserToken.TokenId}");
        }
        private static void SocketListener_OnAccepted(object sender, AcceptedEventArgs e)
        {
            Console.WriteLine($"已接受連入連線，Token ID: {e.DataHoldingUserToken.TokenId}(來自 Accept Token ID: {e.AcceptOperationUserToken.TokenId})");
        }
        public static void WriteInfoToConsole(IPEndPoint localEndPoint)
        {
            Console.WriteLine("緩衝區大小 = " + Program.socketSettingCollection[SocketServerSettingEnum.OperationBufferSize].ToString());
            Console.WriteLine("最大客戶端同時連線數量 = " + Program.socketSettingCollection[SocketServerSettingEnum.MaxConnections].ToString());
            Console.WriteLine("最大同時處理傳送/接受作業的連線數量 = " + Program.socketSettingCollection[SocketServerSettingEnum.MaxProcessingOperationCount].ToString());
            Console.WriteLine("等待連接最大上限 = " + Program.socketSettingCollection[SocketServerSettingEnum.BackLog].ToString());
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("本機接聽位址 = " + IPAddress.Parse(((IPEndPoint)localEndPoint).Address.ToString()) + ": " + ((IPEndPoint)localEndPoint).Port.ToString());
            Console.WriteLine("伺服器名稱 = " + Environment.MachineName);
            Console.WriteLine();
            Console.WriteLine("請注意防火牆是否已經開啟相對應的埠號");
            Console.WriteLine();
        }
    }
}
