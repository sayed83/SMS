using MvcUMS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace MvcUMS.Controllers
{
    public class LibraryController : Controller
    {
        private Library lc = new Library();
        StudentContext sdb = new StudentContext();
        UMSEntities1 sdbs = new UMSEntities1();
        private StuffContext sc = new StuffContext();
        //
        // GET: /Library/

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult getBookCode(string term)
       {

            var ibc = new List<Models.IndividualBookCode>();
            ibc = lc.IndividualBookCode.Where(x => x.BookId.Contains(term.ToLower())).ToList();
            return Json(ibc, JsonRequestBehavior.AllowGet);
        } 

        /// <summary>
        /// BookCode Searching
        /// </summary>
        /// <returns></returns>
        //[HttpGet]
        public ActionResult SearchBookCode(string bookid)
        {
            ViewBag.advancedsearch = "advancedsearch";
            if (bookid == null)
           {
               var ibcs = from b in lc.Book.ToList()
                          join ibc in lc.IndividualBookCode.ToList() on b.BookId equals ibc.BookId
                          orderby ibc.BookCode
                         
                          select new bookid { BookId = ibc.BookId, BookCode = ibc.BookCode };

 
               return View(ibcs);
           }
            else
           {
               ViewBag.FilterValue = bookid;
               var ibcs = from b in lc.Book.ToList()
                          join ibc in lc.IndividualBookCode.ToList() on b.BookId equals ibc.BookId
                          orderby ibc.BookCode
                          where b.BookId.Equals(bookid)
                          select new bookid { BookId = ibc.BookId, BookCode = ibc.BookCode };
               
               
               return View(ibcs);
           }
                  
           
        }
        //[HttpPost]
        //public ActionResult SearchBookCode(string bookid, int? Page_No)
        //{

        //    var ibcs = from b in lc.Book.ToList()
        //               join ibc in lc.IndividualBookCode.ToList() on b.BookId equals ibc.BookId 
        //               orderby ibc.BookCode
        //               where b.BookId.Equals(bookid)
        //               select new bookid { BookId = ibc.BookId, BookCode = ibc.BookCode };
          
        //    int Size_Of_Page = 5;
        //    int No_Of_Page = (Page_No ?? 1);
        //    return View(ibcs.ToPagedList(No_Of_Page, Size_Of_Page));
        //}



       [HttpGet]
        public ActionResult BookSearch(int? Page_No)
        {
            ViewBag.advancedsearch = "advancedsearch";
            ViewBag.BookCatagory = new SelectList(lc.BookCatagory, "Id", "CatagoryName");
            TempData["Password"] = "1";
            var books = from b in lc.Book.ToList()
                        join bs in lc.BookIssue.ToList() on b.BookId equals bs.BookId into bi
                        from tr in bi.Where(x => x.Type.Equals(true)).DefaultIfEmpty()
                       
                        join a in lc.Auther.ToList() on b.AutherId equals a.Id
                        orderby b.BookId
                        group tr by new { Id = (tr == null ? "" : tr.BookId), b.BookId, b.BookName, b.BookCopy, a.authername, b.Price,b.isactive } into s
                        select new BookList
                        {
                            Id = s.Key.Id, 
                            BookId = s.Key.BookId,
                            BookName = s.Key.BookName,
                            authername = s.Key.authername,
                            BookCopy = s.Key.BookCopy,
                            Prise = s.Key.Price,
                            isactive=s.Key.isactive,
                            GetBook = s.Count()
                        };

            
            
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(books.ToPagedList(No_Of_Page, Size_Of_Page));
           
        }
       [HttpPost]
       public ActionResult BookSearch(string catagory, string auther, int? Page_No)
       {
           TempData["Password"] = "1";
           int t = int.Parse(auther);
           var books = from b in lc.Book.ToList()
                       join bs in lc.BookIssue.ToList() on b.BookId equals bs.BookId into bi
                       from tr in bi.Where(x => x.Type.Equals(true)).DefaultIfEmpty()
                      
                       join a in lc.Auther.ToList() on b.AutherId equals a.Id
                       orderby b.BookId
                       where b.AutherId == t

                       group tr by new { Id = (tr == null ? "" : tr.BookId), b.BookId, b.BookName, b.BookCopy, a.authername, b.Price,b.isactive } into s
                       select new BookList
                       {
                           Id = s.Key.Id,
                           BookId = s.Key.BookId,
                           BookName = s.Key.BookName,
                           authername = s.Key.authername,
                           BookCopy = s.Key.BookCopy,
                           Prise = s.Key.Price,
                           isactive = s.Key.isactive,
                           GetBook = s.Count()
                       };

           ViewBag.BookCatagory = new SelectList(lc.BookCatagory, "Id", "CatagoryName");
           ViewBag.Stuff = new SelectList(sc.StuffDepartment, "DepartmentId", "DepartmentName");
           int Size_Of_Page = 10;
           int No_Of_Page = (Page_No ?? 1);
           return View(books.ToPagedList(No_Of_Page, Size_Of_Page));
           
       }
      public ActionResult BookActive()
       {
           return View();
       }

        [HttpPost]
      public ActionResult BookActive(string id , string pass,string p)
       {
           if (pass == p)
           {
               var ac = lc.Book.Where(x => x.BookId.Equals(id)).Select(x => x.isactive).SingleOrDefault();
               Book b = new Book();
               b = lc.Book.Where(x => x.BookId.Equals(id)).SingleOrDefault();
               if (ac == true)
               {
                   var book = lc.BookIssue.Where(x => x.BookId.Equals(id) && x.Type.Equals(true)).Select(x => x.Id).Count();
                   if (book == 0)
                   {
                       b.isactive = false;
                   }
               }
               else
               {
                   b.isactive = true;

                   TempData["AlertMessage"] = "The student successfully Actived";
               }
               lc.Entry(b).State = EntityState.Modified;
               lc.SaveChanges();
               return RedirectToAction("BookSearch");
           }
            else
           {
               TempData["mass"] = "Password Not Match";
               return RedirectToAction("BookSearch");
           }
           
       }
        public ActionResult PurchaseBook(string id)
        {
            Book b = new Book();
            b.BookId = id;
            return View(b);
        }
        [HttpPost]
        public ActionResult UpdateBook(Book id, string Command)
        {
           
            var  b = lc.Book.Where(x => x.BookId.Equals(id.BookId)).SingleOrDefault();
            //var s = lc.Book.Where(x => x.BookId.Equals(id.BookId)).Select(x => x.BookCopy).SingleOrDefault();
            b.BookCopy = b.BookCopy + id.BookCopy;
            lc.Entry(b).State = EntityState.Modified;
            lc.SaveChanges();
            for (var i = 1; i <= id.BookCopy; i++)
            {
                IndividualBookCode ibc = new IndividualBookCode();
                var code = Convert.ToInt32(lc.IndividualBookCode.Where(x => x.BookId.Equals(b.BookId)).Max(t => (int?)t.Code));
                ibc.Code =code+1;
                ibc.Id = Guid.NewGuid();
                ibc.BookId = b.BookId;
                if (b.Catagory == 1)
                {
                    ibc.BookCode = "l" + "00" + ibc.Code;
                }
                else
                {
                    ibc.BookCode = "b" + "00" + ibc.Code;
                }
                lc.IndividualBookCode.Add(ibc);
                lc.SaveChanges();
            }
            return RedirectToAction("BookSearch");
        }


        /// <summary>
        /// Create Book For Library
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult  CreateBook()
        {
            ViewBag.advancedsearch = "advancedsearch";
            ViewBag.BookCatagory = new SelectList(lc.BookCatagory, "Id", "CatagoryName");
            ViewBag.Class = new SelectList(sdb.Semester, "Id", "Semester_name");
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            //ViewBag.Class = new SelectList(sdb.Class, "Id", "ClassName");
            return View();
        }
        [HttpPost]
        public ActionResult CreateBook(Book model)
        {
            if (ModelState.IsValid)
            {
                var bookid = lc.Book.Where(x => x.BookId.Equals(model.BookId)).Select(a => a.BookId).Count();
                if (bookid == 0)
                { 
                model.Id = Guid.NewGuid();
                model.isactive = true;
                lc.Book.Add(model);
                lc.SaveChanges();
               
                 
                for(var i=1 ;i<=model.BookCopy;i++)
                   {
                       IndividualBookCode ibc = new IndividualBookCode();
                       var code = Convert.ToInt32(lc.IndividualBookCode.Where(x => x.BookId.Equals(model.BookId)).Max(t => (int?)t.Code));
                       ibc.Code  += i;
                       ibc.Id=Guid.NewGuid();
                       ibc.BookId=model.BookId;
                       if (model.Catagory == 1)
                       {
                           ibc.BookCode = "l"+"00"+ibc.Code;
                       }
                       else
                       {
                           ibc.BookCode = "b" + "00" + ibc.Code;
                       }
                       lc.IndividualBookCode.Add(ibc);
                       lc.SaveChanges();
                   }

                return RedirectToAction("BookSearch", "Library");
               }
                else
                {
                    TempData["bookid"] = "Book Code alrady exits.";
                    ViewBag.BookCatagory = new SelectList(lc.BookCatagory, "Id", "CatagoryName");
                    ViewBag.Class = new SelectList(sdb.Semester, "Id", "Semester_name");
                    ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
                    //ViewBag.Class = new SelectList(sdb.Class, "Id", "ClassName");
                }
            }
            return View(model);
        }



        /// <summary>
        /// get book from library
        /// </summary>
        /// <returns></returns>
        /// 
      [HttpGet]
      public ActionResult SearchStudentEmployeeList()
        {

            ViewBag.advancedsearch = "advancedsearch";
                ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
                ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
                ViewBag.Section = new SelectList(sdb.Section, "section_name", "section_name");
                ViewBag.StuffList = new SelectList(sc.StuffDepartment, "DepartmentId", "DepartmentName");  
             
            return View();
        }


        [HttpPost]
      public ActionResult SearchStudentEmployeeList(string Std_id, string Department, string Semester, string Section, string Stuffid, FormCollection form)
      {
          ViewBag.advancedsearch = "advancedsearch";
          string strDDLValue = form["Sample"].ToString();
          ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
          ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
          ViewBag.Section = new SelectList(sdb.Section, "section_name", "section_name");
          ViewBag.StuffList = new SelectList(sc.StuffDepartment, "DepartmentId", "DepartmentName");
          if (Std_id != "" && strDDLValue=="1")
          {
              TempData["list"] = "List";
              TempData["emp"] = null;
          }
          else if (Stuffid != "" && strDDLValue == "2")
          {
              TempData["emp"] = "List";
              TempData["list"] = null; 
          }
           
            return View();
        }
        /// <summary>
        /// autocompletd employee textbox
        /// </summary>
        /// <param name="term"></param>
        /// <param name="dep"></param>
        /// <returns></returns>
        public JsonResult getStuffIdData(string term, int dep)
        {

            var stuff = new List<Models.Stuff>();
            stuff = sc.Stuff.Where(x => x.MemberCode.Contains(term.ToLower()) 
                                         && x.Department.Equals(dep)).ToList();
            return Json(stuff, JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult EmployeeDetails(string Stuffid)
        {
            if (Stuffid != "")
            {
                var employee = from st in sc.Stuff.ToList()
                               join sd in sc.StuffDepartment.ToList() on st.Department equals sd.DepartmentId
                               where st.MemberCode.Equals(Stuffid)
                               select new stuffFullDetails
                               {
                                   Stuff = st,
                                   StuffDepartment = sd,
                               };
                return View(employee.ToList());
            }
            else
            {
                var employee = from st in sc.Stuff.ToList()
                               join sd in sc.StuffDepartment.ToList() on st.Department equals sd.DepartmentId
                               select new stuffFullDetails
                               {
                                   Stuff = st,
                                   StuffDepartment = sd,
                               };
                return View(employee.ToList());
            }
          
        }
        
        public ActionResult StudentDetails(string Std_id)
        {
            var student = new List<Models.Sp_StdAdvanceSearc_Result>();
            using (var dc = new MvcUMS.Models.SchooldbEntities())
            {
                ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
                ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
                ViewBag.Section = new SelectList(sdb.Section, "section_name", "section_name");
                ViewBag.StuffList = new SelectList(sc.StuffDepartment, "DepartmentId", "DepartmentName");
                if (Std_id != "")
                    student = dc.Sp_StdAdvanceSearc().Where(a => a.Std_id.Equals(Std_id)).ToList();
              
                else
                    student = dc.Sp_StdAdvanceSearc().ToList();
            }
            return View(student);

        }



        public ActionResult GetboookFromLibrary(string id)
        {
            ViewBag.advancedsearch = "advancedsearch";
            ViewBag.BookCatagory = new SelectList(lc.BookCatagory, "Id", "CatagoryName");
         
            TempData["studentid"] = id;
            return View();
        }
        [HttpPost]
        public ActionResult GetboookFromLibrary(string book, string bookname, string studentid, string Date)
        {
            var getcopy = 0;
            BookIssue p = new BookIssue();
             var student = sdbs.Student_info.Where(x => x.Std_id.Equals(studentid)).Select(x => x.Std_id).Count();
             var stuff =   sc.Stuff.Where(x => x.MemberCode.Equals(studentid)).Select(x => x.Id).SingleOrDefault();
             var BookCopy = lc.Book.Where(x => x.BookId.Equals(book)).Select(x => x.BookCopy).SingleOrDefault();
              if(student>0)
              {
                  getcopy = lc.BookIssue.Where(x => x.BookId.Equals(book) && x.Type == true && x.StudentId.Equals(studentid)).Select(a => a.BookId).Count();
              }
              else
              {
                  getcopy = lc.BookIssue.Where(x => x.BookId.Equals(book) && x.Type == true && x.StuffId.Equals(stuff)).Select(a => a.BookId).Count();
              }
             if (BookCopy - getcopy > 0 && getcopy == 0)
             {
                 p.BookId = book;
                 p.BookCode = bookname;
                 p.Id = Guid.NewGuid();
                 if (student > 0)
                 {
                     p.StudentId = studentid;
                     p.StuffId = Guid.Empty;
                 }
                 else
                 {
                     p.StuffId = stuff;
                     p.StudentId = "0";
                 }
                 p.IssuedDate = DateTime.Now.Date;
                 p.SubmissionDate = Convert.ToDateTime(Date).Date;
                 p.Type = true;
                 p.EmployeeId = Guid.Empty;
                 lc.BookIssue.Add(p);
                 lc.SaveChanges();
                 ViewBag.BookCatagory = new SelectList(lc.BookCatagory, "Id", "CatagoryName");

                 if (student>0 && stuff==Guid.Empty)
                 {
                     TempData["list"] = "List";
                     TempData["emp"] = null;
                 }
                 else if (stuff!=Guid.Empty && student==0)
                 {
                     TempData["emp"] = "List";
                     TempData["list"] = null;
                 }
                 return View();
             }
             else if (getcopy > 0)
             {
                     ViewBag.BookCatagory = new SelectList(lc.BookCatagory, "Id", "CatagoryName");
                
                     TempData["text"] = "You have alrady Colected this Book.";
                     return View();
             }
             else
             {
                 ViewBag.BookCatagory = new SelectList(lc.BookCatagory, "Id", "CatagoryName");
                 TempData["text"] = "There are no Book.";
                 return View();
             }
           
            
         
        }
        public ActionResult GetEmployeeList(string studentid)
        {
            var stuff = sc.Stuff.Where(x => x.MemberCode.Equals(studentid)).Select(x => x.Id).SingleOrDefault();
            var stuffbookissue = from bi in lc.BookIssue.ToList()
                                 join s in sc.Stuff.ToList() on bi.StuffId equals s.Id
                                 where bi.StuffId.Equals(stuff)
                                 select new stuffbookissue
                                 {
                                     BookIssue = bi,
                                     Stuff = s
                                 };
            return View(stuffbookissue.ToList());
        }
        public ActionResult Getlist(string studentid)
        {
            var model = new List<BookIssue>();
            model = lc.BookIssue.Where(x => x.StudentId.Equals(studentid) && x.Type==true).OrderBy(x=>x.IssuedDate).ToList();

            return View(model);
        }

      

        /// <summary>
        /// Summary for library book for a student or a department or a semester 
        /// </summary>
        /// <returns></returns>

        public ActionResult LibraryBookSummaryForStudent()
        {
            ViewBag.advancedsearch = "advancedsearch";
                ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
                ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
                ViewBag.Section = new SelectList(sdb.Section, "section_name", "section_name");
                ViewBag.StuffList = new SelectList(sc.StuffDepartment, "DepartmentId", "DepartmentName");  
                var bookisue =  from bi in lc.BookIssue.ToList()
                                join b in lc.Book.ToList() on bi.BookId equals b.BookId
                                where (bi.Type.Equals(true))
                                orderby bi.BookId
                                select new bookbookissuestudent {BookIssue=bi,Book=b};


                return View(bookisue.ToList());
         
        }
        [HttpPost]
        public ActionResult LibraryBookSummaryForStudent(string Std_id, string Stuffid, string Sample)
        {

            ViewBag.advancedsearch = "advancedsearch";
                ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
                ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
                ViewBag.Section = new SelectList(sdb.Section, "section_name", "section_name");
                ViewBag.StuffList = new SelectList(sc.StuffDepartment, "DepartmentId", "DepartmentName");
                if (Std_id != "" && Sample=="1")
                {
                    var bookis = from bi in lc.BookIssue.ToList()
                                 join b in lc.Book.ToList() on bi.BookId equals b.BookId
                                 where (bi.StudentId.Equals(Std_id) && bi.Type.Equals(true))
                                 select new bookbookissuestudent { Book = b, BookIssue = bi };
                    if(bookis.Count()>0)
                    { ViewBag.Visibility = "true"; }
                    else { ViewBag.Visibility = "false"; }
                   
                    return View(bookis.ToList());
                }
                else if (Stuffid != ""  && Sample == "2")
                {
                    var stuff = sc.Stuff.Where(x => x.MemberCode.Equals(Stuffid)).Select(a => a.Id).SingleOrDefault();
                    var bookis = from bi in lc.BookIssue.ToList()
                                 join b in lc.Book.ToList() on bi.BookId equals b.BookId
                                 where (bi.StuffId.Equals(stuff) && bi.Type.Equals(true))
                                 select new bookbookissuestudent { Book = b, BookIssue = bi };
                    if (bookis.Count() >0)
                    { ViewBag.Visibility = "true"; }
                    else { ViewBag.Visibility = "false"; }
                    return View(bookis.ToList());
                }
  
                else
                {
                    var bookisue = from bi in lc.BookIssue.ToList()
                                   join b in lc.Book.ToList() on bi.BookId equals b.BookId
                                   where (bi.Type.Equals(true))
                                   orderby bi.BookId
                                   select new bookbookissuestudent { BookIssue = bi, Book = b };
                    ViewBag.Visibility = "false";
                    return View(bookisue.ToList());
                }

        }

        /// <summary>
        /// Submit Book from Student or Stuff
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BookSubmission(string[] ids)
        {
             BookIssue bookis=new BookIssue();
            Guid[] id = null;
            if (ids != null)
            {
                id = new Guid[ids.Length];
                int j = 0;
                foreach (string i in ids)
                {
                    Guid.TryParse(i, out id[j++]);
                }
            }
            if (id != null && id.Length > 0)
            {
            
                foreach (var i in id)
                {
                    var user = lc.BookIssue.Single(u => u.Id.Equals(i));

                    user.Type = false;
                 
                    lc.SaveChanges();

                }

            }

            return RedirectToAction("SearchIssuedBook"); 
        }

        
        public ActionResult IssuedBookDetail(string id)
        {
            var student = sdbs.Student_info.Where(x => x.Std_id.Equals(id)).Select(x => x.Std_id).Count();
            var stuff = sc.Stuff.Where(x => x.MemberCode.Equals(id)).Select(x => x.Id).SingleOrDefault();
            if(student==0)
            {
                var bookisue = from bi in lc.BookIssue.ToList()
                               join b in lc.Book.ToList() on bi.BookId equals b.BookId
                               where (bi.Type.Equals(true) && bi.StuffId.Equals(stuff))
                               orderby bi.BookId
                               select new bookbookissuestudent { BookIssue = bi, Book = b };
                return View(bookisue.ToList());
            }
           else
            {
                var bookisue = from bi in lc.BookIssue.ToList()
                               join b in lc.Book.ToList() on bi.BookId equals b.BookId
                               where (bi.Type.Equals(true) && bi.StudentId.Equals(id))
                               orderby bi.BookId
                               select new bookbookissuestudent { BookIssue = bi, Book = b };
                return View(bookisue.ToList());
            }


          
        }
        /// <summary>
        /// Search all Issued Book 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
       public ActionResult SearchIssuedBook()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            ViewBag.Section = new SelectList(sdb.Section, "section_name", "section_name");
            ViewBag.StuffList = new SelectList(sc.StuffDepartment, "DepartmentId", "DepartmentName");  
           var IssuedBook = (from bi in lc.BookIssue.ToList()
                                  join si in sdbs.Student_info.ToList() on bi.StudentId equals si.Std_id
                                  where bi.Type.Equals(true)
                                  group si by new { si.Std_id, si.Std_FName, si.Std_LName, si.Contact_no } into s
                                  select new IssuedBookList
                                  {   
                                      Id=s.Key.Std_id,
                                      Name = s.Key.Std_FName + " " + s.Key.Std_LName,
                                      ContactNumber = s.Key.Contact_no,
                                      Type = "Student",
                                      TotalBook = s.Count()
                                  }).Union(from bi in lc.BookIssue.ToList()
                                           join si in sc.Stuff.ToList() on bi.StuffId equals si.Id
                                           join sd in sc.StuffDepartment.ToList() on si.Department equals sd.DepartmentId
                                           where bi.Type.Equals(true)
                                           group si by new { si.Id,si.MemberCode ,si.FirstName, si.LastName, si.ContactNo, sd.DepartmentName } into s
                                           select new IssuedBookList
                                           {   Id=s.Key.MemberCode,
                                               Name = s.Key.FirstName +" "+ s.Key.LastName,
                                               ContactNumber = s.Key.ContactNo,
                                               Type = s.Key.DepartmentName,
                                               TotalBook = s.Count()
                                           });
            return View(IssuedBook.ToList());
        }
        [HttpPost]
        public ActionResult SearchIssuedBook(string Department, string Semester, string Section, string StuffList, string Sample)
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            ViewBag.Section = new SelectList(sdb.Section, "section_name", "section_name");
            ViewBag.StuffList = new SelectList(sc.StuffDepartment, "DepartmentId", "DepartmentName");  
        
            if (Department!="" && StuffList=="" && Semester=="" && Section=="" && Sample=="1")
            {
                var IssuedBook = (from bi in lc.BookIssue.ToList()
                                  join si in sdbs.Student_info.ToList() on bi.StudentId equals si.Std_id
                                  join ci in sdb.CurrentAcademicInfo.ToList() on si.Std_id equals ci.Std_id
                                  where bi.Type.Equals(true) && ci.Department.Equals(Department)
                                  group si by new { si.Std_id, si.Std_FName, si.Std_LName, si.Contact_no } into s
                                  select new IssuedBookList
                                  {
                                      Name = s.Key.Std_FName + " " + s.Key.Std_LName,
                                      ContactNumber = s.Key.Contact_no,
                                      Type = "Student",
                                      TotalBook = s.Count()
                                  });
                return View(IssuedBook.ToList());
            }
            else if (Department != "" && StuffList == "" && Semester != "" && Section == "" && Sample == "1")
            {
                var IssuedBook = (from bi in lc.BookIssue.ToList()
                                  join si in sdbs.Student_info.ToList() on bi.StudentId equals si.Std_id
                                  join ci in sdb.CurrentAcademicInfo.ToList() on si.Std_id equals ci.Std_id
                                  where bi.Type.Equals(true) && ci.Semester.Equals(Semester) && ci.Department.Equals(Department)
                                  group si by new { si.Std_id, si.Std_FName, si.Std_LName, si.Contact_no } into s
                                  select new IssuedBookList
                                  {
                                      Name = s.Key.Std_FName + " " + s.Key.Std_LName,
                                      ContactNumber = s.Key.Contact_no,
                                      Type = "Student",
                                      TotalBook = s.Count()
                                  });
                return View(IssuedBook.ToList());
            }
            else if (Department != "" && StuffList == "" && Semester == "" && Section != "" && Sample == "1")
            {
                var IssuedBook = (from bi in lc.BookIssue.ToList()
                                  join si in sdbs.Student_info.ToList() on bi.StudentId equals si.Std_id
                                  join ci in sdb.CurrentAcademicInfo.ToList() on si.Std_id equals ci.Std_id
                                  where bi.Type.Equals(true) && ci.Section.Equals(Section) && ci.Department.Equals(Department)
                                  group si by new { si.Std_id, si.Std_FName, si.Std_LName, si.Contact_no } into s
                                  select new IssuedBookList
                                  {
                                      Name = s.Key.Std_FName + " " + s.Key.Std_LName,
                                      ContactNumber = s.Key.Contact_no,
                                      Type = "Student",
                                      TotalBook = s.Count()
                                  });
                return View(IssuedBook.ToList());
            }
            else if (Department == "" && StuffList == "" && Semester != "" && Section != "" && Sample == "1")
            {
                var IssuedBook = (from bi in lc.BookIssue.ToList()
                                  join si in sdbs.Student_info.ToList() on bi.StudentId equals si.Std_id
                                  join ci in sdb.CurrentAcademicInfo.ToList() on si.Std_id equals ci.Std_id
                                  where bi.Type.Equals(true) && ci.Section.Equals(Section) && ci.Semester.Equals(Semester)
                                  group si by new { si.Std_id, si.Std_FName, si.Std_LName, si.Contact_no } into s
                                  select new IssuedBookList
                                  {
                                      Name = s.Key.Std_FName + " " + s.Key.Std_LName,
                                      ContactNumber = s.Key.Contact_no,
                                      Type = "Student",
                                      TotalBook = s.Count()
                                  });
                return View(IssuedBook.ToList());
            }
            else if (Department == "" && StuffList == "" && Semester != "" && Section == "" && Sample == "1")
            {
                var IssuedBook = (from bi in lc.BookIssue.ToList()
                                  join si in sdbs.Student_info.ToList() on bi.StudentId equals si.Std_id
                                  join ci in sdb.CurrentAcademicInfo.ToList() on si.Std_id equals ci.Std_id
                                  where bi.Type.Equals(true) && ci.Semester.Equals(Semester)
                                  group si by new { si.Std_id, si.Std_FName, si.Std_LName, si.Contact_no } into s
                                  select new IssuedBookList
                                  {
                                      Name = s.Key.Std_FName + " " + s.Key.Std_LName,
                                      ContactNumber = s.Key.Contact_no,
                                      Type = "Student",
                                      TotalBook = s.Count()
                                  });
                return View(IssuedBook.ToList());
            }
            else if (Department == "" && StuffList == "" && Semester == "" && Section != "" && Sample == "1")
            {
                var IssuedBook = (from bi in lc.BookIssue.ToList()
                                  join si in sdbs.Student_info.ToList() on bi.StudentId equals si.Std_id
                                  join ci in sdb.CurrentAcademicInfo.ToList() on si.Std_id equals ci.Std_id
                                  where bi.Type.Equals(true) && ci.Section.Equals(Section)
                                  group si by new { si.Std_id, si.Std_FName, si.Std_LName, si.Contact_no } into s
                                  select new IssuedBookList
                                  {
                                      Name = s.Key.Std_FName + " " + s.Key.Std_LName,
                                      ContactNumber = s.Key.Contact_no,
                                      Type = "Student",
                                      TotalBook = s.Count()
                                  });
                return View(IssuedBook.ToList());
            }

            else if (Department != "" && StuffList == "" && Semester != "" && Section != "" && Sample == "1")
            {
                var IssuedBook = (from bi in lc.BookIssue.ToList()
                                  join si in sdbs.Student_info.ToList() on bi.StudentId equals si.Std_id
                                  join ci in sdb.CurrentAcademicInfo.ToList() on si.Std_id equals ci.Std_id
                                  where bi.Type.Equals(true) && ci.Department.Equals(Department) && ci.Section.Equals(Section) && ci.Semester.Equals(Semester)
                                  group si by new { si.Std_id, si.Std_FName, si.Std_LName, si.Contact_no } into s
                                  select new IssuedBookList
                                  {
                                      Name = s.Key.Std_FName +" "+s.Key.Std_LName,
                                      ContactNumber = s.Key.Contact_no,
                                      Type = "Student",
                                      TotalBook = s.Count()
                                  });
                return View(IssuedBook.ToList());
            }
            else if (Department == "" && StuffList != "" && Semester == "" && Section == "" && Sample == "2")
            {
                int sl = int.Parse(StuffList);
                var IssuedBook = (from bi in lc.BookIssue.ToList()
                                  join si in sc.Stuff.ToList() on bi.StuffId equals si.Id
                                  join sd in sc.StuffDepartment.ToList() on si.Department equals sd.DepartmentId
                                  where bi.Type.Equals(true) && sd.DepartmentId.Equals(sl)
                                  group bi by new { si.Id, si.FirstName, si.LastName, si.ContactNo, sd.DepartmentName } into s
                                  select new IssuedBookList
                                  {
                                      Name = s.Key.FirstName +" "+s.Key.LastName,
                                      ContactNumber = s.Key.ContactNo,
                                      Type = s.Key.DepartmentName,
                                      TotalBook = s.Count()
                                  });
                return View(IssuedBook.ToList());
            }
            else
            {
                var IssuedBook = (from bi in lc.BookIssue.ToList()
                                  join si in sdbs.Student_info.ToList() on bi.StudentId equals si.Std_id
                                  where bi.Type.Equals(true)
                                  group si by new { si.Std_id, si.Std_FName, si.Std_LName, si.Contact_no } into s
                                  select new IssuedBookList
                                  {
                                      Name = s.Key.Std_FName + " " + s.Key.Std_LName,
                                      ContactNumber = s.Key.Contact_no,
                                      Type = "Student",
                                      TotalBook = s.Count()
                                  }).Union(from bi in lc.BookIssue.ToList()
                                           join si in sc.Stuff.ToList() on bi.StuffId equals si.Id
                                           join sd in sc.StuffDepartment.ToList() on si.Department equals sd.DepartmentId
                                           where bi.Type.Equals(true)
                                           group si by new { si.Id, si.FirstName, si.LastName, si.ContactNo, sd.DepartmentName } into s
                                           select new IssuedBookList
                                           {
                                               Name = s.Key.FirstName +" "+ s.Key.LastName,
                                               ContactNumber = s.Key.ContactNo,
                                               Type = s.Key.DepartmentName,
                                               TotalBook = s.Count()
                                           });
                return View(IssuedBook.ToList());
            }
        
        }


        /// <summary>
        /// Book catagory with auther Creation
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CatagoryautherCreatin()
        {  
            ViewBag.Auther=new SelectList(lc.Auther,"Id","authername");
            ViewBag.BookCatagory = new SelectList(lc.BookCatagory, "Id", "CatagoryName");
            return View();
        }

        [HttpPost]
        public ActionResult CatagoryautherCreatin(int Auther, int BookCatagory)
        {
            var s = lc.CatagoryAuther.Where(x => x.AutherId.Equals(Auther) && x.CatagoryId.Equals(BookCatagory)).Count();
            if (s > 0)
            { TempData["error"] = "Alrady Created."; }
            else
            {
                var auther = new CatagoryAuther();
                auther.AutherId = Auther;
                auther.CatagoryId = BookCatagory;

                lc.CatagoryAuther.Add(auther);
                lc.SaveChanges();
            }

            ViewBag.Auther = new SelectList(lc.Auther, "Id", "authername");
            ViewBag.BookCatagory = new SelectList(lc.BookCatagory, "Id", "CatagoryName");
            return View();
        }

        public ActionResult BookauCatagoryList(int? Page_No)
        {
            var autherlist = from bs in lc.CatagoryAuther.ToList()
                             join bc in lc.BookCatagory.ToList() on bs.CatagoryId equals bc.Id
                             join a in lc.Auther.ToList() on bs.AutherId equals a.Id
                             select new cataauther {authername=a.authername,
                                                    catagoryname=bc.CatagoryName };
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(autherlist.ToPagedList(No_Of_Page, Size_Of_Page));
        }






        /// <summary>
        /// Book catagory Creation
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CatagoryCreatin()
        { return View(); }

        [HttpPost]
        public ActionResult CatagoryCreatin(string catagoryname)
        {
            var s = lc.BookCatagory.Where(x => x.CatagoryName.Equals(catagoryname)).Count();
            if (s > 0)
            { TempData["error"] = "Alrady Created."; }
            else
            {
                var auther = new  BookCatagory();
                auther.CatagoryName = catagoryname;
                auther.IsActive = true;
                auther.Date = DateTime.Now.Date;
                lc.BookCatagory.Add(auther);
                lc.SaveChanges();
            }
            return View();
        }

        public ActionResult BookCatagoryList(int? Page_No)
        {
            var autherlist = lc.BookCatagory.ToList();
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(autherlist.ToPagedList(No_Of_Page, Size_Of_Page));
        }


        /// <summary>
        /// Auther Creation
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AutherCreatin()
        {
            ViewBag.basicdatatable = "basicdatatable";
            return View();
        }
        [HttpPost]
        public ActionResult AutherCreatin(string authername)
        {
            var s = lc.Auther.Where(x=>x.authername.Equals(authername)).Count();
            if (s > 0)
            { TempData["error"] = "Alrady Created."; }
            else
            {
                var auther = new Auther();
                auther.authername = authername;
                auther.isactive = true;
                auther.createdate = DateTime.Now.Date;
                lc.Auther.Add(auther);
                lc.SaveChanges();
            }
            return View();
        }

        public ActionResult AutherList()
        {
            var autherlist = lc.Auther.ToList();
           
            return View(autherlist);
        }
       

       
        /// <summary>
        /// search booklist
        /// </summary>
        /// <param name="term"></param>
        /// <param name="bc"></param>
        /// <param name="auth"></param>
        /// <returns></returns>
        public JsonResult getbookname(string term, string bc, string auth,string bi)
        {
            int s = int.Parse(bc);
            int t = int.Parse(auth);
           //x => x.BookCode.Contains(term.ToLower()) &&
            //var books = new List<Models.Book>();
            //books = lc.Book.Where(x => x.BookId.Contains(term.ToLower())&& x.Catagory.Equals(s) && x.AutherId.Equals(t)).ToList();
            var books = new List<Models.IndividualBookCode>();
            books = lc.IndividualBookCode.Where(x => x.BookCode.Contains(term.ToLower())&& x.BookId.Contains(bi)).ToList();
            return Json(books, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// get auther name for book catagory
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetAuther(string id)
        {

            int s = int.Parse(id);
            List<Auther> au = lc.Auther.ToList();
            List<CatagoryAuther> cau = lc.CatagoryAuther.Where(a => a.CatagoryId.Equals(s)).ToList(); ;
            var objcity = au.Join(cau, c => c.Id, cc => cc.AutherId, (c, cc) => new { Value = cc.AutherId, Name = c.authername }).ToList();
            SelectList objdata = new SelectList(objcity, "Value", "Name", 0);

            //return Json(objdata);
            return new JsonResult { Data = objdata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult GetBookId(string id)
        {

            int s = int.Parse(id);
            List<Book> book = lc.Book.Where(a=>a.AutherId.Equals(s)).ToList();
            List<Auther> cau = lc.Auther.ToList();
            var objcity = cau.Join(book, c => c.Id, cc => cc.AutherId, (c, cc) => new { Value = cc.BookId, Name = cc.BookName }).ToList();
            SelectList objdata = new SelectList(objcity, "Value", "Name", 0);

            //return Json(objdata);
            return new JsonResult { Data = objdata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        
          /// <summary>
        /// get book name and how many book copy have?
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetBookNameAndCopy(string bookcode)
        {

        
           var BookCopy = lc.Book.Where(x=>x.BookId.Equals(bookcode)).Select(x=>x.BookCopy).SingleOrDefault();
           var BookName = lc.Book.Where(x => x.BookId.Equals(bookcode)).Select(x => x.BookName).SingleOrDefault();
        
           var getcopy = lc.BookIssue.Where(x => x.BookId.Equals(bookcode) && x.Type == true).Select(a=>a.BookId).Count();
           var result = new { BookName = BookName, Copy = BookCopy-getcopy };
           return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }



        
    }
}
