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
using MvcEnergyPac.Models;

namespace MvcUMS.Controllers
{
    public class ReportController : Controller
    {
        //
        // GET: /Report/
       
        StudentContext sdb = new StudentContext();
        UMSEntities1 sdbs = new UMSEntities1();
        PaymentContext pc = new PaymentContext();
        ExpanceContext ec = new ExpanceContext();
        InventoryContext ic = new InventoryContext();
        StuffContext sc = new StuffContext();
        Library lc = new Library();
        UsersContext uc = new UsersContext();
        EventContext evc = new EventContext();
        AccountingContext acdb = new AccountingContext();
       
       
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult AdmissionReport()
        {
            ViewBag.basicdatatable = "basicdatatable";
            var s=new List<PaymentStudent>();
                 var studentpay = from stu in sdbs.Student_info.ToList()
                                       join stupay in sdb.StudentPayment.ToList() on stu.Std_id equals stupay.StudentId
                                       join cai in sdb.CurrentAcademicInfo.ToList() on stu.Std_id equals cai.Std_id
                                       where stupay.AdmissionFee>0
                                       select new PaymentStudent { student=stu, studentpayment=stupay, curacainfo=cai };
                 s = studentpay.ToList();

                 return View(s);
        }

        //[HttpGet]
        public ActionResult PaymentReport(string std_id,string Department, string Semester,string Section)
        {
            ViewBag.advancedsearch = "basicdatatable";
            var s = new List<PaymentReport>();
            if (std_id!="" || Department != "" || Semester != "" || Section != "")
            {
                ViewBag.depart = Department;
                ViewBag.semes = Semester;
                ViewBag.Secid = Section;
                ViewBag.std_id = std_id;
                s = incomereport(std_id, Department, Semester, Section);

            }
            else
            {
                var studentpay = from p in sdb.StudentPayment.ToList()
                                 join pt in pc.PaymentType.ToList() on p.PaymentTypeId equals pt.Id
                                 join si in sdbs.Student_info.ToList() on p.StudentId equals si.Std_id
                                 where (p.AdmissionFee == 0)
                                 group p by new { p.StudentId, si.Std_FName, si.Std_MName, si.Std_LName, p.Date, pt.PaymentType } into t
                                 select new PaymentReport
                                 {
                                     PaymentDate = t.Key.Date,
                                     StudentId = t.Key.StudentId,
                                     studentname = t.Key.Std_FName + " " + t.Key.Std_MName + " " + t.Key.Std_LName,
                                     totalfees = t.Sum(acs => acs.Amount),
                                     Payamount = t.Sum(acs => acs.PayAnount),
                                     
                                 };

                s = studentpay.ToList();
            }

           
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            ViewBag.Section = new SelectList(sdb.Section, "section_name", "section_name");
            
            return View(s);
          
        }

        [HttpGet]
        public ActionResult PaymentReportPrint(string std_id, string Department, string Semester, string Sec)
        {
            var s = new List<PaymentReport>();
            string userid = User.Identity.Name;
            int schoolid = uc.UserProfiles.Where(x => x.UserName.Equals(userid)).Select(x => x.SchoolId??0).SingleOrDefault();
            string schoolname = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.SchoolName).SingleOrDefault();
            string address = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.Address+" "+x.City+" "+x.Country).SingleOrDefault();
           
