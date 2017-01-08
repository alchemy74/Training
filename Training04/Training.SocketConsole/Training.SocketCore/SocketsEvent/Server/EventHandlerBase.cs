using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Training.SocketCore.SocketsEvent.Server
{
    public abstract class EventHandlerBase
    {
        public abstract EventType EventType { get; }
        public abstract string EventName { get; }
        public abstract void handleEvent(SocketListener socketListener, int tokenId, JToken @event);
    }
}
