using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcUMS.Models.ViewModels
{
    public class MonthlyStudentPaymentListViewModel
    {
        public string Semester { get; set; }
        public int? DepartmentId { get; set; }
        public string Department { get; set; }
        public string Month { get; set; }
        public int Year { get; set; }
        public decimal Total { get; set; }
    }
}