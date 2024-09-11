using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcUMS.Models.ViewModels
{
    public class MarksheetViewModel
    {
        public SchoolViewModel SchoolViewModel { get; set; }
        public List<GPAViewModel> GPAViewModels { get; set; }
        public StudentDetailsForMarksheetViewModel StudentDetailsForMarksheetViewModel { get; set; }
        public List<SubjectMarksCourse> SubjectMarksCourses { get; set; }
        public List<ResultReport> ResultReport { get; set; }
        public IEnumerable<int> Postition { get; set; }
    }
}