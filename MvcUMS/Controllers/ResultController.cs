using CrystalDecisions.CrystalReports.Engine;
using MvcEnergyPac.Models;
using MvcUMS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using MvcUMS.Extensions;
using System.Text;
using MvcUMS.Models.EntityManager;
using System.Reflection;
using System.Dynamic;
using System.Data.SqlClient;
using MvcUMS.Models.ViewModels;

namespace MvcUMS.Controllers
{
    public class ResultController : Controller
    {
        RutineContext rc = new RutineContext();
        StudentContext sc = new StudentContext();
        StuffContext stc = new StuffContext();
        ExamContext ec = new ExamContext();
        UMSEntities1 sdbs = new UMSEntities1();
        UsersContext uc = new UsersContext();
        EventContext evc = new EventContext();

        ResultManager RM = new ResultManager();
        CommonManager CM = new CommonManager();

        //
        // GET: /Result/

        public ActionResult Index()
        {
            ViewBag.basicdatatable = "advancedsearch";
            return View();
        }

        public ActionResult SubjectMark(string Department, string Semester, int? Subject, Guid? TeacherList)
        {
            ViewBag.advancedsearch = "advancedsearch";
            if (Department != "" && !String.IsNullOrEmpty(Semester))
            {
                TempData["list"] = "List";
                ViewBag.subjectid = Subject;

            }

            var getclass = from pr in sc.Class.ToList()
                           select new
                           {
                               Value = pr.ClassName,
                               Name = pr.ClassName
                           };


            var classs = from e in sc.Semester.ToList()
                         select new
                         {
                             Value = e.Semester_name,
                             Name = e.Semester_name
                         };


            ViewBag.ClassList = new SelectList(classs.ToList(), "Value", "Name");

            var dept = from e in sc.Department.ToList()
                       select new
                       {
                           Value = e.Dep_name,
                           Name = e.Dep_name
                       };


            ViewBag.DeptList = new SelectList(dept.ToList(), "Value", "Name");


            var list = from e in ec.ExamType.ToList()
                       select new
                       {
                           Id = e.Id,
                           Name = e.Examtype
                       };

            ViewBag.examtypeList = new SelectList(list.ToList(), "Id", "Name");

            if (User.IsInRole("Teacher"))
            {

                var teacherName = from pr in stc.Stuff.ToList()
                                  where pr.Email == User.Identity.Name
                                  select new
                                  {
                                      Id = pr.Id,
                                      Name = pr.FirstName + " " + pr.LastName
                                  };
                ViewBag.TeacherList = new SelectList(teacherName.ToList(), "Id", "Name");
            }
            else
            {
                var sar = from pr in stc.Stuff.ToList()
                          join ps in stc.StuffDepartment.ToList() on pr.Department equals ps.DepartmentId
                          where ps.DepartmentName.Equals("Teacher")
                          select new
                          {
                              Id = pr.Id,
                              Name = pr.FirstName + " " + pr.LastName
                          };
                ViewBag.TeacherList = new SelectList(sar.ToList(), "Id", "Name");
            }

            ViewBag.Subject = new SelectList(sc.Course.ToList(), "Id", "CourseTitle");


            return View();
        }

        public ActionResult TeacherDistribudedList(string Department, string Semester, int Subject, Guid TeacherList, int Examtypeid)
        {
            ViewBag.subject = Subject;
            ViewBag.teacher = TeacherList;
            ViewBag.semester = Semester;
            ViewBag.examnameid = Examtypeid;
            if (Department == "0")
            {
                Department = "";
            }
            var student = new List<Models.Sp_StdAdvanceSearc_Result>();

            using (var dc = new MvcUMS.Models.SchooldbEntities())
            {

                student = dc.Sp_StdAdvanceSearc().Where(a => a.Department.Equals(Department) && a.Semester.Equals(Semester)).ToList();
            }

            var studentsubjectmarks = from s in student.ToList()
                                      join m in ec.StudentSubjectMarks.Where(x => x.SubjectCode == Subject) on s.Std_id equals m.StudentId into sm
                                      from m in sm.DefaultIfEmpty()

                                      select new studentsubjectmarks
                                      {
                                          studentid = s.Std_id,
                                          stuentname = s.Std_FName + " " + s.Std_MName + " " + s.Std_LName,
                                          section = s.Section,
                                          ClassTest = m == null ? 0 : m.ClassTest,
                                          Written = m == null ? 0 : m.Written,
                                          Practical = m == null ? 0 : m.Practical,
                                          MCQ = m == null ? 0 : m.MCQ
                                      };

            return View(studentsubjectmarks.ToList());
        }

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }


        public ActionResult DynamicIndex(int CourseId, string Semester)
        {
            ViewBag.subject = CourseId;
            //ViewBag.teacher = TeacherList;
            ViewBag.semester = Semester;
            ViewBag.examnameid = 1;

            using (var ctx = new SchoolNewDBEntities())
            using (var cmd = ctx.Database.Connection.CreateCommand())
            {
                ctx.Database.Connection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "StudentMarkInput";
                cmd.Parameters.Add(new SqlParameter("@CourseId", CourseId));
                cmd.Parameters.Add(new SqlParameter("@strSmt", Semester));
                using (var reader = cmd.ExecuteReader())
                {
                    //var model = new StudenInputViewModel();
                    var model = RM.Read(reader).ToList();
                    return View(model);
                }
            }
        }


        public ActionResult MarkStudentList(int CourseId, string Semester)
        {
            ViewBag.subject = CourseId;
            //ViewBag.teacher = TeacherList;
            ViewBag.semester = Semester;
            ViewBag.examnameid = 1;

            DataTable dt = RM.GetStudentMark(CourseId, Semester);




            //List<DataRow> list = dt.AsEnumerable().ToList();

            List<MarkFormViewModel> student = new List<MarkFormViewModel>();
            student = ConvertDataTable<MarkFormViewModel>(dt);

            //List<MarkDetailsViewModel> studentDetails = new List<MarkDetailsViewModel>();
            //studentDetails = ConvertDataTable<MarkDetailsViewModel>(dt); 

            //List<MarkFormViewModel> MarkList = dt.ToList<MarkFormViewModel>();
            List<MarkDetailsViewModel> MarkDetails = dt.ToList<MarkDetailsViewModel>();

            var viewModel = new MarkInputView
            {
                MarkFormViewModel = student,
                MarkDetailsViewModel = MarkDetails
            };


            //foreach (DataRow dr in dt.Rows)
            //{

            //    MarkList.Add(dr);
            //}

            //List<MarkFormViewModel> lst = dt.AsEnumerable().ToList<MarkFormViewModel>();

            //foreach (var item in list)
            //{
            //    MarkList.Add(

            //       new MarkFormViewModel
            //       {
            //           StudentId=new Guid(item["StudentId"].ToString()),
            //           ExamId = Convert.ToInt32(item["ExamId"]),
            //           CourseId = Convert.ToInt32(item["CourseId"]),
            //           TotalMarks = Convert.ToDecimal(item["TotalMarks"]),
            //           MarkDetails = new List<MarkDetailsViewModel>
            //           {
            //               new MarkDetailsViewModel
            //                {
            //                     MarkTypeId=Convert.ToInt32(item["MarkTypeId"]),
            //                     Marks=Convert.ToDecimal(item["Marks"])
            //                }
            //           }

            //       });
            //}

            return View(viewModel);



        }

