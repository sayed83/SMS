using MvcUMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MvcUMS.Controllers
{
    public class EventController : Controller
    {
        private StuffContext sc = new StuffContext();
        EventContext ec = new EventContext();
        StudentContext sdb = new StudentContext();
        UMSEntities1 sdbs = new UMSEntities1();
        StudentPayment db = new StudentPayment();
        UMSEntities2 stdAttan = new UMSEntities2();
        StudentAttendenceContex matt = new StudentAttendenceContex();

        ScolarshipContext ssc = new ScolarshipContext();
        private PaymentContext paymentcontext = new PaymentContext();
        private RutineContext rc = new RutineContext();

        private ExamContext et = new ExamContext();
      
       
        //
        // GET: /Event/

        public ActionResult Index()
        {
            //ViewBag.basicdatatable = "basicdatatable";
            ViewBag.advancedsearch = "advancedsearch";
            var events = ec.Events.ToList();
            
           
            return View(events);
        }

        public ActionResult Create()
        {
            ViewBag.studentscripts = "specificscripts";
            return View();
        }
        [HttpPost]
        public ActionResult Create(Events model)
        {
            ViewBag.studentscripts = "specificscripts";
           
            if (ModelState.IsValid)
            {

                string filename = "";

                if (model.picture != null && model.picture.ContentLength > 0)
                {
                    filename = Path.GetFileName(Guid.NewGuid() + "." + model.picture.FileName.Split('.')[1]);
                    string targetPath = Server.MapPath("../uploads//" + filename);
                    Stream strm = model.picture.InputStream;
                    var targetFile = targetPath;

                    GenerateThumbnails(0.5, strm, targetFile);
                }
                

                model.Id = Guid.NewGuid();
                model.StuffId = Guid.Empty;
                model.Image = filename;
                ec.Events.Add(model);
                ec.SaveChanges();
                return RedirectToAction("Index");
            }
               
            return View();
        }

      

        public ActionResult EventEdit(Guid id)
        {
            var evnt=ec.Events.Where(x=>x.Id.Equals(id)).FirstOrDefault();
            return View(evnt);
        }

        [HttpPost]
        public ActionResult EventEdit(Events model)
        {
            var evnt = ec.Events.Where(x => x.Id.Equals(model.Id)).FirstOrDefault();
            if (evnt == null)
            {
                return HttpNotFound();
            }

            if (model.picture != null && model.picture.ContentLength > 0)
            {
                string filename = "";
                filename = Path.GetFileName(Guid.NewGuid() + "." + model.picture.FileName.Split('.')[1]);
                model.Image = filename;
                string targetPath = Server.MapPath("../uploads//" + filename);
                Stream strm = model.picture.InputStream;
                var targetFile = targetPath;

                GenerateThumbnails(0.5, strm, targetFile);
            }

            //model.StuffId = Guid.Empty;
            evnt.EventName = model.EventName;
            evnt.EventFor = model.EventFor;
            evnt.OrganizeDate = model.OrganizeDate;
            evnt.Description = model.Description;
            evnt.Image = model.Image;
            ec.Entry(evnt).State = EntityState.Modified;
            ec.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Event_Delete(Guid id)
        {  
            var eventId=ec.Events.Where(a=>a.Id == id).FirstOrDefault();
            ec.Events.Remove(eventId);
            ec.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Scolarship()
        {
            ViewBag.advancedsearch = "advancedsearch";
            var student = new List<Models.Sp_StdAdvanceSearc_Result>();
            using (var dc = new MvcUMS.Models.SchooldbEntities())
            {
                ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
                ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
                student = dc.Sp_StdAdvanceSearc().ToList();
            }
            return View(student);
        }
        [HttpPost]
        public ActionResult ScolarshipSearchList(string Std_id, string Department, string Semester, string Section, string roll)
        {
            int ro = 0;
            if(roll!="")
            {
                ro = Convert.ToInt32(roll);
            }
            var student = new List<Models.Sp_StdAdvanceSearc_Result>();
            using (var dc = new MvcUMS.Models.SchooldbEntities())
            {
                if (Std_id != "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Std_id.Equals(Std_id)).ToList();
                else if (Department == "" && Semester != "" && Section == "" && roll=="")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Semester.Equals(Semester)).ToList();
                else if (Department == "" && Semester != "" && Section != ""  && roll=="")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Semester.Equals(Semester) && a.Section.Equals(Section)).ToList();
                else if (Department == "" && Semester != "" && Section != "" && roll != "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Semester.Equals(Semester) && a.Section.Equals(Section) && a.RollNo.Equals(ro)).ToList();
                else if (Department != "" && Semester != "" && Section != "" && roll == "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Department.Equals(Department) && a.Semester.Equals(Semester) && a.Section.Equals(Section)).ToList();
                else if (Department != "" && Semester != "" && Section != "" && roll != "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Department.Equals(Department) && a.Semester.Equals(Semester) && a.Section.Equals(Section) && a.RollNo.Equals(ro)).ToList();
                else
                    student = dc.Sp_StdAdvanceSearc().ToList();
            }
            return View(student);
        }


        public ActionResult ScolarShipDetail(string id)
        {
            StudentScholarship sss = new StudentScholarship();
            sss.StudentId = id;
            ViewBag.scolershiptype = new SelectList(ssc.ScholarshipType.Where(x=>x.isactive.Equals(true)),"Id","TypeName");
            return View(sss);
        }
         [HttpPost]
        public ActionResult UpdateSchlership(StudentScholarship sss, string Command, int scolershiptype)
        {
               sss.Id = Guid.NewGuid();
               sss.Date = DateTime.Now.Date;
               sss.isactive = true;
               sss.ScholarshipTypeId = scolershiptype;
               //sss.StuffId = Guid.Empty;
           
                ssc.StudentScholarship.Add(sss);
                ssc.SaveChanges();

                return RedirectToAction("Scolarship");
        }
     
        
        public ActionResult ScholarshipList(int? Page_No)
         {
             var students = from pr in ssc.StudentScholarship.ToList()
                                 join ps in ssc.ScholarshipType.ToList() on pr.ScholarshipTypeId equals ps.Id
                                 select new studentScholarshiptype { scholarship=pr, type=ps };

             int Size_Of_Page = 5;
             int No_Of_Page = (Page_No ?? 1);
             return View(students.ToPagedList(No_Of_Page, Size_Of_Page));
            
         }

        [HttpGet]
        public ActionResult StudentDetails(string id)
        {
            studentScholarshiptype students =new studentScholarshiptype();
            students.stuinfo.Std_FName = "Karim";
            students.stuinfo.Std_MName = "Uddin";
            
            return View(students);
        }
        

        public ActionResult ScholarshiplistPrint()
        {
            var scholarshiplist = from pr in ssc.StudentScholarship.ToList()
                           join ps in ssc.ScholarshipType.ToList() on pr.ScholarshipTypeId equals ps.Id
                           select new  {studentid=pr.StudentId,type=ps.TypeName,date=pr.Date.ToShortDateString(),remarks=pr.Remarks,amount=pr.Amount };
       
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Report"), "RptScholarshipList.rpt"));
            rd.SetDataSource(scholarshiplist);


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
            return View();
        }


        [HttpGet]
        public ActionResult CreateRutine()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            ViewBag.Section = new SelectList(sdb.Section, "section_name", "section_name");
            var sa = from pr in rc.TimeShedule.ToList()
                     select new
                     {
                         Id = pr.Id,
                         Name =pr.StartTime + "-" + pr.EndTime
                       
                     };

            ViewBag.starttime = new SelectList(sa.ToList(), "Id", "Name");
            ViewBag.Week = new SelectList(rc.Week, "Id", "Name");
            
            return View();
        }
        [HttpPost]
        public ActionResult CreateRutine(Rutine model)
        {

            if (ModelState.IsValid)
            {
                var s = rc.Rutine.Where(x => x.SubjectTeacherId.Equals(model.SubjectTeacherId) && x.WeekId.Equals(model.WeekId)).Count();
                var teachrlist = from pr in rc.Rutine.ToList()
                                 join ts in sdb.TeacherSubject.ToList() on pr.SubjectTeacherId equals ts.Id
                                 where ts.TeacherId.Equals(model.TeacherId) && pr.timeId.Equals(model.timeId) && pr.WeekId.Equals(model.WeekId)
                                 select new
                                 {
                                     Id = pr.Id
                                 };
                var t = teachrlist.ToList().Count();

                var subcode = sdb.TeacherSubject.Where(x => x.Id.Equals(model.SubjectTeacherId)).Select(x => x.SubjectCode).SingleOrDefault();
                var sec = sdb.TeacherSubject.Where(x => x.Id.Equals(model.SubjectTeacherId)).Select(x => x.Section).SingleOrDefault();
                var d = sdb.Course.Where(x => x.CourseId.Equals(subcode)).Select(x => x.Department).SingleOrDefault();
                var c = sdb.Course.Where(x => x.CourseId.Equals(subcode)).Select(x => x.Semester).SingleOrDefault();
                var countsems = from pr in rc.Rutine.ToList()
                                join ts in sdb.TeacherSubject.ToList() on pr.SubjectTeacherId equals ts.Id
                                join ps in sdb.Course.ToList() on ts.SubjectCode equals ps.CourseId
                                where pr.timeId.Equals(model.timeId) && pr.WeekId.Equals(model.WeekId)
                                select new
                                {
                                    Department = ps.Department,
                                    Semester = ps.Semester,
                                    Section = ts.Section

                                };
                var count = countsems.ToList().Where(x => x.Department.Equals(d) && x.Semester.Equals(c) && x.Section.Equals(sec)).Count();

                if (s > 0)
                {
                    TempData["error"] = "Alrady Course Code Assigned.";
                }
                else if (t > 0)
                {
                    TempData["error"] = "Alrady assigned this time schdule for this teacher.";
                }
                else if (count > 0)
                {
                    TempData["error"] = "Alrady assigned The time for this Semester.";
                }
                else
                {
                    model.Id = Guid.NewGuid();
                    model.CreationDate = DateTime.Now.Date;
                    model.StuffId = Guid.Empty;
                    model.RoomId = Guid.Empty;
                    rc.Rutine.Add(model);
                    rc.SaveChanges();
                }
            }
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            ViewBag.Section = new SelectList(sdb.Section, "section_name", "section_name");
            ViewBag.Week = new SelectList(rc.Week, "Id", "Name");
            var sa = from pr in rc.TimeShedule.ToList()
                     select new
                     {
                         Id = pr.Id,
                         Name = pr.StartTime + "-" + pr.EndTime
                     };

            ViewBag.starttime = new SelectList(sa.ToList(), "Id", "Name");

            return View();
        }

        public ActionResult IsTeacherIsBusy(Guid TeacherId, int timeId, int WeekId)
        {
            var teachrlist = from pr in rc.Rutine.ToList()
                             join ts in sdb.TeacherSubject.ToList() on pr.SubjectTeacherId equals ts.Id
                             where ts.TeacherId.Equals(TeacherId) && pr.timeId.Equals(timeId) && pr.WeekId.Equals(WeekId)
                             select new
                             {
                                 Id = pr.Id
                             };
            var t = teachrlist.ToList().Count();
            return Json(t,JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteRutine(string Class, string Group, string section, int Day, int Time)
        {
            if (Group == null)
            {
                Group = "0";
            }
            var data = from ts in sdb.TeacherSubject.ToList()
                       join c in sdb.Course.ToList() on ts.SubjectCode equals c.CourseId
                       join r in rc.Rutine.ToList() on ts.Id equals r.SubjectTeacherId
                       where c.Department.Equals(Group) && c.Semester.Equals(Class) && ts.Section.Equals(section) && r.timeId.Equals(Time) && r.WeekId.Equals(Day)
                       select new 
                       {
                           r.Id
                       };
            var t = data.FirstOrDefault();
            Rutine rt = rc.Rutine.First(a=>a.Id.Equals(t.Id));
            rc.Rutine.Remove(rt);
            rc.SaveChanges();
            return RedirectToAction("RotineList", new { Department = Group, Semester = Class, Section = section });
        }

        public ActionResult CreateDayRutine(string Class, string Group, string section, int Day, int Time)
        {
            if (string.IsNullOrEmpty(Group))
            {
                Group = "0";
            }
            var TeacherName = from s in sc.Stuff.ToList()
                              join ts in sdb.TeacherSubject.ToList() on s.Id equals ts.TeacherId
                              join c in sdb.Course.ToList() on ts.SubjectCode equals c.CourseId
                              where c.Department.Equals(Group) && c.Semester.Equals(Class) && ts.Section.Equals(section)
                              group s by new { ts.TeacherId, s.LastName, s.FirstName } into dp
                              select new
                              {
                                  Value = dp.Key.TeacherId,
                                  Name = dp.Key.FirstName + " " + dp.Key.LastName
                              };

            ViewBag.objdata = new SelectList(TeacherName.ToList(), "Value", "Name", 0);
            TempData["Day"] = Day;
            TempData["Time"] = Time;
            TempData["Class"] = Class;
            TempData["Group"] = Group;
            TempData["section"] = section;
            return View();
        }

        [HttpPost]
        public ActionResult CreateDayRutine(Rutine model,string Department,string Semester,string Section)
        {
            model.Id = Guid.NewGuid();
            model.CreationDate = DateTime.Now.Date;
            model.StuffId = Guid.Empty;
            model.RoomId = Guid.Empty;
            rc.Rutine.Add(model);
            rc.SaveChanges();
            return RedirectToAction("RotineList", new { Department = Department, Semester = Semester, Section = Section });
        }

        [HttpGet]
        public ActionResult SearchRutine()
        {
            ViewBag.advancedsearch = "advancedsearch";
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");

            if (User.IsInRole("Teacher"))
            {
                var sar = from pr in sc.Stuff.ToList()
                          where pr.Email == User.Identity.Name
                          select new
                          {
                              Id = pr.Id,
                              Name = pr.FirstName + " " + pr.LastName
                          };
                ViewBag.teacherid = new SelectList(sar.ToList(), "Id", "Name");     
            }
            else
            {
                var sar = from pr in sc.Stuff.ToList()
                          join ps in sc.StuffDepartment.ToList() on pr.Department equals ps.DepartmentId
                          where ps.DepartmentName.Equals("Teacher")
                          select new
                          {
                              Id = pr.Id,
                              Name = pr.FirstName + " " + pr.LastName
                          };
                ViewBag.teacherid = new SelectList(sar.ToList(), "Id", "Name");     
            }

            
               
          
            return View();
        }
      

        public ActionResult RotineList(string Department, string Semester, string Section, Guid? teacherid)
        {
            if (User.IsInRole("Student"))
            {
                Department = sdb.CurrentAcademicInfo.Where(x => x.Std_id == User.Identity.Name).Select(x => x.Department??"").FirstOrDefault();
                Semester = sdb.CurrentAcademicInfo.Where(x => x.Std_id == User.Identity.Name).Select(x => x.Semester).FirstOrDefault();
                Section = sdb.CurrentAcademicInfo.Where(x => x.Std_id == User.Identity.Name).Select(x => x.Section).FirstOrDefault();
            }
            //else { Department = Department; Semester = Semester; Section = Section; }

            ViewData["Department"] = Department;
            ViewData["Semester"] = Semester;
            ViewData["Section"] = Section;
            var sd = new List<RoutineView>();
            var rlist = from r in rc.Rutine.ToList()
                        join ts in rc.TimeShedule.ToList() on r.timeId equals ts.Id
                        join w in rc.Week.ToList() on r.WeekId equals w.Id
                        join tes in sdb.TeacherSubject.ToList() on r.SubjectTeacherId equals tes.Id
                        join c in sdb.Course.ToList() on tes.SubjectCode equals c.CourseId
                        join t in sc.Stuff.ToList() on tes.TeacherId equals t.Id
                        select new
                        {
                            r.timeId,
                            ts.StartTime,
                            ts.EndTime,
                            c.CourseId,
                            c.CourseTitle,
                            c.Department,
                            c.Semester,
                            w.Name,
                            tes.Section,
                            t.FirstName,
                            t.LastName,
                            tes.TeacherId

                        };
            if (Department != "" && Semester != "" && Section != "")
            {
                var routinelist = from r in rlist.ToList()
                                  where (r.Section.Equals(Section) && r.Department.Equals(Department) && r.Semester.Equals(Semester))
                                  orderby r.timeId
                                  group r by new {r.timeId, r.StartTime, r.EndTime} into g
                                  select new RoutineView
                                  {
                                      timeId=g.Key.timeId,
                                      time = g.Key.StartTime + " - " + g.Key.EndTime,
                                      Saturday = g.Where(w => w.Name == "Saturday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Sunday = g.Where(w => w.Name == "Sunday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Monday = g.Where(w => w.Name == "Monday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Tuesday = g.Where(w => w.Name == "Tuesday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Wednesday = g.Where(w => w.Name == "Wednesday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Thusday = g.Where(w => w.Name == "Thuesday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Friday = g.Where(w => w.Name == "Friday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),

                                  };
                sd = routinelist.ToList();
            }
            else if (Department == "" && Semester != "" && Section != "")
            {
                var routinelist = from r in rlist.ToList()
                                  where (r.Section.Equals(Section) && r.Semester.Equals(Semester))
                                  orderby r.timeId
                                  group r by new { r.timeId, r.StartTime, r.EndTime } into g
                                  select new RoutineView
                                  {
                                      timeId = g.Key.timeId,
                                      time = g.Key.StartTime + " - " + g.Key.EndTime,
                                      Saturday = g.Where(w => w.Name == "Saturday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Sunday = g.Where(w => w.Name == "Sunday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Monday = g.Where(w => w.Name == "Monday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Tuesday = g.Where(w => w.Name == "Tuesday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Wednesday = g.Where(w => w.Name == "Wednesday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Thusday = g.Where(w => w.Name == "Thuesday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Friday = g.Where(w => w.Name == "Friday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),

                                  };
                sd = routinelist.ToList();
            }
            else if (teacherid != null)
            {
                var routinelist = from r in rlist.ToList()
                                  where (r.TeacherId.Equals(teacherid))
                                  orderby r.timeId
                                  group r by new { r.timeId, r.StartTime, r.EndTime } into g
                                  select new RoutineView
                                  {
                                      timeId = g.Key.timeId,
                                      time = g.Key.StartTime + " - " + g.Key.EndTime,
                                      Saturday = g.Where(w => w.Name == "Saturday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Sunday = g.Where(w => w.Name == "Sunday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Monday = g.Where(w => w.Name == "Monday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Tuesday = g.Where(w => w.Name == "Tuesday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Wednesday = g.Where(w => w.Name == "Wednesday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Thusday = g.Where(w => w.Name == "Thuesday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      Friday = g.Where(w => w.Name == "Friday").Select(x => x.CourseTitle + " (" + x.CourseId + " ) " + Environment.NewLine + x.FirstName + " " + x.LastName).FirstOrDefault(),

                                  };
                sd = routinelist.ToList();
            }
            List<TimeShedule> tshedule = rc.TimeShedule.ToList();
            var tuple = new Tuple<List<RoutineView>, List<TimeShedule>>(sd, tshedule);
            var TeacherName = from s in sc.Stuff.ToList()
                              join ts in sdb.TeacherSubject.ToList() on s.Id equals ts.TeacherId
                              join c in sdb.Course.ToList() on ts.SubjectCode equals c.CourseId
                              where c.Department.Equals(Department) && c.Semester.Equals(Semester) && ts.Section.Equals(Section)
                              group s by new { ts.TeacherId, s.LastName, s.FirstName } into dp
                              select new
                              {
                                  Value = dp.Key.TeacherId,
                                  Name = dp.Key.FirstName + " " + dp.Key.LastName
                              };

            ViewBag.objdata = new SelectList(TeacherName.ToList(), "Value", "Name", 0);
            return View(tuple);
        }

        /// <summary>
        /// time shedulling
        /// </summary>

        /// <returns></returns>
        /// 
        public ActionResult TimeScheduleList()
        {
            return View(rc.TimeShedule.ToList());
        }

        public ActionResult TimeShedule()
        {
            return View();
        }
        [HttpPost]
        public ActionResult TimeShedule(TimeShedule model)
        {
            model.Date = DateTime.Now.Date;
            
                rc.TimeShedule.Add(model);
                rc.SaveChanges();

                return RedirectToAction("TimeScheduleList");
        }

        public ActionResult TimeEdit(int id)
        {
            var time = rc.TimeShedule.Where(x => x.Id == id).FirstOrDefault();
            return View(time);
        }

        [HttpPost]
        public ActionResult TimeEdit(TimeShedule model)
        {
            var time = rc.TimeShedule.Where(x => x.Id == model.Id).FirstOrDefault();
            time.StartTime = model.StartTime;
            time.EndTime = model.EndTime;
            rc.Entry(time).State = EntityState.Modified;
            rc.SaveChanges();
            return RedirectToAction("TimeScheduleList");
        }

        public ActionResult TimeDelete(int id)
        {
            var time = rc.TimeShedule.Where(x => x.Id == id).FirstOrDefault();
            rc.TimeShedule.Remove(time);
            rc.SaveChanges();
            return RedirectToAction("TimeScheduleList");
        }

           [HttpPost]
        public JsonResult GetTeacherCode(string id, string idd,string sec)
        {

               if(id=="")
               {
                   id = "0";
               }
            var TeacherName = from s in sc.Stuff.ToList()
                              join ts in sdb.TeacherSubject.ToList() on s.Id equals ts.TeacherId
                              join c in sdb.Course.ToList() on ts.SubjectCode equals c.CourseId
                              where c.Department.Equals(id) && c.Semester.Equals(idd) && ts.Section.Equals(sec)
                              group s by new { ts.TeacherId, s.LastName, s.FirstName } into dp
                              select new
                              {
                                  Value = dp.Key.TeacherId,
                                  Name = dp.Key.FirstName + " " + dp.Key.LastName
                              };

            SelectList objdata = new SelectList(TeacherName.ToList(), "Value", "Name", 0);

          
            return new JsonResult { Data = objdata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
           [HttpPost]
           public JsonResult GetSubjectCode(Guid id, string ids, string idd, string sec)
           {
               if(idd=="")
               {
                   idd = "0";
               }
               var subjectlist = from ts  in sdb.TeacherSubject.ToList()
                                 join c in sdb.Course.ToList() on ts.SubjectCode equals c.CourseId
                                 where (ts.TeacherId.Equals(id) && c.Department.Equals(idd) && c.Semester.Equals(ids) && ts.Section.Equals(sec) && c.IsActive==true && ts.IsActive==true)
                                 select new
                                 {
                                     Value = ts.Id,
                                     Name = c.CourseTitle + " ( " + c.CourseId + " )"
                                 };

               SelectList objdata = new SelectList(subjectlist.ToList(), "Value", "Name", 0);


               return new JsonResult { Data = objdata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
           }

        

        public ActionResult DonationCreate()
           {
               var model = new Donation();
               model.CashPay = 0;
               model.BankPay = 0;
               model.Date = DateTime.Now.Date;
               return View(model);
           }
        [HttpPost]
        public ActionResult DonationCreate(Donation model)
        {
            var s = ec.Donation.Where(x => x.Checknumber.Equals(model.Checknumber)).Count();
            if (s > 0)
            {

            }
            else
            {
                if (ModelState.IsValid)
                {
                    model.StuffId = Guid.Empty;
                    ec.Donation.Add(model);
                    ec.SaveChanges();
                    return RedirectToAction("DonationIndex");
                }
            }
            return View();
        }
        public ActionResult DonationIndex()
        {
            ViewBag.advancedsearch = "advancedsearch";
            //var student = new List<Models.Student_info>();
            //using (var dc = new MvcUMS.Models.UMSEntities1())
            //{
            //    student = dc.Student_info.ToList();
            //}
            //return View(student);

            var sd = new List<Models.Donation>();
            using (var sdlist = new MvcUMS.Models.UMSEntities1())
            {
                sd = ec.Donation.ToList();
            }
            //var sdlist = ec.Donation.ToList();
            //sd = sdlist.ToList();
            //int Size_Of_Page = 10;
           // int No_Of_Page = (Page_No ?? 1);
            return View(sd);
        }

        public ActionResult DonationEdit(int id)
        {
             Donation movie = ec.Donation.Find(id);

             return View(movie); 
        }
        [HttpPost]
        public ActionResult DonationEdit(Donation model)
        {
            var s=ec.Donation.Where(x=>x.Checknumber.Equals(model.Checknumber)).Count();
            if (s > 0)
            {

            }
            else
            {
                var b = ec.Donation.Where(x => x.Id.Equals(model.Id)).SingleOrDefault();
                b.BankName = model.BankName;
                b.BankPay = model.BankPay;
                b.CashPay = model.CashPay;
                b.Checknumber = model.Checknumber;
                b.Date = model.Date;
                b.Id = model.Id;
                b.ProviderName = model.ProviderName;
                b.ReciverName = model.ReciverName;
                b.StuffId = Guid.Empty;

                ec.Entry(b).State = EntityState.Modified;
                ec.SaveChanges();

                return RedirectToAction("DonationIndex");
            }
            return View();
        }



        public ActionResult SearchExamRutine()
        {
            ViewBag.ExamType = new SelectList(et.ExamType, "Id", "Examtype");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            return View();
        }

        [HttpPost]
        public ActionResult SearchExamRutine(int ExamType, string Semester,string Department)
        {
            ViewBag.ExamType = new SelectList(et.ExamType, "Id", "Examtype");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            if (ExamType != 0 && Semester!="")
            {
                TempData["list"] = "list";
            }
            return View();
        }

        public ActionResult ListExamRoutine(int ExamType, string Semester , string Department)
        {
            var model = from ex in et.Exam.ToList()
                        join st in et.ExamType.ToList() on ex.ExamtypeId equals st.Id
                        join c in sdb.Course.ToList() on ex.CourseId equals c.Id
                        join s in sc.Stuff.ToList() on ex.StuffId equals s.Id
                        where ex.ExamtypeId == ExamType && c.Semester == Semester
                        select new ExamAndType { ex = ex, et = st, co = c, stuff = s };

            return View(model.ToList());
        }

        public ActionResult ExamRoutineList(int? Page_No, int ExamType, string Department)
        {


            if (Department == null)
            {
                Department = "0";
            }
            var sd = new List<ExamRoutineView>();
            var exlist = from e in et.Exam.ToList()
                         join ex in et.ExamType.ToList() on e.ExamtypeId equals ex.Id
                         join c in sdb.Course.ToList() on e.CourseId equals c.Id
                         where e.ExamtypeId.Equals(ExamType) && c.Department.Equals(Department)
                         select new
                         {
                             coursecode = e.CourseId,
                             coursetitle = c.CourseTitle,
                             department = c.Department ?? "0",
                             semester = c.Semester,
                             time = e.StartTime + " - " + e.EndTime,
                             date = Convert.ToString(e.Date.ToShortDateString()),

                         };
            var exroutinelist = from ex in exlist.ToList()
                                orderby ex.date
                                group ex by new { ex.coursecode, ex.date } into g
                                select new ExamRoutineView
                                {
                                    time = g.Key.date,
                                    first = g.Where(w => w.semester == "Pre-Nursery").Min(x => x.coursetitle + " (" + x.time + " ) "),
                                    second = g.Where(w => w.semester == "Nursery").Min(x => x.coursetitle + " (" + x.time + " ) "),
                                    third = g.Where(w => w.semester == "K.G.").Min(x => x.coursetitle + " (" + x.time + " ) "),
                                    fourth = g.Where(w => w.semester == "One").Min(x => x.coursetitle + " (" + x.time + " ) "),
                                    fifth = g.Where(w => w.semester == "Two").Min(x => x.coursetitle + " (" + x.time + " ) "),
                                    six = g.Where(w => w.semester == "Three").Min(x => x.coursetitle + " (" + x.time + " ) "),
                                    seven = g.Where(w => w.semester == "Four").Min(x => x.coursetitle + " (" + x.time + " ) "),
                                    eight = g.Where(w => w.semester == "five").Min(x => x.coursetitle + " (" + x.time + " ) ")


                                };
            sd = exroutinelist.ToList();
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(sd.ToPagedList(No_Of_Page, Size_Of_Page));
        }

        public ActionResult Noticeboard()
        {
            ViewBag.advancedsearch = "advancedsearch";
           
            if (User.IsInRole("Student"))
            {
                var list = new List<Notice>();
                var noticeStudent = (from std in ec.Notices.ToList()
                                     where std.NoticeFor == "Student"
                                     select new Notice { Title= std.Title, Description= std.Description, Date= std.Date,  NoticeFor= std.NoticeFor, Id= std.Id })
                                      .Union
                                      (from std2 in ec.Notices.ToList()
                                       where std2.NoticeFor == "All"
                                       select new Notice { Title = std2.Title, Description = std2.Description, Date = std2.Date, NoticeFor = std2.NoticeFor, Id = std2.Id });
                list = noticeStudent.ToList();
                return View(list);
            }
            else if (User.IsInRole("Teacher"))
            {
                var noticeTeacher = (from std in ec.Notices.ToList()
                                     where std.NoticeFor == "Teacher"
                                     select new Notice { Title = std.Title, Description = std.Description, Date = std.Date, NoticeFor = std.NoticeFor, Id = std.Id })
                                      .Union
                                      (from std2 in ec.Notices.ToList()
                                       where std2.NoticeFor == "All"
                                       select new Notice { Title = std2.Title, Description = std2.Description, Date = std2.Date, NoticeFor = std2.NoticeFor, Id = std2.Id });
                return View(noticeTeacher);
            }

            var notices = ec.Notices.ToList();


            return View(notices);
        }

        public ActionResult CreateNotice()
        {
            ViewBag.studentscripts = "specificscripts";

            var noticeBoard = new List<SelectListItem>
            {
                new SelectListItem{Text="All",Value="All"},
                new SelectListItem{Text="Teachers",Value="Teacher"},
                new SelectListItem{Text="Students",Value="Student"}
            };

            ViewData["noticeBoard"] = noticeBoard;

            return View();
        }

        [HttpPost]
        public ActionResult CreateNotice(Notice notice)
        {
            if (ModelState.IsValid)
            {
                ViewBag.studentscripts = "specificscripts";

                string filename = "";

                if (notice.picture != null && notice.picture.ContentLength > 0)
                {
                    filename = Path.GetFileName(Guid.NewGuid() + "." + notice.picture.FileName.Split('.')[1]);
                    string targetPath = Server.MapPath("../uploads//" + filename);
                    Stream strm = notice.picture.InputStream;
                    var targetFile = targetPath;

                    GenerateThumbnails(0.5, strm, targetFile);
                }

                ec.Notices.Add(notice);
                notice.Image = filename;
                ec.SaveChanges();
                return RedirectToAction("Noticeboard");
            }

            return View();
        }

        public ActionResult EditNotice(int id)
        {
            var notice = ec.Notices.Where(x => x.Id == id).FirstOrDefault();
            if (notice == null)
                return HttpNotFound();

            var noticeBoard = new List<SelectListItem>
            {
                new SelectListItem{Text="All",Value="All"},
                new SelectListItem{Text="Teachers",Value="Teacher"},
                new SelectListItem{Text="Students",Value="Student"}
            };

            ViewData["noticeBoard"] = noticeBoard;

            return View(notice);
        }

        [HttpPost]
        public ActionResult EditNotice(Notice model)
        {
            var notice = ec.Notices.Where(x => x.Id == model.Id).FirstOrDefault();
            if (notice == null)
                return HttpNotFound();

            if (model.picture != null && model.picture.ContentLength > 0)
            {
                string filename = "";
                filename = Path.GetFileName(Guid.NewGuid() + "." + model.picture.FileName.Split('.')[1]);
                model.Image = filename;
                string targetPath = Server.MapPath("~/uploads//" + filename);
                Stream strm = model.picture.InputStream;
                var targetFile = targetPath;

                GenerateThumbnails(0.5, strm, targetFile);
            }


            notice.Title = model.Title;
            notice.Description = model.Description;
            notice.Date = model.Date;
            notice.NoticeFor = model.NoticeFor;
            notice.Image = model.Image;
            ec.Entry(notice).State = EntityState.Modified;
            ec.SaveChanges();

            return RedirectToAction("Noticeboard");
        }

        public ActionResult Notice_Delete(int id)
        {
            var notice = ec.Notices.Where(a => a.Id == id).FirstOrDefault();
            ec.Notices.Remove(notice);
            ec.SaveChanges();
            return RedirectToAction("Noticeboard");
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
    }
}
