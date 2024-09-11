using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcUMS.Models.ViewModels
{
    public class StudentListViewModel
    {
        public Guid StudentId { get; set; }
        public int SemesterId { get; set; }
        public int DeptId { get; set; }
        public string StdId { get; set; }
        public string Name { get; set; }
        public bool IsChecked { get; set; }
    }
}