        [HttpPost]
        public ActionResult MarkStudentLists(int CourseId, string Semester, int ExamId, StudenInputViewModel model)
        {
            DataSet ds = RM.makePurchaseTable(CourseId, Semester);
            RM.InsertUpdatePurchase(ds, CourseId, ExamId);
            return RedirectToAction("SubjectMark");
        }


        public ActionResult StudentList(string Department, string Semester, string Section, int Subject, int Examtypeid)
        {
            ViewBag.subject = Subject;
            //ViewBag.teacher = TeacherList;
            ViewBag.semester = Semester;
            ViewBag.Section = Section;
            ViewBag.examnameid = Examtypeid;
            if (Department == "0")
            {
                Department = "";
            }
            var student = new List<Models.Sp_StdAdvanceSearc_Result>();

            using (var dc = new MvcUMS.Models.SchooldbEntities())
            {

                student = dc.Sp_StdAdvanceSearc().Where(a => a.Department.Equals(Department)
                    && a.Semester.Equals(Semester)
                    && a.Section.Equals(Section)).OrderBy(x=>x.RollNo).ToList();

            }



            //var studentsubjectmarks = (from s in student.ToList()
            //                           join m in ec.StudentSubjectMarks.Where(x => x.SubjectCode == Subject && x.ExamName == Examtypeid) on s.Std_id equals m.StudentId into sm
            //                           from m in sm.DefaultIfEmpty()
            //                           select new studentsubjectmarks
            //                           {
            //                               studentid = s.Std_id,
            //                               stuentname = s.Std_FName + " " + s.Std_MName + " " + s.Std_LName,
            //                               section = s.Section,
            //                               ClassTest = m == null ? 0 : m.ClassTest,
            //                               Written = m == null ? 0 : m.Written,
            //                               Practical = m == null ? 0 : m.Practical,
            //                               MCQ = m == null ? 0 : m.MCQ
            //                           }).ToList();


            var studentsubjectmarks = (from s in student.ToList()
                                       join m in ec.StudentSubjectMarks.Where(x => x.SubjectCode == Subject && x.ExamName == Examtypeid) on s.Std_id equals m.StudentId into sm
                                       from m in sm.DefaultIfEmpty()
                                       orderby s.Std_id
                                       select new studentsubjectmarks
                                       {
                                           studentid = s.Std_id,
                                           RollNo=s.RollNo??0,
                                           stuentname = s.Std_FName + " " + s.Std_MName + " " + s.Std_LName,
                                           section = s.Section,
                                           MarkNameViewModel = (from cm in ec.CourseMarkSetup.ToList()
                                                                join mt in ec.MarkType.ToList() on cm.MarkTypeId equals mt.Id
                                                                where cm.CourseId.Equals(Subject)
                                                                select new MarkNameViewModel
                                                                {
                                                                    Id = mt.Id,
                                                                    Name = mt.Name,
                                                                    Mark = cm.TotalMarks,
                                                                    PassMark = cm.PassMarks,
                                                                    ObtainMark = m == null ? 0 : (mt.Name == "ClassTest") ? m.ClassTest :
                                                                    (mt.Name == "Written") ? m.Written : (mt.Name == "Practical") ? m.Practical : m.MCQ
                                                                }).ToList()
                                       }).OrderBy(x=>x.RollNo).ToList();

            return View(studentsubjectmarks.ToList());



        }

