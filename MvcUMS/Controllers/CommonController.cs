using MvcUMS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using MvcEnergyPac.Models;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using MvcUMS.Models.ViewModels;

namespace MvcUMS.Controllers
{
    public class CommonController : Controller
    {
        private StudentContext db = new StudentContext();
        private PaymentContext dbpt = new PaymentContext();
        private ScolarshipContext ssc = new ScolarshipContext();
        private ExamContext et = new ExamContext();
        private ExpanceContext ec = new ExpanceContext();
        private StuffContext sc = new StuffContext();
        EventContext evc = new EventContext();
        UsersContext uc = new UsersContext();
        SchoolNewDBEntities _payCtx = new SchoolNewDBEntities();

        //
        // GET: /Common/

        public ActionResult Index()
        {
            List<StudentPaymentTypeViewModel> list = new List<StudentPaymentTypeViewModel>();
            list = (from pt in _payCtx.StudentPaymentTypes.ToList()
                    select new StudentPaymentTypeViewModel
                    {
                        Id = pt.Id,
                        PaymentType = pt.PaymentType,
                        isActive = pt.isActive
                    }).ToList();

            return View(list);
        }

        /// <summary>
        /// scholarshiptype create
        /// </summary>
        /// <returns></returns>
        public ActionResult ScolarShipType()
        {

            return View();
        }
        [HttpPost]
        public ActionResult ScolarShipType(ScholarshipType scholarshiptype)
        {

            if (ModelState.IsValid)
            {

                scholarshiptype.isactive = true;
                scholarshiptype.date = DateTime.Now.Date;
                var s = ssc.ScholarshipType.Where(x => x.TypeName.Equals(scholarshiptype.TypeName)).Count();
                var tn = scholarshiptype.TypeName;
                if (s == 0)
                {
                    ssc.ScholarshipType.Add(scholarshiptype);
                    ssc.SaveChanges();
                }

            }
            return View(scholarshiptype);
        }
        // GET: /Payment Create/
        public ActionResult PaymentType()
        {

            //var paymenttype = new List<Models.PaymentType>();


            //paymenttype = dbpt.PaymentType().ToList();

            //return View(student);
            return View();
        }


        // Post: /Payment Create/
        [HttpPost]
        public ActionResult PaymentType(StudentPaymentTypeViewModel paymentt)
        {
            if (ModelState.IsValid)
            {
                StudentPaymentType SPT = new StudentPaymentType();
                SPT.isActive = true;
                SPT.PaymentType = paymentt.PaymentType;

                _payCtx.StudentPaymentTypes.Add(SPT);
                _payCtx.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(paymentt);
        }



        public ActionResult Edit(int id)
        {
            var list = (from pt in _payCtx.StudentPaymentTypes.ToList()
                        where pt.Id.Equals(id)
                    select new StudentPaymentTypeViewModel
                    {
                        Id = pt.Id,
                        PaymentType = pt.PaymentType,
                        isActive = pt.isActive
                    }).FirstOrDefault();

            if (list == null)
            {
                return HttpNotFound();
            }
            return View(list);
        }

        //
        // POST: /paymenttype/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StudentPaymentTypeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exists = _payCtx.StudentPaymentTypes.Where(x => x.Id == model.Id).FirstOrDefault();
                exists.PaymentType = model.PaymentType;
                exists.isActive = true;
                _payCtx.Entry(exists).State = EntityState.Modified;
                _payCtx.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var paymenttype = _payCtx.StudentPaymentTypes.Find(id);
            _payCtx.StudentPaymentTypes.Remove(paymenttype);
            _payCtx.SaveChanges();
            return RedirectToAction("Index");
        }

