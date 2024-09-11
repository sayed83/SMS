using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcUMS.Models.ViewModels
{
    public class StudentPaymentTypeViewModel
    {
        public int Id { get; set; }

        public string PaymentType { get; set; }
        public int? PaymentTypeId { get; set; }
        public decimal Amount { get; set; }
        public int? MonthId { get; set; }
        public List<Month> PayMonths { get; set; }
        public decimal MonthlyFeesToPay { get; set; }

        public bool? isActive { get; set; }

        public bool IsTypeChecked { get; set; }
    }
}