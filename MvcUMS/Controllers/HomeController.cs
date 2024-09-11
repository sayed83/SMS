using CrystalDecisions.CrystalReports.Engine;
using MvcUMS.Models;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using WebMatrix.WebData;
using PagedList;
using MvcEnergyPac.Models;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Web.Security;
using MvcEnergyPac.Filters;
using MvcUMS.Filters;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Data.Entity.Validation;
using MvcUMS.Models.EntityManager;
using System.Globalization;
using MvcUMS.Models.ViewModels;
using System.Data.Entity.Infrastructure;
using static MvcUMS.Helpers.GlobalHelper;

namespace MvcUMS.Controllers
{

    [Authorize]
    [InitializeSimpleMembership]
    public class HomeController : Controller
    {
        StudentContext sdb = new StudentContext();
        UMSEntities1 sdbs = new UMSEntities1();
        StudentPayment db = new StudentPayment();
        UMSEntities2 stdAttan = new UMSEntities2();
        StudentAttendenceContex matt = new StudentAttendenceContex();
        private PaymentContext paymentcontext = new PaymentContext();
        private ScolarshipContext ssc = new ScolarshipContext();
        UsersContext uc = new UsersContext();
        EventContext evc = new EventContext();
        AccountingContext acdb = new AccountingContext();
        SchooldbEntities d = new SchooldbEntities();
        private StuffContext sc = new StuffContext();

        StudentManager SM = new StudentManager();
        CommonManager CM = new CommonManager();
        [AllowAnonymous]
        public ActionResult FrontPage()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult FrontEvents()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult FrontGallery()
        {
            return View();
        }

        public ActionResult BulkAdmission()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            ViewBag.Section = new SelectList(sdb.Section, "section_name", "section_name");
            return View();
        }

