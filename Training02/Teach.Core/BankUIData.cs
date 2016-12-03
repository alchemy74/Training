using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Teach.Core.Converters.Bank;
using Teach.Core.Infos;
using Teach.Core.Task;
using TEC.Core.Transactions;
using Teach.Util.Extensions;
using Teach.Core.Converters;
using Autofac;

namespace Teach.Core
{
    public class BankUIData
    { 
        public BankUIData(BankAccountConverter bankAccountConverter, BankUserConverter bankUserConverter)
        {
            this.BankAccountConverter = bankAccountConverter;
            this.BankUserConverter = bankUserConverter;
        }
        /// <summary>
        /// 測試用的 FOR AutoMapper
        /// </summary>
        public void testAutoMapper()
        {
            this.BankAccountConverter.convertBack(
             this.BankAccountConverter.convert(new Adapter.Entites.BankAccountEntity()
             {
                 Amount = 50m,
                 BankAccountId = Guid.NewGuid(),
                 BankUserId = Guid.NewGuid()
             }));
            this.BankUserConverter.convertBack(
                   this.BankUserConverter.convert(new Adapter.Entites.BankUserEntity()
                   {
                       UserName = "Antony",
                       LastLoginDate = null,
                       BankUserId = Guid.NewGuid()
                   }));
            this.BankAccountConverter.convertBack(
           this.BankAccountConverter.convert(new Adapter.Entites.BankAccountEntity()
           {
               Amount = 50m,
               BankAccountId = Guid.NewGuid(),
               BankUserId = Guid.NewGuid()
           }));
            this.BankUserConverter.convertBack(
            this.BankUserConverter.convert(new Adapter.Entites.BankUserEntity()
            {
                UserName = "Antony",
                LastLoginDate = null,
                BankUserId = Guid.NewGuid()
            }));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bankUserInfo"></param>
        /// <param name="bankAccountInfoCollection"></param>
        public void addBankUserAndAccount(BankUserInfo bankUserInfo, IEnumerable<BankAccountInfo> bankAccountInfoCollection)
        {
            using (TransactionScope transactionScope = new TransactionScope())
            {
                UIDataConfig.DefaultDataAdapterFactory.BankUserDb.addBankUser(new BankUserConverter().convertBack(bankUserInfo));
                bankAccountInfoCollection
                    .Select(t => this.BankAccountConverter.convertBack(t))
                    .ToList()
                    .ForEach(t => UIDataConfig.DefaultDataAdapterFactory.BankAccountDb.addBankAccount(t));
                transactionScope.Complete();
            }
        }
        public List<BankUserInfo> getBankUserInfos()
        {
            BankUserConverter bankUserConverter = new BankUserConverter();
            return UIDataConfig.DefaultDataAdapterFactory.BankUserDb.getBankUsers()
                .Select(t => bankUserConverter.convert(t))
                .ToList();
        }
        public BankUserInfo getBankUserInfos(string userName)
        {
            return this.getBankUserInfos().FirstOrDefault(t => String.Compare(t.UserName, userName, true) == 0);
        }
        public void setLastLoginDate(Guid userId, DateTime lastLoginDate)
        {
            UIDataConfig.DefaultDataAdapterFactory.BankUserDb.updateLastLoginDate(userId, lastLoginDate);
        }
        public void loginAccount(Guid accountId)
        {
            SequentialTransactionManager sequentialTransactionManager = new SequentialTransactionManager();
            sequentialTransactionManager.EnlistmentNotificationCollection.Add(new LoginTransactionTask());
            sequentialTransactionManager.setContextValue(accountId);
            sequentialTransactionManager.setContextValue(DateTime.Now);
            //sequentialTransactionManager.setContextValue("accountId", accountId);
            //sequentialTransactionManager.setContextValue("loginDateTime", DateTime.Now);
            using (TransactionScope transactionScope = new TransactionScope())
            {
                sequentialTransactionManager.enlistVolatile();
                transactionScope.Complete();
            }
        }
        private BankAccountConverter BankAccountConverter { set; get; }
        private BankUserConverter BankUserConverter { set; get; }
    }
}
