using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcUMS.Models.ViewModels
{
    public class StudentPaymentAmountViewModel
    {
        public int Id { get; set; }

        public decimal? Amount { get; set; }

        public int PaymentTypeId { get; set; }
        public string Name { get; set; }

        public int SemesterId { get; set; }
        public string Semester { get; set; }

        public int? DepartmentId { get; set; }
        public string Department { get; set; }
    }
}