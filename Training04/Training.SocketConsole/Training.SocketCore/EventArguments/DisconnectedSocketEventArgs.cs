using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Training.SocketCore.EventArguments
{
    /// <summary>
    /// 當 Socket 連線已關閉時引發事件的參數
    /// </summary>
    public class DisconnectedSocketEventArgs : EventArgs
    {
        public DisconnectedSocketEventArgs(int tokenId)
        {
            this.TokenId = tokenId;
        }
        public int TokenId { private set; get; }
    }
}