        public ActionResult ImportBulkStudent()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            ViewBag.Section = new SelectList(sdb.Section, "section_name", "section_name");
            return View();
        }

        public ActionResult DownloadCsv(string Semester, string Section)
        {
            IEnumerable<Student_info> students = SM.getAll();
            string facsCsv = GetCsvString(students);
            string fileName = "Cls_" + Semester + "_" + "Sec_" + Section + ".csv";

            // Return the file content with response body. 
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);
            Response.Write(facsCsv);
            Response.End();
            return File(new System.Text.UTF8Encoding().GetBytes(facsCsv), "text/csv", fileName);
        }

        private string GetCsvString(IEnumerable<Student_info> students)
        {
            StringBuilder csv = new StringBuilder();

            csv.AppendLine("FirstName,MiddleName,LastName,FatherName,MotherName,GuardianName,DateOfBirth,Gender,ContactNo,GuardianContact,RollNo,ProximateID,AcademicYear");

            csv.AppendLine($"Test,Test1,Test2,father,mother,guardian,01-jan-2000,male,01829322883,01829322883,1,1,01-jan-2000");

            //foreach (Student_info std in students)
            //{
            //    csv.Append(std.StdNameEnglish + ",");
            //    csv.Append(std.StdFatherNameEnglish + ",");
            //    csv.Append(std.StdMotherNameEnglish + ",");
            //    csv.Append(std.Religion + ",");
            //    csv.AppendLine();
            //}

            return csv.ToString();
        }

        [HttpPost]
        public ActionResult Import(HttpPostedFileBase FileUpload, string Semester, string Department, string Section)
        {
            try
            {
                //Check if there is a file is sent to the controller
                if (FileUpload != null && FileUpload.ContentLength > 0)
                {
                    // Check if the file ends with csv extenstion
                    if (FileUpload.FileName.EndsWith(".csv"))
                    {
                        // Instance of EF Class;
                        UMSEntities1 db = new UMSEntities1();
                        Validation_CVS csvValidate = new Validation_CVS(); //Instance of Validation Class

                        // Read the file as a stream
                        StreamReader streamCsv = new StreamReader(FileUpload.InputStream);

                        string csvDataLine = ""; int CurrentLine = 0;
                        string[] PersonData = null;

                        // Delete Data each time import the file
                        //db.Database.ExecuteSqlCommand("TRUNCATE TABLE Students");

                        #region while loop
                        // Looping to read the File stream and Add Data to the database line by line
                        while ((csvDataLine = streamCsv.ReadLine()) != null)
                        {
                            // Ignore First Line of the file where columns names
                            if (CurrentLine != 0)
                            {
                                // Validate File Data and normalize it
                                //csvDataLine = csvValidate.Validate(csvDataLine);

                                // Add the returned data to an array
                                PersonData = csvDataLine.Split(',');


                                string studentid = "";
                                string adminsiondate = DateTime.Now.Year.ToString();



                                int classid = sdb.Semester.Where(x => x.Semester_name == Semester).Select(x => x.Id).FirstOrDefault();
                                string section = sdb.Section.Where(x => x.section_name == Section).Select(x => x.id).FirstOrDefault().ToString();
                                string department = sdb.Department.Where(x => x.Dep_name == Department).Select(x => x.Id).FirstOrDefault().ToString();


                                if (classid > 10)
                                {
                                    studentid = adminsiondate + classid.ToString() + department + section + PersonData[10];
                                }
                                else
                                {
                                    studentid = adminsiondate + classid.ToString() + section + PersonData[10];
                                }

                                string date = DateTime.Now.ToShortDateString();
                                //string dob = DateTime.ParseExact(PersonData[6], "dd-MMM-yy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                                var bdate = PersonData[6];
                                string[] formats = { "yyyy-MM-dd","yy-MM-dd", "dd-MMM-yy", "dd-MMM-yyyy","MM/dd/yy","MM/dd/yyyy","dd/MM/yy","dd/MM/yyyy" };
                                IFormatProvider provider = CultureInfo.InvariantCulture;
                                DateTimeStyles style = DateTimeStyles.None;
                                string dob = DateTime.ParseExact(bdate, formats, provider, style).ToString("yyyy-MM-dd");

                                var ct = PersonData[8];
                                if (ct.Substring(0, 1) != "0")
                                    ct = "0" + PersonData[8];
                                else
                                    ct = PersonData[8];

                                var gphone = PersonData[9];
                                if (gphone.Substring(0, 1) != "0")
                                    gphone = "0" + PersonData[9];
                                else
                                    gphone = PersonData[9];
                                var schoolid = (int)Session["SchoolId"];
                                // Pass PersonData Array values to the person object
                                var newPersonData = new Student_info
                                {
                                    Id = Guid.NewGuid(),
                                    Std_id = studentid,
                                    Std_FName = PersonData[0],
                                    Std_MName = PersonData[1],
                                    Std_LName = PersonData[2],
                                    Father_Name = PersonData[3],
                                    Mother_Nmae = PersonData[4],
                                    GardianName = PersonData[5],
                                    Birth_date = DateTime.Parse(dob),
                                    Gender = PersonData[7],
                                    Contact_no = ct,
                                    GardianMobile = gphone,
                                    Admission_date = DateTime.Parse(date),
                                    Entry_date = DateTime.Now,
                                    status = "Active",
                                    SchoolId = schoolid
                                };

                                var accyear = PersonData[12].Split('-')[2];

                                if (accyear.Length == 2) 
                                {
                                    var acyear = Convert.ToInt32(accyear);
                                    var curyear = DateTime.Now.Year;
                                    var y = acyear <= curyear ? 19 : 20;
                                    accyear = y + accyear;
                                }                                

                                var currAcInfo = new CurrentAcademicInfo
                                {
                                    Std_id = studentid,
                                    RollNo = int.Parse(PersonData[10]),
                                    ProximateID = int.Parse(PersonData[11]),
                                    Semester = Semester,
                                    Department = string.IsNullOrEmpty(Department) ? null : Department,
                                    Section = Section,
                                    Status = "Regular",
                                    AcademicYear = int.Parse(accyear)
                                };

                                // Add Data to The Database
                                db.Student_info.Add(newPersonData);
                                sdb.CurrentAcademicInfo.Add(currAcInfo);
                                db.SaveChanges();
                                sdb.SaveChanges();
                            }
                            CurrentLine += 1;
                        }
                        #endregion

                    }
                    else
                        TempData["MessageError"] = "File Format is not Supported";
                }
                else
                    TempData["MessageError"] = "Please Add File";

                // Back to the first view
                return RedirectToAction("Student_list", "Home");
            }
            catch (Exception ex)
            {
                TempData["MessageError"] = "Error:" + ex.Message + " Date Formate is dd/MM/yy";
                return RedirectToAction("ImportBulkStudent", "Home");
            }
        }        

        [HttpGet]
        public ActionResult Index()
        {            
            ViewBag.dashboard = "dashboard";
            ViewBag.classes = sdb.Semester.ToList().Count();
            ViewBag.students = sdb.CurrentAcademicInfo.ToList().Count();
            ViewBag.teachers = sc.Stuff.Where(x => x.Department == 1 && x.active == true).Count();
            ViewBag.Management = uc.ManagementProfile.Count();
            return View();

        }
        [HttpGet]
        public JsonResult getdatachart(string da)
        {
            List<Chartview> sd = new List<Chartview>();

            DateTime date = Convert.ToDateTime(da).Date;

            var bookisue = from sa in matt.StudentAttendances.ToList()
                           join c in sdb.Course.ToList() on sa.Course_id equals c.CourseId
                           join cl in sdb.Semester.ToList() on c.Semester equals cl.Semester_name
                           where (sa.AttendanceDate.Equals(date) && sa.IsPresent.Equals(true) && sa.Isfirst.Equals(true))
                           orderby cl.Id
                           group c by new { c.Semester, cl.Id } into g
                           select new Chartview
                           {
                               className = g.Key.Semester,
                               TotalPresent = g.Count(),
                               Total = sdb.CurrentAcademicInfo.Where(a => a.Semester.Equals(g.Key.Semester)).Count()

                           };

            sd = bookisue.ToList();
            var chartData = new object[sd.Count];
            int j = 0;
            foreach (var i in sd)
            {
                chartData[j] = new object[] { i.className, i.TotalPresent, i.Total };
                j++;
            }
            return Json(chartData, JsonRequestBehavior.AllowGet);
        }

        //[AuthorizeRoles("Admin")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult Student_entry()
        {
            ViewBag.last = sdbs.Student_info.OrderByDescending(t => t.Entry_date).Take(1).Select(a => a.Std_id).FirstOrDefault();
            return View();
        }
        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult User_List()
        {
           var schoolid = (int)Session["SchoolId"];
            ViewBag.UserList = from up in uc.UserProfiles.ToList()
                               join inrole in uc.webpages_UsersInRoles.ToList() on up.UserId equals inrole.UserId
                               join r in uc.webpages_Roles.ToList() on inrole.RoleId equals r.RoleId
                               where up.SchoolId == schoolid
                               select new StudenInputViewModel { };
            return View();
        }

        //[AuthorizeRoles("Admin")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public ActionResult Student_entry(Models.Student_info S)
        {
            if (ModelState.IsValid)
            {

                string filename = "";

                if (S.image != null && S.image.ContentLength > 0)
                {
                    filename = Path.GetFileName(Guid.NewGuid() + "." + S.image.FileName.Split('.')[1]);
                    string targetPath = Server.MapPath("../uploads//" + filename);
                    Stream strm = S.image.InputStream;
                    var targetFile = targetPath;

                    GenerateThumbnails(0.5, strm, targetFile);
                }
                var schoolid = (int)Session["SchoolId"];
                S.Id = Guid.NewGuid();
                S.Picture = filename;
                S.Entry_date = DateTime.Now;
                S.status = "Active";
                S.SchoolId = schoolid;
                string st = S.GardianName;

                using (var db = new MvcUMS.Models.UMSEntities1())
                {
                    var exist_id = db.Student_info.Where(a => a.Std_id.Equals(S.Std_id)).FirstOrDefault();


                    db.Student_info.Add(S);
                    db.SaveChanges();

                    TempData["saved"] = "Data succsessfully saved";


                    return RedirectToAction("CreateCurentAcaInfo", "Home", new { id = S.Id });
                }
            }
            return View(S);
        }

        public ActionResult CheckRegID(string term)
        {
            int t = sdbs.Student_info.Where(a => a.Std_id.Equals(term)).Count();
            return Json(t, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getData(string term)
        {

            var users = new List<Models.Student_info>();
            using (UMSEntities1 dc = new UMSEntities1())
            {
                users = dc.Student_info.Where(item => item.Std_id.Contains(term.ToLower())).Take(10).ToList();
            }
            return Json(users, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _StudentDashboard()
        {
            var stdId = User.Identity.Name;
            var stdInfo = from std in sdbs.Student_info.ToList()
                          join curInfo in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals curInfo.Std_id
                          where std.Std_id == stdId
                          select new StudentPaytypeStudentinfo { studentinfo = std, currentacademicinfo = curInfo };
            return View(stdInfo.FirstOrDefault());
        }


        public ActionResult _TeacherDashboard()
        {
            var teacherId = User.Identity.Name;
            var teacherInfo = from t in sc.Stuff.ToList()
                              join d in sc.Designition.ToList() on t.Department equals d.DepatmentId
                              where t.UserName == teacherId
                              select new stuffFullDetails { Stuff = t, Designition = d };

            return View(teacherInfo.FirstOrDefault());
        }

        //[HttpPost]
        [CheckSessionTimeOut]
        public ActionResult StudentSearch(Guid Std_id1)
        {
            ViewBag.advancedsearch = "advancedsearch";
            TempData["Password"] = Session["Password"];
            Models.Student_info S;
            using (UMSEntities1 dc = new UMSEntities1())
            {
                S = dc.Student_info.Where(a => a.Id.Equals(Std_id1)).FirstOrDefault();
                TempData["isActive"] = S.status.ToString().Trim();
            }

            var viewModel = new StudentProfileViewModel
            {
                StudentInfoViewModel = S,
                SchoolViewModel = CM.GetSchoolInfo()
            };

            return View(viewModel);
        }

        [CheckSessionTimeOut]
        public ActionResult StudentDetails(string Std_id1 = null)
        {
            ViewBag.advancedsearch = "advancedsearch";
            TempData["Password"] = Session["Password"];
            Models.Student_info S;
            using (UMSEntities1 dc = new UMSEntities1())
            {
                S = dc.Student_info.Where(a => a.Std_id.Equals(Std_id1)).FirstOrDefault();
                TempData["isActive"] = S.status.ToString().Trim();
            }

            var viewModel = new StudentProfileViewModel
            {
                StudentInfoViewModel = S,
                SchoolViewModel = CM.GetSchoolInfo()
            };

            return View("StudentSearch", viewModel);
        }

        [CheckSessionTimeOut]
        public ActionResult StudentDetals(Guid id)
        {
            ViewBag.advancedsearch = "advancedsearch";
            TempData["Password"] = Session["Password"];
            Models.Student_info S;
            using (UMSEntities1 dc = new UMSEntities1())
            {
                S = dc.Student_info.Where(a => a.Id.Equals(id)).FirstOrDefault();
                TempData["isActive"] = S.status.ToString().Trim();
            }


            var viewModel = new StudentProfileViewModel
            {
                StudentInfoViewModel = S,
                SchoolViewModel = CM.GetSchoolInfo()
            };

            return View("StudentSearch", viewModel);
        }

        //[CheckSessionTimeOut]
        //public ActionResult StudentDetals(Guid id)
        //{
        //    ViewBag.advancedsearch = "advancedsearch";
        //    TempData["Password"] = Session["Password"];
        //    Models.ViewModels.StudentInfoViewmodel S;

        //    using (UMSEntities1 dc = new UMSEntities1())
        //    {
        //        S = dc.Student_info.Where(a => a.Id.Equals(id)).FirstOrDefault();
        //        TempData["isActive"] = S.status.ToString().Trim();
        //    }


        //    var viewModel = new StudentProfileViewModel
        //    {
        //        StudentInfoViewModel = S,
        //        SchoolViewModel = CM.GetSchoolInfo()
        //    };

        //    return View("StudentSearch", viewModel);
        //}

        public ActionResult StudentProfilePrint()
        {
            return View();
        }

        public ActionResult ChangeCurentAcaInfo(string id = null)
        {
            Models.CurrentAcademicInfo info;
            string classname = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Equals(id) && x.RollNo == 0).Select(x => x.Semester).FirstOrDefault();
            int classid = sdb.Semester.Where(x => x.Semester_name == classname).Select(x => x.Id).FirstOrDefault();
            using (var dc = new StudentContext())
            {
                info = dc.CurrentAcademicInfo.Where(a => a.Std_id.Equals(id)).FirstOrDefault();
                ViewBag.Department = new SelectList(dc.Department.ToList(), "Dep_name", "Dep_name", info.Department);
                ViewBag.Semester = new SelectList(dc.Semester.Where(x => x.Id == classid || x.Id == classid + 1).ToList(), "Semester_name", "Semester_name", info.Semester);
                ViewBag.Status = dc.Status.ToList();
            }
            return View(info);
        }


        [HttpPost]
        public ActionResult ChangeCurentAcaInfo(CurrentAcademicInfo info)
        {
            var Id = sdbs.Student_info.Where(x => x.Std_id == info.Std_id).Select(x => x.Id).FirstOrDefault();
            if (ModelState.IsValid)
            {

                using (var db = new StudentContext())
                {
                    if (info.Semester == "One" || info.Semester == "Two" || info.Semester == "Three" || info.Semester == "Four" || info.Semester == "Five" || info.Semester == "Six"
                      || info.Semester == "Seven" || info.Semester == "Eight")
                    {
                        info.Department = null;
                    }

                    db.Entry(info).State = EntityState.Modified;
                    db.SaveChanges();
                    Session["Stdid"] = info.Std_id;
                    TempData["Promotion"] = "Promoted";
                }

            }
            return RedirectToAction("StudentDetals", new { id = Id });
        }

        public ActionResult EditProximateId(string id)
        {

            var curStd = sdb.CurrentAcademicInfo.Where(x => x.Std_id == id).FirstOrDefault();
            if (curStd == null)
                return HttpNotFound();

            return View(curStd);
        }

        [HttpPost]
        public ActionResult EditProximateId(CurrentAcademicInfo model)
        {
            var Id = sdbs.Student_info.Where(x => x.Std_id == model.Std_id).Select(x => x.Id).FirstOrDefault();
            var curStd = sdb.CurrentAcademicInfo.Where(x => x.Std_id == model.Std_id).FirstOrDefault();
            if (curStd == null)
                return HttpNotFound();
            curStd.RollNo = model.RollNo;
            curStd.ProximateID = model.ProximateID;

            var roll = sdb.CurrentAcademicInfo.Where(s => s.Semester == model.Semester && s.Section == model.Section && s.Std_id != model.Std_id).Select(x => x.RollNo).ToList();
            var proId = sdb.CurrentAcademicInfo.Where(s => s.Semester == model.Semester && s.Section == model.Section && s.Std_id != model.Std_id).Select(x => x.ProximateID).ToList();
            bool rExists = roll.Any(x => x.Equals(model.RollNo));
            bool pExists = proId.Any(x => x.Equals(model.ProximateID));
            if (rExists && pExists)
            {
                TempData["ProError"] = "Data Already Exists!Please Try Again";
            }
            else if (rExists)
            {
                TempData["ProError"] = "Data Already Exists!Please Try Again";
            }
            else if (pExists)
            {
                TempData["ProError"] = "Data Already Exists!Please Try Again";
            }
            else
            {
                sdb.Entry(curStd).Property(x => x.ProximateID).IsModified = true;
                sdb.Entry(curStd).Property(x => x.RollNo).IsModified = true;
                sdb.SaveChanges();
                TempData["ProSuccess"] = "Data Successfully Updated!";
            }


            return RedirectToAction("StudentDetals", new { id = Id });
        }

        public ActionResult CreateCurentAcaInfo(string id = null)
        {
            ViewBag.studentscripts = "specificscripts";
            CurrentAcademicInfo info = new CurrentAcademicInfo();
            using (var dc = new StudentContext())
            {
                ViewBag.Department = dc.Department.ToList();
                ViewBag.Status = dc.Status.ToList();
                var v = dc.CurrentAcademicInfo.Where(a => a.Std_id.Equals(id)).FirstOrDefault();
                if (v == null)
                {
                    ViewBag.Semester = dc.Semester.ToList();
                    ViewBag.Id = id;
                }
                else
                {
                    ViewBag.Semester = dc.Semester.ToList();
                    info = v;
                }
            }
            return View(info);
        }

        [HttpPost]
        public ActionResult CreateCurentAcaInfo(CurrentAcademicInfo info, Guid? stdId)
        {
            Guid? stdudentId = Guid.Empty;
            if (stdId == null)
            {
                if (ModelState.IsValid)
                {
                    using (var db = new StudentContext())
                    {
                        if (info.Semester == "One" || info.Semester == "Two" || info.Semester == "Three" || info.Semester == "Four" || info.Semester == "Five" || info.Semester == "Six"
                            || info.Semester == "Seven" || info.Semester == "Eight")
                        {
                            info.Department = null;
                        }
                        var Id = sdbs.Student_info.Where(x => x.Std_id == info.Std_id).Select(x => x.Id).FirstOrDefault();
                        db.CurrentAcademicInfo.Add(info);
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException e)
                        {

                            Console.WriteLine(e);
                        }
                        Session["Stdid"] = info.Std_id;
                        stdudentId = Id;
                    }
                    return RedirectToAction("StudentDetals", new { id = stdudentId });
                }
            }
            else
            {
                string rollno = "";
                string studentid = "";
                string adminsiondate = sdbs.Student_info.Where(x => x.Id == stdId).Select(x => x.Admission_date).FirstOrDefault()?.Year.ToString();
                int classid = sdb.Semester.Where(x => x.Semester_name == info.Semester).Select(x => x.Id).FirstOrDefault();
                string section = sdb.Section.Where(x => x.section_name == info.Section).Select(x => x.id).FirstOrDefault().ToString();
                string department = sdb.Department.Where(x => x.Dep_name == info.Department).Select(x => x.Id).FirstOrDefault().ToString();
                if (classid > 9)
                {
                    rollno = info.RollNo.ToString("D3");
                }
                else
                {
                    rollno = info.RollNo.ToString("D4");
                }
                if (classid > 10)
                {
                    studentid = adminsiondate + classid.ToString() + department + section + rollno;
                }
                else
                {
                    studentid = adminsiondate + classid.ToString() + section + rollno;
                }
                info.Std_id = studentid;
                var isexist = WebSecurity.UserExists(studentid);
                if (isexist)
                {
                    using (var dc = new StudentContext())
                    {
                        ViewBag.Department = dc.Department.ToList();
                        ViewBag.Status = dc.Status.ToList();
                        var v = dc.CurrentAcademicInfo.Where(a => a.Std_id.Equals(studentid)).FirstOrDefault();
                        if (v == null)
                        {
                            ViewBag.Semester = dc.Semester.ToList();
                            ViewBag.Id = studentid;
                        }
                        else
                        {
                            ViewBag.Semester = dc.Semester.ToList();
                            info = v;
                        }
                    }
                    TempData["Message"] = "Academic Info Alredy Added";
                    return View(info);
                }
                sdb.CurrentAcademicInfo.Add(info);
                sdb.SaveChanges();
                var stdinfo = sdbs.Student_info.Where(x => x.Id == stdId).FirstOrDefault();
                stdinfo.Std_id = studentid;
                
                WebSecurity.CreateUserAndAccount(studentid, info.Password);
                var userdata = $"Student UserId: {studentid} Pass: {info.Password}";
                //SaveTxtData(userdata);
                var userdetails = uc.UserProfiles.Where(a => a.UserName.Equals(studentid)).FirstOrDefault();
                userdetails.SchoolId = (int)Session["SchoolId"];
                uc.Entry(userdetails).State = EntityState.Modified;
                webpages_UsersInRoles usrRoles = new webpages_UsersInRoles();
                UserProfile up = new UserProfile();                
                usrRoles.UserId = userdetails.UserId;
                var rolename = RoleType.Student.ToString();
                var roleid = uc.webpages_Roles.Where(r => r.RoleName == rolename).FirstOrDefault().RoleId;
                usrRoles.RoleId = roleid;
                uc.webpages_UsersInRoles.Add(usrRoles);
                uc.SaveChanges();
                sdbs.Entry(stdinfo).State = EntityState.Modified;
                sdbs.SaveChanges();
                Session["Stdid"] = studentid;
                stdudentId = stdId;

                //sms send after admission

                string mobilenumber = "+88" + Convert.ToString(stdinfo.Contact_no);
                string msg = "Thanks You. Your Admission Successfully Completed. Your ID is: " + studentid + " And Password is: " + info.Password;
                string sender = "TSTSMS";
                RootObject results = SmsSend(sender, mobilenumber, msg);

                TempData["success"] = "Message Send successfully.";

            }
            return RedirectToAction("StudentDetals", new { id = stdudentId });
        }



        #region Student Promotion

        public ActionResult StdPromotion()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            ViewBag.Section = new SelectList(sdb.Section, "section_name", "section_name");
            return View();
        }


        [HttpPost]
        public PartialViewResult StdListForPromotion(string Department, string Semester, string Section)
        {
            var student = new List<Models.StudentListForPromotion>();
            if (Department == "" || Department == null)
            {
                var studentlist = from s in sdbs.Student_info.ToList()
                                  join c in sdb.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                                  where c.Semester.Equals(Semester, StringComparison.OrdinalIgnoreCase)
                                  && c.Section.Equals(Section, StringComparison.OrdinalIgnoreCase)
                                  orderby s.Std_id
                                  select new StudentListForPromotion
                                  {
                                      Std_id = s.Std_id,
                                      RollNo = c.RollNo,
                                      FullName = s.Std_FName + " " + s.Std_MName + " " + s.Std_LName,
                                      Std_FName = s.Std_FName,
                                      Std_MName = s.Std_MName,
                                      Std_LName = s.Std_LName,
                                      Department = c.Department,
                                      Semester = c.Semester,
                                      Section = c.Section,

                                  };
                student = studentlist.ToList();

            }
            else
            {
                var studentlist = from s in sdbs.Student_info.ToList()
                                  join c in sdb.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                                  where c.Semester.Equals(Semester, StringComparison.OrdinalIgnoreCase) && c.Department.Equals(Department, StringComparison.OrdinalIgnoreCase) && c.Section.Equals(Section, StringComparison.OrdinalIgnoreCase)
                                  orderby s.Std_id
                                  select new StudentListForPromotion
                                  {
                                      Std_id = s.Std_id,
                                      Std_FName = s.Std_FName,
                                      Std_MName = s.Std_MName,
                                      Std_LName = s.Std_LName,
                                      Department = c.Department,
                                      Semester = c.Semester,
                                      Section = c.Section,

                                  };
                student = studentlist.ToList();

            }

            int m = student.Count();
            if (m > 0)
            {
                ViewBag.c = "t";
            }
            else
            {
                ViewBag.c = "f";
            }

            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            ViewBag.Section = new SelectList(sdb.Section, "section_name", "section_name");

            return PartialView(student);
        }

        [HttpPost]
        public ActionResult Promotion(List<StudentListForPromotion> model, string Department, string Semester, string Section, int AcademicYear)
        {
            foreach (var item in model)
            {

                if (item.IsPromotion == true)
                {
                    var stu = sdb.CurrentAcademicInfo.Where(x => x.Std_id == item.Std_id).FirstOrDefault();

                    if (Semester == "One" || Semester == "Two" ||
                    Semester == "Three" || Semester == "Four" ||
                    Semester == "Five" || Semester == "Six" ||
                    Semester == "Seven" || Semester == "Eight")
                    {
                        stu.Department = null;
                    }
                    else if (Semester == "Nine" || Semester == "Ten")
                    {
                        stu.Department = Department;
                    }
                    stu.Semester = Semester;
                    stu.Section = Section;
                    stu.AcademicYear = AcademicYear;
                    stu.RollNo = item.RollNo;
                    sdb.Entry(stu).State = EntityState.Modified;
                    sdb.SaveChanges();
                }


            }

            //sdb.CurrentAcademicInfo(model, SemesterId, PSemesterId, PromotionTypeId, AcademicYear);
            return RedirectToAction("StdPromotion");
        }


        #endregion

        //sms method


        //public Result SmsResult(string msgsender, string destinationaddr, string message)
        //{
        //    string url;
        //    string serverResult = "";
        //    //long l;
        //    //int resultvaule = 1;
        //    Result result;
        //    try
        //    {

        //        string apikey = "3591FF7C93435B";
        //        //string username = "swopnil.it";
        //        //string password = "swopnil.it";

        //        url = "http://xsms.dianahost.com/app/smsapi/index.php?"
        //        + "key=" + HttpUtility.UrlEncode(apikey) + "&routeid=100015" + "&type=text"
        //        + "&contacts=" + HttpUtility.UrlEncode(destinationaddr)
        //        + "&senderid=" + HttpUtility.UrlEncode(msgsender)
        //        + "&msg=" + HttpUtility.UrlEncode(message);


        //        //if (long.TryParse(msgsender, out l))
        //        //{
        //        //    url = url + "&sourceAddr=" + msgsender;
        //        //}
        //        //else
        //        //{
        //        //    url = url + "&fromAlpha=" + msgsender;
        //        //}

        //        serverResult = DownloadString(url);
        //    }
        //    catch (Exception ex)
        //    {

        //    }


        //    result = ParseServerResult(serverResult);


        //    return result;
        //}


        //private string DownloadString(string URL)
        //{
        //    using (System.Net.WebClient wlc = new System.Net.WebClient())
        //    {
        //        // Create WebClient instanse.
        //        try
        //        {
        //            // Download and return the xml response
        //            return wlc.DownloadString(URL);
        //        }
        //        catch (WebException ex)
        //        {
        //            // Failed to connect to server. Throw an exception with a customized text.
        //            throw new WebException("Error occurred while connecting to server. " + ex.Message, ex);
        //        }
        //    }
        //}

        //private Result ParseServerResult(string ServerResult)
        //{
        //    System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
        //    System.Xml.XmlNode ack;
        //    Result result = new Result();
        //    //xDoc.LoadXml(ServerResult);
        //    //ack = xDoc.GetElementsByTagName("ack")[0];
        //    //result.errorcode = int.Parse(ack.Attributes["errorcode"].Value);
        //    //result.errormessage = ack.InnerText;
        //    //result.success = (result.errorcode == 0);
        //    return result;
        //}


        public ActionResult GetRendomRollNo(string sem, string sec)
        {
            var schoolid = (int)Session["SchoolId"];
            int t = 0;
            int count = sdb.CurrentAcademicInfo.Where(a => a.Semester.Equals(sem) && a.Section.Equals(sec)).Count();
            if (count == 0)
            {
                t = 1;
            }
            else
            {
                var rollno = (from s in sdbs.Student_info.ToList()
                             join a in sdb.CurrentAcademicInfo on s.Std_id equals a.Std_id
                             where a.Semester.Equals(sem) && a.Section.Equals(sec) && s.SchoolId.Equals(schoolid)
                             select a.RollNo).DefaultIfEmpty().Max();
                t = rollno + 1;

                //t = sdb.CurrentAcademicInfo.Where(a => a.Semester.Equals(sem) && a.Section.Equals(sec)).Max(a => a.RollNo) + 1;
            }
            return Json(t, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckRollNo(string sem, string sec, int term)
        {
            int t = sdb.CurrentAcademicInfo.Where(a => a.Semester.Equals(sem) && a.Section.Equals(sec) && a.RollNo.Equals(term) && a.RollNo != 0).Count();
            return Json(t, JsonRequestBehavior.AllowGet);
        }

        public ActionResult StudentPass(string id = null)
        {
            var v = sdb.CurrentAcademicInfo.Where(a => a.Std_id.Equals(id)).FirstOrDefault();
            StudentPreviousAcademicInfo stp = new StudentPreviousAcademicInfo();
            stp.Department = v.Department;
            stp.RollNo = v.RollNo;
            stp.Semester = v.Semester;
            stp.Section = v.Section;
            stp.Status = v.Status;
            stp.StdId = v.Std_id;
            return View(stp);
        }

        [HttpPost]
        public ActionResult StudentPass(StudentPreviousAcademicInfo stp)
        {
            CurrentAcademicInfo v = sdb.CurrentAcademicInfo.Where(a => a.Std_id.Equals(stp.StdId)).FirstOrDefault();
            var sId = sdbs.Student_info.Where(x => x.Std_id == stp.StdId).Select(x => x.Id).FirstOrDefault();
            v.RollNo = 0;
            sdb.Entry(v).State = EntityState.Modified;
            sdb.StudentPreviousAcademicInfo.Add(stp);
            sdb.SaveChanges();
            return RedirectToAction("StudentDetals", new { id = sId });
        }

        public ActionResult StudentPreviousHistory(string id = null)
        {
            List<StudentPreviousAcademicInfo> std = sdb.StudentPreviousAcademicInfo.Where(a => a.StdId.Equals(id)).ToList();
            return View(std);
        }


        [HttpGet]
        public ActionResult PrintIdCard(string id)
        {

            string userid = User.Identity.Name;
            int schoolid = uc.UserProfiles.Where(x => x.UserName.Equals(userid)).Select(x => x.SchoolId ?? 0).SingleOrDefault();
            string schoolname = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.SchoolName).SingleOrDefault();
            string address = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.Address + " " + x.City + " " + x.Country).SingleOrDefault();
            string logo = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.Logo).SingleOrDefault();
            string opclogo = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.opcityimg).SingleOrDefault();
            var logourl = Server.MapPath("~/uploads/" + logo);
            var opclogourl = Server.MapPath("~/Images/" + opclogo);

            string fname = sdbs.Student_info.Where(x => x.Std_id.Equals(id)).Select(x => x.Std_FName).SingleOrDefault();
            string mname = sdbs.Student_info.Where(x => x.Std_id.Equals(id)).Select(x => x.Std_MName).SingleOrDefault();
            string lname = sdbs.Student_info.Where(x => x.Std_id.Equals(id)).Select(x => x.Std_LName).SingleOrDefault();
            string name = fname + " " + mname + " " + lname;

            int roll = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Equals(id)).Select(x => x.RollNo).SingleOrDefault();
            string semester = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Equals(id)).Select(x => x.Semester).SingleOrDefault();
            string section = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Equals(id)).Select(x => x.Section).SingleOrDefault();
            string mobile = sdbs.Student_info.Where(x => x.Std_id.Equals(id)).Select(x => x.GardianMobile).SingleOrDefault();
            string dob = sdbs.Student_info.Where(x => x.Std_id.Equals(id)).Select(x => x.Birth_date).SingleOrDefault().ToString();
            string stdimage = sdbs.Student_info.Where(x => x.Std_id.Equals(id)).Select(x => x.Picture).SingleOrDefault();
            var stdimageurl = Server.MapPath("~/uploads/" + stdimage);

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Report"), "StudentIDCardReport.rpt"));

            rd.SetParameterValue("studentname", name.ToString());
            rd.SetParameterValue("roll", roll.ToString());
            rd.SetParameterValue("regno", id.ToString());
            rd.SetParameterValue("semester", semester.ToString());
            rd.SetParameterValue("section", section.ToString());
            rd.SetParameterValue("schoolname", schoolname.ToString());
            rd.SetParameterValue("address", address.ToString());
            rd.SetParameterValue("mobile", mobile.ToString());
            rd.SetParameterValue("dob", dob.ToString());
            rd.SetParameterValue("stdimageurl", stdimageurl.ToString());
            rd.SetParameterValue("logourl", logourl.ToString());
            rd.SetParameterValue("opclogourl", opclogourl.ToString());

            rd.SummaryInfo.ReportTitle = "IdCard";
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                Session["Stdid"] = null;
                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ActionResult StudentFullProfilePrint(string id)
        {

            var Students = from st in sdbs.Student_info.ToList()
                           where st.Std_id == id
                           select new
                           {
                               Admission_date = st.Admission_date?.Date.ToString("dd-MMM-yyyy"),
                               BeforeClass = st.BeforeClass,
                               BeforeGpa = st.BeforeGpa,
                               BeforeSchool = st.BeforeSchool,
                               Birth_date = st.Birth_date?.ToString("dd-MMM-yyyy"),
                               Contact_no = st.Contact_no,
                               Email_id = st.Email_id,
                               Entry_date = st.Entry_date?.Date.ToString("dd-MMM-yyyy"),
                               Father_Name = st.Father_Name,
                               GardianMobile = st.GardianMobile,
                               GardianName = st.GardianName,
                               GardianOccupation = st.GardianOccupation,
                               Gender = st.Gender,
                               GurdianRelation = st.GurdianRelation,
                               Id = st.Id,
                               Mother_Nmae = st.Mother_Nmae,
                               Parmenent_address = st.Parmenent_address,
                               Present_address = st.Present_address,
                               status = st.status,
                               Std_FName = st.Std_FName,
                               Std_id = st.Std_id,
                               Std_LName = st.Std_LName,
                               Std_MName = st.Std_MName,

                           };

            string userid = User.Identity.Name;
            int schoolid = uc.UserProfiles.Where(x => x.UserName.Equals(userid)).Select(x => x.SchoolId ?? 0).SingleOrDefault();
            string schoolname = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.SchoolName).SingleOrDefault();
            string address = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.Address).SingleOrDefault();
            string country = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.City + " , " + x.Country).SingleOrDefault();
            string logo = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.Logo).SingleOrDefault();
            string opclogo = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.opcityimg).SingleOrDefault();
            var logourl = Server.MapPath("~/uploads/" + logo);
            string fname = sdbs.Student_info.Where(x => x.Std_id.Equals(id)).Select(x => x.Std_FName).SingleOrDefault();
            string mname = sdbs.Student_info.Where(x => x.Std_id.Equals(id)).Select(x => x.Std_MName).SingleOrDefault();
            string lname = sdbs.Student_info.Where(x => x.Std_id.Equals(id)).Select(x => x.Std_LName).SingleOrDefault();
            string name = fname + " " + mname + " " + lname;
            int roll = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Equals(id)).Select(x => x.RollNo).SingleOrDefault();
            string semester = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Equals(id)).Select(x => x.Semester).SingleOrDefault();
            string section = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Equals(id)).Select(x => x.Section).SingleOrDefault();

            string stdimage = sdbs.Student_info.Where(x => x.Std_id.Equals(id)).Select(x => x.Picture).SingleOrDefault();
            var stdimageurl = Server.MapPath("~/uploads/" + stdimage);

            ReportDocument rd = new ReportDocument();

            rd.Load(Path.Combine(Server.MapPath("~/Report"), "StdprofileReport.rpt"));
            rd.SetDataSource(Students);
            rd.SetParameterValue("schoolname", schoolname.ToString());
            rd.SetParameterValue("address", address.ToString());
            rd.SetParameterValue("logourl", logourl.ToString());


            rd.SetParameterValue("roll", roll.ToString());
            rd.SetParameterValue("semester", semester.ToString());
            rd.SetParameterValue("section", section.ToString());
            rd.SetParameterValue("stdimageurl", stdimageurl.ToString());
            rd.SetParameterValue("country", country.ToString());
            rd.SetParameterValue("name", name.ToString());


            rd.SummaryInfo.ReportTitle = "Student Profile";
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public PartialViewResult Std_academic_info(string id = null)
        {
            TempData["create"] = "";
            Models.CurrentAcademicInfo info = new CurrentAcademicInfo();
            using (var dc = new StudentContext())
            {
                var vs = dc.CurrentAcademicInfo.Where(a => a.Std_id.Equals(id)).FirstOrDefault();
                TempData["create"] = "Yes";
                info = vs;
                ViewBag.stdid = id;
            }

            return PartialView(info);
        }


        public PartialViewResult Stdudentacademicinfo(string id = null)
        {
            TempData["create"] = "";
            Models.CurrentAcademicInfo info = new CurrentAcademicInfo();
            using (var dc = new StudentContext())
            {
                var vs = dc.CurrentAcademicInfo.Where(a => a.Std_id.Equals(id)).FirstOrDefault();
                TempData["create"] = "Yes";
                info = vs;
                ViewBag.stdid = id;
            }

            return PartialView(info);
        }
        public PartialViewResult Schoolinfo(string id = null)
        {
            TempData["create"] = "";
            Models.SchoolSetup scinfo = new SchoolSetup();

            var vs = evc.SchoolSetup.FirstOrDefault();

            scinfo = vs;

            return PartialView(scinfo);
        }

        public ActionResult AdvanceSearch()
        {
            ViewBag.advancedsearch = "advancedsearch";
            var student = new List<Models.Sp_StdAdvanceSearc_Result>();
            using (var dc = new MvcUMS.Models.SchooldbEntities())
            {
                ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
                ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
                ViewBag.Status = new SelectList(sdb.Status, "Std_status", "Std_status");
                student = dc.Sp_StdAdvanceSearc().ToList();
            }
            return View(student);
        }


        [HttpPost]
        public ActionResult AdvanceSearchPartial(string Std_id, string DFrom, string DTo, string Department, string Semester, string Section, string roll, string Status)
        {

            var student = new List<Models.Sp_StdAdvanceSearc_Result>();
            using (var dc = new MvcUMS.Models.SchooldbEntities())
            {
                if (Std_id != "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Std_id.Equals(Std_id)).ToList();
                else if (DFrom != "" && DTo != "" && Semester == "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => Convert.ToDateTime(DTo) >= a.Admission_date && Convert.ToDateTime(DFrom) <= a.Admission_date).ToList();
                else if (DFrom != "" && DTo != "" && Semester != "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => Convert.ToDateTime(DTo) >= a.Admission_date && Convert.ToDateTime(DFrom) <= a.Admission_date && a.Semester.Equals(Semester)).ToList();
                else if (roll != "")
                {
                    int r = Convert.ToInt32(roll);
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Semester.Equals(Semester) && a.Section.Equals(Section) && a.RollNo.Equals(r)).ToList();
                }
                else if (Semester != "" && Department == "" && Section == "" && Status == "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Semester.Equals(Semester)).ToList();
                else if (Semester != "" && Department == "" && Section != "" && Status == "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Section.Equals(Section) && a.Semester.Equals(Semester)).ToList();
                else if (Semester != "" && Department == "" && Section != "" && Status != "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Section.Equals(Section) && a.Semester.Equals(Semester) && a.Status.Equals(Status)).ToList();
                else if (Semester != "" && Department != "" && Section == "" && Status == "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Department.Equals(Department) && a.Semester.Equals(Semester)).ToList();
                else if (Department != "" && Semester != "" && Section != "" && Status == "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Department.Equals(Department) && a.Semester.Equals(Semester) && a.Section.Equals(Section)).ToList();
                else if (Department != "" && Semester != "" && Section != "" && Status != "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Department.Equals(Department) && a.Semester.Equals(Semester) && a.Section.Equals(Section) && a.Status.Equals(Status)).ToList();
                else if (Department != "" && Semester != "" && Status != "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Department.Equals(Department) && a.Semester.Equals(Semester) && a.Status.Equals(Status)).ToList();
                else if (Department != "" && Status != "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Department.Equals(Department) && a.Status.Equals(Status)).ToList();
                else if (Status != "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Status.Equals(Status)).ToList();
                else
                    student = dc.Sp_StdAdvanceSearc().ToList();
            }
            return View(student);
        }
        public ActionResult Student_Edit(Guid id)
        {
            Models.Student_info S;
            using (UMSEntities1 dc = new UMSEntities1())
            {
                S = dc.Student_info.Where(a => a.Id.Equals(id)).FirstOrDefault();
            }
            return View(S);
        }

        [CheckSessionTimeOut]
        public ActionResult Student_Inactive(string id = null)
        {

            Models.Student_info S;
            using (UMSEntities1 dc = new UMSEntities1())
            {
                S = dc.Student_info.Where(a => a.Std_id.Equals(id)).FirstOrDefault();
                S.status = "Inactive";
                dc.Entry(S).State = EntityState.Modified;
                dc.SaveChanges();
                TempData["AlertMessage"] = "The student successfully Inactived";
            }

            return RedirectToAction("Student_list", "Home");
        }

        [CheckSessionTimeOut]
        public ActionResult Student_Active(string id = null)
        {
            Models.Student_info S;
            using (UMSEntities1 dc = new UMSEntities1())
            {
                S = dc.Student_info.Where(a => a.Std_id.Equals(id)).FirstOrDefault();
                S.status = "Active";
                dc.Entry(S).State = EntityState.Modified;
                dc.SaveChanges();
                TempData["AlertMessage"] = "The student successfully Actived";
            }
            return RedirectToAction("Student_list", "Home");
        }

        [HttpPost]
        public ActionResult Student_update(Models.Student_info S)
        {

            if (ModelState.IsValid)
            {
                //if (S.image != null && S.image.ContentLength > 0)
                //{
                //    var uploadDir = "~/uploads";
                //    var imagePath = Path.Combine(Server.MapPath(uploadDir), S.image.FileName);
                //    var imageUrl = Path.Combine(uploadDir, S.image.FileName);
                //    S.image.SaveAs(imagePath);
                //}

                if (S.image != null && S.image.ContentLength > 0)
                {
                    string filename = "";
                    filename = Path.GetFileName(Guid.NewGuid() + "." + S.image.FileName.Split('.')[1]);
                    S.Picture = filename;
                    string targetPath = Server.MapPath("~/uploads//" + filename);
                    Stream strm = S.image.InputStream;
                    var targetFile = targetPath;

                    GenerateThumbnails(0.5, strm, targetFile);
                }

                S.Entry_date = DateTime.Now;
                S.status = "Active";
                var schoolid = (int)Session["SchoolId"];
                using (var db = new MvcUMS.Models.UMSEntities1())
                {
                    var std = db.Student_info.Where(x => x.Id == S.Id).FirstOrDefault();
                    std.Picture = S.Picture;
                    std.Std_FName = S.Std_FName;
                    std.Std_MName = S.Std_MName;
                    std.Std_LName = S.Std_LName;
                    std.Father_Name = S.Father_Name;
                    std.Mother_Nmae = S.Mother_Nmae;
                    std.Present_address = S.Present_address;
                    std.Parmenent_address = S.Parmenent_address;
                    std.Admission_date = S.Admission_date;
                    std.Birth_date = S.Birth_date;
                    std.Contact_no = S.Contact_no;
                    std.Gender = S.Gender;
                    std.GardianName = S.GardianName;
                    std.GardianMobile = S.GardianMobile;
                    std.GardianOccupation = S.GardianOccupation;
                    std.GurdianRelation = S.GurdianRelation;
                    std.SchoolId = schoolid;
                    db.Entry(std).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["saved"] = "Data successfully updated";
                }
                return RedirectToAction("StudentDetals", "Home", new { id = S.Id });
            }
            return View("Student_entry");
        }

        //public ActionResult Student_list(string Semester, string Section, string Department, int? Page_No)
        //{
        //    var student = from si in sdbs.Student_info
        //                  join sai in sdb.CurrentAcademicInfo on si.Std_id equals sai.Std_id
        //                  select new StudentPaytypeStudentinfo
        //                  {
        //                      studentinfo = si,
        //                      currentacademicinfo = sai
        //                  };
        //    return View(student);
        //}

        [CheckSessionTimeOut]
        public ActionResult Student_list()
        {

            var student = new List<Models.ViewModels.StudentInfoViewmodel>();

            var studentInfo = sdbs.Student_info.ToList();

            var currentAcademy = sdb.CurrentAcademicInfo.ToList();

            var promotion = from cai in currentAcademy
                            join si in studentInfo on cai.Std_id equals si.Std_id
                            select new StudentInfoViewmodel
                            {
                                Id = si.Id.ToString(),
                                Std_id = si.Std_id,
                                Std_FullName = si.Std_FName + " " + si.Std_MName + " " + si.Std_LName,
                                Semester = cai.Semester,
                                Section = cai.Section,
                                Department = cai.Department,
                                RollNo = cai.RollNo,
                                status = sdb.StudentPreviousAcademicInfo.Where(x => x.id == (sdb.StudentPreviousAcademicInfo.Where(m => m.StdId == si.Std_id).Max(m => m.id))).Select(x => x.Result).FirstOrDefault(),
                                Status = cai.Status,
                            };

            foreach (var item in promotion)
            {
                StudentInfoViewmodel sivm = new StudentInfoViewmodel();
                sivm.Id = item.Id;
                sivm.Std_id = item.Std_id;
                sivm.Std_FullName = item.Std_FullName;
                sivm.Semester = item.Semester;
                sivm.Section = item.Section;
                sivm.Department = item.Department;
                sivm.RollNo = item.RollNo;
                sivm.Status = item.Status;
                sivm.status = item.status;
                student.Add(sivm);
            }

            using (var dc = new MvcUMS.Models.SchooldbEntities())
            {
                ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
                ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
                ViewBag.Status = new SelectList(sdb.Status, "Std_status", "Std_status");
            }
            return View(student);
        }

        //[CheckSessionTimeOut]
        //public ActionResult Student_list()
        //{
        //    var student = new List<Models.Student_info>();
        //    using (var dc = new MvcUMS.Models.UMSEntities1())
        //    {
        //        student = dc.Student_info.ToList();

        //        var promotion = from l in dc.Student_info.ToList()
        //                        select new Student_info
        //                        {
        //                            Id = l.Id,
        //                            Std_id = l.Std_id,
        //                            Std_FName = l.Std_FName + " " + l.Std_MName + " " + l.Std_LName,
        //                            Father_Name = l.Father_Name,
        //                            Mother_Nmae = l.Mother_Nmae,
        //                            Contact_no = l.Contact_no,
        //                            Admission_date = l.Admission_date,
        //                            Entry_date = l.Entry_date,
        //                            status = sdb.StudentPreviousAcademicInfo.Where(x => x.id == (sdb.StudentPreviousAcademicInfo.Where(m => m.StdId == l.Std_id).Max(m => m.id))).Select(x => x.Result).FirstOrDefault()
        //                        };
        //        student = promotion.ToList();
        //    }
        //    return View(student);
        //}


        public ActionResult std_attendence()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            return View();
        }
        public ActionResult std_daily_attendence()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            return View();
        }

        public ActionResult StdPass()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            return View();
        }


        [HttpPost]
        public ActionResult GetSection(string dep, string sem)
        {
            List<MvcUMS.Models.Section> objcity = new List<MvcUMS.Models.Section>();
            objcity = sdb.Section.Where(m => m.deparment_Name == dep && m.semester_Name == sem).ToList();
            SelectList obgcity = new SelectList(objcity, "section_name", "section_name", 0);
            return Json(obgcity);
        }

        [HttpPost]
        public ActionResult GetCourse(string dep, string sem)
        {
            if (dep == "")
            {
                dep = "0";
            }
            List<Course> objcity = new List<Course>();
            objcity = sdb.Course.Where(m => m.Department == dep && m.Semester == sem).ToList();
            SelectList obgcity = new SelectList(objcity, "CourseId", "CourseTitle", 0);
            return Json(obgcity);
        }
        [HttpPost]
        public ActionResult GetCourseId(string dep, string sem)
        {
            if (sem == "One" || sem == "Two" || sem == "Three" || sem == "Four" || sem == "Five" || sem == "Six" || sem == "Seven" || sem == "Eight" || sem == "Play" || sem == "Nassary")
            {
                dep = "";
            }
            List<Course> objcity = new List<Course>();
            objcity = sdb.Course.Where(m => m.Department == dep && m.Semester == sem).ToList();
            SelectList obgcity = new SelectList(objcity, "CourseId", "CourseTitle", 0);
            return Json(obgcity);
        }

        [HttpPost]
        public PartialViewResult stdAttendanceList(string Department, string Semester, string Section)
        {
            var student = new List<Models.Sp_studentAttendance_Result>();
            if (Department == "" || Department == null)
            {
                var studentlist = from s in sdbs.Student_info.ToList()
                                  join c in sdb.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                                  where c.Semester.Equals(Semester, StringComparison.OrdinalIgnoreCase)
                                  && c.Section.Equals(Section, StringComparison.OrdinalIgnoreCase)
                                  orderby s.Std_id
                                  select new Sp_studentAttendance_Result
                                  {
                                      Std_id = s.Std_id,
                                      RollNo = c.RollNo,
                                      Std_FName = s.Std_FName,
                                      Std_MName = s.Std_MName,
                                      Std_LName = s.Std_LName,
                                      Department = c.Department,
                                      Semester = c.Semester,
                                      Section = c.Section,

                                  };
                student = studentlist.ToList();

            }
            else
            {
                var studentlist = from s in sdbs.Student_info.ToList()
                                  join c in sdb.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                                  where c.Semester.Equals(Semester, StringComparison.OrdinalIgnoreCase) && c.Department.Equals(Department, StringComparison.OrdinalIgnoreCase) && c.Section.Equals(Section, StringComparison.OrdinalIgnoreCase)
                                  orderby s.Std_id
                                  select new Sp_studentAttendance_Result
                                  {
                                      Std_id = s.Std_id,
                                      Std_FName = s.Std_FName,
                                      Std_MName = s.Std_MName,
                                      Std_LName = s.Std_LName,
                                      Department = c.Department,
                                      Semester = c.Semester,
                                      Section = c.Section,

                                  };
                student = studentlist.ToList();

            }

            int m = student.Count();
            if (m > 0)
            {
                ViewBag.c = "t";
            }
            else
            {
                ViewBag.c = "f";
            }
            return PartialView(student);
        }




        [HttpPost]
        public PartialViewResult StdPassList(string Department, string Semester, string Section)
        {
            var student = new List<Models.Sp_studentAttendance_Result>();
            if (Department == "" || Department == null)
            {
                var studentlist = from s in sdbs.Student_info.ToList()
                                  join c in sdb.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                                  where c.Semester.Equals(Semester, StringComparison.OrdinalIgnoreCase)
                                  && c.Section.Equals(Section, StringComparison.OrdinalIgnoreCase)
                                  orderby s.Std_id
                                  select new Sp_studentAttendance_Result
                                  {
                                      Std_id = s.Std_id,
                                      RollNo = c.RollNo,
                                      Std_FName = s.Std_FName,
                                      Std_MName = s.Std_MName,
                                      Std_LName = s.Std_LName,

                                      Department = c.Department,
                                      Semester = c.Semester,
                                      Section = c.Section,

                                  };
                student = studentlist.ToList();

            }
            else
            {
                var studentlist = from s in sdbs.Student_info.ToList()
                                  join c in sdb.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                                  where c.Semester.Equals(Semester, StringComparison.OrdinalIgnoreCase) && c.Department.Equals(Department, StringComparison.OrdinalIgnoreCase) && c.Section.Equals(Section, StringComparison.OrdinalIgnoreCase)
                                  orderby s.Std_id
                                  select new Sp_studentAttendance_Result
                                  {
                                      Std_id = s.Std_id,
                                      RollNo = c.RollNo,
                                      Std_FName = s.Std_FName,
                                      Std_MName = s.Std_MName,
                                      Std_LName = s.Std_LName,
                                      Department = c.Department,
                                      Semester = c.Semester,
                                      Section = c.Section,

                                  };
                student = studentlist.ToList();

            }

            int m = student.Count();
            if (m > 0)
            {
                ViewBag.c = "t";
            }
            else
            {
                ViewBag.c = "f";
            }
            return PartialView(student);
        }

        //take student attendance
        [HttpPost]
        public ActionResult getAttendance(string[] ids, string Department, string Semester, string Section)
        {
            string msg = "";
            if (Department == "" || Department == null)
            {
                var studentlist = (from s in sdbs.Student_info.ToList()
                                   join c in sdb.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                                   where c.Semester.Equals(Semester, StringComparison.OrdinalIgnoreCase) && c.Section.Equals(Section, StringComparison.OrdinalIgnoreCase)
                                   select new Sp_studentAttendance_Result
                                   {
                                       Std_id = c.Std_id,
                                       Std_FName = s.Std_FName,
                                       Std_MName = s.Std_MName,
                                       Std_LName = s.Std_LName,
                                       Department = c.Department,
                                       Semester = c.Semester,
                                       Section = c.Section,
                                       ContactNo = s.Contact_no
                                   }).ToList();

                var student = studentlist.ToList().OrderBy(x => x.Std_id);

                StudentAttendances att = new StudentAttendances();
                int j = 0;
                foreach (var i in student)
                {
                    att.Std_id = i.Std_id;
                    att.AttendanceDate = DateTime.Now;
                    StudentAttendances isExistToday = matt.StudentAttendances
                        .Where(a => a.Std_id.Equals(att.Std_id)
                            && a.AttendanceDate.Value.Day.Equals(DateTime.Now.Day)
                            && a.AttendanceDate.Value.Month.Equals(DateTime.Now.Month)
                            && a.AttendanceDate.Value.Year.Equals(DateTime.Now.Year)).FirstOrDefault();

                    if (isExistToday == null)
                    {
                        att.Isfirst = true;
                    }
                    else
                    {
                        if (isExistToday.IsPresent == false)
                            att.Isfirst = true;
                        else
                            att.Isfirst = false;
                    }
                    if (ids[j] == i.Std_id)
                    {
                        att.IsPresent = true;
                        matt.StudentAttendances.Add(att);
                        msg = @"Dear Guardian,According to our record,your children has been Presented in today's school session.
Thank you,
Executive HeadTeacher";

                        if ((j + 1) < ids.Count())
                            j++;
                    }
                    else
                    {
                        att.IsPresent = false;
                        matt.StudentAttendances.Add(att);
                        msg = @"Dear Guardian,According to our record,your children has been Absent in today's school session.
Thank you,
Executive HeadTeacher";
                    }
                    matt.SaveChanges();

                    string mobilenumber = "+88" + Convert.ToString(i.ContactNo);

                    string sender = "InfoSMS";
                    RootObject results = SmsSend(sender, mobilenumber, msg);

                }
            }


            else
            {
                var studentlist = from s in sdbs.Student_info.ToList()
                                  join c in sdb.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                                  where c.Semester.Equals(Semester, StringComparison.OrdinalIgnoreCase) && c.Department.Equals(Department, StringComparison.OrdinalIgnoreCase) && c.Section.Equals(Section, StringComparison.OrdinalIgnoreCase)
                                  select new Sp_studentAttendance_Result
                                  {
                                      Std_id = c.Std_id,
                                      Std_FName = s.Std_FName,
                                      Std_MName = s.Std_MName,
                                      Std_LName = s.Std_LName,
                                      Department = c.Department,
                                      Semester = c.Semester,
                                      Section = c.Section,
                                      ContactNo = s.Contact_no

                                  };
                var student = studentlist.ToList().OrderBy(x => x.Std_id);

                StudentAttendances att = new StudentAttendances();
                int j = 0;
                foreach (var i in student)
                {
                    att.Std_id = i.Std_id;
                    att.AttendanceDate = DateTime.Now.AddHours(9);
                    StudentAttendances isExistToday = matt.StudentAttendances
                        .Where(a => a.Std_id.Equals(att.Std_id)
                            && a.AttendanceDate.Value.Day.Equals(DateTime.Now.Day)
                            && a.AttendanceDate.Value.Month.Equals(DateTime.Now.Month)
                            && a.AttendanceDate.Value.Year.Equals(DateTime.Now.Year)).FirstOrDefault();

                    if (isExistToday == null)
                    {
                        att.Isfirst = true;
                    }
                    else
                    {
                        if (isExistToday.IsPresent == false)
                            att.Isfirst = true;
                        else
                            att.Isfirst = false;
                    }
                    if (ids[j] == i.Std_id)
                    {
                        att.IsPresent = true;
                        matt.StudentAttendances.Add(att);

                        if ((j + 1) < ids.Count())
                            j++;
                    }
                    else
                    {
                        att.IsPresent = false;
                        matt.StudentAttendances.Add(att);
                    }
                    matt.SaveChanges();

                    string mobilenumber = "+88" + Convert.ToString(i.ContactNo);

                    string sender = "InfoSMS";
                    RootObject results = SmsSend(sender, mobilenumber, msg);

                }


            }

            return RedirectToAction("std_attendence");
        }

        [HttpPost]
        public ActionResult GetPass(string[] ids, string Department, string Semester, string Section)
        {
            if (Department == "" || Department == null)
            {
                var studentlist = (from s in sdbs.Student_info.ToList()
                                   join c in sdb.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                                   where c.Semester.Equals(Semester, StringComparison.OrdinalIgnoreCase) && c.Section.Equals(Section, StringComparison.OrdinalIgnoreCase)
                                   select new Sp_studentAttendance_Result
                                   {
                                       Std_id = c.Std_id,
                                       Std_FName = s.Std_FName,
                                       Std_MName = s.Std_MName,
                                       Std_LName = s.Std_LName,
                                       Department = c.Department,
                                       Semester = c.Semester,
                                       Section = c.Section,
                                       ContactNo = s.Contact_no
                                   }).ToList();

                var student = studentlist.ToList().OrderBy(x => x.Std_id);

                StudentPass att = new StudentPass();
                int j = 0;
                foreach (var i in student)
                {
                    att.Std_id = i.Std_id;
                    att.Date = DateTime.Now;

                    StudentPass isExistToday = matt.StudentPasses
                        .Where(a => a.Std_id.Equals(att.Std_id)).FirstOrDefault();

                    if (isExistToday == null)
                    {
                        if (ids[j] == i.Std_id)
                        {
                            att.IsPass = true;
                            matt.StudentPasses.Add(att);

                            if ((j + 1) < ids.Count())
                                j++;
                        }
                        else
                        {
                            att.IsPass = false;
                            matt.StudentPasses.Add(att);
                        }
                    }
                    else
                    {

                        if (ids[j] == i.Std_id)
                        {
                            att.IsPass = true;
                            matt.Entry(isExistToday).State = EntityState.Modified;

                            if ((j + 1) < ids.Count())
                                j++;
                        }
                        else
                        {
                            att.IsPass = false;
                            matt.Entry(isExistToday).State = EntityState.Modified;
                        }
                    }

                    matt.SaveChanges();






                }
            }


            else
            {
                var studentlist = from s in sdbs.Student_info.ToList()
                                  join c in sdb.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                                  where c.Semester.Equals(Semester, StringComparison.OrdinalIgnoreCase) && c.Department.Equals(Department, StringComparison.OrdinalIgnoreCase) && c.Section.Equals(Section, StringComparison.OrdinalIgnoreCase)
                                  select new Sp_studentAttendance_Result
                                  {
                                      Std_id = c.Std_id,
                                      Std_FName = s.Std_FName,
                                      Std_MName = s.Std_MName,
                                      Std_LName = s.Std_LName,
                                      Department = c.Department,
                                      Semester = c.Semester,
                                      Section = c.Section,
                                      ContactNo = s.Contact_no

                                  };
                var student = studentlist.ToList().OrderBy(x => x.Std_id);

                StudentPass att = new StudentPass();
                int j = 0;
                foreach (var i in student)
                {
                    att.Std_id = i.Std_id;
                    att.Date = DateTime.Now.AddHours(9);

                    StudentPass isExistToday = matt.StudentPasses
                        .Where(a => a.Std_id.Equals(att.Std_id)).FirstOrDefault();


                    if (isExistToday.IsPass == false)
                        att.IsPass = true;
                    else
                        att.IsPass = false;

                    if (ids[j] == i.Std_id)
                    {
                        att.IsPass = true;
                        matt.StudentPasses.Add(att);

                        if ((j + 1) < ids.Count())
                            j++;
                    }
                    else
                    {
                        att.IsPass = false;
                        matt.StudentPasses.Add(att);
                    }
                    matt.SaveChanges();

                }


            }

            return RedirectToAction("StdPass");
        }

        //show student attendence
        public ActionResult showAttendence()
        {
            ViewBag.advancedsearch = "advancedsearch";
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            ViewBag.DepartmentM = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.SemesterM = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            return View();
        }

        public ActionResult ManualAttendenceSearch()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            ViewBag.DepartmentM = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.SemesterM = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            return View();
        }

        public ActionResult StdPassSearch()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            ViewBag.DepartmentM = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.SemesterM = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            return View();
        }



        [HttpPost]
        public PartialViewResult ShowAttendanceList(string Department, string Semester, string Section, DateTime ADate, string text)
        {
            List<ShowStdAttendence> list = new List<ShowStdAttendence>();
            DateTime date = Convert.ToDateTime(ADate.ToShortDateString());
            if (text == "sd")
            {
                TempData["test"] = null;
                if (Department == "")
                {
                    var stdAttenList = from att in matt.StdAttendances.ToList()
                                       join cinfo in sdb.CurrentAcademicInfo.ToList() on att.ProximateID equals cinfo.ProximateID
                                       join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                       where (att.CheckDate == ADate) && (cinfo.Semester == Semester) && (cinfo.Section == Section)
                                       select new ShowStdAttendence
                                       {
                                           StdId = sInfo.Std_id,
                                           rollNo = cinfo.RollNo,
                                           StdFather = sInfo.Father_Name,
                                           StdName = (sInfo.Std_FName + " " + sInfo.Std_MName + " " + sInfo.Std_LName),
                                           CheckIn = att.CheckIn,
                                           CheckOut = att.CheckOut,
                                           ContactNo = sInfo.Contact_no
                                       };


                    list = stdAttenList.ToList();
                    TempData["stdAttenanceList"] = list;
                    return PartialView(stdAttenList.ToList());
                }
                else
                {

                    var stdAttenList = from att in matt.StdAttendances.ToList()
                                       join cinfo in sdb.CurrentAcademicInfo.ToList() on att.ProximateID equals cinfo.ProximateID
                                       join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                       where (att.CheckDate == date) && (cinfo.Department == Department) && (cinfo.Semester == Semester) && (cinfo.Section == Section)
                                       select new ShowStdAttendence
                                       {
                                           StdId = sInfo.Std_id,
                                           rollNo = cinfo.RollNo,
                                           StdFather = sInfo.Father_Name,
                                           StdName = (sInfo.Std_FName + " " + sInfo.Std_MName + " " + sInfo.Std_LName),
                                           CheckIn = att.CheckIn,
                                           CheckOut = att.CheckOut,
                                           ContactNo = sInfo.Contact_no
                                       };

                    TempData["stdAbsenceList"] = list;
                    return PartialView(stdAttenList.ToList());
                }

            }
            else
            {
                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                var lastDayOfMonth = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
                TempData["test"] = "Please View";
                if (Department == "")
                {
                    var stdAttenList = from att in matt.StdAttendances.ToList()
                                       join cinfo in sdb.CurrentAcademicInfo.ToList() on att.ProximateID equals cinfo.ProximateID
                                       join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                       where (att.CheckDate >= firstDayOfMonth && att.CheckDate <= lastDayOfMonth) && (cinfo.Semester == Semester) && (cinfo.Section == Section)
                                       group att by new
                                       {
                                           sInfo.Std_id,
                                           cinfo.RollNo,
                                           sInfo.Father_Name,
                                           sInfo.Std_FName,
                                           sInfo.Std_MName,
                                           sInfo.Std_LName,
                                           sInfo.Contact_no
                                       } into g
                                       select new ShowStdAttendence
                                       {
                                           StdId = g.Key.Std_id,
                                           rollNo = g.Key.RollNo,
                                           StdFather = g.Key.Father_Name,
                                           StdName = (g.Key.Std_FName + " " + g.Key.Std_MName + " " + g.Key.Std_LName),
                                           totalPresent = g.Where(a => a.CheckIn.Equals(true)).Count(),
                                           ContactNo = g.Key.Contact_no

                                       };

                    TempData["teacherAttenanceList"] = list;
                    return PartialView(stdAttenList.ToList());
                }
                else
                {
                    var stdAttenList = from att in matt.StdAttendances.ToList()
                                       join cinfo in sdb.CurrentAcademicInfo.ToList() on att.ProximateID equals cinfo.ProximateID
                                       join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                       where (att.CheckDate >= firstDayOfMonth && att.CheckDate <= lastDayOfMonth) && (cinfo.Department == Department) && (cinfo.Semester == Semester) && (cinfo.Section == Section)
                                       group att by new
                                       {
                                           sInfo.Std_id,
                                           cinfo.RollNo,
                                           sInfo.Father_Name,
                                           sInfo.Std_FName,
                                           sInfo.Std_MName,
                                           sInfo.Std_LName,
                                           sInfo.Contact_no
                                       } into g
                                       select new ShowStdAttendence
                                       {
                                           StdId = g.Key.Std_id,
                                           rollNo = g.Key.RollNo,
                                           StdFather = g.Key.Father_Name,
                                           StdName = (g.Key.Std_FName + " " + g.Key.Std_MName + " " + g.Key.Std_LName),
                                           totalPresent = g.Where(a => a.CheckIn.Equals(true)).Count(),
                                           ContactNo = g.Key.Contact_no
                                       };

                    TempData["teacherAbsenceList"] = list;
                    return PartialView(stdAttenList.ToList());
                }
            }
        }

        [HttpPost]
        public PartialViewResult ShowManualAttendanceList(string Department, string Semester, string Section, DateTime ADate, string text)
        {

            DateTime date = Convert.ToDateTime(ADate.ToShortDateString());
            if (text == "sd")
            {
                TempData["test"] = null;
                if (Department == "")
                {
                    var stdAttenList = (from att in matt.StudentAttendances.ToList()

                                        join cinfo in sdb.CurrentAcademicInfo.ToList() on att.Std_id equals cinfo.Std_id
                                        join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                        where (att.AttendanceDate == date) && (cinfo.Semester == Semester) && (cinfo.Section == Section)
                                        select new ShowStdAttendence
                                        {
                                            StdId = att.Std_id,
                                            rollNo = cinfo.RollNo,
                                            StdFather = sInfo.Father_Name,
                                            StdName = (sInfo.Std_FName + " " + sInfo.Std_MName + " " + sInfo.Std_LName),
                                            isPresent = att.IsPresent
                                        }).ToList();

                    ViewBag.Total = stdAttenList.Count();
                    ViewBag.TotalPresent = stdAttenList.Where(a => a.isPresent.Equals(true)).Count();
                    ViewBag.TotalAbsent = stdAttenList.Where(a => a.isPresent.Equals(false)).Count();

                    return PartialView(stdAttenList.ToList());
                }
                else
                {

                    var stdAttenList = from att in matt.StudentAttendances.ToList()
                                       join cinfo in sdb.CurrentAcademicInfo.ToList() on att.Std_id equals cinfo.Std_id
                                       join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                       where (att.AttendanceDate == date) && (cinfo.Department == Department) && (cinfo.Semester == Semester) && (cinfo.Section == Section)
                                       select new ShowStdAttendence { StdId = att.Std_id, rollNo = cinfo.RollNo, StdFather = sInfo.Father_Name, StdName = (sInfo.Std_FName + " " + sInfo.Std_MName + " " + sInfo.Std_LName), isPresent = att.IsPresent };
                    return PartialView(stdAttenList.ToList());
                }
            }
            else
            {
                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                var lastDayOfMonth = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
                TempData["test"] = "Please View";
                if (Department == "")
                {
                    var stdAttenList = from att in matt.StudentAttendances.ToList()
                                       join cinfo in sdb.CurrentAcademicInfo.ToList() on att.Std_id equals cinfo.Std_id
                                       join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                       where (att.AttendanceDate >= firstDayOfMonth && att.AttendanceDate <= lastDayOfMonth) && (cinfo.Semester == Semester) && (cinfo.Section == Section) && att.Isfirst == true
                                       group att by new
                                       {
                                           sInfo.Std_id,
                                           cinfo.RollNo,
                                           sInfo.Father_Name,
                                           sInfo.Std_FName,
                                           sInfo.Std_MName,
                                           sInfo.Std_LName
                                       } into g
                                       select new ShowStdAttendence
                                       {
                                           StdId = g.Key.Std_id,
                                           rollNo = g.Key.RollNo,
                                           StdFather = g.Key.Father_Name,
                                           StdName = (g.Key.Std_FName + " " + g.Key.Std_MName + " " + g.Key.Std_LName),
                                           totalPresent = g.Where(a => a.IsPresent.Equals(true)).Count(),
                                           totalAbsent = g.Where(a => a.IsPresent.Equals(false)).Count()
                                       };
                    return PartialView(stdAttenList.ToList());
                }
                else
                {
                    var stdAttenList = from att in matt.StudentAttendances.ToList()
                                       join cinfo in sdb.CurrentAcademicInfo.ToList() on att.Std_id equals cinfo.Std_id
                                       join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                       where (att.AttendanceDate >= firstDayOfMonth && att.AttendanceDate <= lastDayOfMonth) && (cinfo.Department == Department) && (cinfo.Semester == Semester) && (cinfo.Section == Section) && att.Isfirst == true
                                       group att by new { sInfo.Std_id, cinfo.RollNo, sInfo.Father_Name, sInfo.Std_FName, sInfo.Std_MName, sInfo.Std_LName } into g
                                       select new ShowStdAttendence
                                       {
                                           StdId = g.Key.Std_id,
                                           rollNo = g.Key.RollNo,
                                           StdFather = g.Key.Father_Name,
                                           StdName = (g.Key.Std_FName + " " + g.Key.Std_MName + " " + g.Key.Std_LName),
                                           totalPresent = g.Where(a => a.IsPresent.Equals(true)).Count(),
                                           totalAbsent = g.Where(a => a.IsPresent.Equals(false)).Count()
                                       };
                    return PartialView(stdAttenList.ToList());
                }

            }


        }

        [HttpPost]
        public PartialViewResult ShowStdPassList(string Department, string Semester, string Section, string text)
        {

            //DateTime date = Convert.ToDateTime(ADate.ToShortDateString());
            if (text == "sd")
            {
                TempData["test"] = null;
                if (Department == "")
                {
                    var stdAttenList = (from att in matt.StudentPasses.ToList()

                                        join cinfo in sdb.CurrentAcademicInfo.ToList() on att.Std_id equals cinfo.Std_id
                                        join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                        where (cinfo.Semester == Semester) && (cinfo.Section == Section)
                                        select new ShowStdPass
                                        {
                                            StdId = att.Std_id,
                                            rollNo = cinfo.RollNo,
                                            StdFather = sInfo.Father_Name,
                                            StdName = (sInfo.Std_FName + " " + sInfo.Std_MName + " " + sInfo.Std_LName),
                                            isPass = att.IsPass
                                        }).ToList();

                    ViewBag.Total = stdAttenList.Count();
                    ViewBag.TotalPresent = stdAttenList.Where(a => a.isPass.Equals(true)).Count();
                    ViewBag.TotalAbsent = stdAttenList.Where(a => a.isPass.Equals(false)).Count();

                    return PartialView(stdAttenList.ToList());
                }
                else
                {

                    var stdAttenList = from att in matt.StudentPasses.ToList()
                                       join cinfo in sdb.CurrentAcademicInfo.ToList() on att.Std_id equals cinfo.Std_id
                                       join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                       where (cinfo.Department == Department) && (cinfo.Semester == Semester) && (cinfo.Section == Section)
                                       select new ShowStdPass
                                       {
                                           StdId = att.Std_id,
                                           rollNo = cinfo.RollNo,
                                           StdFather = sInfo.Father_Name,
                                           StdName = (sInfo.Std_FName + " " + sInfo.Std_MName + " " + sInfo.Std_LName),
                                           isPass = att.IsPass
                                       };
                    return PartialView(stdAttenList.ToList());
                }
            }
            else
            {
                //var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                //var lastDayOfMonth = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
                TempData["test"] = "Please View";
                if (Department == "")
                {
                    var stdAttenList = from att in matt.StudentPasses.ToList()
                                       join cinfo in sdb.CurrentAcademicInfo.ToList() on att.Std_id equals cinfo.Std_id
                                       join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                       where (cinfo.Semester == Semester) && (cinfo.Section == Section)
                                       group att by new
                                       {
                                           sInfo.Std_id,
                                           cinfo.RollNo,
                                           sInfo.Father_Name,
                                           sInfo.Std_FName,
                                           sInfo.Std_MName,
                                           sInfo.Std_LName
                                       } into g
                                       select new ShowStdPass
                                       {
                                           StdId = g.Key.Std_id,
                                           rollNo = g.Key.RollNo,
                                           StdFather = g.Key.Father_Name,
                                           StdName = (g.Key.Std_FName + " " + g.Key.Std_MName + " " + g.Key.Std_LName),
                                           totalPass = g.Where(a => a.IsPass.Equals(true)).Count(),
                                           totalFail = g.Where(a => a.IsPass.Equals(false)).Count()
                                       };
                    return PartialView(stdAttenList.ToList());
                }
                else
                {
                    var stdAttenList = from att in matt.StudentPasses.ToList()
                                       join cinfo in sdb.CurrentAcademicInfo.ToList() on att.Std_id equals cinfo.Std_id
                                       join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                       where (cinfo.Department == Department) && (cinfo.Semester == Semester) && (cinfo.Section == Section)
                                       group att by new { sInfo.Std_id, cinfo.RollNo, sInfo.Father_Name, sInfo.Std_FName, sInfo.Std_MName, sInfo.Std_LName } into g
                                       select new ShowStdPass
                                       {
                                           StdId = g.Key.Std_id,
                                           rollNo = g.Key.RollNo,
                                           StdFather = g.Key.Father_Name,
                                           StdName = (g.Key.Std_FName + " " + g.Key.Std_MName + " " + g.Key.Std_LName),
                                           totalPass = g.Where(a => a.IsPass.Equals(true)).Count(),
                                           totalFail = g.Where(a => a.IsPass.Equals(false)).Count()
                                       };
                    return PartialView(stdAttenList.ToList());
                }

            }


        }

        [HttpPost]
        public PartialViewResult ShowAbsenceList(string Department, string Semester, string Section, DateTime ADate, string text)
        {
            List<ShowStdAttendence> list = new List<ShowStdAttendence>();
            DateTime date = Convert.ToDateTime(ADate.ToShortDateString());
            if (text == "sd")
            {
                TempData["test"] = null;
                if (Department == "")
                {

                    var stdAttenList = from std in sdbs.Student_info.ToList()
                                       join att in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals att.Std_id
                                       where !(from o in matt.StdAttendances.ToList()
                                               where o.CheckDate == date
                                               select o.ProximateID)
                                               .Contains(att.ProximateID) && att.Semester == Semester && att.Section == Section
                                       select new ShowStdAttendence
                                       {
                                           StdId = std.Std_id,
                                           rollNo = att.RollNo,
                                           StdFather = std.Father_Name,
                                           StdName = (std.Std_FName + " " + std.Std_MName + " " + std.Std_LName),
                                           ContactNo = std.Contact_no
                                       };



                    list = stdAttenList.ToList();
                    TempData["AbsenceList"] = list;
                    return PartialView(stdAttenList.ToList());

                }
                else
                {

                    var stdAttenList = from std in sdbs.Student_info.ToList()
                                       join att in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals att.Std_id
                                       where !(from o in matt.StdAttendances.ToList()
                                               where o.CheckDate == date
                                               select o.ProximateID)
                                               .Contains(att.ProximateID) && att.Semester == Semester && att.Department == Department && att.Section == Section
                                       select new ShowStdAttendence
                                       {
                                           StdId = std.Std_id,
                                           rollNo = att.RollNo,
                                           StdFather = std.Father_Name,
                                           StdName = (std.Std_FName + " " + std.Std_MName + " " + std.Std_LName),
                                           ContactNo = std.Contact_no
                                       };



                    list = stdAttenList.ToList();
                    TempData["AbsenceList"] = list;
                    return PartialView(stdAttenList.ToList());
                }

            }

            return PartialView();



        }

        [HttpPost]
        public PartialViewResult ShowManualAbsenceList(string Department, string Semester, string Section, DateTime ADate, string text)
        {
            List<ShowStdAttendence> list = new List<ShowStdAttendence>();
            DateTime date = Convert.ToDateTime(ADate.ToShortDateString());
            if (text == "sd")
            {
                TempData["test"] = null;
                if (Department == "")
                {

                    var stdAttenList = (from std in sdbs.Student_info.ToList()
                                        join att in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals att.Std_id
                                        where !(from o in matt.StudentAttendances.ToList()
                                                where o.AttendanceDate == date
                                                select o.Std_id)
                                                .Contains(att.Std_id) && att.Semester == Semester && att.Section == Section
                                        select new ShowStdAttendence
                                        {
                                            StdId = std.Std_id,
                                            rollNo = att.RollNo,
                                            StdFather = std.Father_Name,
                                            StdName = (std.Std_FName + " " + std.Std_MName + " " + std.Std_LName),
                                            ContactNo = std.Contact_no
                                        }).ToList();



                    list = stdAttenList.ToList();
                    TempData["AbsenceList"] = list;
                    return PartialView(stdAttenList.ToList());

                }
                else
                {

                    var stdAttenList = from std in sdbs.Student_info.ToList()
                                       join att in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals att.Std_id
                                       where !(from o in matt.StudentAttendances.ToList()
                                               where o.AttendanceDate == date
                                               select o.Std_id)
                                               .Contains(att.Std_id) && att.Semester == Semester && att.Department == Department && att.Section == Section
                                       select new ShowStdAttendence
                                       {
                                           StdId = std.Std_id,
                                           rollNo = att.RollNo,
                                           StdFather = std.Father_Name,
                                           StdName = (std.Std_FName + " " + std.Std_MName + " " + std.Std_LName),
                                           ContactNo = std.Contact_no
                                       };



                    list = stdAttenList.ToList();
                    TempData["AbsenceList"] = list;
                    return PartialView(stdAttenList.ToList());
                }

            }

            return PartialView();



        }



        //Multiple Partial View
        [HttpPost]
        public ActionResult ShowAttendanceAbsenceList(string Department, string Semester, string Section, DateTime ADate, string text)
        {
            List<ShowStdAttendence> AttenList = new List<ShowStdAttendence>();
            DateTime AttenDate = Convert.ToDateTime(ADate.ToShortDateString());
            if (text == "std")
            {
                TempData["test"] = null;
                if (Department == "" && Section == "")
                {
                    AttenList = (from att in matt.StdAttendances.ToList()
                                 join cinfo in sdb.CurrentAcademicInfo.ToList() on att.ProximateID equals cinfo.ProximateID
                                 join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                 where (att.CheckDate == ADate) && (cinfo.Semester == Semester)
                                 select new ShowStdAttendence
                                 {
                                     StdId = sInfo.Std_id,
                                     rollNo = cinfo.RollNo,
                                     StdFather = sInfo.Father_Name,
                                     StdName = (sInfo.Std_FName + " " + sInfo.Std_MName + " " + sInfo.Std_LName),
                                     CheckIn = att.CheckIn,
                                     CheckOut = att.CheckOut,
                                     ContactNo = sInfo.Contact_no
                                 }).ToList();


                    TempData["stdAttendanceList"] = AttenList;
                }
                else if (Department == "")
                {
                    AttenList = (from att in matt.StdAttendances.ToList()
                                 join cinfo in sdb.CurrentAcademicInfo.ToList() on att.ProximateID equals cinfo.ProximateID
                                 join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                 where (att.CheckDate == ADate) && (cinfo.Semester == Semester) && (cinfo.Section == Section)
                                 select new ShowStdAttendence
                                 {
                                     StdId = sInfo.Std_id,
                                     rollNo = cinfo.RollNo,
                                     StdFather = sInfo.Father_Name,
                                     StdName = (sInfo.Std_FName + " " + sInfo.Std_MName + " " + sInfo.Std_LName),
                                     CheckIn = att.CheckIn,
                                     CheckOut = att.CheckOut,
                                     ContactNo = sInfo.Contact_no
                                 }).ToList();


                    TempData["stdAttendanceList"] = AttenList;
                }
                else
                {

                    AttenList = (from att in matt.StdAttendances.ToList()
                                 join cinfo in sdb.CurrentAcademicInfo.ToList() on att.ProximateID equals cinfo.ProximateID
                                 join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                 where (att.CheckDate == AttenDate) && (cinfo.Department == Department) && (cinfo.Semester == Semester) && (cinfo.Section == Section)
                                 select new ShowStdAttendence
                                 {
                                     StdId = sInfo.Std_id,
                                     rollNo = cinfo.RollNo,
                                     StdFather = sInfo.Father_Name,
                                     StdName = (sInfo.Std_FName + " " + sInfo.Std_MName + " " + sInfo.Std_LName),
                                     CheckIn = att.CheckIn,
                                     CheckOut = att.CheckOut,
                                     ContactNo = sInfo.Contact_no
                                 }).ToList();

                    TempData["stdAttendanceList"] = AttenList;
                }

            }
            else
            {

                AttenList = (from att in matt.StdAttendances.ToList()
                             join cinfo in sc.Stuff.ToList() on att.ProximateID equals cinfo.ProximateID
                             where (att.CheckDate == ADate)
                             select new ShowStdAttendence
                             {
                                 StuffName = (cinfo.FirstName + " " + cinfo.LastName),
                                 CheckIn = att.CheckIn,
                                 CheckOut = att.CheckOut,
                                 ContactNo = cinfo.ContactNo,
                                 Type = text
                             }).ToList();


                TempData["teacherAttendanceList"] = AttenList;




            }



            List<ShowStdAttendence> AbsenceList = new List<ShowStdAttendence>();
            DateTime AbsDate = Convert.ToDateTime(ADate.ToShortDateString());
            if (text == "std")
            {
                TempData["test"] = null;
                if (Department == "" && Section == "")
                {
                    AbsenceList = (from std in sdbs.Student_info.ToList()
                                   join att in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals att.Std_id
                                   where !(from o in matt.StdAttendances.ToList()
                                           where o.CheckDate == AbsDate
                                           select o.ProximateID)
                                           .Contains(att.ProximateID) && att.Semester == Semester
                                   select new ShowStdAttendence
                                   {
                                       StdId = std.Std_id,
                                       rollNo = att.RollNo,
                                       StdFather = std.Father_Name,
                                       StdName = (std.Std_FName + " " + std.Std_MName + " " + std.Std_LName),
                                       Section = att.Section,
                                       ContactNo = std.Contact_no
                                   }).ToList();


                    TempData["stdAbsenceList"] = AbsenceList;
                }
                else if (Department == "")
                {
                    AbsenceList = (from std in sdbs.Student_info.ToList()
                                   join att in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals att.Std_id
                                   where !(from o in matt.StdAttendances.ToList()
                                           where o.CheckDate == AbsDate
                                           select o.ProximateID)
                                           .Contains(att.ProximateID) && att.Semester == Semester && att.Section == Section
                                   select new ShowStdAttendence
                                   {
                                       StdId = std.Std_id,
                                       rollNo = att.RollNo,
                                       StdFather = std.Father_Name,
                                       StdName = (std.Std_FName + " " + std.Std_MName + " " + std.Std_LName),
                                       ContactNo = std.Contact_no
                                   }).ToList();


                    TempData["stdAbsenceList"] = AbsenceList;
                }
                else
                {

                    AbsenceList = (from std in sdbs.Student_info.ToList()
                                   join att in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals att.Std_id
                                   where !(from o in matt.StdAttendances.ToList()
                                           where o.CheckDate == AbsDate
                                           select o.ProximateID)
                                           .Contains(att.ProximateID) && att.Semester == Semester && att.Department == Department && att.Section == Section
                                   select new ShowStdAttendence
                                   {
                                       StdId = std.Std_id,
                                       rollNo = att.RollNo,
                                       StdFather = std.Father_Name,
                                       StdName = (std.Std_FName + " " + std.Std_MName + " " + std.Std_LName),
                                       ContactNo = std.Contact_no
                                   }).ToList();

                    TempData["stdAbsenceList"] = AbsenceList;
                }

            }
            else
            {
                AbsenceList = (from cinfo in sc.Stuff.ToList()
                               where !(from o in matt.StdAttendances.ToList()
                                       where o.CheckDate == AbsDate
                                       select o.ProximateID)
                                         .Contains(cinfo.ProximateID)
                               select new ShowStdAttendence
                               {
                                   StuffName = (cinfo.FirstName + " " + cinfo.LastName),
                                   ContactNo = cinfo.ContactNo,
                                   Type = text
                               }).ToList();


                TempData["teacherAbsenceList"] = AbsenceList;

            }


            var totalValuesPartialView = RenderRazorViewToString(this.ControllerContext, "ShowAttendanceList", AttenList);
            var summaryValuesPartialView = RenderRazorViewToString(this.ControllerContext, "ShowAbsenceList", AbsenceList);
            var json = Json(new { totalValuesPartialView, summaryValuesPartialView });
            return json;

            //    return new MultipleViewResult(
            //PartialView(AttenList.ToList()),
            //PartialView(AbsenceList.ToList()));

        }

        [HttpPost]
        public ActionResult ShowManualAttendanceAbsenceList(string Department, string Semester, string Section, DateTime ADate, string text)
        {
            List<ShowStdAttendence> AttenList = new List<ShowStdAttendence>();
            DateTime AttenDate = Convert.ToDateTime(ADate.ToShortDateString());
            if (text == "sd")
            {
                TempData["test"] = null;
                if (Department == "" && Section == "")
                {
                    AttenList = (from att in matt.StudentAttendances.ToList()
                                 join cinfo in sdb.CurrentAcademicInfo.ToList() on att.Std_id equals cinfo.Std_id
                                 join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                 where (att.AttendanceDate == AttenDate) && (cinfo.Semester == Semester)
                                 select new ShowStdAttendence
                                 {
                                     StdId = sInfo.Std_id,
                                     rollNo = cinfo.RollNo,
                                     StdFather = sInfo.Father_Name,
                                     StdName = (sInfo.Std_FName + " " + sInfo.Std_MName + " " + sInfo.Std_LName),
                                     ContactNo = sInfo.Contact_no
                                 }).ToList();


                    TempData["AbsenceList"] = AttenList;
                }
                else if (Department == "")
                {
                    AttenList = (from att in matt.StudentAttendances.ToList()
                                 join cinfo in sdb.CurrentAcademicInfo.ToList() on att.Std_id equals cinfo.Std_id
                                 join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                 where (att.AttendanceDate == ADate) && (cinfo.Semester == Semester) && (cinfo.Section == Section)
                                 select new ShowStdAttendence
                                 {
                                     StdId = sInfo.Std_id,
                                     rollNo = cinfo.RollNo,
                                     StdFather = sInfo.Father_Name,
                                     StdName = (sInfo.Std_FName + " " + sInfo.Std_MName + " " + sInfo.Std_LName),
                                     ContactNo = sInfo.Contact_no
                                 }).ToList();


                    TempData["AbsenceList"] = AttenList;
                }
                else
                {

                    AttenList = (from att in matt.StudentAttendances.ToList()
                                 join cinfo in sdb.CurrentAcademicInfo.ToList() on att.Std_id equals cinfo.Std_id
                                 join sInfo in sdbs.Student_info.ToList() on cinfo.Std_id equals sInfo.Std_id
                                 where (att.AttendanceDate == AttenDate) && (cinfo.Department == Department) && (cinfo.Semester == Semester) && (cinfo.Section == Section)
                                 select new ShowStdAttendence
                                 {
                                     StdId = sInfo.Std_id,
                                     rollNo = cinfo.RollNo,
                                     StdFather = sInfo.Father_Name,
                                     StdName = (sInfo.Std_FName + " " + sInfo.Std_MName + " " + sInfo.Std_LName),
                                     ContactNo = sInfo.Contact_no
                                 }).ToList();

                    TempData["AbsenceList"] = AttenList;
                }

            }

            List<ShowStdAttendence> AbsenceList = new List<ShowStdAttendence>();
            DateTime AbsDate = Convert.ToDateTime(ADate.ToShortDateString());
            if (text == "sd")
            {
                TempData["test"] = null;
                if (Department == "" && Section == "")
                {
                    AbsenceList = (from std in sdbs.Student_info.ToList()
                                   join att in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals att.Std_id
                                   where !(from o in matt.StudentAttendances.ToList()
                                           where o.AttendanceDate == AbsDate
                                           select o.Std_id)
                                           .Contains(att.Std_id) && att.Semester == Semester
                                   select new ShowStdAttendence
                                   {
                                       StdId = std.Std_id,
                                       rollNo = att.RollNo,
                                       StdFather = std.Father_Name,
                                       StdName = (std.Std_FName + " " + std.Std_MName + " " + std.Std_LName),
                                       Section = att.Section,
                                       ContactNo = std.Contact_no
                                   }).ToList();


                    TempData["AbsenceList"] = AbsenceList;
                }
                else if (Department == "")
                {
                    AbsenceList = (from std in sdbs.Student_info.ToList()
                                   join att in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals att.Std_id
                                   where !(from o in matt.StudentAttendances.ToList()
                                           where o.AttendanceDate == AbsDate
                                           select o.Std_id)
                                           .Contains(att.Std_id) && att.Semester == Semester && att.Section == Section
                                   select new ShowStdAttendence
                                   {
                                       StdId = std.Std_id,
                                       rollNo = att.RollNo,
                                       StdFather = std.Father_Name,
                                       StdName = (std.Std_FName + " " + std.Std_MName + " " + std.Std_LName),
                                       ContactNo = std.Contact_no
                                   }).ToList();


                    TempData["AbsenceList"] = AbsenceList;
                }
                else
                {

                    AbsenceList = (from std in sdbs.Student_info.ToList()
                                   join att in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals att.Std_id
                                   where !(from o in matt.StudentAttendances.ToList()
                                           where o.AttendanceDate == AbsDate
                                           select o.Std_id)
                                           .Contains(att.Std_id) && att.Semester == Semester && att.Department == Department && att.Section == Section
                                   select new ShowStdAttendence
                                   {
                                       StdId = std.Std_id,
                                       rollNo = att.RollNo,
                                       StdFather = std.Father_Name,
                                       StdName = (std.Std_FName + " " + std.Std_MName + " " + std.Std_LName),
                                       ContactNo = std.Contact_no
                                   }).ToList();

                    TempData["AbsenceList"] = AbsenceList;
                }

            }

            var totalValuesPartialView = RenderRazorViewToString(this.ControllerContext, "ShowManualAttendanceList", AttenList);
            var summaryValuesPartialView = RenderRazorViewToString(this.ControllerContext, "ShowAbsenceList", AbsenceList);
            var json = Json(new { totalValuesPartialView, summaryValuesPartialView });
            return json;

            //    return new MultipleViewResult(
            //PartialView(AttenList.ToList()),
            //PartialView(AbsenceList.ToList()));

        }

        public static String RenderRazorViewToString(ControllerContext controllerContext, String viewName, Object model)
        {
            controllerContext.Controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var ViewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                var ViewContext = new ViewContext(controllerContext, ViewResult.View, controllerContext.Controller.ViewData, controllerContext.Controller.TempData, sw);
                ViewResult.View.Render(ViewContext, sw);
                ViewResult.ViewEngine.ReleaseView(controllerContext, ViewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }



        public RootObject SmsSend(string sender, string to, string message)
        {
            using (var http = new HttpClient())
            {
                // Define authorization headers here, if any
                //http.DefaultRequestHeaders.Add("Authorization", "Basic R2xvYmFsc29mdHRlY2hub2xvZ3k6Z2xvYmFsc29mdHRlY2hub2xvZ3k=");
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
        public ActionResult AbsenceSendSMS()
        {
            bool status = false;
            var list = (List<ShowStdAttendence>)TempData["stdAbsenceList"];
            //RootObject results = SmsSend();
            foreach (var item in list)
            {
                string mobilenumber = "+88" + Convert.ToString(item.ContactNo);
                string msg = @"Dear Guardian,According to our record,your children has been Absent in today's school session.
Thank you,
Executive HeadTeacher";
                string sender = "InfoSMS";
                RootObject results = SmsSend(sender, mobilenumber, msg);

                status = true;

            }



            if (status == true)
                TempData["success"] = "Message Send successfully.";

            TempData["success"] = "Message Failed";



            return new JsonResult { Data = new { status = status } };
        }

        [HttpPost]
        public ActionResult PresentSendSMS()
        {
            bool status = false;
            var list = (List<ShowStdAttendence>)TempData["stdAttendanceList"];
            //RootObject results = SmsSend();
            foreach (var item in list)
            {
                string mobilenumber = "+88" + Convert.ToString(item.ContactNo);
                string msg = @"Dear Guardian,According to our record,your children has been Presented in today's school session.
Thank you,
Executive HeadTeacher";
                string sender = "InfoSMS";
                RootObject results = SmsSend(sender, mobilenumber, msg);

                status = true;

            }



            if (status == true)
                TempData["success"] = "Message Send successfully.";

            TempData["success"] = "Message Failed";



            return new JsonResult { Data = new { status = status } };
        }

        // Get : Stuent Payment
        public ActionResult StudentPayment()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            return View();
        }
        // Post : Student Payment
        [HttpPost]
        public ActionResult SearchStudentPaymentList(string Std_id, string Department, string Semester, string Section)
        {
            //string strDDLValue = Sample;
            //int ro = 0;
            //if (roll != "")
            //{
            //    ro = Convert.ToInt32(roll);
            //}

            ViewBag.Semester = Semester;
            var student = new List<Models.Sp_StdAdvanceSearc_Result>();
            using (var dc = new MvcUMS.Models.SchooldbEntities())
            {
                if (Std_id != "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Std_id.Equals(Std_id)).ToList();
                else if (Department != "" && Semester != "" && Section != "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Department.Equals(Department) && a.Semester.Equals(Semester) && a.Section.Equals(Section)).ToList();
                else if (Std_id == "" && Semester != "" && Section != "")
                {
                    var students = from pr in sdb.CurrentAcademicInfo.ToList()
                                   join si in sdbs.Student_info.ToList() on pr.Std_id equals si.Std_id
                                   where (pr.Semester.Equals(Semester) && pr.Section.Equals(Section))

                                   select new Sp_StdAdvanceSearc_Result
                                   {
                                       Id = si.Id,
                                       Std_id = si.Std_id,
                                       Department = pr.Department,
                                       Section = pr.Section,
                                       Semester = pr.Semester,
                                       Std_FName = si.Std_FName,
                                       Std_MName = si.Std_MName,
                                       Std_LName = si.Std_LName,
                                       RollNo = pr.RollNo,
                                       Father_Name = si.Father_Name
                                   };
                    return View(students.ToList());

                }
                else
                    student = dc.Sp_StdAdvanceSearc().ToList();
            }
            return View(student);
        }
        /// <summary>
        /// get studentlist by studentid dpatatment semester section 
        /// </summary>
        /// <param name="term"></param>
        /// <param name="dep"></param>
        /// <param name="sem"></param>
        /// <param name="sec"></param>
        /// <returns></returns>
        public JsonResult getStudentIdData(string term, string dep, string sem, string sec)
        {

            var users = new List<Models.CurrentAcademicInfo>();

            users = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Contains(term.ToLower())
                    && x.Semester.Contains(sem.ToLower()) && x.Section.Contains(sec.ToLower())).ToList();

            return Json(users, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Payment(string id)
        {

            StudentPayment model = new StudentPayment();
            var st = ssc.StudentScholarship.Where(x => x.StudentId.Equals(id)).Select(x => x.ScholarshipTypeId).SingleOrDefault();
            var at = ssc.StudentScholarship.Where(x => x.StudentId.Equals(id)).Select(x => x.Amount).SingleOrDefault();
            var type = ssc.ScholarshipType.Where(x => x.Id.Equals(st)).Select(x => x.TypeName).SingleOrDefault();
            TempData["amount"] = at;
            TempData["type"] = type;

            var payamount = sdb.StudentPayment.Where(s => s.StudentId.Equals(id)).Sum(x => (decimal?)x.PayAnount) ?? 0;

            var amount = sdb.StudentPayment.Where(s => s.StudentId.Equals(id)).Sum(x => (decimal?)x.Amount) ?? 0;

            var dueamount = amount - payamount;
            var advanceamount = payamount - amount;
            model.StudentId = id;
            if (dueamount > 0)
            {
                model.DueAnount = dueamount;
                model.advancepay = 0;
            }
            else
            {
                model.DueAnount = 0;
                model.advancepay = advanceamount;
            }


            ViewBag.PaymentTypeList = new SelectList(paymentcontext.PaymentType, "Id", "TypeName");

            model.Date = DateTime.Now.Date;
            return View(model);

        }


        //[HttpPost]
        //public ActionResult Payment(StudentPayment model,string amt)
        //{
        //    Decimal amo=0;
        //    if(amt!=null)
        //    {
        //        amo = Convert.ToDecimal(amt);
        //    }
        //    else
        //    {
        //        amo = 0;
        //    }
        //        model.Amount = model.TotalAnount ;
        //    if(amo>0)
        //    {
        //        if (amo < model.Amount)
        //        {
        //            model.PayAnount = amo + model.CashPay + model.BankPay;
        //        }
        //        else
        //        {
        //            model.PayAnount = model.TotalAnount;
        //        }
        //    }
        //    else
        //    {
        //        model.PayAnount = model.CashPay + model.BankPay;
        //    }

        //        model.Date = DateTime.Now.Date;
        //        model.Id = Guid.NewGuid();
        //        if (model.PaymentTypeId == null)
        //        {
        //            model.PaymentTypeId = Guid.Empty;
        //        }
        //        model.EmployeeId = Guid.Empty;
        //        sdb.StudentPayment.Add(model);
        //        sdb.SaveChanges();
        //        if (amo > 0)
        //        {
        //            var b = ssc.StudentScholarship.Where(x => x.StudentId.Equals(model.StudentId)).SingleOrDefault();
        //            if (amo < model.Amount)
        //            {
        //                b.Amount = amo;
        //            }
        //            else
        //            {
        //                b.Amount = b.Amount - model.PayAnount;
        //            }
        //            ssc.Entry(b).State = EntityState.Modified;
        //            ssc.SaveChanges();
        //        }
        //        if (model.TypeId == Guid.Empty)
        //        {
        //            return RedirectToAction("StudentAdmissionInvoceRpt", new { Id = model.StudentId });
        //        }
        //        else
        //        {
        //            return RedirectToAction("StudentInvoceRpt", new { Id = model.StudentId, amu = amo });
        //        }


        //}

        public ActionResult StudentAdmissionInvoceRpt(string Id)
        {
            string userid = User.Identity.Name;
            int schoolid = uc.UserProfiles.Where(x => x.UserName.Equals(userid)).Select(x => x.SchoolId ?? 0).SingleOrDefault();
            string schoolname = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.SchoolName).SingleOrDefault();
            string address = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.Address + " " + x.City + " " + x.Country).SingleOrDefault();
            StudentPayment model = new Models.StudentPayment();
            var studenpayment = from pr in sdb.StudentPayment.ToList()
                                where (pr.StudentId.Equals(Id))
                                select new
                                {
                                    Date = pr.Date.ToShortDateString(),
                                    af = pr.AdmissionFee,
                                    df = pr.DevelopmentFee,
                                    lif = pr.LibraryFee,
                                    laf = pr.LabFee,
                                    trs = pr.TransportFee,
                                    saf = pr.SMSAlartFee,
                                    ca = pr.CashPay,
                                    bp = pr.BankPay
                                };

            string name = sdbs.Student_info.Where(x => x.Std_id.Equals(Id)).Select(x => x.Std_FName).SingleOrDefault();
            string lastname = sdbs.Student_info.Where(x => x.Std_id.Equals(Id)).Select(x => x.Std_LName).SingleOrDefault();
            string mname = sdbs.Student_info.Where(x => x.Std_id.Equals(Id)).Select(x => x.Std_MName).SingleOrDefault();

            string studentname = name + " " + mname + " " + lastname;
            //string department = sdb.CurrentAcademicInfo.Where(x=>x.Std_id.Equals(Id)).Select(x=>x.Department).SingleOrDefault();
            //string semester = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Equals(Id)).Select(x => x.Semester).SingleOrDefault();
            //string section = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Equals(Id)).Select(x => x.Section).SingleOrDefault();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Report"), "AdmissionPaymentReport.rpt"));
            rd.SetDataSource(studenpayment);

            rd.SetParameterValue("studentname", studentname.ToString());
            rd.SetParameterValue("schoolname", schoolname.ToString());
            rd.SetParameterValue("address", address.ToString());
            //rd.SetParameterValue("section", section.ToString());

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        /// <summary>
        /// Student details Payment View by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //[HttpGet]
        //public ActionResult PaymentView(string id, int? Page_No)
        //{

        //    var studentinfo = from pr in sdb.StudentPayment.ToList()
        //                      join p in paymentcontext.PaymentType.ToList() on pr.TypeId equals p.Id
        //                      join si in sdbs.Student_info.ToList() on pr.StudentId equals si.Std_id
        //                      orderby pr.Date
        //                      where (pr.StudentId.Equals(id))
        //                      select new StudentPaytypeStuff { studentpayment=pr,paymenttype=p ,studentinfo=si};

        //    TempData["Department"] = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Equals(id)).Select(x=>x.Department).SingleOrDefault();
        //    TempData["Semester"] = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Equals(id)).Select(x => x.Semester).SingleOrDefault();
        //    TempData["Section"] = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Equals(id)).Select(x => x.Section).SingleOrDefault();
        //    string sfn= sdbs.Student_info.Where(x => x.Std_id.Equals(id)).Select(x => x.Std_FName).SingleOrDefault();
        //    string  smn= sdbs.Student_info.Where(x => x.Std_id.Equals(id)).Select(x => x.Std_MName ).SingleOrDefault();
        //    string sln = sdbs.Student_info.Where(x => x.Std_id.Equals(id)).Select(x => x.Std_LName).SingleOrDefault();
        //    var s = ssc.StudentScholarship.Where(x => x.StudentId.Equals(id)).Select(x => x.ScholarshipTypeId).SingleOrDefault();
        //    var amount = ssc.StudentScholarship.Where(x => x.StudentId.Equals(id)).Select(x => x.Amount).SingleOrDefault();
        //    var type = ssc.ScholarshipType.Where(x => x.Id.Equals(s)).Select(x => x.TypeName).SingleOrDefault();
        //    TempData["amount"] = amount;
        //    TempData["type"]=type;
        //    TempData["studentname"] = sfn +" "+ smn +" "+ sln;
        //    TempData["studentid"] = id;

        //    int Size_Of_Page = 10;
        //    int No_Of_Page = (Page_No ?? 1);
        //    return View(studentinfo.ToPagedList(No_Of_Page, Size_Of_Page));
        //    //return View(studentinfo.ToList());



        //}

        /// <summary>
        /// student payment voucher print
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        //public ActionResult StudentInvoceRpt(string Id,string amu)
        //{
        //    string userid = User.Identity.Name;
        //    int schoolid = uc.UserProfiles.Where(x => x.UserName.Equals(userid)).Select(x => x.SchoolId??0).SingleOrDefault();
        //    string schoolname = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.SchoolName).SingleOrDefault();
        //    string address = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.Address + " " + x.City + " " + x.Country).SingleOrDefault();
        //    decimal amou = 0;
        //    decimal amount = 0;
        //     var type="";
        //     if (amu != null)
        //     {
        //         amou = Convert.ToDecimal(amu);
        //     }
        //     else
        //     {
        //         amou = 0;
        //     }
        //    var studenpayment = from pr in sdb.StudentPayment.ToList()
        //                        join p in paymentcontext.PaymentType.ToList() on pr.TypeId equals p.Id
        //                        where (pr.StudentId.Equals(Id) && pr.Date.Equals(DateTime.Now.Date))
        //                                 select new  { Date = pr.Date.ToShortDateString(), TypeName = p.TypeName, PayAnount = pr.PayAnount };
        //    DateTime date = DateTime.Now.Date;
        //    string name = sdbs.Student_info.Where(x => x.Std_id.Equals(Id)).Select(x => x.Std_FName).SingleOrDefault();
        //    string lastname = sdbs.Student_info.Where(x => x.Std_id.Equals(Id)).Select(x => x.Std_LName).SingleOrDefault();
        //    string mname = sdbs.Student_info.Where(x => x.Std_id.Equals(Id)).Select(x => x.Std_MName).SingleOrDefault();

        //    string studentname = name + " " + mname + " " + lastname;
        //    string department = sdb.CurrentAcademicInfo.Where(x=>x.Std_id.Equals(Id)).Select(x=>x.Department).SingleOrDefault();
        //    string semester = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Equals(Id)).Select(x => x.Semester).SingleOrDefault();
        //    string section = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Equals(Id)).Select(x => x.Section).SingleOrDefault();
        //    string cashpay = sdb.StudentPayment.Where(x => x.StudentId.Equals(Id) && x.Date.Equals(date)).Sum(x => x.CashPay).ToString();
        //    string bankpay = sdb.StudentPayment.Where(x => x.StudentId.Equals(Id) && x.Date.Equals(date)).Sum(x => x.BankPay).ToString();

        //    if (amou > 0)
        //    {
        //        var s = ssc.StudentScholarship.Where(x => x.StudentId.Equals(Id)).Select(x => x.ScholarshipTypeId).SingleOrDefault();
        //         amount = ssc.StudentScholarship.Where(x => x.StudentId.Equals(Id)).Select(x => x.Amount).SingleOrDefault();
        //         type = ssc.ScholarshipType.Where(x => x.Id.Equals(s)).Select(x => x.TypeName).SingleOrDefault();
        //    }

        //    ReportDocument rd = new ReportDocument();
        //    rd.Load(Path.Combine(Server.MapPath("~/Report"), "Rpt_studentPayment.rpt"));
        //    rd.SetDataSource(studenpayment);

        //    rd.SetParameterValue("studentname", studentname.ToString());
        //    rd.SetParameterValue("department", department.ToString());
        //    rd.SetParameterValue("semester", semester.ToString());
        //    rd.SetParameterValue("section", section.ToString());
        //    rd.SetParameterValue("cashpay", cashpay.ToString());
        //    rd.SetParameterValue("bankpay", bankpay.ToString());
        //    rd.SetParameterValue("schoolname", schoolname.ToString());
        //    rd.SetParameterValue("address", address.ToString());
        //    if (amou > 0)
        //    {
        //        rd.SetParameterValue("amount", amount.ToString());
        //        rd.SetParameterValue("type", type.ToString());
        //    }
        //    else
        //    {
        //        rd.SetParameterValue("amount", "");
        //        rd.SetParameterValue("type", "");

        //    }

        //    Response.Buffer = false;
        //    Response.ClearContent();
        //    Response.ClearHeaders();
        //    try
        //    {
        //        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
        //        stream.Seek(0, SeekOrigin.Begin);
        //        return File(stream, "application/pdf");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }

        //}

        /// <summary>
        /// Student Payment Description Details
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        //public ActionResult StudentPaymentPrint(string Id)
        //{
        //    string userid = User.Identity.Name;
        //    int schoolid = uc.UserProfiles.Where(x => x.UserName.Equals(userid)).Select(x => x.SchoolId??0).SingleOrDefault();
        //    string schoolname = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.SchoolName).SingleOrDefault();
        //    string address = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.Address + " " + x.City + " " + x.Country).SingleOrDefault();
        //    var studenpayment = from pr in sdb.StudentPayment.ToList()
        //                        join p in paymentcontext.PaymentType.ToList() on pr.TypeId equals p.Id
        //                        where (pr.StudentId.Equals(Id))
        //                        select new { Date = pr.Date.ToShortDateString(),Amount=pr.Amount, TypeName = p.TypeName, PayAnount = pr.PayAnount, DueAmount =  pr.PayAnount-pr.Amount  };

        //    string name = sdbs.Student_info.Where(x => x.Std_id.Equals(Id)).Select(x => x.Std_FName).SingleOrDefault();
        //    string lastname = sdbs.Student_info.Where(x => x.Std_id.Equals(Id)).Select(x => x.Std_LName).SingleOrDefault();
        //    string mname = sdbs.Student_info.Where(x => x.Std_id.Equals(Id)).Select(x => x.Std_MName).SingleOrDefault();

        //    string studentname = name + " " + mname + " " + lastname;
        //    string department = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Equals(Id)).Select(x => x.Department).SingleOrDefault();
        //    string semester = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Equals(Id)).Select(x => x.Semester).SingleOrDefault();
        //    string section = sdb.CurrentAcademicInfo.Where(x => x.Std_id.Equals(Id)).Select(x => x.Section).SingleOrDefault();


        //    ReportDocument rd = new ReportDocument();
        //    rd.Load(Path.Combine(Server.MapPath("~/Report"), "StudentPaymentCrystalReport.rpt"));
        //    rd.SetDataSource(studenpayment);

        //    rd.SetParameterValue("studentname", studentname.ToString());
        //    rd.SetParameterValue("department", department.ToString());
        //    rd.SetParameterValue("semester", semester.ToString());
        //    rd.SetParameterValue("section", section.ToString());
        //    rd.SetParameterValue("schoolname", schoolname.ToString());
        //    rd.SetParameterValue("address", address.ToString());

        //    Response.Buffer = false;
        //    Response.ClearContent();
        //    Response.ClearHeaders();
        //    try
        //    {
        //        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
        //        stream.Seek(0, SeekOrigin.Begin);
        //        return File(stream, "application/pdf");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        // }
        /// <summary>
        /// get due amount by student id
        /// </summary>
        /// <param name="typeid"></param>
        /// <param name="studentid"></param>
        /// <returns></returns>
        //public ActionResult Gettypeamount(string typeid, string studentid)
        //{
        //    Guid type;
        //    if(typeid==null)
        //    {
        //        type = Guid.Empty;
        //    }
        //     type = new Guid(typeid);

        //    var departname = sdb.CurrentAcademicInfo.Where(s => s.Std_id.Equals(studentid)).Select(s => s.Department).SingleOrDefault();
        //    var semestername = sdb.CurrentAcademicInfo.Where(s => s.Std_id.Equals(studentid)).Select(s => s.Semester).SingleOrDefault();
        //    var deptid = sdb.Department.Where(s => s.Dep_name.Equals(departname)).Select(s => s.Id).SingleOrDefault();
        //    var semid = sdb.Semester.Where(s => s.Semester_name.Equals(semestername)).Select(s => s.Id).SingleOrDefault();
        //    var amount = paymentcontext.PaymentAmount.Where(x=>x.SemesterId.Equals(semid) && x.DepartmentId.Equals(deptid)&& x.TypeId.Equals(type)).Select(x=>x.Amount).SingleOrDefault();
        //    if(amount==null)
        //    {
        //        amount = 0;
        //    }


        //    return new JsonResult {Data=amount, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        //}

        /// <summary>
        /// amount to due amount calculation
        /// </summary>
        /// <param name="payamount"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public ActionResult Getautocomplete(string payamount, string amount)
        {
            Decimal pa = Convert.ToDecimal(payamount);
            Decimal a = Convert.ToDecimal(amount);
            var s = a - pa;
            return new JsonResult { Data = s, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }
        public ActionResult GetautocTotalAmount(string smsamount, string transport, string adminssion, string develop, string libary, string lab)
        {
            Decimal sa, t, ad, de, l, la;
            if (smsamount == null)
            {
                sa = 0;
            }
            else
            {
                sa = Convert.ToDecimal(smsamount);
            }
            if (transport == null)
            {
                t = 0;
            }
            else
            {
                t = Convert.ToDecimal(transport);
            }

            if (adminssion == null)
            {
                ad = 0;
            }
            else
            {
                ad = Convert.ToDecimal(adminssion);
            }
            if (develop == null)
            {
                de = 0;
            }
            else
            {
                de = Convert.ToDecimal(develop);
            }
            if (libary == null)
            {
                l = 0;
            }
            else
            {
                l = Convert.ToDecimal(libary);
            }
            if (lab == null)
            {
                la = 0;
            }
            else
            {
                la = Convert.ToDecimal(lab);
            }

            var s = sa + t + ad + de + l + la;
            return new JsonResult { Data = s, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }
        private void GenerateThumbnails(double scaleFactor, Stream sourcePath, string targetPath)
        {
            using (var image = System.Drawing.Image.FromStream(sourcePath))
            {
                int newWidth;
                int newHeight;

                newWidth = 512;
                newHeight = 384;

                var thumbnailImg = new Bitmap(newWidth, newHeight);
                var thumbGraph = Graphics.FromImage(thumbnailImg);
                thumbGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                thumbGraph.DrawImage(image, imageRectangle);
                if (System.IO.File.Exists(targetPath))
                {
                    System.IO.File.Delete(targetPath);
                }

                thumbnailImg.Save(targetPath, image.RawFormat);
            }
        }

        public ActionResult StudentPaymentEntry(string id = null)
        {
            ViewBag.advancedsearch = "advancedsearch";
            TempData["stdId"] = id;
            var ps = acdb.PaymentSatup.Where(a => a.Std_id.Equals(id)).FirstOrDefault();
            return View(ps);
        }

        [HttpPost]
        public ActionResult StudentPaymentEntry(PaymentSatup stp)
        {
            ViewBag.advancedsearch = "advancedsearch";
            if (ModelState.IsValid)
            {
                try
                {
                    acdb.PaymentSatup.Add(stp);
                    acdb.SaveChanges();
                    return RedirectToAction("StudentPaymentEntry", new { id = stp.Std_id });
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }
            return View(stp);
        }

        public ActionResult StudentPEntry(PaymentSatup stp)
        {
            var lastPay = from pay in acdb.StudentPaymentDetails.ToList()
                          join plist in acdb.PaymentList.ToList() on pay.Id equals plist.PayId
                          join tution in acdb.TutionFees.ToList() on plist.Id equals tution.PayListId
                          where pay.std_id.Equals(stp.Std_id)
                          orderby tution.PayMonth descending
                          select tution.PayMonth;
            if (lastPay.Count() != 0)
            {
                TempData["lastPDate"] = lastPay.Take(1).FirstOrDefault().AddMonths(1);
            }
            else
            {
                TempData["lastPDate"] = stp.SessionStartMonth;
            }
            return View(stp);
        }

        [HttpPost]
        public ActionResult StudentPEntry(string Std_id, string[] ids, string[] month, string[] payM, string Tution, string Admision, string Lab, string Development, string Exam, string Library, string Others, string Late, string textCash, string textBank, string Remarks)
        {
            StudentPaymentDetails stdPay = new StudentPaymentDetails();
            stdPay.std_id = Std_id;
            stdPay.PayDate = DateTime.Now;
            foreach (string pm in payM)
            {
                if (pm == "cash")
                {
                    stdPay.CashPay = Convert.ToDecimal(textCash);
                }
                else if (pm == "bank")
                {
                    stdPay.BankPay = Convert.ToDecimal(textBank);
                    stdPay.Remarks = Remarks;
                }
            }
            acdb.StudentPaymentDetails.Add(stdPay);
            acdb.SaveChanges();
            stdPay.Id = acdb.StudentPaymentDetails.Max(a => a.Id);
            foreach (string plist in ids)
            {
                PaymentList pl = new PaymentList();
                pl.PayId = stdPay.Id;
                if (plist == "1")
                {
                    pl.PayType = "Tution Fee";
                    pl.Amount = Convert.ToDecimal(Tution);
                    acdb.PaymentList.Add(pl);
                    acdb.SaveChanges();
                    foreach (var i in month)
                    {
                        TutionFees tf = new TutionFees();
                        tf.PayListId = acdb.PaymentList.Max(a => a.Id);
                        tf.PayMonth = Convert.ToDateTime(i);
                        acdb.TutionFees.Add(tf);
                        acdb.SaveChanges();
                    }
                }
                else if (plist == "2")
                {
                    pl.PayType = "Admission Fee";
                    pl.Amount = Convert.ToDecimal(Admision);
                    acdb.PaymentList.Add(pl);
                    acdb.SaveChanges();
                }
                else if (plist == "3")
                {
                    pl.PayType = "Lab Fee";
                    pl.Amount = Convert.ToDecimal(Lab);
                    acdb.PaymentList.Add(pl);
                    acdb.SaveChanges();
                }

                else if (plist == "4")
                {
                    pl.PayType = "Development Fee";
                    pl.Amount = Convert.ToDecimal(Development);
                    acdb.PaymentList.Add(pl);
                    acdb.SaveChanges();
                }

                else if (plist == "5")
                {
                    pl.PayType = "Exame Fee";
                    pl.Amount = Convert.ToDecimal(Exam);
                    acdb.PaymentList.Add(pl);
                    acdb.SaveChanges();
                }

                else if (plist == "6")
                {
                    pl.PayType = "Library Fee";
                    pl.Amount = Convert.ToDecimal(Library);
                    acdb.PaymentList.Add(pl);
                    acdb.SaveChanges();
                }
                else if (plist == "7")
                {
                    pl.PayType = "Others Fee";
                    pl.Amount = Convert.ToDecimal(Others);
                    acdb.PaymentList.Add(pl);
                    acdb.SaveChanges();
                }
                else if (plist == "8")
                {
                    pl.PayType = "Late Fine";
                    pl.Amount = Convert.ToDecimal(Late);
                    acdb.PaymentList.Add(pl);
                    acdb.SaveChanges();
                }
            }
            return RedirectToAction("StudentPayment");
            //return RedirectToAction("StudentPaymentEntryPrint", new StudentPaymentDetails { Id = stdPay.Id, PayDate = stdPay.PayDate,std_id=stdPay.std_id, CashPay = stdPay.CashPay, BankPay = stdPay.BankPay, Remarks = stdPay.Remarks });
        }
        public ActionResult StudentPaymentEntryPrint(StudentPaymentDetails sp)
        {
            List<PaymentList> plist = acdb.PaymentList.Where(a => a.PayId.Equals(sp.Id)).ToList();
            int tpId = plist.Where(a => a.PayType.Equals("Tution Fee")).Select(a => a.Id).FirstOrDefault();
            if (tpId != null)
            {
                List<TutionFees> tp = acdb.TutionFees.Where(a => a.PayListId.Equals(tpId)).ToList();

            }
            var stdIno = from std in sdbs.Student_info.ToList()
                         join cinfo in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals cinfo.Std_id
                         where std.Std_id.Equals(sp.std_id)
                         select new StudentInfoForPayment
                         {
                             stdId = std.Std_id,
                             FullName = std.Std_FName + " " + " " + std.Std_MName + " " + std.Std_LName,
                             FatherName = std.Father_Name,
                             Class = cinfo.Semester,
                             Section = cinfo.Section,
                             RolNo = cinfo.RollNo
                         };
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Report"), "Rpt_studentPaymentPrint.rpt"));

            rd.Database.Tables[0].SetDataSource(stdIno);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public ActionResult StudentDetailsForPayment(string StdId)
        {
            var list = new StudentInfoForPayment();
            var stdIno = from std in sdbs.Student_info.ToList()
                         join cinfo in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals cinfo.Std_id
                         where std.Id.Equals(new Guid(StdId))
                         select new StudentInfoForPayment
                         {
                             stdId = std.Std_id,
                             FullName = std.Std_FName + " " + " " + std.Std_MName + " " + std.Std_LName,
                             FatherName = std.Father_Name,
                             Class = cinfo.Semester,
                             Section = cinfo.Section,
                             RolNo = cinfo.RollNo
                         };

            list = stdIno.FirstOrDefault();

            return View(stdIno.FirstOrDefault());
        }

        public ActionResult UpdatePaymentSetup(string std_id)
        {
            var ps = acdb.PaymentSatup.Where(a => a.Std_id.Equals(std_id)).FirstOrDefault();
            return View(ps);
        }

        [HttpPost]
        public ActionResult UpdatePaymentSetup(PaymentSatup stp)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    acdb.Entry(stp).State = EntityState.Modified;
                    acdb.SaveChanges();
                    return RedirectToAction("StudentPaymentEntry", new { id = stp.Std_id });
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }
            return View(stp);
        }

        public ActionResult StdPaymentDetailsShow(string std_id)
        {
            ViewBag.advancedsearch = "advancedsearch";
            TempData["std_id"] = std_id;
            return View();
        }

        public ActionResult StdViewPayment(string std_id, string StartDate, string EndDate)
        {
            DateTime dateStart = new DateTime();
            DateTime DateEnd = new DateTime();
            if (StartDate == "" && EndDate == "")
            {

                dateStart = DateTime.Now.Date;
                DateEnd = DateTime.Now.Date;
            }
            else
            {
                dateStart = Convert.ToDateTime(StartDate);
                DateEnd = Convert.ToDateTime(EndDate);
            }
            ViewBag.stdId = std_id;
            //var pay = acdb.StudentPaymentDetails.Where(a => a.std_id.Equals(std_id) && a.PayDate >= dateStart && a.PayDate <= DateEnd).ToList();
            var pay = acdb.StudentsPayment.Where(a => a.StudentId.Equals(std_id) && a.PaymentDate >= dateStart.Date && a.PaymentDate <= DateEnd.Date).ToList();
            return View(pay);
        }

        public ActionResult PaymentTypeView(int id)
        {
            var plist = acdb.PaymentList.Where(a => a.PayId.Equals(id)).ToList();
            return View(plist);
        }

        public ActionResult ViewTutionMonth(int id)
        {
            var pmonth = acdb.TutionFees.Where(a => a.PayListId.Equals(id)).ToList();
            return View(pmonth);
        }
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        public ActionResult notAuthorize()
        {
            return View();
        }
        [HttpPost]
        public void SaveTxtData(string data)
        {
            string filePath = Server.MapPath("~/App_Data/StudentUser.txt"); // Path to your desired text file
            string existingContent = System.IO.File.ReadAllText(filePath);
            string currdata = existingContent + Environment.NewLine + data;
            // Write the data to the text file
            System.IO.File.WriteAllText(filePath, currdata);
        }
    }
}
