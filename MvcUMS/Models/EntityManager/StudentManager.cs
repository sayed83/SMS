using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcUMS.Models.EntityManager
{
    public class StudentManager
    {
        UMSEntities1 _std = new UMSEntities1();

        public List<Student_info> getAll()
        {
            return _std.Student_info.ToList();
        }
    } 
}