using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcUMS.Models;
using System.Collections;
using System.Data.SqlClient;
using VIT.DataLogicLayer;
using System.Data.Entity.Infrastructure;
using MvcUMS.Models.EntityManager;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using MvcUMS.Models.ViewModels;

namespace MvcUMS.Controllers
{
    public class PaymentController : Controller
    {
        StudentContext sdb = new StudentContext();
        PaymentManager PM = new PaymentManager();
        CommonManager CM = new CommonManager();

        SchoolNewDBEntities _payCtx = new SchoolNewDBEntities();

        public ActionResult StudentPayment()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            return View();
        }

        

        [HttpPost]
        public ActionResult GetPaymentType(string sem)
        {
            List<StudentPaymentTypeViewModel> pTypeList = new List<StudentPaymentTypeViewModel>();
            pTypeList = PM.getAllPaymentType(sem).ToList();
            //pTypeList = sdb.Section.Where(m => m.deparment_Name == dep && m.semester_Name == sem).ToList();
            SelectList obgcity = new SelectList(pTypeList, "Id", "PaymentType",0);
            return Json(obgcity,JsonRequestBehavior.AllowGet);
        }

        public ActionResult MonthlyPaymentSetup()
        {
            ViewBag.DepartmentList = new SelectList(sdb.Department, "Id", "Dep_name");
            ViewBag.SemesterList = new SelectList(sdb.Semester, "Id", "Semester_name");
            ViewBag.MonthList = new SelectList(_payCtx.Months, "Id", "MonthName");
            ViewBag.PaymentTypeList = new SelectList(PM.GetPaymentTypeList().ToList(), "Id", "PaymentType");
            return View();
        }

        [HttpPost]
        public ActionResult MonthlyPaymentSetup(MonthlyStudentPaymentSetupViewModel model,
            List<StudentPaymentTypeViewModel> pType,List<StudentListViewModel> stdList,int[] noMonth)
        {
            PM.AddMonthlyPaymentSetup(model, pType,stdList, noMonth);
            return RedirectToAction("SearchInvoiceList");
        }

        public ActionResult MonthlyPaymentList()
        {
            List<MonthlyStudentPaymentListViewModel> pTypeList = new List<MonthlyStudentPaymentListViewModel>();
            pTypeList = PM.MonthlyPaymentList();
            return View(pTypeList);
        }

        


        public ActionResult StudentPaymentType(int? id,string sem = null)
        {
            List<StudentPaymentTypeViewModel> pTypeList = new List<StudentPaymentTypeViewModel>();
            ViewBag.MonthList = new SelectList(PM.GetMonths(id??0).ToList(), "Id", "MonthName");
            pTypeList = PM.getAllPaymentType(sem).ToList();

            return View(pTypeList);
        }

        public ActionResult StudentListForPayment(string sem = null)
        {
            var list = new List<StudentListViewModel>();
            list = PM.GetStudentList(sem).ToList();
            return View(list);
        }

        [HttpPost]
        public ActionResult StudentPaymentCollection(PaymentTypeName model, int[] noMonth, List<PaymentTypeViewModel> pType)
        {

            var result = PM.AddPayment(model, pType, noMonth);

            return RedirectToAction("InvoiceView", new { id = result });

        }

