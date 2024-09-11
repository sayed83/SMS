using MvcUMS.Models;
using MvcUMS.Models.EntityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MvcUMS.Controllers.api
{
    public class StudentController : ApiController
    {
        private StudentContext _context;
        StudentManager SM = new StudentManager();

        public StudentController ()
	    {
            _context = new StudentContext();
	    }

        public IHttpActionResult GetStudents()
        {
            var student = (from s in SM.getAll().ToList()
                           select new { s.Id, s.Std_id, Name = s.Std_FName + " " + s.Std_MName + " " + s.Std_LName, s.Contact_no }).ToList();

            return Ok(student);
        }
    }
}
