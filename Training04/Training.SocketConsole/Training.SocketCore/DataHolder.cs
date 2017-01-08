using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEC.Core.Sockets.Core;

namespace Training.SocketCore
{
    /// <summary>
    /// 儲存傳送/接收資料所使用的儲存物件
    /// </summary>
    public class DataHolder : DataHolderBase
    {
        public DataHolder()
        {
            this.PendingDataDictionary = new Dictionary<int, Tuple<byte[], int>>();
        }
        /// <summary>
        /// 將目前的資料切換至以[MessageId]為主的資料，若有資料得切換則傳回<c>true</c>；無法切換則傳回<c>false</c>
        /// </summary>
        /// <returns></returns>
        public void switchData(int targetMessageId)
        {
            if (this.MessageId.HasValue && this.MessageId.Value == targetMessageId)
            {
                //一樣的資料，不轉換
                return;
            }
            if (!this.MessageId.HasValue)
            {
                //第一次建立
                this.MessageId = targetMessageId;
                base.Data = null;
                this.LastWriteDataOffset = 0;
                return;
            }
            lock (this.PendingDataDictionary)
            {
                this.PendingDataDictionary.Add(this.MessageId.Value, new Tuple<byte[], int>(this.Data, this.LastWriteDataOffset));
                if (!this.PendingDataDictionary.ContainsKey(targetMessageId))
                {
                    //將目前資料移至字典中暫存，並將Data清空以接受新的資料
                    this.MessageId = targetMessageId;
                    this.LastWriteDataOffset = 0;
                    base.Data = null;
                    return;
                }
                this.MessageId = targetMessageId;
                Tuple<byte[], int> tuple = this.PendingDataDictionary[this.MessageId.Value];
                base.Data = tuple.Item1;
                this.LastWriteDataOffset = tuple.Item2;
                this.PendingDataDictionary.Remove(targetMessageId);
            }
        }
        /// <summary>
        /// 設定或取得訊息ID，若尚未設定過則為<c>null</c>參考
        /// </summary>
        public int? MessageId { private set; get; }
        /// <summary>
        /// 設定或取得最後發生的錯誤資料，若無則為<c>null</c>。
        /// </summary>
        public Exception LastError { set; get; }
        /// <summary>
        /// 設定或取得最後寫入的資料位元組位置，紀錄以便下一個資料進入時能夠繼續寫入
        /// </summary>
        public int LastWriteDataOffset { set; get; }
        /// <summary>
        /// 設定或取得尚未經過訊息切割的訊息位元組陣列
        /// </summary>
        public byte[] OriginalDataToSend { set; get; }
        /// <summary>
        /// 取得暫存資料的字典，目的用於同時傳輸多個ID的資料時，需要互相切換使用。
        /// </summary>
        public Dictionary<int, Tuple<byte[], int>> PendingDataDictionary { set; get; }
    }
}
