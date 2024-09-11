using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcUMS.Models.ViewModels
{
    public class StudentProfileViewModel
    {
        public Student_info StudentInfoViewModel { get; set; }

        public SchoolViewModel SchoolViewModel { get; set; }
    }

    public class StudentInfoViewModel
    {
        public System.Guid Id { get; set; }
        public string Std_id { get; set; }
        public string Std_FName { get; set; }
        public string Std_MName { get; set; }
        public string Std_LName { get; set; }
        public string Father_Name { get; set; }
        public string Mother_Nmae { get; set; }
        public string Parmenent_address { get; set; }
        public string Present_address { get; set; }
        public string Picture { get; set; }
        public DateTime Birth_date { get; set; }
        public string Gender { get; set; }
        public string Contact_no { get; set; }
        public string Email_id { get; set; }

        public DateTime Admission_date { get; set; }
        public DateTime Entry_date { get; set; }
        public string status { get; set; }
        public string GardianName { get; set; }
        public string GardianMobile { get; set; }
        public string GardianOccupation { get; set; }
        public string GurdianRelation { get; set; }
        public string BeforeSchool { get; set; }
        public string BeforeClass { get; set; }
        public string BeforeGpa { get; set; }
        public int ProximateID { get; set; }
        public HttpPostedFileBase image { get; set; }
    }
}