            if (!string.IsNullOrEmpty(std_id) || !string.IsNullOrEmpty(Department) || Semester != "" || Sec != "")
            {

                s = incomereport(std_id, Department, Semester, Sec);

            }
          


            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Report"), "PaymentCrystalReport.rpt"));
            rd.SetDataSource(s);
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
        public List<PaymentReport> incomereport(string std_id, string Department, string Semester, string section)
        {
            List<PaymentReport> pr = new List<PaymentReport>();
            if (!string.IsNullOrEmpty(std_id) && !string.IsNullOrEmpty(Department) && !string.IsNullOrEmpty(Semester)  && !string.IsNullOrEmpty(section))
            {
               
                var studentpay = from p in acdb.StudentsPayment.ToList()
                                
                                 join si in sdbs.Student_info.ToList() on p.StudentId equals si.Std_id
                                 join ca in sdb.CurrentAcademicInfo.ToList() on p.StudentId equals ca.Std_id
                                 where (p.StudentId.Equals(std_id) && ca.Department.Equals(Department) && ca.Semester.Equals(Semester) && ca.Section.Equals(section) )
                                 group p by new
                                 {
                                     p.StudentId,
                                     si.Std_FName,
                                     si.Std_MName,
                                     si.Std_LName,
                                     p.PaymentDate,

                                 } into t
                                 select new PaymentReport
                                 {
                                     PaymentDate = t.Key.PaymentDate,
                                     StudentId = t.Key.StudentId,
                                     studentname = t.Key.Std_FName + " " + t.Key.Std_MName + " " + t.Key.Std_LName,
                                     totalfees = t.Sum(acs => acs.totalfees),
                                     Payamount = t.Sum(acs => acs.Payamount),
                                     AdmissionFee = t.Sum(acs => acs.AdmissionFee),
                                     ExamFee = t.Sum(acs => acs.ExamFee),
                                     TutionFee = t.Sum(acs => acs.TutionFee),
                                     OtherFee = t.Sum(acs => acs.OtherFee),
                                     Khata = t.Sum(acs => acs.Khata),
                                     Diary = t.Sum(acs => acs.Diary),
                                     Dueamount = t.Sum(acs => acs.Dueamount)
                                 };
                pr = studentpay.ToList();
                //return studentpay.ToList();
            }

            else if (!string.IsNullOrEmpty(std_id))
            {
               
                var studentpay = from p in acdb.StudentsPayment.ToList()
                                 join si in sdbs.Student_info.ToList() on p.StudentId equals si.Std_id
                                 join ca in sdb.CurrentAcademicInfo.ToList() on p.StudentId equals ca.Std_id
                                 where (si.Std_id.Equals(std_id))
                                 group p by new
                                 {
                                     p.StudentId,
                                     si.Std_FName,
                                     si.Std_MName,
                                     si.Std_LName,
                                     p.PaymentDate,

                                 } into t
                                 select new PaymentReport
                                 {
                                     PaymentDate = t.Key.PaymentDate,
                                     StudentId = t.Key.StudentId,
                                     studentname = t.Key.Std_FName + " " + t.Key.Std_MName + " " + t.Key.Std_LName,
                                     totalfees = t.Sum(acs => acs.totalfees),
                                     Payamount = t.Sum(acs => acs.Payamount),
                                     AdmissionFee = t.Sum(acs => acs.AdmissionFee),
                                     ExamFee = t.Sum(acs => acs.ExamFee),
                                     TutionFee = t.Sum(acs => acs.TutionFee),
                                     OtherFee = t.Sum(acs => acs.OtherFee),
                                     Khata = t.Sum(acs => acs.Khata),
                                     Diary = t.Sum(acs => acs.Diary),
                                     Dueamount = t.Sum(acs => acs.Dueamount)
                                 };
                pr = studentpay.ToList();
                //return studentpay.ToList();
            }
            
            else if (!string.IsNullOrEmpty(Department) && !string.IsNullOrEmpty(Semester) && !string.IsNullOrEmpty(section))
            {
               
                var studentpay = from p in acdb.StudentsPayment.ToList()
                                 join si in sdbs.Student_info.ToList() on p.StudentId equals si.Std_id
                                 join ca in sdb.CurrentAcademicInfo.ToList() on p.StudentId equals ca.Std_id
                                 where (sdb.Semester.Equals(Semester) && sdb.Department.Equals(Department) && sdb.Department.Equals(section))
                                 group p by new
                                 {
                                     p.StudentId,
                                     si.Std_FName,
                                     si.Std_MName,
                                     si.Std_LName,
                                     p.PaymentDate,

                                 } into t
                                 select new PaymentReport
                                 {
                                     PaymentDate = t.Key.PaymentDate,
                                     StudentId = t.Key.StudentId,
                                     studentname = t.Key.Std_FName + " " + t.Key.Std_MName + " " + t.Key.Std_LName,
                                     totalfees = t.Sum(acs => acs.totalfees),
                                     Payamount = t.Sum(acs => acs.Payamount),
                                     AdmissionFee = t.Sum(acs => acs.AdmissionFee),
                                     ExamFee = t.Sum(acs => acs.ExamFee),
                                     TutionFee = t.Sum(acs => acs.TutionFee),
                                     OtherFee = t.Sum(acs => acs.OtherFee),
                                     Khata = t.Sum(acs => acs.Khata),
                                     Diary = t.Sum(acs => acs.Diary),
                                     Dueamount = t.Sum(acs => acs.Dueamount)
                                 };
                pr = studentpay.ToList();
                //return studentpay.ToList();

            }


            else if (!string.IsNullOrEmpty(Semester) && !string.IsNullOrEmpty(section))
            {
             
                var studentpay = from p in acdb.StudentsPayment.ToList()
                                 join si in sdbs.Student_info.ToList() on p.StudentId equals si.Std_id
                                 join ca in sdb.CurrentAcademicInfo.ToList() on p.StudentId equals ca.Std_id
                                 where (ca.Semester.Equals(Semester) && ca.Section.Equals(section))
                                 group p by new 
                                 {
                                     p.StudentId, 
                                     si.Std_FName, 
                                     si.Std_MName,
                                     si.Std_LName,
                                     p.PaymentDate,
                                  
                                 } into t
                                 select new PaymentReport
                                 {
                                     PaymentDate = t.Key.PaymentDate,
                                     StudentId = t.Key.StudentId,
                                     studentname = t.Key.Std_FName + " " + t.Key.Std_MName + " " + t.Key.Std_LName,
                                     totalfees = t.Sum(acs => acs.totalfees),
                                     Payamount = t.Sum(acs => acs.Payamount),
                                     AdmissionFee = t.Sum(acs => acs.AdmissionFee),
                                     ExamFee = t.Sum(acs => acs.ExamFee),
                                     TutionFee = t.Sum(acs => acs.TutionFee),
                                     OtherFee = t.Sum(acs => acs.OtherFee),
                                     Khata = t.Sum(acs => acs.Khata),
                                     Diary = t.Sum(acs => acs.Diary),
                                     Dueamount = t.Sum(acs => acs.Dueamount)
                                 };
                pr = studentpay.ToList();
                //return studentpay.ToList();

            }
            return pr;
         
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

        public ActionResult Report()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Report(int Sample,string fdate,string tdate)
        { 
            if(Sample==1)
            {
                TempData["list"] = "List";
            }
            else if(Sample==2 && fdate!="" && tdate!="")
            {
                TempData["list"] = "List";
            }
            return View();
        }

        public ActionResult ReportList(int Sample, string fdate, string tdate)
        {
            var _report = new List<reportlist>();
            var _income = new List<incomelist>();
           
            if (Sample ==1)
            {
                fdate = DateTime.Now.Date.ToString();
                tdate = DateTime.Now.Date.ToString();
                _report = expenselis(tdate, fdate);

                _income = incomestatementlist(tdate, fdate);
                ViewBag.tdate = tdate;
                ViewBag.fdate = fdate;
            }
            else if (Sample ==2 && fdate != "" && tdate != "")
            {
                _report = expenselis(tdate, fdate);

                _income = incomestatementlist(tdate, fdate);
                ViewBag.tdate = tdate;
                ViewBag.fdate = fdate;
            }

           
            var tupule = new Tuple<List<reportlist>, List<incomelist>>(_report,_income);


            return View(tupule);
         
        }

        private List<reportlist> expenselis(string tdate, string fdate)
        {
            var expandaturelist = from ex in ec.Expandature.ToList()
                                  join e in ec.Expancetype.ToList() on ex.Expancetype equals e.Id
                                  where ex.date <= Convert.ToDateTime(tdate).Date && ex.date >= Convert.ToDateTime(fdate).Date
                                  group ex by new { e.TypeName } into s
                                  select new reportlist
                                  {
                                      Name = s.Key.TypeName,
                                      Amount = s.Sum(x => x.Amount)
                                  };

            var purchaselist = from p in ic.Purchase.ToList()
                               join i in ic.Inventory.ToList() on p.ProductId equals i.Id
                               where p.Date <= Convert.ToDateTime(tdate).Date && p.Date >= Convert.ToDateTime(fdate).Date
                               group p by new { i.Name } into s
                               select new reportlist
                               {
                                   Name = s.Key.Name,
                                   Amount = s.Sum(x => x.Prize * x.Quantity)
                               };

            var salarylist = from s in sc.StuffSalary.ToList()
                             where s.payDate <= Convert.ToDateTime(tdate).Date && s.payDate >= Convert.ToDateTime(fdate).Date
                             group s by new { s.payDate } into g
                             select new reportlist
                             {
                                 Name = "Stuff Salary",
                                 Amount = g.Sum(s => s.bankPay??0 + s.cashPay??0)

                             };
            var bookprize=from b in lc.Book.ToList()
                           where b.RegisterDate <= Convert.ToDateTime(tdate).Date && b.RegisterDate >= Convert.ToDateTime(fdate).Date
                           group b by new{b.RegisterDate}into s
                          select new reportlist
                          {
                              Name = "Book Cost",
                              Amount = s.Sum(b=>b.Price*b.BookCopy)

                          };
            

            var expandaturepurchaselist = expandaturelist.ToList().Union(purchaselist.ToList());
            var salary = expandaturepurchaselist.ToList().Union(salarylist.ToList());
            var bookp = salary.ToList().Union(bookprize.ToList());
            return bookp.ToList();
        }

        private List<incomelist> incomestatementlist(string tdate, string fdate)
        {
            //var StudentPaymentlist = from sp in sdb.StudentPayment.ToList()
            //                         join pt in pc.PaymentType.ToList() on sp.TypeId equals pt.Id
            //                         where sp.Date <= Convert.ToDateTime(tdate).Date && sp.Date >= Convert.ToDateTime(fdate).Date
            //                         group sp by new { pt.TypeName } into s
            //                         select new incomelist
            //                         {
            //                             Name = s.Key.TypeName,
            //                             Amount = s.Sum(x => x.PayAnount)
            //                         };
            //var devfee = from sp in sdb.StudentPayment.ToList()
            //             where sp.Date <= Convert.ToDateTime(tdate).Date && sp.Date >= Convert.ToDateTime(fdate).Date
            //             group sp by new { sp.DevelopmentFee } into s
            //             select new incomelist
            //             {
            //                 Name = "Development Fee",
            //                 Amount = s.Sum(x => x.DevelopmentFee)
            //             };
            //var adfee = from sp in sdb.StudentPayment.ToList()
            //            where sp.Date <= Convert.ToDateTime(tdate).Date && sp.Date >= Convert.ToDateTime(fdate).Date
            //            group sp by new { sp.AdmissionFee } into s
            //            select new incomelist
            //            {
            //                Name = "Admission Fee",
            //                Amount = s.Sum(x => x.AdmissionFee)
            //            };
            //var labfee = from sp in sdb.StudentPayment.ToList()
            //             where sp.Date <= Convert.ToDateTime(tdate).Date && sp.Date >= Convert.ToDateTime(fdate).Date
            //             group sp by new { sp.LabFee } into s
            //             select new incomelist
            //             {
            //                 Name = "Lab Fee",
            //                 Amount = s.Sum(x => x.LabFee)
            //             };
            //var lifee = from sp in sdb.StudentPayment.ToList()
            //            where sp.Date <= Convert.ToDateTime(tdate).Date && sp.Date >= Convert.ToDateTime(fdate).Date
            //            group sp by new { sp.LibraryFee } into s
            //            select new incomelist
            //            {
            //                Name = "Library Fee",
            //                Amount = s.Sum(x => x.LibraryFee)
            //            };
            //var SMSAlartfee = from sp in sdb.StudentPayment.ToList()
            //                  where sp.Date <= Convert.ToDateTime(tdate).Date && sp.Date >= Convert.ToDateTime(fdate).Date
            //                  group sp by new { sp.SMSAlartFee } into s
            //                  select new incomelist
            //                  {
            //                      Name = "SMSAlart Fee",
            //                      Amount = s.Sum(x => x.SMSAlartFee)
            //                  };
            //var Transportfee = from sp in sdb.StudentPayment.ToList()
            //                   where sp.Date <= Convert.ToDateTime(tdate).Date && sp.Date >= Convert.ToDateTime(fdate).Date
            //                   group sp by new { sp.TransportFee } into s
            //                   select new incomelist
            //                   {
            //                       Name = "Transport Fee",
            //                       Amount = s.Sum(x => x.TransportFee)
            //                   };
            //var ts = SMSAlartfee.ToList().Union(Transportfee.ToList());
            //var lst = lifee.ToList().Union(ts.ToList());
            //var tslst = labfee.ToList().Union(lst.ToList());
            //var la = adfee.ToList().Union(tslst.ToList());
            //var last = devfee.ToList().Union(la.ToList());
            //var incomelist = StudentPaymentlist.ToList().Union(last.ToList());
            var incomelist = from PDetails in acdb.StudentPaymentDetails.ToList()
                             join PList in acdb.PaymentList.ToList() on PDetails.Id equals PList.PayId
                             where PDetails.PayDate <= Convert.ToDateTime(tdate).Date && PDetails.PayDate >= Convert.ToDateTime(fdate).Date
                             group PList by new { PList.PayType } into s
                             select new incomelist
                             {
                                 Name = s.Key.PayType,
                                 Amount = s.Sum(a => a.Amount)
                             };
            return incomelist.ToList();
        }

        public ActionResult DailyReportPrint(string tdate, string fdate)
        {
            string userid = User.Identity.Name;
            int schoolid = uc.UserProfiles.Where(x => x.UserName.Equals(userid)).Select(x => x.SchoolId??0).SingleOrDefault();
            string schoolname = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.SchoolName).SingleOrDefault();
            string address = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.Address + " " + x.City + " " + x.Country).SingleOrDefault();
            var report = new List<reportlist>();
            var income = new List<incomelist>();
            report = expenselis(tdate, fdate);
            var reportlist = from r in report.ToList()
                             select new { ExpName = r.Name, ExpAmount = r.Amount };
            income = incomestatementlist(tdate, fdate);

            var incomelist = from i in income.ToList()
                             select new { Name = i.Name, Amount = i.Amount };
         

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Report"), "IncomeCrystalReport.rpt"));

            rd.Database.Tables[0].SetDataSource(reportlist);
            rd.Database.Tables[1].SetDataSource(incomelist);
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
    }
}