        // Payment Amount creation aginsed Payment Type
        public ActionResult PaymentIndex()
        {
            List<StudentPaymentAmountViewModel> paymentlist = new List<StudentPaymentAmountViewModel>();
            var paymentamountlist = from pa in _payCtx.StudentPaymentAmounts.ToList()
                                    join d in db.Department.ToList() on pa.DepartmentId equals d.Id into dtp
                                    from d in dtp.DefaultIfEmpty()
                                    join s in db.Semester.ToList() on pa.SemesterId equals s.Id
                                    join pt in _payCtx.StudentPaymentTypes.ToList() on pa.PaymentTypeId equals pt.Id
                                    select new StudentPaymentAmountViewModel
                                    {
                                        Id = pa.Id,
                                        Department = d == null ? "" : d.Dep_name,
                                        Semester = s.Semester_name,
                                        Name = pt.PaymentType,
                                        Amount = pa.Amount
                                    };
            paymentlist = paymentamountlist.ToList();
            return View(paymentlist);
        }

        public ActionResult PayAmountEdit(int id)
        {
            ViewBag.DepartmentList = new SelectList(db.Department, "Id", "Dep_name");
            ViewBag.SemesterList = new SelectList(db.Semester, "Id", "Semester_name");
            ViewBag.PaymentTypeList = new SelectList(_payCtx.StudentPaymentTypes, "Id", "PaymentType");

            //var payAmt = _payCtx.StudentPaymentAmounts.Where(x => x.Id == id).FirstOrDefault();
            var paymentamountlist = (from pa in _payCtx.StudentPaymentAmounts.ToList()
                                    join d in db.Department.ToList() on pa.DepartmentId equals d.Id into dtp
                                    from d in dtp.DefaultIfEmpty()
                                    join s in db.Semester.ToList() on pa.SemesterId equals s.Id
                                    join pt in _payCtx.StudentPaymentTypes.ToList() on pa.PaymentTypeId equals pt.Id
                                    where pa.Id.Equals(id)
                                    select new StudentPaymentAmountViewModel
                                    {
                                        Id = pa.Id,
                                        Department = d == null ? "" : d.Dep_name,
                                        Semester = s.Semester_name,
                                        Name = pt.PaymentType,
                                        Amount = pa.Amount,
                                        SemesterId=s.Id,
                                        DepartmentId= d == null ? 0 : d.Id,
                                        PaymentTypeId=pt.Id
                                    }).FirstOrDefault();

            if (paymentamountlist == null)
                return HttpNotFound();

            return View(paymentamountlist);
        }

        [HttpPost]
        public ActionResult PayAmountEdit(StudentPaymentAmountViewModel model)
        {
            var payAmt = _payCtx.StudentPaymentAmounts.Where(x => x.Id == model.Id).FirstOrDefault();

            var count = _payCtx.StudentPaymentAmounts.Where(x => x.DepartmentId == (model.DepartmentId ?? 0)
                && x.SemesterId == model.SemesterId
                && x.PaymentTypeId == model.PaymentTypeId).Count();

            if (payAmt == null)
                return HttpNotFound();


            payAmt.SemesterId = model.SemesterId;
            payAmt.DepartmentId = model.DepartmentId ?? 0;
            payAmt.Amount = model.Amount;
            payAmt.PaymentTypeId = model.PaymentTypeId;
            _payCtx.Entry(payAmt).State = EntityState.Modified;
            _payCtx.SaveChanges();

            return RedirectToAction("PaymentIndex");


            ViewBag.DepartmentList = new SelectList(db.Department, "Id", "Dep_name");
            ViewBag.SemesterList = new SelectList(db.Semester, "Id", "Semester_name");
            ViewBag.PaymentTypeList = new SelectList(_payCtx.StudentPaymentTypes, "Id", "PaymentType");
            return View(model);
        }

        public ActionResult PaymentDelete(int id)
        {
            var payAmt = _payCtx.StudentPaymentAmounts.Where(x => x.Id == id).FirstOrDefault();

            if (payAmt == null)
                return HttpNotFound();

            _payCtx.StudentPaymentAmounts.Remove(payAmt);
            _payCtx.SaveChanges();

            return RedirectToAction("PaymentIndex", "Common");
        }

