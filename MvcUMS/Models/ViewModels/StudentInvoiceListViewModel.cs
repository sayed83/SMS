using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcUMS.Models.ViewModels
{
    public class StudentInvoiceListViewModel
    {
        public int Id { get; set; }
        public string InvoiceNo { get; set; }
        public string StudentName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PreviousDue { get; set; }
        public decimal DueAmount { get; set; }
        public string DueDate { get; set; }
        public string Std_id { get; set; }
        public string Department { get; set; }
        public int SemesterId { get; set; }
        public string Semester { get; set; }
        public string Section { get; set; }
        public int MonthId { get; set; }
        public string Month { get; set; }
        public int Year { get; set; }
        public string GurdianMobile { get; set; }
        public string Status
        {
            get
            {
                if (DueAmount==0)
                {
                    return "Paid";
                }
                else if (PaidAmount <=0)
                {
                    return "UnPaid";
                }
                else
                    return "Partially Paid";

            }
        }
    }


}