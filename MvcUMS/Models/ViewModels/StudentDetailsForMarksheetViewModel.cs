using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcUMS.Models.ViewModels
{
    public class StudentDetailsForMarksheetViewModel
    {
        public string Name { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string StdId { get; set; }
        public string Class { get; set; }
        public int Roll { get; set; }
        public string Photo { get; set; }
        public string Exam { get; set; }
        public int Session { get; set; }
        public string Section { get; set; }
        public string Group { get; set; }
    }
}