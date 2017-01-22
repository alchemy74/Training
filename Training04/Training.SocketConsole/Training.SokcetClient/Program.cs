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
        public static void Main()
        {
            Program.TimerManager = new TimerManager();
            SocketClient socketClient = new SocketClient();
            Program.KeepConnectionTimerEvent = new KeepConnectionTimerEvent(socketClient, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9527));
            Program.TimersTimerStorage = Program.TimerManager.createTimersTimer(Program.KeepConnectionTimerEvent,
                  new TimePeriodCollection(new[] { new TimeRange(DateTime.Now, DateTime.MaxValue) }), null, 2000, NextTimeEvaluationType.ExecutionEndTime);
            Program.TimersTimerStorage.start();
            while (true)
            {
                string data = Console.ReadLine();
                if (!Program.KeepConnectionTimerEvent.TokenId.HasValue)
                {
                    Console.WriteLine("Socket server has not connected.");
                    continue;
                }
                socketClient.sendDataAsync(Program.KeepConnectionTimerEvent.TokenId.Value, data);
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

        private static TimerManager TimerManager { set; get; }
        private static KeepConnectionTimerEvent KeepConnectionTimerEvent { set; get; }
        private static TimersTimerStorage TimersTimerStorage { set; get; }
    }
}
