using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TEC.Core.Scheduler.Timers;
using TEC.Core.Sockets.Client;
using Training.SocketCore;

namespace Training.SokcetClient.TimerEvents
{
    public class KeepConnectionTimerEvent : ITimerEvent
    {
        public KeepConnectionTimerEvent(Func<bool> getShouldReconnectFunc, Action reconnectAction)
        {
            this.GetShouldReconnectFunc = getShouldReconnectFunc;
            this.ReconnectAction = reconnectAction;
        }

        public void execute()
        {
            if (this.GetShouldReconnectFunc())
            {
                this.ReconnectAction();
            }
        }
        public void Dispose()
        {

        }
        /// <summary>
        /// 設定或取得 取得是否應該重新連線的方法封裝
        /// </summary>
        private Func<bool> GetShouldReconnectFunc { set; get; }
        /// <summary>
        /// 設定或取得 重新連線的方法
        /// </summary>
        private Action ReconnectAction { set; get; }
    }
}
