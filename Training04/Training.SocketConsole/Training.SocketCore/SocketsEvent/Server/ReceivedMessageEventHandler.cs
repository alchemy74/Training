using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEC.Core.Sockets.Server;

namespace Training.SocketCore.SocketsEvent.Server
{
    public class ReceivedMessageEventHandler : EventHandlerBase
    {
        public override EventType EventType { get; } = EventType.System;
        public override string EventName { get; } = nameof(ReceivedMessageEvent);
        public override void handleEvent(SocketListener socketListener, int tokenId, JToken @event)
        {
            
            Console.WriteLine(@event);
        }
    }
}
