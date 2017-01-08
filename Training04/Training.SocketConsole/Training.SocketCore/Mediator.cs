using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TEC.Core.Sockets.Client;
using TEC.Core.Sockets.Server;

namespace Training.SocketCore
{
    /// <summary>
    /// 處理傳送及接收資料的中介類別，通常整個訊息處理結束準備送出資料時，都可以由此類別取得或設定相關資料。
    /// </summary>
    public class Mediator : IServerMediator<DataHolder>, IClientMediator<DataHolder>
    {
        #region Core
        /// <summary>
        /// 將要傳送的資料經過指定格式運算後，設定至[SocketAsyncEventArgs]以準備傳送作業
        /// </summary>
        /// <param name="dataToSend">要送出的原始資料</param>
        /// <param name="dataHolder">用於送出資料的[TEC.Core.Sockets.Server.DataHolderBase]</param>
        /// <param name="bufferSizeForEachSendOperation">傳送的緩稱區大小</param>
        public void prepareOutgoingData(byte[] dataToSend, DataHolder dataHolder, int bufferSizeForEachSendOperation, ref int messageId)
        {
            dataHolder.OriginalDataToSend = dataToSend;
            Encoding encoding = Encoding.UTF8;
            // 第一則
            // 1. 0-4 byte => 固定字 HEAD
            // 2. 5-8 byte => 訊息 ID
            // 3. 9-12 byte => 訊息總長度
            // 4. 13~ => 訊息內文
            // 第二則以後
            // 1. 0-4 byte => 訊息 ID
            // 2. 5~ => 訊息內文
            // 每一則訊息的緩衝區
            //指定此訊息的ID
            int currentMessgaeId = Interlocked.Increment(ref messageId);
            //儲存準備發送的資料
            List<byte> finalMessageList = new List<byte>();

            #region 處理Header
            //放入HEAD字串
            foreach (byte textByte in encoding.GetBytes("HEAD"))
            {
                finalMessageList.Add(textByte);
            }
            #endregion

            #region 計算總送出的位元組
            // 1. 第一則實際內文長度 = bufferSizeForEachSendOperation - 4 bytes(HEAD 字串) - 4bytes(Total Message Length) - 4bytes(ID)
            int firstContextLength = bufferSizeForEachSendOperation - 12;
            // 2. 非第一則實際內文長度 = bufferSizeForEachSendOperation -  4bytes(ID)
            int nonFirstContextLength = bufferSizeForEachSendOperation - 4;
            #endregion

            #region 開始填入位元組
            int currentHandleIndex = 0;
            byte[] messageIdByteArray = BitConverter.GetBytes(currentMessgaeId);
            //填入ID
            finalMessageList.AddRange(messageIdByteArray);
            //先塞空的，之後會補上訊息總長度
            finalMessageList.Add(0);
            finalMessageList.Add(0);
            finalMessageList.Add(0);
            finalMessageList.Add(0);
            //處理第一則訊息，輸入第一則訊息應包含的資料 (firstContextLength 位元組)
            for (; currentHandleIndex < firstContextLength && currentHandleIndex < dataToSend.Length; currentHandleIndex++)
            {
                finalMessageList.Add(dataToSend[currentHandleIndex]);
            }
            for (int currentIndex = 0; currentHandleIndex < dataToSend.Length; currentIndex++, currentHandleIndex++)
            {
                if (currentIndex == nonFirstContextLength)
                {
                    currentIndex = 0;
                }
                if (currentIndex == 0)
                {
                    //訊息的第一個 byte ，加入標頭
                    finalMessageList.AddRange(messageIdByteArray);
                }
                finalMessageList.Add(dataToSend[currentHandleIndex]);
            }
            #endregion

            #region 總長度計算
            byte[] lengthOfCurrentOutgoingMessageByteArray = BitConverter.GetBytes(dataToSend.Length);
            finalMessageList.RemoveRange(8, 4);
            finalMessageList.InsertRange(8, lengthOfCurrentOutgoingMessageByteArray);
            #endregion

            #region 計算填補字元(補上最後一則訊息不滿位元組的部分)
            int padingCount = bufferSizeForEachSendOperation - (finalMessageList.Count % bufferSizeForEachSendOperation);
            if (padingCount == bufferSizeForEachSendOperation)
            {
                padingCount = 0;
            }
            for (int index = 0; index < padingCount; index++)
            {
                finalMessageList.Add(0);
            }
            #endregion
            //設定要傳送的資料
            dataHolder.Data = finalMessageList.ToArray();
        }
        /// <summary>
        /// 處理接收到的資料，並傳回此訊息是否已接收完成。
        /// </summary>
        /// <param name="receivedData">已接收到的資料</param>
        /// <param name="dataHolder">儲存資料的物件參考</param>
        /// <returns><c>true</c>:訊息已接收完成；<c>false</c>:訊息尚未接收完成</returns>
        public bool handleData(byte[] receivedData, DataHolder dataHolder)
        {
            Encoding encoding = Encoding.UTF8;
            bool isHead = String.Compare(encoding.GetString(receivedData.Take(4).ToArray()), "HEAD", false) == 0;
            int messageId;
            if (isHead)
            {
                byte[] dataByteArray;
                try
                {
                    dataByteArray = new byte[BitConverter.ToInt32(receivedData.Skip(8).Take(4).ToArray(), 0)];
                    messageId = BitConverter.ToInt32(receivedData.Skip(4).Take(4).ToArray(), 0);
                }
                catch (Exception ex)
                {
                    //有發生錯誤則停止接收資料
                    dataHolder.LastError = ex;
                    return true;
                }
                dataHolder.switchData(messageId);
                dataHolder.Data = dataByteArray;
                //開始處理第一則訊息的內文
                dataHolder.LastWriteDataOffset = receivedData.Length - 12;
                if (dataHolder.LastWriteDataOffset > dataHolder.Data.Length)
                {
                    //第一則就已經是完整訊息
                    Buffer.BlockCopy(receivedData, 12, dataHolder.Data, 0, dataHolder.Data.Length);
                    return true;
                }
                Buffer.BlockCopy(receivedData, 12, dataHolder.Data, 0, dataHolder.LastWriteDataOffset);
                //回傳false以繼續接收新的資料
                return false;
            }
            else
            {
                //第二則內文的前4個byte是 ID
                try
                {
                    messageId = BitConverter.ToInt32(receivedData.Take(4).ToArray(), 0);
                }
                catch (Exception ex)
                {
                    //有發生錯誤則停止接收資料
                    dataHolder.LastError = ex;
                    return true;
                }
                messageId = BitConverter.ToInt32(receivedData.Take(4).ToArray(), 0);
                if (dataHolder.PendingDataDictionary.ContainsKey(messageId))
                {
                    dataHolder.switchData(messageId);
                }
                else if (dataHolder.MessageId != messageId)
                {
                    return true;
                }
                //計算本次應寫入多少位元組
                int bytesCountToWrite = receivedData.Length - 4;//扣掉ID 的 位元組陣列
                if (bytesCountToWrite > dataHolder.Data.Length - dataHolder.LastWriteDataOffset)
                {
                    //若有傳輸超過的狀況，捨棄多出來的位元組(已經溝通好多的部分補0的情況)
                    bytesCountToWrite = dataHolder.Data.Length - dataHolder.LastWriteDataOffset;
                }
                Buffer.BlockCopy(receivedData, 4, dataHolder.Data, dataHolder.LastWriteDataOffset, bytesCountToWrite);
                dataHolder.LastWriteDataOffset += bytesCountToWrite;
                if (dataHolder.LastWriteDataOffset >= dataHolder.Data.Length)
                {
                    //已接收完成
                    return true;
                }
                return false;
            }
        }
        #endregion
        #region ServerSide
        private int serverMessgaeId = 0;
        /// <summary>
        /// 將要傳送的資料經過指定格式運算後，設定至[SocketAsyncEventArgs]以準備傳送作業
        /// </summary>
        /// <param name="dataToSend">要送出的原始資料</param>
        /// <param name="dataHolder">用於送出資料的[TEC.Core.Sockets.Server.DataHolderBase]</param>
        /// <param name="readOnlySocketSettingDictionary">準備發送訊息的相關設定檔唯讀字典</param>
        public void prepareOutgoingData(byte[] dataToSend, DataHolder dataHolder, IReadOnlyDictionary<SocketServerSettingEnum, object> readOnlySocketSettingDictionary)
        {
            this.prepareOutgoingData(dataToSend, dataHolder, (int)readOnlySocketSettingDictionary[SocketServerSettingEnum.OperationBufferSize], ref this.serverMessgaeId);
        }
        /// <summary>
        /// 處理接收到的資料，並傳回此訊息是否已接收完成。
        /// </summary>
        /// <param name="receivedData">已接收到的資料</param>
        /// <param name="dataHolder">儲存資料的物件參考</param>
        /// <param name="readOnlySocketSettingDictionary">接收訊息的相關設定檔唯讀字典</param>
        /// <returns><c>true</c>:訊息已接收完成；<c>false</c>:訊息尚未接收完成</returns>
        public bool handleData(byte[] receivedData, DataHolder dataHolder, IReadOnlyDictionary<SocketServerSettingEnum, object> readOnlySocketSettingDictionary)
        {
            return this.handleData(receivedData, dataHolder);
        }
        #endregion
        #region ClientSide
        private int clientMessageId = 0;
        /// <summary>
        /// 將要傳送的資料經過指定格式運算後，設定至[SocketAsyncEventArgs]以準備傳送作業
        /// </summary>
        /// <param name="dataToSend">要送出的原始資料</param>
        /// <param name="dataHolder">用於送出資料的[TEC.Core.Sockets.Server.DataHolderBase]</param>
        /// <param name="readOnlySocketSettingDictionary">準備發送訊息的相關設定檔唯讀字典</param>
        public void prepareOutgoingData(byte[] dataToSend, DataHolder dataHolder, IReadOnlyDictionary<SocketClientSettingEnum, object> readOnlySocketSettingDictionary)
        {
            this.prepareOutgoingData(dataToSend, dataHolder, (int)readOnlySocketSettingDictionary[SocketClientSettingEnum.OperationBufferSize], ref this.clientMessageId);
        }
        /// <summary>
        /// 處理接收到的資料，並傳回此訊息是否已接收完成。
        /// </summary>
        /// <param name="receivedData">已接收到的資料</param>
        /// <param name="dataHolder">儲存資料的物件參考</param>
        /// <param name="readOnlySocketSettingDictionary">接收訊息的相關設定檔唯讀字典</param>
        /// <returns><c>true</c>:訊息已接收完成；<c>false</c>:訊息尚未接收完成</returns>
        public bool handleData(byte[] receivedData, DataHolder dataHolder, IReadOnlyDictionary<SocketClientSettingEnum, object> readOnlySocketSettingDictionary)
        {
            return this.handleData(receivedData, dataHolder);
        }
        #endregion
    }
}
