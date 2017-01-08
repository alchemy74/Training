using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Training.SocketCore.SocketsEvent
{
    public class AuthenticateEvent
    {
        public EventType EventType { get; } = EventType.System;
        public string EventName { get; } = nameof(AuthenticateEvent);
        public string EventArgument { set; get; }
    }
}
