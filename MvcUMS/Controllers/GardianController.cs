using MvcUMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace MvcUMS.Controllers
{
    public class GardianController : Controller
    {
        StudentContext sdb = new StudentContext();
        UMSEntities1 sdbs = new UMSEntities1();
        //
        // GET: /Gardian/

        /// <summary>
        /// Gurdian Details 
        /// </summary>
        /// <param name="Page_No"></param>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.advancedsearch = "advancedsearch";
            var student = sdbs.Student_info.ToList();
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            return View();
        }

        [HttpPost]
        public ActionResult GurdianList( string Std_id,string Department, string Semester, string Section)
        {
           if(Department!="Nine" || Department!="Ten"|| Department!="Eleven" || Department!="Twelve")
           {
               Department = "";
           };

            if (Std_id!="")
            {
                var student = from si in sdbs.Student_info.ToList()
                              join ca in sdb.CurrentAcademicInfo.ToList() on si.Std_id equals ca.Std_id
                              where si.Std_id.Equals(Std_id)
                              select new Student_info
                              {
                                  Std_FName = si.Std_FName,
                                  Std_MName = si.Std_MName,
                                  Std_LName = si.Std_LName,
                                  GardianName = si.GardianName,
                                  GardianMobile = si.GardianMobile,
                                  GardianOccupation = si.GardianOccupation,
                                  GurdianRelation = si.GurdianRelation
                              };
                TempData["GuardianList"] = student.ToList();
                return View(student.ToList());
            }
            else if (Department != "" && Semester == "" && Section == "" )
            {
                var student = from si in sdbs.Student_info.ToList()
                              join ca in sdb.CurrentAcademicInfo.ToList() on si.Std_id equals ca.Std_id
                              where ca.Department.Equals(Department)
                              select new Student_info
                              {
                                  Std_FName=si.Std_FName,
                                  Std_MName=si.Std_MName,
                                  Std_LName=si.Std_LName,
                                  GardianName = si.GardianName,
                                  GardianMobile = si.GardianMobile,
                                  GardianOccupation = si.GardianOccupation,
                                  GurdianRelation = si.GurdianRelation
                              };
                TempData["GuardianList"] = student.ToList();
                return View(student.ToList());
            }
           
            else if (Department == "" && Semester != "" && Section == "")
            {
                var student = from si in sdbs.Student_info.ToList()
                              join ca in sdb.CurrentAcademicInfo.ToList() on si.Std_id equals ca.Std_id
                              where ca.Semester.Equals(Semester)
                              select new Student_info {
                                  Std_FName = si.Std_FName,
                                  Std_MName = si.Std_MName,
                                  Std_LName = si.Std_LName,
                                           GardianName=si.GardianName,
                                           GardianMobile=si.GardianMobile,
                                           GardianOccupation=si.GardianOccupation
                                         };

                TempData["GuardianList"] = student.ToList();
                return View(student.ToList());
            }
            else if (Department == "" && Semester != "" && Section != "")
            {
                var student = from si in sdbs.Student_info.ToList()
                              join ca in sdb.CurrentAcademicInfo.ToList() on si.Std_id equals ca.Std_id
                              where ca.Section.Equals(Section) && ca.Semester.Equals(Semester)
                              select new Student_info
                              {
                                  Std_FName = si.Std_FName,
                                  Std_MName = si.Std_MName,
                                  Std_LName = si.Std_LName,
                                  GardianName = si.GardianName,
                                  GardianMobile = si.GardianMobile,
                                  GardianOccupation = si.GardianOccupation,
                                  GurdianRelation = si.GurdianRelation
                              };
                TempData["GuardianList"] = student.ToList();
                return View(student.ToList());
            }
            else if (Department != "" && Semester != "" && Section == "")
            {
                var student = from si in sdbs.Student_info.ToList()
                              join ca in sdb.CurrentAcademicInfo.ToList() on si.Std_id equals ca.Std_id
                              where ca.Department.Equals(Department) && ca.Semester.Equals(Semester)
                              select new Student_info
                              {
                                  Std_FName = si.Std_FName,
                                  Std_MName = si.Std_MName,
                                  Std_LName = si.Std_LName,
                                  GardianName = si.GardianName,
                                  GardianMobile = si.GardianMobile,
                                  GardianOccupation = si.GardianOccupation,
                                  GurdianRelation = si.GurdianRelation
                              };
                TempData["GuardianList"] = student;
                return View(student.ToList());
            }
            else if (Department != "" && Semester != "" && Section != "")
            {
                var student = from si in sdbs.Student_info.ToList()
                              join ca in sdb.CurrentAcademicInfo.ToList() on si.Std_id equals ca.Std_id
                              where ca.Section.Equals(Section) && ca.Semester.Equals(Semester) && ca.Department.Equals(Department)
                              select new Student_info
                              {
                                  Std_FName = si.Std_FName,
                                  Std_MName = si.Std_MName,
                                  Std_LName = si.Std_LName,
                                  GardianName = si.GardianName,
                                  GardianMobile = si.GardianMobile,
                                  GardianOccupation = si.GardianOccupation,
                                  GurdianRelation = si.GurdianRelation
                              };

                TempData["GuardianList"] = student.ToList();
                return View(student.ToList());
            }
            else
            {
                var student = sdbs.Student_info.ToList();
                TempData["GuardianList"] = student.ToList();
                return View(student.ToList());
            }
            
        }

        public RootObject SmsSend(string sender, string to, string message)
        {
            using (var http = new HttpClient())
            {
                // Define authorization headers here, if any
                http.DefaultRequestHeaders.Add("Authorization", "Basic Z2xvYmFsc29mdDpnbG9iYWxzb2Z0dGVjaG5vbG9neQ==");

                var data = new RootObject
                {
                    from = sender,
                    to = to,
                    text = message
                };

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var request = http.PostAsync("http://107.20.199.106/restapi/sms/1/text/single", content);

                var response = request.Result.Content.ReadAsStringAsync().Result;

                return JsonConvert.DeserializeObject<RootObject>(response);
            }
        }

        [HttpPost]
        public ActionResult GuardianSendSMS(string txtMsg)
        {
            bool status = false;
            var list = (List<Student_info>)TempData["GuardianList"];
            //RootObject results = SmsSend();
            foreach (var item in list)
            {
                string mobilenumber = "+88" + Convert.ToString(item.GardianMobile);
                
                string sender = "InfoSMS";
                RootObject results = SmsSend(sender, mobilenumber, txtMsg);

                status = true;

            }

            if (status == true)
                TempData["success"] = "Message Send successfully.";

            TempData["success"] = "Message Failed";

            return new JsonResult { Data = new { status = status } };
        }

    }
}
