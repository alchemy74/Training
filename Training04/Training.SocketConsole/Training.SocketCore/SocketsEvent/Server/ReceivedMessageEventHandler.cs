using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEC.Core.Sockets.Server;

namespace Training.SocketCore.SocketsEvent
{
    public class ReceivedMessageEventHandler
    {
        public EventType EventType { get; } = EventType.System;
        public string EventName { get; } = nameof(ReceivedMessageEvent);
        public void handleEvent(SocketListener<DataHolder> socketListener,int tokenId,object @event)
        {
            socketListener.sendDataAsync(tokenId, System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new SocketCore.SocketsEvent.ReceivedMessageEvent()
            {
                EventArgument = @event
            })));
        }
    }
}
