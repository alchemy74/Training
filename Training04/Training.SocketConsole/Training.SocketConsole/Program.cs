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
            socketListener.ConnectedMemberCollection.CollectionChanged += Program.ConnectedMemberCollection_CollectionChanged;
            //socketListener.OnDataReceived += Program.SocketListener_OnDataReceived;
            Console.ReadLine();
        }

        private static void ConnectedMemberCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                Tuple<int, string> tuple = e.NewItems[0] as Tuple<int, string>;
                Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 已對應會員 Token，Token ID: {tuple.Item1}，會員 ID: {tuple.Item2}");
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                Tuple<int, string> tuple = e.OldItems[0] as Tuple<int, string>;
                Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}- 已中斷會員連線 Token，Token ID: {tuple.Item1}，會員 ID: {tuple.Item2}");
            }
        }

        //private static void SocketListener_OnDataReceived(object sender, SocketCore.EventArgs.ServerDataReceivedEventArgs e)
        //{
        //    ReceivedMessageEventHandler receivedMessageEventHandler = new ReceivedMessageEventHandler();
        //    receivedMessageEventHandler.handleEvent(sender as SocketListener, e.TokenId, e.Event);
        //}
    }
}