        [HttpGet]
        public JsonResult IsMarkGreater(int Subjectid, decimal Mark)
        {
            bool isGreater = ec.CourseMarkSetup.Any(a => a.CourseId.Equals(Subjectid) && a.TotalMarks < Mark);
            return Json(!isGreater, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult MarksDistribution(string id, string subject, Guid teacher)
        {
            var model = new Marks();
            var sar = from pr in ec.Exam.ToList()
                      join ps in ec.ExamType.ToList() on pr.ExamtypeId equals ps.Id
                      where pr.CourseId.Equals(subject)
                      select new
                      {
                          Id = pr.Id,
                          Name = ps.Examtype
                      };
            ViewBag.ExamType = new SelectList(sar.ToList(), "Id", "Name");
            TempData["subject"] = subject;
            model.StudentId = id;
            model.TeacherId = teacher;

            return View(model);
        }

        [HttpPost]
        public ActionResult MarksDistributionUpdate(Marks model, string subject)
        {
            var department = sc.Course.Where(x => x.CourseId.Equals(subject)).Select(x => x.Department).SingleOrDefault();
            var semester = sc.Course.Where(x => x.CourseId.Equals(subject)).Select(x => x.Semester).SingleOrDefault();

            if (ModelState.IsValid)
            {
                var sar = ec.Exam.Where(x => x.Id.Equals(model.ExamId)).Select(x => x.ExamtypeId).SingleOrDefault();
                var ext = ec.ExamType.Where(x => x.Id.Equals(sar)).Select(x => x.Examtype).SingleOrDefault();

                var s = ec.Marks.Where(x => x.StudentId.Equals(model.StudentId) && x.ExamId.Equals(model.ExamId)).Count();
                if (s > 0)
                {
                    TempData["error"] = "Alrady Created.";
                }
                else
                {
                    model.StudentId = model.StudentId;
                    model.TeacherId = model.TeacherId;
                    model.Id = Guid.NewGuid();
                    model.Date = DateTime.Now.Date;
                    ec.Marks.Add(model);
                    ec.SaveChanges();

                }
            }

            return RedirectToAction("SubjectMark", new { Department = department, Semester = semester, Subject = subject, TeacherList = model.TeacherId });
        }
        public ActionResult SubjectMarks(string id, string subject, Guid teacher)
        {

            var model = new SubjectMarks();
            model.StudentId = id;
            model.SubjectCode = subject;
            TempData["teacher"] = teacher;
            return View(model);
        }

        [HttpGet]
        public JsonResult IsObtainMarksGreater(List<studentsubjectmarks> subjectmarks)
        {
            bool isGreater = false;


            //foreach (var item in subjectmarks)
            //{
            //    foreach (var score in item.MarkNameViewModel)
            //    {
            //        isGreater = ec.CourseMarkSetup.Any(a => a.TotalMarks < score.ObtainMark);
            //    }
            //}
            return Json(!isGreater, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SubjectMarks(int Subjectid, int examtype, List<studentsubjectmarks> subjectmarks)
        {
            decimal MaxMarks = 0;
            decimal totalObtainMark = 0;
            decimal totalGradePoint = 0;

            var courseID = sc.Course.Where(x => x.Id == Subjectid).Select(x => x.CourseId).FirstOrDefault();
            var courseClass = sc.Course.Where(x => x.Id == Subjectid).Select(x => x.Semester).FirstOrDefault();


            foreach (var item in subjectmarks)
            {
                var stdInfo = ec.StudentSubjectMarks.Where(a => a.StudentId == item.studentid && a.SubjectCode == Subjectid && a.ExamName == examtype).FirstOrDefault();
                if (stdInfo == null)
                {
                    var model = new StudentSubjectMarks();

                    model.Id = Guid.NewGuid();
                    model.ExamName = examtype;

                    model.StudentId = item.studentid;
                    model.SubjectCode = Subjectid;


                    foreach (var score in item.MarkNameViewModel)
                    {
                        if (score.Name == "ClassTest")
                        {
                            model.ClassTest = score.ObtainMark;
                        }
                        else if (score.Name == "Written")
                        {
                            model.Written = score.ObtainMark;
                        }
                        else if (score.Name == "MCQ")
                        {
                            model.MCQ = score.ObtainMark;
                        }
                        else if (score.Name == "Practical")
                        {
                            model.Practical = score.ObtainMark;
                        }

                    }

                    var fullMarks3 = sc.Course.Where(x => x.Id == Subjectid).Select(x => x.FullMarks).FirstOrDefault();

                    model.TotalMarks = item.MarkNameViewModel.Sum(x => x.ObtainMark);
                    totalObtainMark += model.TotalMarks;

                    if (courseClass=="Nine" && (courseID != "101" || courseID != "102" || courseID != "107" || courseID != "108"))
                    {
                        foreach (var score in item.MarkNameViewModel)
                        {
                            if (score.Name == "ClassTest" && score.ObtainMark<score.PassMark)
                            {
                                model.GradePoint = 0;
                            }
                            else if (score.Name == "Written" && score.ObtainMark < score.PassMark)
                            {
                                model.GradePoint = 0;
                            }
                            else if (score.Name == "MCQ" && score.ObtainMark < score.PassMark)
                            {
                                model.GradePoint = 0;
                            }
                            else if (score.Name == "Practical" && score.ObtainMark < score.PassMark)
                            {
                                model.GradePoint = 0;
                            }
                            else
                            {
                                model.GradePoint = getGP(model.TotalMarks, fullMarks3);
                                model.LetterGrade = GPAmark(model.GradePoint);
                            }

                        }
                    }
                    else if((courseClass=="Six" || courseClass=="Seven") && (courseID == "101" || courseID == "102" || courseID == "107" || courseID == "108"))
                    {
                        if (model.TotalMarks >= 33)
                            model.GradePoint = getGP(model.TotalMarks, fullMarks3);
                        else if (model.TotalMarks > 0 && model.TotalMarks < 33)
                        {
                            model.GradePoint = 1;
                        }
                        else
                            model.GradePoint = 0;

                        model.LetterGrade = GPAmark(model.GradePoint);
                    }
                    else
                    {
                        model.GradePoint = getGP(model.TotalMarks, fullMarks3);
                        model.LetterGrade = GPAmark(model.GradePoint);
                    }



                    totalObtainMark += model.GradePoint;

                    if (model.TotalMarks > MaxMarks)
                    {
                        MaxMarks = model.TotalMarks;
                    }

                    ec.StudentSubjectMarks.Add(model);

                    //var markSummery = new StudentMarksSummery();
                    //markSummery.StudentId = item.studentid;
                    //markSummery.ExamId = examtype;
                    //markSummery.TotalObtainMarks = totalObtainMark;
                    //markSummery.TotalGradePoint = totalGradePoint;

                }
                else
                {
                    //stdInfo.ClassTest = item.ClassTest;
                    //stdInfo.Written = item.Written;
                    //stdInfo.MCQ = item.MCQ;
                    //stdInfo.Practical = item.Practical;


                    var fullMarks2 = sc.Course.Where(x => x.Id == Subjectid).Select(x => x.FullMarks).FirstOrDefault();

                    foreach (var score in item.MarkNameViewModel)
                    {
                        if (score.Name == "ClassTest")
                        {
                            stdInfo.ClassTest = score.ObtainMark;
                        }
                        else if (score.Name == "Written")
                        {
                            stdInfo.Written = score.ObtainMark;
                        }
                        else if (score.Name == "MCQ")
                        {
                            stdInfo.MCQ = score.ObtainMark;
                        }
                        else if (score.Name == "Practical")
                        {
                            stdInfo.Practical = score.ObtainMark;
                        }

                    }

                    stdInfo.TotalMarks = item.MarkNameViewModel.Sum(x => x.ObtainMark);

                    if (courseClass == "Nine" && (courseID != "101" || courseID != "102" || courseID != "107" || courseID != "108"))
                    {
                        foreach (var score in item.MarkNameViewModel)
                        {
                            if (score.Name == "ClassTest" && score.ObtainMark < score.PassMark)
                            {
                                stdInfo.GradePoint = 0;
                                break;
                            }
                            else if (score.Name == "Written" && score.ObtainMark < score.PassMark)
                            {
                                stdInfo.GradePoint = 0;
                                break;
                            }
                            else if (score.Name == "MCQ" && score.ObtainMark < score.PassMark)
                            {
                                stdInfo.GradePoint = 0;
                                break;
                            }
                            else if (score.Name == "Practical" && score.ObtainMark < score.PassMark)
                            {
                                stdInfo.GradePoint = 0;
                                break;
                            }
                            else
                            {
                                stdInfo.GradePoint = getGP(stdInfo.TotalMarks, fullMarks2);
                                stdInfo.LetterGrade = GPAmark(stdInfo.GradePoint);
                            }

                        }
                    }
                    else if ((courseClass == "Six" || courseClass == "Seven") && (courseID == "101" || courseID == "102" || courseID == "107" || courseID == "108"))
                    {
                        if (stdInfo.TotalMarks >= 33)
                            stdInfo.GradePoint = getGP(stdInfo.TotalMarks, fullMarks2);
                        else if (stdInfo.TotalMarks > 0 && stdInfo.TotalMarks < 33)
                        {
                            stdInfo.GradePoint = 1;
                        }
                        else
                            stdInfo.GradePoint = 0;
                          

                        stdInfo.LetterGrade = GPAmark(stdInfo.GradePoint);
                    }
                    else
                    {
                        stdInfo.GradePoint = getGP(stdInfo.TotalMarks, fullMarks2);
                        stdInfo.LetterGrade = GPAmark(stdInfo.GradePoint);
                    }


                   
                    //stdInfo.GradePoint = getGP(stdInfo.TotalMarks, fullMarks2);
                  
                    if (stdInfo.TotalMarks > MaxMarks)
                    {
                        MaxMarks = stdInfo.TotalMarks;
                    }
                    ec.Entry(stdInfo).State = EntityState.Modified;

                }

                ec.SaveChanges();
            }

            var submarks = ec.SubjectHighestMarks.Where(a => a.ExamId == examtype && a.SubjectCode == Subjectid).FirstOrDefault();

            var fullMarks = sc.Course.Where(x => x.Id == Subjectid).Select(x => x.FullMarks).FirstOrDefault();

            if (submarks == null)
            {
                var shm = new SubjectHighestMarks();
                shm.ExamId = examtype;
                shm.SubjectCode = Subjectid;
                shm.HM = MaxMarks;
                shm.HGP = getGP(MaxMarks, fullMarks);
                ec.SubjectHighestMarks.Add(shm);
            }
            else
            {
                submarks.ExamId = examtype;
                submarks.SubjectCode = Subjectid;
                submarks.HM = MaxMarks;
                submarks.HGP = getGP(MaxMarks, fullMarks);
                ec.Entry(submarks).State = EntityState.Modified;
            }




            ec.SaveChanges();


            //return RedirectToAction("SubjectMark", new { Department = department, Semester = semester, Subject = model.SubjectCode, TeacherList = teacher });
            return RedirectToAction("SubjectMark");
        }

        public ActionResult ShowResult(string Department, string Semester, string Section, string Std_id)
        {

            var marksheetType = ec.MarksheetType.Where(x => x.IsActive == true).Select(x => x.MarksheetFor).FirstOrDefault();

            ViewBag.marksheetType = marksheetType;


            if (!String.IsNullOrEmpty(Semester) && !String.IsNullOrEmpty(Section))
            {
                TempData["list"] = "List";

            }

            var getclass = from pr in sc.Class.ToList()
                           select new
                           {
                               Value = pr.ClassName,
                               Name = pr.ClassName
                           };
            ViewBag.ClassList = new SelectList(getclass.ToList(), "Value", "Name");


            var list = from e in ec.ExamType.ToList()
                       select new
                       {
                           Id = e.Id,
                           Name = e.Examtype
                       };

            ViewBag.examtypeList = new SelectList(list.ToList(), "Id", "Name");
            ViewBag.Department = new SelectList(sc.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sc.Semester, "Semester_name", "Semester_name");
            ViewBag.Section = new SelectList(sc.Section, "section_name", "section_name");




            return View();
        }

        public ActionResult ResultView(string StudentId, int Examtypeid)
        {

            var model = new List<SubjectMarksCourse>();
            TempData["studentid"] = StudentId;
            TempData["examid"] = Examtypeid;
            if (Examtypeid == 1)
            {
                TempData["ex"] = "Half Yearly Examination.";
            }
            else if (Examtypeid == 2)
            {
                TempData["ex"] = "Final Examination.";
            }

            var shm = ec.SubjectHighestMarks.Where(x => x.ExamId.Equals(Examtypeid)).ToList();
            var totallist = from ca in ec.StudentSubjectMarks.ToList()
                            join c in sc.Course.ToList() on ca.SubjectCode equals c.Id
                            join ex in shm.ToList() on ca.SubjectCode equals ex.SubjectCode
                            orderby c.CourseId
                            where ca.StudentId.Equals(StudentId) && ca.ExamName.Equals(Examtypeid)
                            select new SubjectMarksCourse
                            {
                                ssm = ca,
                                c = c,
                                highestmarks = ex.HM,
                                hightgrade = ex.HGP
                            };
            model = totallist.ToList();
            return View(model);
        }

        //Common Marksheet
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        [HttpPost]
        public PartialViewResult MarkSheet(string StudentId, string Semester, string Section, string Department, int? Examtypeid)
        {
            var list = new MarksheetViewModel();
            TempData["studentid"] = StudentId;
            TempData["examid"] = Examtypeid;
            if (Examtypeid == 1)
            {
                TempData["ex"] = "Half Yearly Examination.";
            }
            else if (Examtypeid == 2)
            {
                TempData["ex"] = "Final Examination.";
            }

            var stdList = new List<string>();

            if (!string.IsNullOrEmpty(Department))
            {
                stdList = (from s in sdbs.Student_info.ToList()
                           join c in sc.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                           where (c.Semester == Semester && c.Section == Section && c.Department == Department) || c.Std_id == StudentId
                           select s.Std_id).ToList();
            }
            else
            {

                stdList = (from s in sdbs.Student_info.ToList()
                           join c in sc.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                           where (c.Semester == Semester && c.Section == Section) || c.Std_id == StudentId
                           select s.Std_id).ToList();
            }





            var slist = new List<MarksheetViewModel>();

            foreach (var item in stdList)
            {

                var data = new MarksheetViewModel
                {
                    SchoolViewModel = CM.GetSchoolInfo(),
                    GPAViewModels = CM.GetGPAInfo(),
                    StudentDetailsForMarksheetViewModel = CM.GetStudentInfo(item),
                    //SubjectMarksCourses = RM.GetStudentResult(item, Semester, Examtypeid ?? 0)
                };


                slist.Add(data);
            }
            
            return PartialView(slist);
        }



        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        [HttpPost]
        public PartialViewResult FullMarkSheet(string StudentId, string Semester, string Section, string Department, int? Examtypeid)
        {
            var list = new MarksheetViewModel();
            TempData["studentid"] = StudentId;
            TempData["examid"] = Examtypeid;
            TempData["ex"] = "All Examination.";

            var stdList = new List<string>();

            if (!string.IsNullOrEmpty(Department))
            {
                stdList = (from s in sdbs.Student_info.ToList()
                           join c in sc.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                           where (c.Semester == Semester && c.Section == Section && c.Department == Department) || c.Std_id == StudentId
                           select s.Std_id).ToList();
            }
            else
            {

                stdList = (from s in sdbs.Student_info.ToList()
                           join c in sc.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                           where (c.Semester == Semester && c.Section == Section) || c.Std_id == StudentId
                           select s.Std_id).ToList();
            }

            var slist = new List<MarksheetViewModel>();

            foreach (var item in stdList)
            {

                var data = new MarksheetViewModel
                {
                    SchoolViewModel = CM.GetSchoolInfo(),
                    GPAViewModels = CM.GetGPAInfo(),
                    StudentDetailsForMarksheetViewModel = CM.GetStudentInfo(item),
                    ResultReport = RM.GetStudentFullResult(item, Semester, Examtypeid ?? 0)
                };


                slist.Add(data);
            }

            return PartialView(slist);
        }

        //Eidgah Marksheet
        [HttpPost]
        public PartialViewResult EidgahMarkSheet(string StudentId, string Semester, string Section, string Department, int? Examtypeid,int? RollNo)
        {
            var list = new MarksheetViewModel();
            TempData["studentid"] = StudentId;
            TempData["examid"] = Examtypeid;
            if (Examtypeid == 1)
            {
                TempData["ex"] = "Half Yearly Examination.";
            }
            else if (Examtypeid == 2)
            {
                TempData["ex"] = "Final Examination.";
            }

            var stdList = new List<string>();

            if (!string.IsNullOrEmpty(Department))
            {
                if (RollNo != null && RollNo > 0)
                {
                    stdList = (from s in sdbs.Student_info.ToList()
                               join c in sc.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                               where c.Std_id == StudentId
                               || (c.Semester == Semester && c.Section == Section && c.Department == Department && c.RollNo == RollNo)
                               select s.Std_id).ToList();
                }
                else
                {
                    stdList = (from s in sdbs.Student_info.ToList()
                               join c in sc.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                               where (c.Semester == Semester && c.Section == Section && c.Department == Department) || c.Std_id == StudentId
                               select s.Std_id).ToList();
                }
            }
            else
            {
                if (RollNo != null && RollNo > 0)
                {
                    stdList = (from s in sdbs.Student_info.ToList()
                               join c in sc.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                               where c.Std_id == StudentId
                               || (c.Semester == Semester && c.Section == Section && c.RollNo == RollNo)
                               select s.Std_id).ToList();
                }
                else
                {
                    stdList = (from s in sdbs.Student_info.ToList()
                               join c in sc.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                               where (c.Semester == Semester && c.Section == Section) || c.Std_id == StudentId
                               select s.Std_id).ToList();
                }
            }


            var slist = new List<MarksheetViewModel>();

            foreach (var item in stdList)
            {

                var data = new MarksheetViewModel
                {
                    SchoolViewModel = CM.GetSchoolInfo(),
                    GPAViewModels = CM.GetGPAInfo(),
                    StudentDetailsForMarksheetViewModel = CM.GetStudentInfo(item),
                    SubjectMarksCourses = RM.GetStudentResult(item, Semester, Examtypeid ?? 0),
                };


                slist.Add(data);
            }

            var orderlist = slist.OrderByDescending(x => x.SubjectMarksCourses.Select(y => y.TotalObMarks).FirstOrDefault()).ToList();


            return PartialView(orderlist);
        }



        [HttpPost]
        public PartialViewResult EidgahFullMarkSheet(string StudentId, string Semester, string Section, string Department, int? Examtypeid,int? RollNo)
        {
            var list = new MarksheetViewModel();
            TempData["studentid"] = StudentId;
            TempData["examid"] = Examtypeid;
            if (Examtypeid == 1)
            {
                TempData["ex"] = "Half Yearly Examination.";
            }
            else if (Examtypeid == 2)
            {
                TempData["ex"] = "Final Examination.";
            }

            var stdList = new List<string>();

            if (!string.IsNullOrEmpty(Department))
            {
                if (RollNo != null && RollNo > 0)
                {
                    stdList = (from s in sdbs.Student_info.ToList()
                               join c in sc.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                               where c.Std_id == StudentId
                               || (c.Semester == Semester && c.Section == Section && c.Department == Department && c.RollNo == RollNo)
                               select s.Std_id).ToList();
                }
                else
                {
                    stdList = (from s in sdbs.Student_info.ToList()
                               join c in sc.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                               where (c.Semester == Semester && c.Section == Section && c.Department == Department) || c.Std_id == StudentId
                               select s.Std_id).ToList();
                }
            }
            else
            {
                if (RollNo != null && RollNo > 0)
                {
                    stdList = (from s in sdbs.Student_info.ToList()
                               join c in sc.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                               where c.Std_id == StudentId
                               || (c.Semester == Semester && c.Section == Section && c.RollNo == RollNo)
                               select s.Std_id).ToList();
                }
                else
                {
                    stdList = (from s in sdbs.Student_info.ToList()
                               join c in sc.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                               where (c.Semester == Semester && c.Section == Section) || c.Std_id == StudentId
                               select s.Std_id).ToList();
                }
            }





            var slist = new List<MarksheetViewModel>();

            foreach (var item in stdList)
            {

                var data = new MarksheetViewModel
                {
                    SchoolViewModel = CM.GetSchoolInfo(),
                    GPAViewModels = CM.GetGPAInfo(),
                    StudentDetailsForMarksheetViewModel = CM.GetStudentInfo(item),
                    SubjectMarksCourses = RM.GetStudentResult(item, Semester, Examtypeid ?? 0)
                };


                slist.Add(data);
            }

            var orderlist = slist.OrderByDescending(x => x.SubjectMarksCourses.Select(y => y.TotalObMarks).FirstOrDefault()).ToList();


            return PartialView(orderlist);
        }


        public ActionResult StudentResultPrint(string stdId, int Examtypeid)
        {
            List<ResultReport> resultreport = new List<ResultReport>();
            ReportDocument rd = new ReportDocument();
            string examname = "";
            decimal halfexammarks = 0; decimal fullexammarks = 0; decimal ftotalgp = 0; decimal htotalgp = 0; decimal fgp = 0; decimal hgp = 0; decimal avgGp = 0;
            int subjectcount;
            string fplace = "";
            string hplace = "";
            string userid = User.Identity.Name;
            int schoolid = uc.UserProfiles.Where(x => x.UserName.Equals(userid)).Select(x => x.SchoolId ?? 0).SingleOrDefault();
            string schoolname = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.SchoolName).SingleOrDefault();
            string address = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.Address + " " + x.City + " " + x.Country).SingleOrDefault();
            subjectcount = ExamMarks(stdId, 1).Count();

            if (Examtypeid == 1)
            {
                examname = "Half Yearly Examination";
                halfexammarks = ExamMarks(stdId, 1).Sum(x => x.totalmarks);
                htotalgp = ExamMarks(stdId, 1).Sum(x => x.gradepoint);
                hgp = htotalgp / subjectcount;
                hplace = ExamMarksList(subjectcount, Examtypeid, stdId);
            }
            else if (Examtypeid == 2)
            {
                examname = "Final Examination";
                halfexammarks = ExamMarks(stdId, 2).Sum(x => x.totalmarks);
                htotalgp = ExamMarks(stdId, 2).Sum(x => x.gradepoint);
                hgp = htotalgp / subjectcount;
                hplace = ExamMarksList(subjectcount, Examtypeid, stdId);
            }

            if (Examtypeid == 3)
            {
                halfexammarks = ExamMarks(stdId, 1).Sum(x => x.totalmarks);
                fullexammarks = ExamMarks(stdId, 2).Sum(x => x.totalmarks);
                htotalgp = ExamMarks(stdId, 1).Sum(x => x.gradepoint);
                ftotalgp = ExamMarks(stdId, 2).Sum(x => x.gradepoint);

                fgp = ftotalgp / subjectcount;
                hgp = htotalgp / subjectcount;
                avgGp = (fgp + hgp) / 2;
                hplace = ExamMarksList(subjectcount, 1, stdId);
                fplace = ExamMarksList(subjectcount, 2, stdId);
                var HyExammarklist = ExamMarks(stdId, 1).ToList();
                var FExammarklist = ExamMarks(stdId, 2).ToList();
                var allExamMarks = from fe in FExammarklist.ToList()
                                   join hy in HyExammarklist.ToList() on fe.SubjectCode equals hy.SubjectCode
                                   select new
                                   {
                                       SubjectCode = hy.SubjectCode,
                                       SubjectTitle = hy.SubjectTitle,
                                       fullmarks = hy.fullmarks,
                                       passmarks = hy.passmarks,
                                       classtest = hy.classtest,
                                       subjective = hy.subjective,
                                       objective = Change(hy.objective),
                                       practical = Change(hy.practical),
                                       totalmarks = hy.totalmarks,
                                       gradepoint = hy.gradepoint,
                                       lettergrade = hy.lettergrade,
                                       hightestmarks = hy.hightestmarks,

                                       fclasstest = fe.classtest,
                                       fsubjective = fe.subjective,
                                       fobjective = Change(fe.objective),
                                       fpractical = Change(fe.practical),
                                       ftotalmarks = fe.totalmarks,
                                       fgradepoint = fe.gradepoint,
                                       flettergrade = fe.lettergrade,
                                       fhightestmarks = fe.hightestmarks
                                   };

                rd.Load(Path.Combine(Server.MapPath("~/Report"), "FullResultCrystalReport.rpt"));
                rd.SetDataSource(allExamMarks);
                rd.SetParameterValue("fgp", fgp.ToString());
                rd.SetParameterValue("hgp", hgp.ToString());
                rd.SetParameterValue("halfexammarks", halfexammarks.ToString());
                rd.SetParameterValue("fullexammarks", fullexammarks.ToString());
                rd.SetParameterValue("htotalgp", htotalgp.ToString());
                rd.SetParameterValue("ftotalgp", ftotalgp.ToString());
                rd.SetParameterValue("hplace", hplace.ToString());
                rd.SetParameterValue("fplace", fplace.ToString());
                rd.SetParameterValue("avgGp", avgGp.ToString());

            }
            else
            {
                var marklist = from hy in ExamMarks(stdId, Examtypeid).ToList()
                               select new
                               {
                                   SubjectCode = hy.SubjectCode,
                                   SubjectTitle = hy.SubjectTitle,
                                   fullmarks = hy.fullmarks,
                                   passmarks = hy.passmarks,
                                   classtest = hy.classtest,
                                   subjective = hy.subjective,
                                   objective = Change(hy.objective),
                                   practical = Change(hy.practical),
                                   totalmarks = hy.totalmarks,
                                   gradepoint = hy.gradepoint,
                                   lettergrade = hy.lettergrade,
                                   hightestmarks = hy.hightestmarks
                               };


                rd.Load(Path.Combine(Server.MapPath("~/Report"), "StudentResultReport.rpt"));
                rd.SetDataSource(marklist);
                rd.SetParameterValue("halfexammarks", halfexammarks.ToString());
                rd.SetParameterValue("hgp", hgp.ToString());
                rd.SetParameterValue("htotalgp", htotalgp.ToString());
                rd.SetParameterValue("hplace", hplace.ToString());

            }


            string name = sdbs.Student_info.Where(x => x.Std_id.Equals(stdId)).Select(x => x.Std_FName).SingleOrDefault();
            string lastname = sdbs.Student_info.Where(x => x.Std_id.Equals(stdId)).Select(x => x.Std_LName).SingleOrDefault();
            string mname = sdbs.Student_info.Where(x => x.Std_id.Equals(stdId)).Select(x => x.Std_MName).SingleOrDefault();
            string studentname = name + " " + mname + " " + lastname;
            string semester = sc.CurrentAcademicInfo.Where(x => x.Std_id.Equals(stdId)).Select(x => x.Semester).SingleOrDefault();
            string section = sc.CurrentAcademicInfo.Where(x => x.Std_id.Equals(stdId)).Select(x => x.Section).SingleOrDefault();



            rd.SetParameterValue("studentname", studentname.ToString());
            rd.SetParameterValue("semester", semester.ToString());
            rd.SetParameterValue("section", section.ToString());
            rd.SetParameterValue("examname", examname.ToString());
            rd.SetParameterValue("schoolname", schoolname.ToString());
            rd.SetParameterValue("address", address.ToString());


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
        public string Change(decimal s)
        {
            string ms = "";
            if (s == 0)
            {
                ms = "";
            }
            else
            {
                ms = s.ToString();
            }
            return ms;
        }
        public ActionResult GPAType()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GPAType(GPAType model)
        {
            ec.GPAType.Add(model);
            ec.SaveChanges();
            TempData["list"] = "List";
            ModelState.Clear();
            return View();
        }

        public ActionResult EditGPA(int id)
        {
            var gpa = ec.GPAType.Where(x => x.Id == id).FirstOrDefault();
            if (gpa == null)
                return HttpNotFound();

            return View(gpa);
        }

        [HttpPost]
        public ActionResult EditGPA(GPAType model)
        {
            var gpa = ec.GPAType.Where(x => x.Id == model.Id).FirstOrDefault();
            if (gpa == null)
                return HttpNotFound();

            gpa.Startmark = model.Startmark;
            gpa.endmark = model.endmark;
            gpa.GPA = model.GPA;
            gpa.GradePoint = model.GradePoint;

            ec.Entry(gpa).State = EntityState.Modified;
            ec.SaveChanges();

            return RedirectToAction("GPAType", "result");
        }

        public ActionResult DeleteGPA(int id)
        {
            var gpa = ec.GPAType.Where(x => x.Id == id).FirstOrDefault();
            if (gpa == null)
                return HttpNotFound();

            ec.GPAType.Remove(gpa);
            ec.SaveChanges();

            return RedirectToAction("GPAType", "result");
        }

        public ActionResult FullResult(string StudentId, int Examtypeid)
        {
            TempData["studentid"] = StudentId;
            TempData["examid"] = Examtypeid;
            TempData["ex"] = "All Examination.";

            var HyExammarklist = ExamMarks(StudentId, 1).ToList();
            var FExammarklist = ExamMarks(StudentId, 2).ToList();
            var allExamMarks = from fe in FExammarklist.ToList()
                               join hy in HyExammarklist.ToList() on fe.SubjectCode equals hy.SubjectCode
                               select new ResultReport
                               {
                                   SubjectCode = hy.SubjectCode,
                                   SubjectTitle = hy.SubjectTitle,
                                   fullmarks = hy.fullmarks,
                                   passmarks = hy.passmarks,
                                   classtest = hy.classtest,
                                   subjective = hy.subjective,
                                   objective = hy.objective,
                                   practical = hy.practical,
                                   totalmarks = hy.totalmarks,
                                   gradepoint = hy.gradepoint,
                                   lettergrade = hy.lettergrade,
                                   hightestmarks = hy.hightestmarks,

                                   fclasstest = fe.classtest,
                                   fsubjective = fe.subjective,
                                   fobjective = fe.objective,
                                   fpractical = fe.practical,
                                   ftotalmarks = fe.totalmarks,
                                   fgradepoint = fe.gradepoint,
                                   flettergrade = fe.lettergrade,
                                   fhightestmarks = fe.hightestmarks
                               };
            return View(allExamMarks.ToList());
        }



        /// <summary>
        /// GPA calculation
        /// </summary>
        /// <param name="Marks"></param>
        /// <returns></returns>
        /// 
        private string GPAmark(decimal Marks)
        {
            string s = string.Empty;
            var m = ec.GPAType.ToList();
            foreach (var i in m)
            {
                if (Marks == i.GradePoint)
                {
                    s = i.GPA;
                }

                //if (Marks >= i.Startmark && Marks <= i.endmark)
                //{
                //    s = i.GPA;
                //}
            }

            return s;
        }

        private decimal getGP(decimal Marks, decimal fullMarks)
        {
            decimal s = 0;

            //if (fullMarks == 50)
            //{
            //    if (Marks >= PercentageCalculate(80, fullMarks))
            //    {
            //        s = 5;
            //    }
            //    else if (Marks >= PercentageCalculate(70, fullMarks) && Marks < PercentageCalculate(80, fullMarks))
            //    {
            //        s = 4;
            //    }
            //    else if (Marks >= PercentageCalculate(60, fullMarks) && Marks < PercentageCalculate(70, fullMarks))
            //    {
            //        s = 3.5m;
            //    }
            //    else if (Marks >= PercentageCalculate(50, fullMarks) && Marks < PercentageCalculate(60, fullMarks))
            //    {
            //        s = 3;
            //    }
            //    else if (Marks >= PercentageCalculate(40, fullMarks) && Marks < PercentageCalculate(50, fullMarks))
            //    {
            //        s = 2;
            //    }
            //    else if (Marks >= PercentageCalculate(33, fullMarks) && Marks < PercentageCalculate(40, fullMarks))
            //    {
            //        s = 1;
            //    }
            //    else
            //    {
            //        s = 0;
            //    }
            //}
            //else
            //{

            //    var m = ec.GPAType.ToList();
            //    foreach (var i in m)
            //    {
            //        if (Marks >= i.Startmark && Marks <= i.endmark)
            //        {
            //            s = i.GradePoint;
            //        }
            //    }
            //}

            if (fullMarks == 50)
                Marks = Marks * 2;

            var m = ec.GPAType.ToList();
            foreach (var i in m)
            {
                if (Marks >= i.Startmark && Marks <= i.endmark)
                {
                    s = i.GradePoint;
                }
            }

            return s;
        }

        private decimal PercentageCalculate(decimal percentage, decimal fullMarks)
        {
            decimal r = 0;

            r = (percentage / 100) * fullMarks;

            return r;
        }

        private string GPA(decimal Marks)
        {
            string s;
            if (Marks >= 80 && Marks <= 100)
            {
                s = "A+";
            }
            else if (Marks >= 70 && Marks < 80)
            {
                s = "A";
            }
            else if (Marks >= 60 && Marks < 70)
            {
                s = "A-";
            }
            else if (Marks >= 50 && Marks < 60)
            {
                s = "B";
            }
            else if (Marks >= 40 && Marks < 50)
            {
                s = "C";
            }
            else if (Marks >= 33 && Marks < 40)
            {
                s = "D";
            }
            else
            {
                s = "F";
            }
            return s;
        }

        // end
        public JsonResult GetDepatment(Guid idd)
        {

            var semester = from pr in sc.TeacherSubject.ToList()
                           join ps in sc.Course.ToList() on pr.SubjectCode equals ps.CourseId
                           where pr.TeacherId.Equals(idd)
                           group ps by new { ps.Semester } into s
                           select new
                           {
                               Value = s.Key.Semester,
                               Name = s.Key.Semester
                           };

            SelectList objdata = new SelectList(semester.ToList(), "Value", "Name", 0);


            return new JsonResult { Data = objdata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        public JsonResult GetSemester(string id)
        {

            var department = from pr in sc.TeacherSubject.ToList()
                             join ps in sc.Course.ToList() on pr.SubjectCode equals ps.CourseId
                             where ps.Semester.Equals(id)
                             group ps by new { ps.Department } into s
                             select new
                             {
                                 Value = s.Key.Department,
                                 Name = s.Key.Department
                             };

            SelectList objdata = new SelectList(department.ToList(), "Value", "Name", 0);


            return new JsonResult { Data = objdata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        //public JsonResult GetSemester(Guid idd, string id)
        //{

        //    var department = from pr in sc.TeacherSubject.ToList()
        //                     join ps in sc.Course.ToList() on pr.SubjectCode equals ps.CourseId
        //                     where pr.TeacherId.Equals(idd) && ps.Semester.Equals(id)
        //                     group ps by new { ps.Department } into s
        //                     select new
        //                     {
        //                         Value = s.Key.Department,
        //                         Name = s.Key.Department
        //                     };

        //    SelectList objdata = new SelectList(department.ToList(), "Value", "Name", 0);


        //    return new JsonResult { Data = objdata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        //}


        public JsonResult GetAllSubject(string id, string se)
        {
            List<Course> model = new List<Course>();
            if (id == null)
            {
                var subject = from pr in sc.Course.ToList()
                              where pr.Semester.Equals(se)
                              select new Course
                              {
                                  Id = pr.Id,
                                  CourseTitle = pr.CourseTitle + "(" + pr.CourseId + ")"
                              };
                model = subject.ToList();

            }
            else
            {
                var subject = from pr in sc.Course.ToList()
                              where
                              pr.Department.Equals(id) && pr.Semester.Equals(se)
                              select new Course
                              {
                                  Id = pr.Id,
                                  CourseTitle = pr.CourseTitle + "(" + pr.CourseId + ")"
                              };

                model = subject.ToList();

            }

            SelectList objdata = new SelectList(model.ToList(), "Id", "CourseTitle", 0);


            return new JsonResult { Data = objdata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        public JsonResult GetAllRoll(string id, string se,string sec)
        {
            if (id == null)
            {
                var roll = sc.CurrentAcademicInfo.Where(a => a.Semester.Equals(se) && a.Section==sec).OrderBy(a=>a.RollNo).ToList();
                SelectList obgcity = new SelectList(roll, "RollNo", "RollNo", 0);
                return Json(obgcity);

            }
            else
            {
                var roll = sc.CurrentAcademicInfo.Where(a => a.Semester.Equals(se) && a.Department == id && a.Section == sec).OrderBy(a => a.RollNo).ToList();
                SelectList obgcity = new SelectList(roll, "RollNo", "RollNo", 0);
                return Json(obgcity);

            }
            
            //return new JsonResult { Data = objdata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetSubject(Guid idd, string id, string se)
        {
            List<Course> model = new List<Course>();
            if (id == null)
            {
                var subject = from pr in sc.TeacherSubject.ToList()
                              join ps in sc.Course.ToList() on pr.SubjectCode equals ps.CourseId
                              where pr.TeacherId.Equals(idd) && ps.Semester.Equals(se)

                              select new Course
                              {
                                  Id = ps.Id,
                                  CourseTitle = ps.CourseTitle + "(" + ps.CourseId + ")"
                              };
                model = subject.ToList();

            }
            else
            {
                var subject = from pr in sc.TeacherSubject.ToList()
                              join ps in sc.Course.ToList() on pr.SubjectCode equals ps.CourseId
                              where pr.TeacherId.Equals(idd) && ps.Department.Equals(id) && ps.Semester.Equals(se)

                              select new Course
                              {
                                  Id = ps.Id,
                                  CourseTitle = ps.CourseTitle + "(" + ps.CourseId + ")"
                              };
                model = subject.ToList();

            }

            SelectList objdata = new SelectList(model.ToList(), "Id", "CourseTitle", 0);


            return new JsonResult { Data = objdata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }



        public JsonResult GetStudentrolllist(string classid)
        {
            var studentlist = from cai in sc.CurrentAcademicInfo.ToList()
                              join si in sdbs.Student_info.ToList() on cai.Std_id equals si.Std_id
                              where cai.Semester.Equals(classid)
                              select new
                              {
                                  stuid = cai.Std_id,
                                  stdname = si.Std_FName + " " + si.Std_MName + " " + si.Std_LName + " ( " + cai.RollNo + " )"
                              };
            SelectList objdata = new SelectList(studentlist.ToList(), "stuid", "stdname", 0);


            return new JsonResult { Data = objdata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        private List<ResultReport> ExamMarks(string stdId, int Examtypeid)
        {
            var shm = ec.SubjectHighestMarks.Where(x => x.ExamId.Equals(Examtypeid)).ToList();
            var totallist = from ca in ec.StudentSubjectMarks.ToList()
                            where ca.ExamName.Equals(Examtypeid)
                            join ex in shm.ToList() on ca.SubjectCode equals ex.SubjectCode
                            join c in sc.Course.ToList() on ex.SubjectCode equals c.Id
                            orderby ca.SubjectCode
                            where ca.StudentId.Equals(stdId) && ca.ExamName.Equals(Examtypeid)
                            select new ResultReport
                            {
                                SubjectCode = c.CourseId,
                                SubjectTitle = c.CourseTitle,
                                fullmarks = c.FullMarks,
                                passmarks = c.MinimumPassMarks,
                                classtest = ca.ClassTest,
                                subjective = ca.Written,
                                objective = ca.MCQ,
                                practical = ca.Practical,
                                totalmarks = ca.TotalMarks,
                                gradepoint = ca.GradePoint,
                                lettergrade = ca.LetterGrade,
                                hightestmarks = ex.HM
                            };
            return totallist.ToList();
        }
        private string ExamMarksList(int totalsubject, int Examtypeid, string stdid)
        {
            List<ResultReport> ls = new List<ResultReport>();
            string classes = sc.CurrentAcademicInfo.Where(x => x.Std_id == stdid).Select(x => x.Semester).FirstOrDefault();
            var courselist = sc.Course.Where(x => x.Semester.Equals(classes)).ToList();
            var shm = ec.StudentSubjectMarks.Where(x => x.LetterGrade == "F" && x.ExamName == Examtypeid).ToList();
            var totallist = (from ca in ec.StudentSubjectMarks.ToList()
                             where !(from g in shm.ToList() select g.StudentId).Contains(ca.StudentId)
                             join c in courselist.ToList() on ca.SubjectCode equals c.Id
                             where ca.ExamName.Equals(Examtypeid)
                             group ca by ca.StudentId into g
                             select new ResultReport
                             {
                                 SubjectTitle = g.Key,
                                 totalmarks = g.Sum(x => x.TotalMarks),
                                 gradepoint = g.Sum(x => x.GradePoint) / totalsubject,
                             }).OrderByDescending(p => p.gradepoint).ThenBy(x => x.totalmarks);

            ls = totallist.ToList();
            int index = ls.FindIndex(a => a.SubjectTitle == stdid);
            string place = AddOrdinal(index + 1);
            return place;
        }
        public static string AddOrdinal(int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }

        }

    }
}