        public ActionResult PaymentCreate()
        {
            ViewBag.DepartmentList = new SelectList(db.Department, "Id", "Dep_name");
            ViewBag.SemesterList = new SelectList(db.Semester, "Id", "Semester_name");
            ViewBag.PaymentTypeList = new SelectList(_payCtx.StudentPaymentTypes, "Id", "PaymentType");
            return View();
        }
        [HttpPost]
        public ActionResult PaymentCreate(StudentPaymentAmountViewModel model)
        {
            var s = _payCtx.StudentPaymentAmounts.Where(x => x.DepartmentId == model.DepartmentId
                && x.SemesterId == model.SemesterId
                && x.PaymentTypeId == model.PaymentTypeId).Count();

            if (s > 0)
            {
                TempData["error"] = "Alrady Created.";
            }
            else
            {
                if (ModelState.IsValid)
                {
                    StudentPaymentAmount SPA = new StudentPaymentAmount();
                    SPA.Amount = model.Amount;
                    SPA.SemesterId = model.SemesterId;
                    SPA.DepartmentId = model.DepartmentId ?? 0;
                    SPA.PaymentTypeId = model.PaymentTypeId;
                    _payCtx.StudentPaymentAmounts.Add(SPA);
                    _payCtx.SaveChanges();
                    return RedirectToAction("PaymentIndex");
                }
            }

            ViewBag.DepartmentList = new SelectList(db.Department, "Id", "Dep_name");
            ViewBag.SemesterList = new SelectList(db.Semester, "Id", "Semester_name");
            ViewBag.PaymentTypeList = new SelectList(_payCtx.StudentPaymentTypes, "Id", "PaymentType");
            return View(model);

        }


