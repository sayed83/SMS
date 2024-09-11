using MvcUMS.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace MvcUMS.Helpers
{
    public static class GlobalHelper
    {
        public static string DateConvert(DateTime date)
        {
            string[] formats = { "yyyy-MM-dd", "yy-MM-dd", "dd-MMM-yy", "dd-MMM-yyyy", "MM/dd/yy", "MM/dd/yyyy", "dd/MM/yy", "dd/MM/yyyy" };
            IFormatProvider provider = CultureInfo.InvariantCulture;
            DateTimeStyles style = DateTimeStyles.None;
            string dob = DateTime.ParseExact(date.ToString(), formats, provider, style).ToString("yyyy-MM-dd");
            return dob;
        }
        public enum RoleType
        {
            SuperAdmin,
            Admin,
            Guardian,
            Teacher,
            Student
        }
        public enum RoleId
        {
            Admin = 1,
            Teacher = 2,
            Student = 3,
            SuperAdmin = 4,
            Guardian = 5
        }
    }
}