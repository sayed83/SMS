using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcUMS.Models.ViewModels
{
    public class GPAViewModel
    {
        public string Range { get; set; }
        public decimal Grade { get; set; }
        public string GPA { get; set; }
    }
}