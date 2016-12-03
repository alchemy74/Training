using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEC.Core.Text.Format;

namespace Teach.Core.Infos
{
    public class BankAccountInfo
    {
        [OrderedFormatMember(0),DisplayName("帳號ID")]
        public Guid BankAccountId { set; get; }
        [OrderedFormatMember(1),DisplayName("使用者ID")]
        public Guid BankUserId { set; get; }
        [OrderedFormatMember(2),DisplayName("摳摳")]
        public decimal Amount { set; get; }
    }
}
