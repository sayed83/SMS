using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcUMS.Models.ViewModels
{
    public class StudentDueListViewModel
    {
        public decimal TotalDue { get; set; }
        public string StdId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }

        public List<StudentInvoiceListViewModel> StudentInvoiceList { get; set; }
    }
}