        [HttpGet]
        public ActionResult CourseCreate()
        {
            ViewBag.Department = new SelectList(db.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(db.Semester, "Semester_name", "Semester_name");
            return View();
        }
        [HttpPost]
        public ActionResult CourseCreate(Course model, string Department, string Semester, string Command)
        {

            if (Command == "Search")
            {
                TempData["Department"] = Department;
                TempData["Semester"] = Semester;
                //return RedirectToAction("CourseList", new { Department = Department, Semester = Semester });
            }

            if (Command == "Create")
            {
                if (ModelState.IsValid)
                {
                    var coursename = db.Course.Where(x => x.Semester.Equals(Semester) && x.CourseTitle.Equals(model.CourseTitle)).Count();
                    var corsecode = db.Course.Where(x => x.CourseId.Equals(model.CourseId)).Count();
                    if (corsecode == 0 && coursename == 0)
                    {
                        model.Department = Department;
                        model.Semester = Semester;
                        db.Course.Add(model);
                        db.SaveChanges();
                        TempData["list"] = "List";
                    }
                    else if (coursename > 0)
                    {
                        TempData["error"] = "Course Title alrady Exit of this Semester.";
                    }
                    else
                    {
                        TempData["error"] = "Course Code alrady Exit.";
                    }

                }
            }
            ViewBag.Department = new SelectList(db.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(db.Semester, "Semester_name", "Semester_name");
            return View();
        }

        public ActionResult CourseList(int? Page_No, string Department, string Semester)
        {
            var model = new List<Course>();
            if (Department != null && Semester != null)
            {
                model = db.Course.Where(x => x.Department.Equals(Department) && x.Semester.Equals(Semester)).OrderBy(x => x.CourseId).ToList();
            }
            else
            {
                model = db.Course.OrderBy(x => x.CourseId).ToList();
            }

            int Size_Of_Page = 5;
            int No_Of_Page = (Page_No ?? 1);
            return View(model.ToPagedList(No_Of_Page, Size_Of_Page));
        }

        [HttpGet]
        public ActionResult EditCourse(string id)
        {
            Course model = new Course();
            model = db.Course.Where(x => x.CourseId.Equals(id)).SingleOrDefault();
            model.CourseId = id;
            model.CourseTitle = model.CourseTitle;
            model.Department = model.Department;
            model.Semester = model.Semester;
            ViewBag.Department = new SelectList(db.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(db.Semester, "Semester_name", "Semester_name");
            return View(model);
        }
        [HttpPost]
        public ActionResult EditCourse(Course model, string Department, string Semester)
        {
            var b = db.Course.Where(x => x.CourseId.Equals(model.CourseId)).SingleOrDefault();
            if (Semester == "One" || Semester == "Two" || Semester == "Three" || Semester == "Four" || Semester == "Five" || Semester == "Six"
                     || Semester == "Seven" || Semester == "Eight")
            {
                b.Department = null;
            }
            else
            {
                b.Department = Department;
            }
            b.Semester = Semester;
            db.Entry(b).State = EntityState.Modified;

            db.SaveChanges();
            TempData["list"] = "List";
            return RedirectToAction("CourseCreate");
        }

        [HttpGet]
        public ActionResult DetailCourse(string id)
        {
            Course model = new Course();
            model = db.Course.Where(x => x.CourseId.Equals(id)).SingleOrDefault();
            model.CourseId = id;
            model.CourseTitle = model.CourseTitle;
            model.Department = model.Department;
            model.Semester = model.Semester;

            return View(model);
        }

        [HttpGet]
        public ActionResult Examtype()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Examtype(ExamType model)
        {
            if (ModelState.IsValid)
            {
                var examname = et.ExamType.Where(x => x.Examtype.Equals(model.Examtype)).Count();

                if (examname == 0)
                {
                    model.date = DateTime.Now.Date;
                    model.isactive = true;
                    et.ExamType.Add(model);
                    et.SaveChanges();
                    TempData["list"] = "List";
                }

                else
                {
                    TempData["error"] = "Exam type alrady Exit.";
                }

            }

            return View();
        }

        public ActionResult EditExamType(int id)
        {
            var examType = et.ExamType.Where(a => a.Id == id).FirstOrDefault();
            ViewBag.examTypeName = examType;
            return View(examType);
        }

        [HttpPost]
        public ActionResult EditExamType(ExamType model)
        {
            var examType = et.ExamType.Where(a => a.Id == model.Id).FirstOrDefault();
            if (ModelState.IsValid)
            {
                model.date = DateTime.Now.Date;
                model.isactive = true;
                examType.Examtype = model.Examtype;
                et.Entry(examType).State = EntityState.Modified;
                et.SaveChanges();
            }
            return RedirectToAction("Examtype");
        }

        public ActionResult ExamTypeDelete(int id)
        {
            var examType = et.ExamType.Where(a => a.Id == id).FirstOrDefault();

            et.ExamType.Remove(examType);
            et.SaveChanges();
            return RedirectToAction("ExamType");
        }

        public ActionResult ExamList(int? Page_No)
        {
            var model = new List<ExamType>();
            model = et.ExamType.OrderBy(x => x.Id).ToList();

            int Size_Of_Page = 5;
            int No_Of_Page = (Page_No ?? 1);
            return View(model.ToPagedList(No_Of_Page, Size_Of_Page));
        }

        public ActionResult GPAList(int? Page_No)
        {
            var model = new List<GPAType>();
            model = et.GPAType.OrderBy(x => x.Id).ToList();

            int Size_Of_Page = 5;
            int No_Of_Page = (Page_No ?? 1);
            return View(model.ToPagedList(No_Of_Page, Size_Of_Page));
        }


        [HttpGet]
        public ActionResult Exam(int? Page_No, int? page)
        {
            ViewBag.advancedsearch = "advancedsearch";
            ViewBag.ExamType = new SelectList(et.ExamType, "Id", "Examtype");
            ViewBag.Department = new SelectList(db.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(db.Semester, "Semester_name", "Semester_name");
            IEnumerable<SelectListItem> type = new List<SelectListItem>
            {
                new SelectListItem { Text = "Written", Value = "Written" },
                new SelectListItem { Text = "MCQ", Value = "MCQ" },
                new SelectListItem { Text = "Viva", Value = "Viva" }
            };
            ViewBag.type = new SelectList(type.ToList(), "Value", "Text");
            ViewBag.Page_No = Page_No;
            ViewBag.page = page;

            var teacher = from pr in sc.Stuff.ToList()
                          join ps in sc.StuffDepartment.ToList() on pr.Department equals ps.DepartmentId
                          where ps.DepartmentName.Equals("Teacher")
                          select new
                          {
                              Id = pr.Id,
                              Name = pr.FirstName + " " + pr.LastName
                          };

            ViewBag.TeacherList = new SelectList(teacher.ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult Exam(Exam model, int ExamType, int? Page_No)
        {
            if (ModelState.IsValid)
            {
                var coursecode = db.Course.Where(x => x.CourseId.Equals(model.CourseCode)).Select(x=> x.Id).FirstOrDefault();

                if (coursecode > 0 && DateTime.Now.Date <= model.Date)
                {
                    //var checkcourse = et.Exam.Where(x => x.CourseCode.Equals(model.CourseCode) && x.ExamtypeId.Equals(ExamType)).Count();

                    model.Id = Guid.NewGuid();
                    model.ExamtypeId = ExamType;
                    model.CourseId = coursecode;
                    et.Exam.Add(model);
                    et.SaveChanges();
                    TempData["list"] = "List";

                }
                else if (coursecode == 0)
                {
                    TempData["error"] = "Incorrect Course Code.";
                }
                else
                {
                    TempData["error"] = "Incorrect Date.";
                }
            }
            ViewBag.ExamType = new SelectList(et.ExamType, "Id", "Examtype");
            ViewBag.Department = new SelectList(db.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(db.Semester, "Semester_name", "Semester_name");
         
            ViewBag.Page_No = Page_No;
            return RedirectToAction("Exam");
        }
        public JsonResult getCourseCode(string term, string dep, string sem)
        {
            //&& x.Department.Contains(dep.ToLower())
            var coursecodes = new List<Models.Course>();
            coursecodes = db.Course.Where(x => x.CourseId.Contains(term.ToLower()) && x.Semester.Contains(sem.ToLower())).ToList();
            return Json(coursecodes, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ExamsList(int? Page_No, int? page)
        {
            var model = from ex in et.Exam.ToList()
                        join st in et.ExamType.ToList() on ex.ExamtypeId equals st.Id
                        join s in sc.Stuff.ToList() on ex.StuffId equals s.Id
                        join c in db.Course.ToList() on ex.CourseId equals c.Id 
                        select new ExamAndType { ex = ex, et = st, co = c, stuff = s };




            List<ExamAndType> exs = new List<ExamAndType>();
            exs = model.ToList();
            IPagedList<ExamAndType> exalist = null;
            int Size_Of_Page = 5;
            int pageIndex = 1;
            pageIndex = (Page_No ?? 1);
            exalist = model.ToPagedList(pageIndex, Size_Of_Page);
            return View(exalist);

            //return View(model.ToList().ToPagedList(Page_No ?? 1, 5));
        }

        [HttpGet]
        public ActionResult ExamEdit(Guid id)
        {
            Exam model = new Exam();
            model = et.Exam.Where(x => x.Id.Equals(id)).SingleOrDefault();
            model.Id = id;
            model.CourseId = model.CourseId;
            model.ExamtypeId = model.ExamtypeId;
            model.Date = model.Date;
            model.StartTime = model.StartTime;
            model.EndTime = model.EndTime;
            model.Type = model.Type;
            model.StuffId = model.StuffId;
            ViewBag.ExamType = new SelectList(et.ExamType, "Id", "Examtype");
            ViewBag.Department = new SelectList(db.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(db.Semester, "Semester_name", "Semester_name");
            var teacher = from pr in sc.Stuff.ToList()
                          join ps in sc.StuffDepartment.ToList() on pr.Department equals ps.DepartmentId
                          where ps.DepartmentName.Equals("Teacher")
                          select new
                          {
                              Id = pr.Id,
                              Name = pr.FirstName + " " + pr.LastName
                          };

            ViewBag.TeacherList = new SelectList(teacher.ToList(), "Id", "Name");
            return View(model);
        }
        [HttpPost]
        public ActionResult ExamEdit(Exam model)
        {
            var b = et.Exam.Where(x => x.Id.Equals(model.Id)).SingleOrDefault();
            b.Type = model.Type;
            b.StartTime = model.StartTime;
            b.EndTime = model.EndTime;
            b.CourseId = model.CourseId;
            b.Date = model.Date;
            b.ExamtypeId = model.ExamtypeId;
            b.StuffId = model.StuffId;
            et.Entry(b).State = EntityState.Modified;
            et.SaveChanges();
            TempData["list"] = "List";
            return RedirectToAction("Exam");
        }
        [HttpGet]
        public ActionResult DetailExam(Guid id)
        {

            var model = from ex in et.Exam.ToList()
                        join st in et.ExamType.ToList() on ex.ExamtypeId equals st.Id
                        join c in db.Course.ToList() on ex.CourseId equals c.Id
                        where (ex.Id.Equals(id))
                        select new ExamAndType { ex = ex, et = st, co = c };



            return View(model.SingleOrDefault());
        }


        public ActionResult ExpenseList()
        {
            ViewBag.advancedsearch = "advancedsearch";
            return View(ec.Expancetype.ToList());
        }

        [HttpGet]
        public ActionResult Expacncetype()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Expacncetype(Expancetype model)
        {
            if (ModelState.IsValid)
            {
                model.isactive = true;
                model.date = DateTime.Now.Date;
                ec.Expancetype.Add(model);
                ec.SaveChanges();
            }
            return RedirectToAction("ExpenseList");
        }

        public ActionResult ExpenseEdit(int id)
        {
            var exp = ec.Expancetype.Where(a => a.Id == id).FirstOrDefault();
            return View(exp);
        }

        [HttpPost]
        public ActionResult ExpenseEdit(Expancetype model)
        {
            var expense = ec.Expancetype.Where(a => a.Id == model.Id).FirstOrDefault();
            expense.TypeName = model.TypeName;
            expense.date = DateTime.Now;
            expense.isactive = true;

            ec.Entry(expense).State = EntityState.Modified;
            ec.SaveChanges();
            return RedirectToAction("ExpenseList");
        }

        public ActionResult SectionDelete(int id)
        {
            var exp = ec.Expancetype.Where(a => a.Id == id).FirstOrDefault();
            ec.Expancetype.Remove(exp);
            ec.SaveChanges();
            return RedirectToAction("ExpenseList");
        }

        [HttpGet]
        public ActionResult BankCreation()
        {

            return View();
        }
        [HttpPost]
        public ActionResult BankCreation(Bank model)
        {
            if (ModelState.IsValid)
            {
                var s = dbpt.Bank.Where(x => x.BankName.Equals(model.BankName) && x.AccountNumber.Equals(model.AccountNumber)).Count();
                if (s > 0)
                {
                    TempData["s"] = "Alrady Created.";
                }
                else
                {
                    model.IsActive = true;
                    model.CreationDate = DateTime.Now.Date;
                    dbpt.Bank.Add(model);
                    dbpt.SaveChanges();
                    return RedirectToAction("BankList");
                }
            }
            return RedirectToAction("BankCreation");
        }

        public ActionResult BankList()
        {
            ViewBag.advancedsearch = "advancedsearch";
            var list = dbpt.Bank.ToList();
            return View(list);
        }

        public ActionResult BankEdit(int id)
        {
            var bank = dbpt.Bank.Where(x => x.Id == id).FirstOrDefault();

            return View(bank);
        }

        [HttpPost]
        public ActionResult BankEdit(Bank model)
        {
            var bank = dbpt.Bank.Where(x => x.Id == model.Id).FirstOrDefault();

            bank.BankName = model.BankName;
            bank.AccountNumber = model.AccountNumber;
            bank.Amount = model.Amount;
            bank.CreationDate = DateTime.Now;
            bank.IsActive = true;
            dbpt.Entry(bank).State = EntityState.Modified;
            dbpt.SaveChanges();

            return RedirectToAction("BankList");
        }

        public ActionResult BankDelete(int id)
        {
            var bank = dbpt.Bank.Where(x => x.Id == id).FirstOrDefault();
            dbpt.Bank.Remove(bank);
            dbpt.SaveChanges();
            return RedirectToAction("BankList");
        }

        /// <summary>
        /// Course Asignment For Teacher
        /// </summary>
        /// <returns></returns>
        public ActionResult CourseAsignment()
        {
            ViewBag.Department = new SelectList(db.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(db.Semester, "Semester_name", "Semester_name");
            ViewBag.Section = new SelectList(db.Section, "section_name", "section_name");
            var sar = from pr in sc.Stuff.ToList()
                      join ps in sc.StuffDepartment.ToList() on pr.Department equals ps.DepartmentId
                      where ps.DepartmentName.Equals("Teacher")
                      select new
                      {
                          Id = pr.Id,
                          Name = pr.FirstName + " " + pr.LastName
                      };
            ViewBag.TeacherList = new SelectList(sar.ToList(), "Id", "Name");
            var course = db.Course.ToList();

            return View(course.ToList());
        }
        [HttpPost]
        public ActionResult CourseAsignment(string Department, string Semester, string Section, Guid TeacherList)
        {
            ViewBag.Department = new SelectList(db.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(db.Semester, "Semester_name", "Semester_name");
            ViewBag.Section = new SelectList(db.Section, "section_name", "section_name");
            var sar = from pr in sc.Stuff.ToList()
                      join ps in sc.StuffDepartment.ToList() on pr.Department equals ps.DepartmentId
                      where ps.DepartmentName.Equals("Teacher")
                      select new
                      {
                          Id = pr.Id,
                          Name = pr.FirstName + " " + pr.LastName
                      };
            //String.IsNullOrEmpty(Department)
            ViewBag.TeacherList = new SelectList(sar.ToList(), "Id", "Name");
            if (Department != null && Semester != "" && Section != "" && TeacherList != Guid.Empty)
            {
                var course = db.Course.Where(x => x.Department.Equals(Department) && x.Semester.Equals(Semester)).ToList();
                var subjectteacher = from c in db.Course
                                     where c.Department.Equals(Department) && c.Semester.Equals(Semester) && !db.TeacherSubject.Any(x => x.SubjectCode.Equals(c.CourseId) && x.TeacherId.Equals(TeacherList))
                                     select c;

                if (subjectteacher.Count() > 0)
                {
                    ViewBag.Visibility = "true";
                    ViewBag.teacherid = TeacherList;
                    ViewBag.sec = Section;
                }
                else { ViewBag.Visibility = "false"; }

                return View(subjectteacher.ToList());

            }
            else
            {
                var course = db.Course.ToList();
                ViewBag.Visibility = "false";
                return View(course.ToList());

            }

        }


        [HttpPost]
        public ActionResult TeacherSubject(string[] ids, Guid teacherId, string section)
        {

            if (ids != null)
            {

                foreach (var i in ids)
                {
                    var model = new TeacherSubject();
                    model.Id = Guid.NewGuid();
                    model.TeacherId = teacherId;
                    model.SubjectCode = i;
                    model.Section = section;
                    model.IsActive = true;

                    db.TeacherSubject.Add(model);
                    db.SaveChanges();

                }

            }

            return RedirectToAction("CourseAsignment");
        }

        public ActionResult SchoolList()
        {
            return View(evc.SchoolSetup.ToList());
        }

        public ActionResult SchoolCreate()
        {
            ViewBag.studentscripts = "specificscripts";
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SchoolCreate(SchoolSetup model)
        {
            try
            {
                string userid = User.Identity.Name;
                int schoolid = uc.UserProfiles.Where(x => x.UserName.Equals(userid)).Select(x => x.SchoolId ?? 0).SingleOrDefault();
                if (schoolid == 0)
                {
                    if (ModelState.IsValid)
                    {

                        string filename = "";
                        string opcityimgfilename = "";


                        if (model.picture != null && model.picture.ContentLength > 0)
                        {

                            filename = Path.GetFileName(Guid.NewGuid() + "." + model.picture.FileName.Split('.')[1]);
                            string targetPath = Server.MapPath("../uploads//" + filename);
                            Stream strm = model.picture.InputStream;
                            var targetFile = targetPath;
                            //Based on scalefactor image size will vary
                            GenerateThumbnails(0.5, strm, targetFile);

                        }
                        if (model.picture != null && model.picture.ContentLength > 0)
                        {

                            opcityimgfilename = Path.GetFileName(Guid.NewGuid() + "." + model.picture.FileName.Split('.')[1]);
                            string targetPath = Server.MapPath("../Images//" + opcityimgfilename);
                            Stream strm = model.picture.InputStream;
                            var targetFile = targetPath;
                            //Based on scalefactor image size will vary
                            float opacityvalue = 0.1f;
                            ChangeOpacity(strm, opacityvalue, targetPath);
                        }
                        model.Logo = filename;
                        model.opcityimg = opcityimgfilename;
                        model.crdate = DateTime.Now.Date;
                        evc.SchoolSetup.Add(model);
                        evc.SaveChanges();
                        int maxid = evc.SchoolSetup.Max(x => x.Id);
                        var users = uc.UserProfiles.Where(x => x.UserName.Equals(userid)).FirstOrDefault();
                        users.SchoolId = maxid;
                        uc.Entry(users).State = EntityState.Modified;
                        uc.SaveChanges();
                    }
                }
                else
                {
                    TempData["error"] = "Alrady Create School for current User.";
                }
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.InnerException;
                return View(model);
            }            
        }

        public ActionResult SchoolEdit(int id)
        {
            var school = evc.SchoolSetup.Where(a => a.Id == id).FirstOrDefault();
            return View(school);
        }

        [HttpPost]
        public ActionResult SchoolEdit(SchoolSetup model)
        {
            if (model.picture != null && model.picture.ContentLength > 0)
            {
                string filename = "";
                filename = Path.GetFileName(Guid.NewGuid() + "." + model.picture.FileName.Split('.')[1]);
                model.Logo = filename;
                string targetPath = Server.MapPath("~/uploads//" + filename);
                Stream strm = model.picture.InputStream;
                var targetFile = targetPath;

                GenerateThumbnails(0.5, strm, targetFile);
            }


            var school = evc.SchoolSetup.Where(a => a.Id == model.Id).FirstOrDefault();
            school.SchoolName = model.SchoolName;
            school.Address = model.Address;
            school.City = model.City;
            school.Country = model.Country;
            school.Logo = model.Logo;
            evc.Entry(school).State = EntityState.Modified;
            evc.SaveChanges();
            return RedirectToAction("SchoolList");
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

        public void ChangeOpacity(Stream sourcePath, float opacityvalue, string targetPath)
        {
            using (var image = System.Drawing.Image.FromStream(sourcePath))
            {

                int width;
                int height;

                width = 512;
                height = 384;

                Bitmap bmp = new Bitmap(width, height);

                Graphics graphics = Graphics.FromImage(bmp);

                ColorMatrix colormatrix = new ColorMatrix();

                colormatrix.Matrix33 = opacityvalue;

                ImageAttributes imgAttribute = new ImageAttributes();

                imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                var imageRectangle = new Rectangle(0, 0, bmp.Width, bmp.Height);
                graphics.DrawImage(image, imageRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imgAttribute);
                graphics.Dispose();

                if (System.IO.File.Exists(targetPath))
                {
                    System.IO.File.Delete(targetPath);
                }

                bmp.Save(targetPath, image.RawFormat);

            }
        }
    }
}