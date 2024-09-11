using CrystalDecisions.CrystalReports.Engine;
using MvcEnergyPac.Models;
using MvcUMS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcUMS.Controllers
{
    public class AccountingController : Controller
    {
        AccountingContext ac = new AccountingContext();
        PaymentContext pc = new PaymentContext();
        ExpanceContext ec = new ExpanceContext();
        InventoryContext ic = new InventoryContext();
        StuffContext sc = new StuffContext();
        EventContext evc = new EventContext();
        StudentContext sdb = new StudentContext();
        UMSEntities1 sdbs = new UMSEntities1();
        Library lc = new Library();
        UsersContext uc = new UsersContext();
      
      

        //
        // GET: /Accounting/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult OpenningBalance()
        {
            ViewBag.advancedsearch = "advancedsearch";

            var bl = from b in pc.Bank.ToList()
                     select new
                     {
                         Id=b.Id,
                         BankName = b.BankName + " (" + b.AccountNumber + ") "
                     };
            ViewBag.BalanceTypeList = new SelectList(ac.BalanceType, "Id", "Name");
            ViewBag.Banklist = new SelectList(bl.ToList(), "Id", "BankName");
            return View();
        }
        [HttpPost]
        public ActionResult OpenningBalance(OpenningBalance model)
       {
           var s = ac.OpenningBalance.Where(x => x.BalanceType.Equals(model.BalanceType)).Count();
           if (s > 0)
           {
               TempData["error"] = "Not Possible. Alrady Created";
           }
           else
           {
               if (ModelState.IsValid)
               {
                   if (model.Date == null)
                   {
                       model.Date = DateTime.Now.Date;
                   }
                   else
                   {
                       model.Date = model.Date;
                   }
                   ac.OpenningBalance.Add(model);
                   ac.SaveChanges();
                   if(model.BalanceType==1 && model.BankId!=0)
                   {
                     
                       var bank = pc.Bank.Where(x => x.Id.Equals(model.BankId)).SingleOrDefault();
                       bank.Amount += model.Amount;
                       pc.Entry(bank).State = EntityState.Modified;
                       pc.SaveChanges();

                   }
               }
           }
           var bl = from b in pc.Bank.ToList()
                    select new
                    {
                        Id = b.Id,
                        BankName = b.BankName + " (" + b.AccountNumber + ") "
                    };
           ViewBag.BalanceTypeList = new SelectList(ac.BalanceType, "Id", "Name");
           ViewBag.Banklist = new SelectList(bl.ToList(), "Id", "BankName");
           return View();
       }
        [HttpGet]
        public ActionResult IncomeExapance()
        {
           
          ViewBag.expancelist= expancedetails();
          ViewBag.incomelist = Incomedetails();
      
          return View();
        }
        [HttpPost]
        public ActionResult IncomeExapance(DateTime fdate, DateTime tdate)
       {

           ViewBag.expancelist = expancedetails().Where(x => x.date >= fdate.Date && x.date <= tdate.Date).OrderByDescending(x => x.date).ToList();
           ViewBag.incomelist = Incomedetails().Where(x => x.date >= fdate.Date && x.date <= tdate.Date).OrderByDescending(x => x.date).ToList();
           ViewBag.fdate = fdate;
           ViewBag.tdate = tdate;

           return View();
       }
        [HttpGet]
        public ActionResult IncomeExpenseLedger()
       {
           List<incomemodel> ledgermodel = new List<incomemodel>();

           return View(ledgermodel);
       }
        [HttpPost]
        public ActionResult IncomeExpenseLedger(DateTime fdate, DateTime tdate)
        {
            ViewBag.fdate = fdate;
            ViewBag.tdate = tdate;
            Decimal incomeamount = 0;
            Decimal expanceamount = 0;
            decimal openningbalance = 0;
            List<incomemodel> ledgermodel = new List<incomemodel>();
            incomeamount = Incomedetails().Where(x => x.date <= fdate.Date).Sum(x => x.totalamount);
            expanceamount = expancedetails().Where(x=>x.date <= fdate.Date).Sum(x=>x.totalamount);
            openningbalance = incomeamount - expanceamount;
            ViewBag.openbalance = openningbalance;
            var ladgerlist = (from ex in expancedetails().ToList()
                              select new incomemodel { name = "Debit", totalamount = ex.bankpay + ex.cashpay, date = ex.date, particulars = ex.particulars })
                                .Union
                                (from ic in Incomedetails().ToList()
                                 select new incomemodel { name = "Credit", totalamount = ic.bankpay + ic.cashpay, date = ic.date, particulars = ic.particulars }
                                 );
            var laddetails = ladgerlist.Where(x => x.date >= fdate.Date && x.date <= tdate.Date).OrderByDescending(x => x.date).ToList();
            int i = 0;
            decimal balanc;
            foreach (var item in laddetails)
            {
                balanc = openningbalance;
                ledgermodel.Add(new incomemodel()
                {
                    index = i,
                    particulars = item.particulars,
                    name = item.name,
                    totalamount = item.totalamount,
                    balance = getbalance(item.name, balanc, item.totalamount),
                    bankpay = item.bankpay,
                    cashpay = item.cashpay,
                    date = item.date
                });

                openningbalance = ledgermodel.Where(x => x.index == i).Select(x => x.balance).FirstOrDefault();
                i++;
            }
            return View(ledgermodel);
        }
        public ActionResult LadgerPrint(DateTime fdate, DateTime tdate)
        {
            List<incomemodel> ledgermodel = new List<incomemodel>();
            Decimal incomeamount = 0;
            Decimal expanceamount = 0;
            decimal openningbalance = 0;
            decimal openbalance = 0;
            incomeamount = Incomedetails().Where(x => x.date <= fdate.Date).Sum(x => x.totalamount);
            expanceamount = expancedetails().Where(x => x.date <= fdate.Date).Sum(x => x.totalamount);
            openningbalance = incomeamount - expanceamount;
            openbalance = openningbalance;
            var ladgerlist = (from ex in expancedetails().ToList()
                              select new incomemodel { name = "Debit", debitamount = ex.bankpay + ex.cashpay, creditamount = 0, totalamount = ex.bankpay + ex.cashpay, date = ex.date, particulars = ex.particulars })
                                .Union
                                (from ic in Incomedetails().ToList()
                                 select new incomemodel { name = "Credit", creditamount = ic.bankpay + ic.cashpay, debitamount = 0, totalamount = ic.bankpay + ic.cashpay, date = ic.date, particulars = ic.particulars }
                                 );
            var laddetails = ladgerlist.Where(x => x.date >= fdate.Date && x.date <= tdate.Date).OrderBy(x => x.date).ToList();
            int i = 0;
            decimal balanc = 0;
            decimal closingbalnce = 0;
            foreach (var item in laddetails)
            {
                closingbalnce = openningbalance;
                balanc = openningbalance;
                ledgermodel.Add(new incomemodel()
                {
                    index = i,
                    particulars = item.particulars,
                    name = item.name,
                    totalamount = item.totalamount,
                    creditamount = item.creditamount,
                    debitamount = item.debitamount,
                    balance = getbalance(item.name, balanc, item.totalamount),
                    bankpay = item.bankpay,
                    cashpay = item.cashpay,
                    sdate = item.date.ToString("dd-MM-yyyy")
                });
                openningbalance = ledgermodel.Where(x => x.index == i).Select(x => x.balance).FirstOrDefault();
                closingbalnce = ledgermodel.Where(x => x.index == i).Select(x => x.balance).FirstOrDefault();
                i++;

            }

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Report"), "rpt_ladgerReport.rpt"));
            rd.SetDataSource(ledgermodel);
            rd.SetParameterValue("openningbalance", openbalance.ToString());
            rd.SetParameterValue("fdate", fdate.ToString("dd-MM-yyyy"));
            rd.SetParameterValue("tdate", tdate.ToString("dd-MM-yyyy"));
            rd.SetParameterValue("closingbalnce", closingbalnce.ToString());

            rd.SummaryInfo.ReportTitle = "Ledger Report";
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
        public ActionResult IncomeExpenseCashBookPrint(DateTime? fdate, DateTime? tdate)
        {
            List<expancelist> exlist = new List<expancelist>();
            List<Incomelist> inclist = new List<Incomelist>();
            if (fdate == null && tdate == null)
            {
                exlist = expancedetails().OrderByDescending(x => x.date).ToList();
                inclist = Incomedetails().OrderByDescending(x => x.date).ToList(); 
            }
            else
            {
                exlist = expancedetails().Where(x => x.date >= fdate.Value.Date && x.date <= tdate.Value.Date).OrderByDescending(x => x.date).ToList();
                inclist = Incomedetails().Where(x => x.date >= fdate.Value.Date && x.date <= tdate.Value.Date).OrderByDescending(x => x.date).ToList(); 
            }

           ReportDocument report = new ReportDocument();
           report.Load(Path.Combine(Server.MapPath("~/Report"), "IncomeExpenseCashBookReport.rpt"));

           report.Database.Tables[0].SetDataSource(inclist);
           report.Database.Tables[1].SetDataSource(exlist);
       
           Response.Buffer = false;
           Response.ClearContent();
           Response.ClearHeaders();
           try
           {
               Stream stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
               stream.Seek(0, SeekOrigin.Begin);
               return File(stream, "application/pdf");
           }
           catch (Exception ex)
           {
               throw;
           }
       }
        private  List<expancelist> expancedetails()
        {
            List<expancelist> exlist = new List<expancelist>();
            var expanselist = (from ex in ec.Expandature.ToList()
                               join et in ec.Expancetype.ToList() on ex.Expancetype equals et.Id
                               select new expancelist
                               {
                                   bankpay = ex.bankpay ??0,
                                   cashpay = ex.cashpay??0,
                                   date = ex.date,
                                   particulars = et.TypeName,
                                   totalamount = ex.bankpay + ex.cashpay??0

                               }).Union

                            (from ex in ic.Purchase.ToList()
                             select new expancelist
                             {
                                
                                 bankpay = 0,
                                 cashpay = ex.Prize,
                                 date = ex.Date,
                                 particulars = "Purchase Book",
                                 totalamount = 0 + ex.Prize

                             }).Union

                            (from ex in sc.StuffSalary.ToList()
                             select new expancelist
                             {
                                 
                                 bankpay = ex.bankPay??0,
                                 cashpay = ex.cashPay??0,
                                 date = ex.payDate,
                                 particulars = "Staff Salary",
                                 totalamount = ex.bankPay??0 + ex.cashPay??0

                             });

          return  exlist = expanselist.OrderBy(x => x.date).ToList();

        }
        private List<Incomelist> Incomedetails()
        {
            List<Incomelist> inclist = new List<Incomelist>();
            var incomelist = (from ex in evc.Donation.ToList()
                              select new Incomelist
                              {
                                  bankpay = ex.BankPay,
                                  cashpay = ex.CashPay,
                                  date = ex.Date,
                                  particulars = "Donation from " + ex.ProviderName,
                                  totalamount = ex.BankPay + ex.CashPay
                              }).Union
                              (from ex in ac.StudentsPayment.ToList()
                               select new Incomelist
                               {
                                   bankpay = 0,
                                   cashpay = ex.Payamount,
                                   date = ex.PaymentDate,
                                   particulars = "Student Payment ",
                                   totalamount = 0 + ex.Payamount

                               });


            return inclist = incomelist.OrderBy(x => x.date).ToList();

        }
        private decimal getbalance(string sname, decimal balnce, decimal amount)
        {
            decimal currentbalance = 0;
            if (sname == "Credit")
            {
                currentbalance = balnce + amount;

            }
            else if (sname == "Debit")
            {
                currentbalance = balnce - amount;

            }
            return currentbalance;
        }



        public ActionResult StudentPaymentCallection(string id = null,string s=null)
        {
            studentpaydetaisl model = new studentpaydetaisl();
            var studentpay = from si in sdbs.Student_info.ToList()
                             join ci in sdb.CurrentAcademicInfo.ToList() on si.Std_id equals ci.Std_id
                             where si.Std_id == id
                             select new studentpaydetaisl
                             {
                                  StudentId=si.Std_id,
                                  studentname = si.Std_FName + " " + si.Std_MName + " " + si.Std_LName,
                                  classname=ci.Semester,
                                  rollno=ci.RollNo,
                               
                             };
            var amount= ac.StudentDue.Where(x=>x.StudentId==id).Select(x=>x.TotalDueamount).FirstOrDefault();
         
            ViewBag.previosdue = amount;
            if(s!=null)
            {
                ViewBag.control = s;
            }
            model = studentpay.FirstOrDefault();
            return View(model);
        }

        [HttpPost]
        public ActionResult StudentPaymentCallection(studentpaydetaisl model, string control)
        {
            string classes = sdb.CurrentAcademicInfo.Where(x => x.Std_id == model.StudentId && x.Status == "Regular").Select(x => x.Semester).FirstOrDefault();
            int clasid = sdb.Semester.Where(x => x.Semester_name == classes).Select(x => x.Id).FirstOrDefault();
            int rollno = sdb.CurrentAcademicInfo.Where(x => x.Std_id == model.StudentId && x.Status == "Regular").Select(x => x.RollNo).FirstOrDefault();
            StudentsPayment stp=new StudentsPayment();
            StudentDue sd = new StudentDue();
            stp.Id = Guid.NewGuid();
            stp.StudentId = model.StudentId;
            stp.roll = rollno;
            stp.classid = clasid;
            stp.AdmissionFee = model.Admision;
            stp.Dueamount = model.dueamount;
            stp.ExamFee = model.Exam;
            stp.Khata = model.Khata;
            stp.Diary = model.Diary;
            stp.Less = model.Less;
            stp.OtherFee = model.Others;
            stp.Payamount = model.textCash;
            stp.PaymentDate = DateTime.Now.Date;
            stp.totalfees = model.totalfees;
            stp.TutionFee = model.Tution;
            ac.StudentsPayment.Add(stp);
            ac.SaveChanges();
            var count = ac.StudentDue.Where(x => x.StudentId == stp.StudentId).Count();
            if(count>0)
            {
                var due = ac.StudentDue.Where(x => x.StudentId == stp.StudentId).FirstOrDefault();
                due.TotalDueamount = stp.Dueamount;
                ac.Entry(due).State = EntityState.Modified;
                ac.SaveChanges();
            }
            else
            {
                
                sd.StudentId = stp.StudentId;
                sd.TotalDueamount = stp.Dueamount;
                ac.StudentDue.Add(sd);
                ac.SaveChanges();
            }
            Session["transectionid"] = stp.Id;
            if (control=="sp")
            {
                return RedirectToAction("StudentPayment","Home");
            }
            else
            {
                return RedirectToAction("Student_list", "Home");
            }
            
        }

        public ActionResult PaymentPrint(Guid id)
        {
            
            string userid = User.Identity.Name;
            int schoolid = uc.UserProfiles.Where(x => x.UserName.Equals(userid)).Select(x => x.SchoolId??0).SingleOrDefault();
            string schoolname = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.SchoolName).SingleOrDefault();
            string address = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.Address + " " + x.City + " " + x.Country).SingleOrDefault();
        
           //PaymentReport
                var studentpay = from p in ac.StudentsPayment.ToList()
                                 join si in sdbs.Student_info.ToList() on p.StudentId equals si.Std_id
                                 join ci in sdb.CurrentAcademicInfo.ToList() on p.StudentId equals ci.Std_id
                                 join cl in sdb.Semester.ToList() on p.classid equals cl.Id
                                 where p.Id==id
                                 select new PaymentReport
                                 {
                                     //.ToString("dd/MM/yyyy")
                                     PaymentDate = p.PaymentDate,
                                     StudentId = p.StudentId,
                                     studentname = si.Std_FName + " " + si.Std_MName + " " + si.Std_LName,
                                     classname=cl.Semester_name,
                                     roll = p.roll,
                                     AdmissionFee = p.AdmissionFee,
                                     TutionFee = p.TutionFee,
                                     OtherFee = p.OtherFee,
                                     ExamFee = p.ExamFee,
                                     Less = p.Less,
                                     Khata = p.Khata,
                                     Diary = p.Diary,
                                     total=p.totalfees+p.Less,
                                     totalfees = p.totalfees,
                                     Payamount = p.Payamount,
                                     Dueamount = p.Dueamount,
                                 };
                List < PaymentReport> l= new List<PaymentReport>();
                l = studentpay.ToList();
               
                
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Report"), "StudentsPaymentCrystalReport.rpt"));
            rd.SetDataSource(studentpay);
            rd.SetParameterValue("schoolname", schoolname.ToString());
            rd.SetParameterValue("address", address.ToString());
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                Session["transectionid"] = null;
                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                throw;
            }
          
        }
    }
}
