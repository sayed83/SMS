using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcUMS.Models.ViewModels
{
    public class CreateStudentInvoiceViewModel
    {
        public int Id { get; set; }
        public string InvoiceNo { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public decimal Discount { get; set; }
        public int PaymentMethodId { get; set; }
        public int BankId { get; set; }
        public string CheckNo { get; set; }
        public string CheckDate { get; set; }
        public int MonthlyPaymentSetupId { get; set; }
        public string DueDate { get; set; }
        public Guid StudentId { get; set; }
    }
}