using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TEC.Core.Collections;
using TEC.Core.Sockets.Server;
using Training.SocketCore.SocketsEvent;
using Training.SocketCore.SocketsEvent.Server;

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
            this.RegisteredEventHandlers = new ThreadSafeObservableCollection<EventHandlerBase>();
            this.ConnectedMemberCollection = new ThreadSafeObservableCollection<Tuple<int, string>>();
            Mediator mediator = new Mediator();
            SocketServerSettingCollection socketSettingCollection = new SocketServerSettingCollection();
            socketSettingCollection[SocketServerSettingEnum.BackLog] = 100;
            socketSettingCollection[SocketServerSettingEnum.LocalEndPoint] = ipEndPoint;
            socketSettingCollection[SocketServerSettingEnum.MaxConnections] = 30;
            socketSettingCollection[SocketServerSettingEnum.MaxSimultaneousAcceptOperation] = 10;
            socketSettingCollection[SocketServerSettingEnum.MaxProcessingOperationCount] = 3000;
            socketSettingCollection[SocketServerSettingEnum.OperationBufferSize] = 25;
            this.SocketListenerInernal = new SocketListener<DataHolder>(mediator, socketSettingCollection, false);
            this.SocketListenerInernal.OnDataReceived += this.SocketListenerInernal_OnDataReceived;
            ((INotifyCollectionChanged)this.SocketListenerInernal.ReadOnlyConnectedTokenIdCollection).CollectionChanged += this.SocketListener_CollectionChanged;
            this.SocketListenerInernal.inital();
        }

        private void SocketListener_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                int tokenId = (int)e.OldItems[0];
                Tuple<int, string> tupleToRemove;
                lock (this.ConnectedMemberCollection)
                {
                    tupleToRemove = this.ConnectedMemberCollection.Where(t => t.Item1 == tokenId).FirstOrDefault();
                }
                 this.ConnectedMemberCollection.Where(t => t.Item1 == tokenId).FirstOrDefault();
                if (tupleToRemove != null)
                {
                    lock (this.ConnectedMemberCollection)
                    {
                        this.ConnectedMemberCollection.Remove(tupleToRemove);
                    }
                }
            }
        }

        private void SocketListenerInernal_OnDataReceived(object sender, TEC.Core.Sockets.Core.DataReceivedEventArgs e)
        {
            JObject jObject = JObject.Parse(System.Text.Encoding.UTF8.GetString(e.DataHolder.Data));
            string eventName = jObject.SelectToken("EventName").ToString();
            EventType eventType = jObject.SelectToken("EventType").ToObject<EventType>();
            List<EventHandlerBase> eventHandlerBaseToExecute = this.RegisteredEventHandlers
                 .Where(t => t.EventType == eventType && String.Compare(t.EventName, eventName, false) == 0)
                 .ToList();
            eventHandlerBaseToExecute.ForEach(t => t.handleEvent(this, e.TokenId, jObject.SelectToken("EventArgument")));
        }

        /// <summary>
        /// 非同步傳送資料
        /// </summary>
        /// <param name="tokenId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<int> sendDataAsync(int tokenId, string data)
        {
            return this.SocketListenerInernal.sendDataAsync(tokenId, System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new SocketCore.SocketsEvent.ReceivedMessageEvent()
            {
                EventArgument = data
            })));
        }

        private SocketListener<DataHolder> SocketListenerInernal { set; get; }
        public ThreadSafeObservableCollection<EventHandlerBase> RegisteredEventHandlers { private set; get; }
        /// <summary>
        /// 設定或取得已經連線的會員資訊集合
        /// </summary>
        public ThreadSafeObservableCollection<Tuple<int, string>> ConnectedMemberCollection { private set; get; }
    }
}
