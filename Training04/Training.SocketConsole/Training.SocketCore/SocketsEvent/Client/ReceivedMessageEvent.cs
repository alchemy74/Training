using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Training.SocketCore.SocketsEvent
{
    public class ReceivedMessageEvent
    {
        public EventType EventType { get; } = EventType.System;
        public string EventName { get; } = nameof(ReceivedMessageEvent);
        /// <summary>
        /// 設定或取得事件參數
        /// </summary>
        public object EventArgument { set; get; }
    }
}
