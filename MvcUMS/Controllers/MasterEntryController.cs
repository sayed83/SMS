using MvcUMS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcUMS.Controllers
{
    public class MasterEntryController : Controller
    {
        //
        // GET: /MasterEntry/
        StudentContext mdb = new StudentContext();
        StudentContext sdb = new StudentContext();
        StuffContext stdb = new StuffContext();
        ExamContext _xm = new ExamContext();
        private StuffContext sc = new StuffContext();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CourseList()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");

            if (User.IsInRole("Student"))
            {

                var clasname = mdb.CurrentAcademicInfo.Where(x => x.Std_id == User.Identity.Name).Select(x => x.Semester).FirstOrDefault();

                List<Course> stdCourse = mdb.Course.Where(a => a.Semester.Equals(clasname)).ToList();

                return View(stdCourse);
            }

            List<Course> cr = mdb.Course.Where(a => a.IsActive == true).ToList();
            return View(cr);
        }

        public ActionResult NewCourseEntry()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            ViewBag.MarkType = new SelectList(_xm.MarkType.ToList(), "Id", "Name");
            var viewModel = new CourseMarkTypeViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult NewCourseEntry(CourseMarkTypeViewModel model)
        {

            Course cr = new Course();
            cr.CourseId = model.CourseId;
            cr.CourseTitle = model.CourseTitle;
            cr.Semester = model.Semester;
            cr.IsOptional = model.IsOptional;
            cr.ExcludeFromResult = model.ExcludeFromResult;
            if (string.IsNullOrWhiteSpace(model.Department))
            {
                cr.Department = "0";
            }
            else
            {
                cr.Department = model.Department;
            }
            cr.IsActive = true;
            mdb.Course.Add(cr);
            mdb.SaveChanges();

            foreach (var item in model.MarkTypes)
            {
                CourseMarkTypeSetup mark = new CourseMarkTypeSetup();
                mark.CourseId = cr.Id;
                mark.MarkTypeId = item.MarkTypeId;
                mark.TotalMarks = item.TotalMarks;
                mark.PassMarks = item.PassMarks;
                cr.FullMarks += mark.TotalMarks;
                cr.MinimumPassMarks += mark.PassMarks;
                _xm.CourseMarkSetup.Add(mark);
                mdb.Entry(cr).State = EntityState.Modified;
            }


            mdb.SaveChanges();

            try
            {
                _xm.SaveChanges();
            }
            catch (DbUpdateException e)
            {

                Console.WriteLine(e);
            }


            TempData["success"] = "Course Successfully Saved";
            return RedirectToAction("CourseList");
        }



        public ActionResult CourseEdit(int id)
        {
            var course = (from c in mdb.Course.ToList()
                          join cd in _xm.CourseMarkSetup.ToList() on c.Id equals cd.CourseId
                          where c.Id == id
                          select new CourseViewModel
                          {
                              Id = c.Id,
                              CourseId = c.CourseId,
                              CourseTitle = c.CourseTitle,
                              FullMarks = c.FullMarks,
                              Semester = c.Semester,
                              Department = c.Department,
                              semester_Name = c.Semester,
                              IsOptional = c.IsOptional??false,
                              ExcludeFromResult=c.ExcludeFromResult??false,
                              CoourseDetails = (from ma in _xm.CourseMarkSetup.ToList()
                                                join mt in _xm.MarkType.ToList() on ma.MarkTypeId equals mt.Id
                                                where ma.CourseId == id
                                                select new CourseMarkTypeSetupViewModel
                                                {
                                                    Id = ma.Id,
                                                    MarkTypeId = ma.MarkTypeId,
                                                    MarkType = mt.Name,
                                                    PassMarks = ma.PassMarks,
                                                    TotalMarks = ma.TotalMarks
                                                }).ToList()

                          }).FirstOrDefault();

            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            ViewBag.MarkType = new SelectList(_xm.MarkType.ToList(), "Id", "Name");


            foreach (var item in _xm.MarkType.ToList())
            {
                course.AvailableMarkType.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });
            }



            return View(course);
        }


        [HttpPost]
        public ActionResult CourseEdit(CourseViewModel model)
        {
            var course = mdb.Course.Where(x => x.Id == model.Id).FirstOrDefault();

            course.CourseId = model.CourseId;
            course.CourseTitle = model.CourseTitle;
            course.Semester = model.semester_Name;
            course.IsOptional = model.IsOptional;
            course.ExcludeFromResult = model.ExcludeFromResult;
            course.FullMarks = model.CoourseDetails.Sum(x => x.TotalMarks);
            course.MinimumPassMarks = model.CoourseDetails.Sum(x => x.PassMarks);

            if (string.IsNullOrWhiteSpace(model.Department))
            {
                course.Department = "0";
            }
            else
            {
                course.Department = model.Department;
            }

            course.IsActive = true;
            course.IsOptional = false;
            mdb.Entry(course).State = EntityState.Modified;

            foreach (var item in model.CoourseDetails)
            {
                var mark = _xm.CourseMarkSetup.Where(x => x.Id == item.Id).FirstOrDefault();
                mark.CourseId = course.Id;
                mark.MarkTypeId = item.MarkTypeId;
                mark.TotalMarks = item.TotalMarks;
                mark.PassMarks = item.PassMarks;
                _xm.Entry(mark).State = EntityState.Modified;
            }


            mdb.SaveChanges();

            _xm.SaveChanges();




            TempData["success"] = "Course Successfully Updated";
            return RedirectToAction("CourseList");
        }


        public ActionResult DeleteMarkSetup(int id)
        {
            var marktype = _xm.CourseMarkSetup.Where(x => x.Id == id).FirstOrDefault();
            _xm.CourseMarkSetup.Remove(marktype);
            _xm.SaveChanges();
            return RedirectToAction("CourseList");
        }

        public ActionResult CourseDelete(string id = null)
        {
            Course cr = mdb.Course.First(a => a.CourseId.Equals(id));
            cr.IsActive = false;
            mdb.Entry(cr).State = EntityState.Modified;
            mdb.SaveChanges();
            TempData["success"] = "Course Successfully Deleted";
            return RedirectToAction("CourseList");
        }


        public ActionResult CourseAssignList()
        {
            ViewBag.advancedsearch = "advancedsearch";
            var cslist = from course in mdb.Course.ToList()
                         join ass in mdb.TeacherSubject.ToList() on course.CourseId equals ass.SubjectCode
                         join teacher in stdb.Stuff.ToList() on ass.TeacherId equals teacher.Id
                         where ass.IsActive == true
                         select new CourseAssignList
                         {
                             Id = ass.Id,
                             courseCode = course.CourseId,
                             courseName = course.CourseTitle,
                             className = course.Semester,
                             sectionName = ass.Section,
                             teacherCode = teacher.Id,
                             teacherName = teacher.FirstName + " " + teacher.LastName
                         };
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            var sar = from pr in sc.Stuff.ToList()
                      join ps in sc.StuffDepartment.ToList() on pr.Department equals ps.DepartmentId
                      where ps.DepartmentName.Equals("Teacher")
                      select new
                      {
                          Id = pr.Id,
                          Name = pr.FirstName + " " + pr.LastName
                      };
            ViewBag.TeacherList = new SelectList(sar.ToList(), "Id", "Name");
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "A", Value = "A" });

            ViewBag.SectionList = new SelectList(items.ToList(), "Text", "Value");
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            return View(cslist.ToList());

        }



        public ActionResult DeleteSubjectAssign(Guid id)
        {
            TeacherSubject ts = mdb.TeacherSubject.First(a => a.Id.Equals(id));
            ts.IsActive = false;
            mdb.Entry(ts).State = EntityState.Modified;
            mdb.SaveChanges();
            return RedirectToAction("CourseAssignList");
        }

        [HttpPost]
        public ActionResult GetCourse(string sem)
        {
            List<Course> objcity = new List<Course>();

            var cslist = from course in mdb.Course.ToList()
                         where course.Semester.Equals(sem) && course.IsActive == true
                         select new Course
                         {
                             CourseId = course.CourseId,
                             CourseTitle = course.CourseTitle
                         };
            objcity = cslist.ToList();
            SelectList obgcity = new SelectList(objcity, "CourseId", "CourseTitle", 0);
            return Json(obgcity);
        }


        [HttpPost]
        public ActionResult GetSection(string sem)
        {
            List<Section> objcity = mdb.Section.Where(a => a.semester_Name.Equals(sem)).ToList();
            SelectList obgcity = new SelectList(objcity, "section_name", "section_name", 0);
            return Json(obgcity);
        }

        [HttpPost]
        public ActionResult AssignCourse(Guid TeacherList, string Semester, string Section, string Department, string Course)
        {
            TeacherSubject ts = new TeacherSubject();
            ts.Id = Guid.NewGuid();
            ts.Section = Section;
            ts.Department = Department;
            ts.SubjectCode = Course;
            ts.TeacherId = TeacherList;
            ts.IsActive = true;
            mdb.TeacherSubject.Add(ts);
            mdb.SaveChanges();
            return RedirectToAction("CourseAssignList");
        }

        public ActionResult ClassList()
        {
            var clsList = mdb.Semester.ToList();
            return View(clsList);
        }

        [HttpPost]
        public ActionResult NewClassEntry(string ClassName)
        {
            Semester cls = new Semester();
            cls.Semester_name = ClassName;
            mdb.Semester.Add(cls);
            mdb.SaveChanges();
            TempData["success"] = "Class/Programm Successfully Added";
            return RedirectToAction("ClassList");
        }

        [HttpGet]
        public ActionResult ClassEdit(int id)
        {
            Semester cls = mdb.Semester.SingleOrDefault(a => a.Id == id);

            return View(cls);
        }

        [HttpPost]
        public ActionResult ClassEdit(Semester model)
        {
            Semester cls = mdb.Semester.First(a => a.Id.Equals(model.Id));
            if (cls == null)
                return HttpNotFound();

            cls.Semester_name = model.Semester_name;
            mdb.SaveChanges();
            return RedirectToAction("ClassList");
        }

        public ActionResult ClassDelete(Semester model)
        {
            Semester cls = mdb.Semester.First(a => a.Id.Equals(model.Id));
            mdb.Semester.Remove(cls);
            mdb.SaveChanges();
            TempData["success"] = "Class Successfully Deleted";
            return RedirectToAction("ClassList");
        }

        public ActionResult SectionList()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            return View(mdb.Section.ToList());
        }

        [HttpPost]
        public ActionResult SectionEntry(string SectioinName, string Semester, string Department)
        {
            Section s = new Section();
            s.section_name = SectioinName;
            s.semester_Name = Semester;

            if (Department == "")
            {
                s.deparment_Name = "0";
            }
            else
            {
                s.deparment_Name = Department;
            }
            bool sectionExist = mdb.Section.Any(c => c.section_name.Equals(SectioinName) && (c.semester_Name.Equals(Semester)));
            if (sectionExist)
            {
                TempData["success"] = "Section Already Saved";
                return RedirectToAction("SectionList");
            }
            else
            {
                mdb.Section.Add(s);
                mdb.SaveChanges();
                TempData["success"] = "Section Successfully Saved";
            }

            return RedirectToAction("SectionList");
        }

        public ActionResult SectionEdit(int id)
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            var section = mdb.Section.Where(a => a.id == id).FirstOrDefault();
            return View(section);
        }

        [HttpPost]
        public ActionResult SectionEdit(Section model)
        {
            var section = mdb.Section.Where(a => a.id == model.id).FirstOrDefault();
            section.section_name = model.section_name;
            section.semester_Name = model.semester_Name;
            if (section.deparment_Name == "")
            {
                section.deparment_Name = "0";
            }
            else
            {
                section.deparment_Name = model.deparment_Name;
            }
            mdb.Entry(section).State = EntityState.Modified;
            mdb.SaveChanges();
            return RedirectToAction("SectionList");
        }

        public ActionResult SectionDelete(int id)
        {
            var section = mdb.Section.Where(a => a.id == id).FirstOrDefault();
            mdb.Section.Remove(section);
            mdb.SaveChanges();
            return RedirectToAction("SectionList");
        }

        public ActionResult DepartmentList()
        {
            return View(mdb.Department.ToList());
        }

        [HttpPost]
        public ActionResult DeptEntry(string deptName)
        {
            if (ModelState.IsValid)
            {
                Department dpt = new Department();
                dpt.Dep_name = deptName;
                mdb.Department.Add(dpt);
                mdb.SaveChanges();
                return RedirectToAction("DepartmentList");
            }

            return View();
        }

        public ActionResult DeptEdit(int id)
        {
            var dept = mdb.Department.Where(m => m.Id == id).FirstOrDefault();
            return View(dept);
        }

        [HttpPost]
        public ActionResult DeptEdit(Department dpt)
        {
            var dept = mdb.Department.Where(m => m.Id == dpt.Id).FirstOrDefault();
            dept.Dep_name = dpt.Dep_name;
            mdb.Entry(dept).State = EntityState.Modified;
            mdb.SaveChanges();
            return RedirectToAction("DepartmentList");
        }

        public ActionResult DeptDelete(int id)
        {
            var dept = mdb.Department.Where(m => m.Id == id).FirstOrDefault();
            mdb.Department.Remove(dept);
            mdb.SaveChanges();
            return RedirectToAction("DepartmentList");
        }

        public ActionResult StatusList()
        {
            return View(mdb.Status.ToList());
        }



        [HttpPost]
        public ActionResult StatusCreate(string Std_status)
        {
            if (ModelState.IsValid)
            {
                Status sts = new Status();
                sts.Std_status = Std_status;
                mdb.Status.Add(sts);
                mdb.SaveChanges();
                return RedirectToAction("StatusList");
            }

            return View();
        }

        public ActionResult StatusEdit(int id)
        {
            var sts = mdb.Status.Where(m => m.id == id).FirstOrDefault();
            return View(sts);
        }

        [HttpPost]
        public ActionResult StatusEdit(Status sts)
        {
            var stts = mdb.Status.Where(m => m.id == sts.id).FirstOrDefault();
            stts.Std_status = sts.Std_status;
            mdb.Entry(stts).State = EntityState.Modified;
            mdb.SaveChanges();
            return RedirectToAction("StatusList");
        }

        public ActionResult StatusDelete(int id)
        {
            var stts = mdb.Status.Where(m => m.id == id).FirstOrDefault();
            mdb.Status.Remove(stts);
            mdb.SaveChanges();
            return RedirectToAction("StatusList");
        }
    }
}
