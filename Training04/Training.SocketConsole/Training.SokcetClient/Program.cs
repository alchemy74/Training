using Itenso.TimePeriod;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TEC.Core.Scheduler.Timers;
using TEC.Core.Sockets.Client;
using TEC.Core.Sockets.Core;
using Training.SocketCore;
using Training.SokcetClient.TimerEvents;

namespace Training.SokcetClient
{
    class Program
    {
        private static object connectData = new object();
        public static void Main()
        {
            Program.TimerManager = new TimerManager();
            SocketClient socketClient = new SocketClient();
            Program.TimerManager.createTimersTimer(new KeepConnectionTimerEvent(()=>!Program.TokenId.HasValue,
                ()=> socketClient.connectToServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9527), Program.connectData)),
                new TimePeriodCollection(new[] { new TimeRange(DateTime.Now, DateTime.MaxValue) }),
                null, 2000, NextTimeEvaluationType.ExecutionEndTime).start();
            socketClient.OnConnected += Program.SocketClient_OnConnected;
            socketClient.OnDisconnected += Program.SocketClient_OnDisconnected;
            while (true)
            {
                string data = Console.ReadLine();
                if (!Program.TokenId.HasValue)
                {
                    Console.WriteLine("Socket server has not connected.");
                    continue;
                }
                socketClient.sendDataAsync(Program.TokenId.Value, data);
            }
        }

        private static void SocketClient_OnDisconnected(object sender, SocketCore.EventArguments.DisconnectedSocketEventArgs e)
        {
            if (Program.TokenId.HasValue && e.TokenId == Program.TokenId)
            {
                Program.TokenId = null;
            }
        }

        private static void SocketClient_OnConnected(object sender, ConnectedEventArgs e)
        {
            if (e.ConnectOperationUserToken.Data == Program.connectData)
            {
                SocketClient socketClient = sender as SocketClient;
                Program.TokenId = e.DataHoldingUserToken.TokenId;
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
        private static TimerManager TimerManager { set; get; }
    }
}
