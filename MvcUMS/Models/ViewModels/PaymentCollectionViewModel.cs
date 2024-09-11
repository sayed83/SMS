using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcUMS.Models.ViewModels
{
    public class PaymentCollectionViewModel
    {
        public int Id { get; set; }

        public decimal? Amount { get; set; }
        public int PaymentMethodId { get; set; }
        public string PaymentMehtod { get; set; }
        public int BankId { get; set; }
        public string CheckNo { get; set; }
        public DateTime CheckDate { get; set; }
        public string PayDate { get; set; }
    }
}