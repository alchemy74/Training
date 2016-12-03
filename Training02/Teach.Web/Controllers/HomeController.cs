using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Teach.Core;
using Teach.Core.Infos;
using Teach.Core.Task;
using Teach.Web.Models.Home;
using TEC.Core.Text.Format;
using TEC.Core.Text.RandomText;
using TEC.Core.Transactions;

namespace Teach.Web.Controllers
{
    public class HomeController : Controller
    {
        private static SchedulerUIData schedulerUIData = new SchedulerUIData(TimerManagerConfig.TimerManager, ProducerAndConsumerMediatorConfig.ProducerAndConsumerMediator);
        static HomeController()
        {
            HomeController.schedulerUIData.createConsumerTimer().start();
            HomeController.schedulerUIData.createConsumerTimer().start();
            HomeController.schedulerUIData.createConsumerTimer().start();
            HomeController.schedulerUIData.createProducerTimer().start();
            HomeController.schedulerUIData.createProducerTimer().start();
            HomeController.schedulerUIData.createConsumerTimer().start();
            HomeController.schedulerUIData.createConsumerTimer().start();
            HomeController.schedulerUIData.createConsumerTimer().start();
            HomeController.schedulerUIData.createProducerTimer().start();
            HomeController.schedulerUIData.createProducerTimer().start();
            HomeController.schedulerUIData.createConsumerTimer().start();
            HomeController.schedulerUIData.createConsumerTimer().start();
            HomeController.schedulerUIData.createConsumerTimer().start();
            HomeController.schedulerUIData.createProducerTimer().start();
            HomeController.schedulerUIData.createProducerTimer().start();
            HomeController.schedulerUIData.createConsumerTimer().start();
            HomeController.schedulerUIData.createConsumerTimer().start();
            HomeController.schedulerUIData.createConsumerTimer().start();
            HomeController.schedulerUIData.createProducerTimer().start();
            HomeController.schedulerUIData.createProducerTimer().start();
            HomeController.schedulerUIData.createConsumerTimer().start();
            HomeController.schedulerUIData.createConsumerTimer().start();
            HomeController.schedulerUIData.createConsumerTimer().start();
            HomeController.schedulerUIData.createProducerTimer().start();
            HomeController.schedulerUIData.createProducerTimer().start();
        }
        // GET: Home
        public ActionResult Index()
        {
            List<BankAccountInfo> dataSource = new List<BankAccountInfo>()
            {
                new BankAccountInfo()
                {
                     Amount=500,
                     BankAccountId=Guid.NewGuid(),
                     BankUserId = Guid.NewGuid()
                },
                new BankAccountInfo()
                {
                     Amount=400,
                     BankAccountId=Guid.NewGuid(),
                     BankUserId = Guid.NewGuid()
                }
            };
            string firstLine = OrderedMemberCSVFormatter.serializeAttribute<BankAccountInfo, System.ComponentModel.DisplayNameAttribute>(t => t.DisplayName, ",");

            string multiRecordResult = firstLine + Environment.NewLine + String.Join(Environment.NewLine,
              dataSource.Select(t => OrderedMemberCSVFormatter.serializeObject(t, ",", (propertyInfo, value) =>
              {
                  if (propertyInfo.PropertyType == typeof(Guid))
                  {
                      //force to specific format
                      return ((Guid)value).ToString("N");
                  }
                  if (value == null)
                  {
                      return String.Empty;
                  }
                  //default to object string 
                  return value.ToString();
              })));

            //SequentialTransactionManager sequentialTransactionManager = new SequentialTransactionManager();
            //sequentialTransactionManager.EnlistmentNotificationCollection.Add(new LoginTransactionTask());
            ////sequentialTransactionManager.setContextValue("accountId", accountId);
            ////sequentialTransactionManager.setContextValue("loginDateTime", DateTime.Now);
            //using (TransactionScope transactionScope = new TransactionScope())
            //{
            //    sequentialTransactionManager.enlistVolatile();
            //    transactionScope.Complete();
            //}
            return View(new IndexModel()
            {
                TimerManager = TimerManagerConfig.TimerManager
            });
        }
        [HttpPost]
        public ActionResult Index(IndexModel indexModel)
        {

            return View(indexModel);
        }
        public ActionResult GetTimerStatusPagePartial()
        {
            System.Threading.Thread.Sleep(3000);
            return View("_TimerStatusTable", new Teach.Web.Models.Home.TimerStatusModel() { TimerManager = TimerManagerConfig.TimerManager });
        }
    }
}