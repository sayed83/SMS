using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcUMS.Models.ViewModels
{
    public class MonthlyStudentPaymentSetupViewModel
    {
        public int Id { get; set; }
        public int SemesterId { get; set; }
        public int? DepartmentId { get; set; }
        public int MonthId { get; set; }
        public decimal Total { get; set; }
        public int Year { get; set; }
        public int[] PaymentTypeId { get; set; }
        public string[] PaymentType { get; set; }

        public decimal TotalAmount { get; set; }

        public List<StudentPaymentTypeViewModel> PaymentTypeViewModel { get; set; }
    }
}