        public ActionResult SearchInvoiceList()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Id", "Semester_name");
            ViewBag.MonthList = new SelectList(_payCtx.Months, "Id", "MonthName");
            return View();
        }

        [HttpPost]
        public ActionResult InvoiceList(string Std_id, int? SemesterId, int? MonthId)
        {
            List<StudentInvoiceListViewModel> invList = new List<StudentInvoiceListViewModel>();
            invList = PM.GetInvoiceList(Std_id,SemesterId, MonthId);
            TempData["InvoiceList"] = invList.ToList();
            return View(invList);
        }

        [HttpPost]
        public ActionResult SendPaymentSMS()
        {
            bool status = false;
            var list = (List<StudentInvoiceListViewModel>)TempData["InvoiceList"];
            //RootObject results = SmsSend();
            foreach (var item in list)
            {
                string mobilenumber = "+88" + Convert.ToString(item.GurdianMobile);
                string txtMsg = "On " + item.Month + " Total Payable is " + item.TotalAmount + " and Last date of Payment is " + item.DueDate;
                string sender = "InfoSMS";
                RootObject results = CM.SmsSend(sender, mobilenumber, txtMsg);

                status = true;

            }

            if (status == true)
                TempData["success"] = "Message Send successfully.";

            TempData["success"] = "Message Failed";

            return new JsonResult { Data = new { status = status } };
        }

         [HttpPost]
        public ActionResult DueInvoiceList(string Std_id, int? SemesterId, int? MonthId,int? MonthIdTo)
        {
            List<StudentInvoiceListViewModel> invList = new List<StudentInvoiceListViewModel>();
            invList = PM.GetDueInvoiceList(Std_id, SemesterId, MonthId,MonthIdTo);
            return View(invList);
        }

        


        public ActionResult SearchDueList()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Id", "Semester_name");
            ViewBag.MonthList = new SelectList(_payCtx.Months, "Id", "MonthName");
            return View();
        }

        [HttpPost]
        public ActionResult DueList(string Std_id, int? SemesterId, int? MonthId, int? MonthIdTo)
        {
            List<StudentDueListViewModel> invList = new List<StudentDueListViewModel>();
            invList = PM.GetDueList(Std_id, SemesterId, MonthId,MonthIdTo);
            return View(invList);
        }

        public ActionResult InvoiceView(int id)
        {
            ViewBag.User = User.Identity.Name;
            var data = PM.invoiceInsertedData(id);
            int num = Convert.ToInt32(data.PaidAmount);
            var numToWord = PaymentManager.NumberToWords(num);
            ViewBag.words = numToWord;
            return View(data);
        }


        [HttpGet]
        public JsonResult IsAmountGreater(int id,decimal amount)
        {
            bool isGreater = _payCtx.StudentInvoiceLists.Any(a => a.Id.Equals(id) && a.DueAmount < amount);
            return Json(!isGreater, JsonRequestBehavior.AllowGet);  
        }

        public ActionResult PaymentCollection(int id)
        {
            var data = PM.invoiceInsertedData(id);
            ViewBag.PaymentMethodList = new SelectList(PM.GetPaymentMethodList(), "Id", "MethodName");
            ViewBag.BankList = new SelectList(PM.GetBankList(), "Id", "BankName");
            return View(data);
        }

        [HttpPost]
        public ActionResult PaymentCollection(PaymentCollectionViewModel model)
        {
            PM.CreatePaymentCollection(model);
            return RedirectToAction("InvoiceView", new { id = model.Id });
            //return RedirectToAction("SearchInvoiceList");
        }


        public ActionResult EditPaymentType(int id)
        {
            var data = PM.invoiceInsertedData(id);
            ViewBag.MonthList = new SelectList(_payCtx.Months, "Id", "MonthName");
            ViewBag.PaymentTypeList = new SelectList(PM.GetPaymentTypeList().ToList(), "Id", "PaymentType");
            return View(data);
        }

        [HttpPost]
        public ActionResult EditPaymentType(InvoiceDetailsViewModel model,List<StudentPaymentTypeViewModel> pType, int[] noMonth)
        {
            PM.UpdateMonthlyPaymentSetup(model,pType, noMonth);
            return RedirectToAction("InvoiceView", new { id = model.Id });
        }
       
        

       

        public ActionResult StdPaymentDetailsShow(string id)
        {
            TempData["std_id"] = id;
            TempData["stdid"] = id;
            return View();
        }

        public ActionResult StdViewPayment(string stdId)
        {
            ViewBag.stdId = stdId;
            TempData["stdid"] = stdId;
            var pay = PM.PaymentList(stdId);
            return View(pay);
        }

        public ActionResult PaymentReportsShow(Guid id)
        {
            ViewBag.User = User.Identity.Name;
            TempData["std_id"] = id;
            TempData["stdid"] = id;
            return View();
        }

        public ActionResult PaymentReports(Guid id)
        {
            DataTable data = PM.GetPaymentReport(id);

            ViewBag.userdetails = data;
            return View(data);
        }

        public ActionResult StudentPaymentReport()
        {
            ViewBag.Department = new SelectList(sdb.Department, "Dep_name", "Dep_name");
            ViewBag.Semester = new SelectList(sdb.Semester, "Semester_name", "Semester_name");
            return View();
        }

        public ActionResult StudentPaymentReportList(string Department, string Semester, string Section)
        {
            DataTable list = PM.GetStdPaymentList(Department, Semester, Section);
            ViewBag.dptId = Department !=null ? Department : "";
            ViewBag.semester = Semester;
            ViewBag.section = Section;
            return View(list);
        }

        public ActionResult StudentDuePrint(string Department, string Semester, string Section)
        {
            if (Department == null)
                Department = "";

            DataTable dueRecords = PM.GetStdPaymentList(Department, Semester, Section);

            ViewBag.dptId = Department;
            ViewBag.semester = Semester;
            ViewBag.section = Section;

            var stdSchool = PM.getSchoolList().Select(s=>s.SchoolName).FirstOrDefault();
            string stdimage = PM.getSchoolList().Select(x => x.Logo).FirstOrDefault();
            var stdimageurl = Server.MapPath("~/uploads/" + stdimage);

            ReportDocument rd = new ReportDocument();

            rd.Load(Path.Combine(Server.MapPath("~/Report"), "Rpt_DueReports.rpt"));
            rd.SetDataSource(dueRecords);
            rd.SetParameterValue("stdDpt", ViewBag.dptId.ToString());
            rd.SetParameterValue("stdSemester", ViewBag.semester.ToString());
            rd.SetParameterValue("stdSection", ViewBag.section.ToString());
            rd.SetParameterValue("stdSchool", stdSchool.ToString());
            rd.SetParameterValue("stdimageurl", stdimageurl.ToString());
            rd.SummaryInfo.ReportTitle = "Student Due Reports";
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

        public JsonResult TestJson()
        {
            var result = PM.MonthlyPaymentList();
            var status = true;
            //return Json(result, JsonRequestBehavior.AllowGet);
            return new JsonResult { Data = new { Status=status },JsonRequestBehavior=JsonRequestBehavior.AllowGet };
        }
    }
}