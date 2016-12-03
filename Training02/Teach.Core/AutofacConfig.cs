using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teach.Core
{
    public static class AutofacConfig
    {
        public static void configAutofac()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterType<BankUIData>().InstancePerLifetimeScope();
            builder.RegisterType<Converters.Bank.BankAccountConverter>();
            //builder.RegisterType(typeof(Converters.Bank.BankAccountConverter));
            builder.RegisterType<Converters.Bank.BankUserConverter>();
            AutofacConfig.CoreContainer = builder.Build();

        }
        public static IContainer CoreContainer { private set; get; }
    }
}
