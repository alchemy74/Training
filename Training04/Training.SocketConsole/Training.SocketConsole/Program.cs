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
using Training.SocketCore.SocketsEvent.Server;

namespace Training.SocketConsole
{
    class Program
    {
        public static void Main()
        {
            SocketListener socketListener = new SocketListener(new IPEndPoint(IPAddress.Any, 9527));
            socketListener.RegisteredEventHandlers.Add(new ReceivedMessageEventHandler());
            socketListener.RegisteredEventHandlers.Add(new AuthenticateEventHandler());
            //socketListener.OnDataReceived += Program.SocketListener_OnDataReceived;
            Console.ReadLine();
        }

        //private static void SocketListener_OnDataReceived(object sender, SocketCore.EventArgs.ServerDataReceivedEventArgs e)
        //{
        //    ReceivedMessageEventHandler receivedMessageEventHandler = new ReceivedMessageEventHandler();
        //    receivedMessageEventHandler.handleEvent(sender as SocketListener, e.TokenId, e.Event);
        //}
    }
}
