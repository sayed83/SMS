using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcUMS.Models.ViewModels
{
    public class InvoiceDetailsViewModel
    {
        public int Id { get; set; }
        public string InvoiceNo { get; set; }
        public string StudentName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PreviousDue { get; set; }
        public decimal DueAmount { get; set; }
        public string DueDate { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public int? Session { get; set; }

        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "Invalid Number")]
        [Remote("IsAmountGreater", "Payment", ErrorMessage = "Pay Amount is greater then due amount", AdditionalFields = "Id")]
        public decimal Amount { get; set; }
        public string Status
        {
            get
            {
                if (TotalAmount == PaidAmount)
                {
                    return "Paid";
                }
                else if (TotalAmount == DueAmount)
                {
                    return "UnPaid";
                }
                else
                    return "Partially Paid";
                
            }
        }

        public SchoolViewModel SchoolViewModel { get; set; }

        public List<StudentPaymentTypeViewModel> PaymentTypeList { get; set; }
        public List<PaymentCollectionViewModel> Collections { get; set; }
    }
}