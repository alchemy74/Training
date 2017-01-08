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
    public class AuthenticateEventHandler : EventHandlerBase
    {
        public override EventType EventType { get; } = EventType.System;
        public override string EventName { get; } = nameof(AuthenticateEvent);
        public override void handleEvent(SocketListener socketListener, int tokenId, JToken @event)
        {
            Tuple<int, string> tuple;
            lock (socketListener.ConnectedMemberCollection)
            {
                tuple = socketListener.ConnectedMemberCollection.Where(t => t.Item1 == tokenId).FirstOrDefault();
                if (tuple != null)
                {
                    socketListener.ConnectedMemberCollection.Remove(tuple);
                }
                socketListener.ConnectedMemberCollection.Add(new Tuple<int, string>(tokenId, @event.ToString()));
            }
        }
    }
}
