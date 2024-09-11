using MvcUMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;

namespace MvcUMS.Controllers
{
    public class ExpanceController : Controller
    {
        ExpanceContext ec = new ExpanceContext();
        private StuffContext db = new StuffContext();
        //
        // GET: /Expance/

        public ActionResult Expandature()
        {
            ViewBag.advancedsearch = "advancedsearch";
             ViewBag.ExpanceType = new SelectList(ec.Expancetype, "Id", "TypeName");

            return View();
        }
        [HttpPost]
        public ActionResult Expandature(Expandature model)
        {
            ViewBag.advancedsearch = "advancedsearch";
            if (ModelState.IsValid)
            {
                if(model.bankpay==null)
                {
                    model.bankpay = 0;
                }
                if(model.cashpay==null)
                {
                    model.cashpay = 0;
                }
                model.Id = Guid.NewGuid();
                model.date = DateTime.Now.Date;
                model.StuffId = Guid.Empty;
                ec.Expandature.Add(model);
                ec.SaveChanges();
            }
            ViewBag.ExpanceType = new SelectList(ec.Expancetype, "Id", "TypeName");

            return View("Expandature");
        }

        
        public ActionResult ExpanceSearch(string date,string Todate)
        {
            ViewBag.advancedsearch = "advancedsearch";
            if(date!="" && Todate!="")
            {
                TempData["date"] = date;
                TempData["Todate"] = Todate;
            }
            else
            {
                TempData["date"] = DateTime.Now;
                TempData["Todate"] = DateTime.Now;
                //TempData["error"] = "Select Date";
            }
         
            return View();
        }

        public ActionResult SalarySearch(string date, string Todate)
        {
            ViewBag.advancedsearch = "advancedsearch";
            if(date!="" && Todate!="")
            {
                TempData["date"] = date;
                TempData["Todate"] = Todate;
            }
            else
            {
                TempData["date"] = DateTime.Now;
                TempData["Todate"] = DateTime.Now;
                //TempData["error"] = "Select Date";
            }
         
            return View();
        }

        public ActionResult ExpanceList(string date, string Todate)
        {
            var s = new List<expancedetailslist>();
            if (date !=null)
            {
                var model = (from ex in ec.Expandature.ToList()
                            join st in ec.Expancetype.ToList() on ex.Expancetype equals st.Id
                            where ex.date >= Convert.ToDateTime(date).Date && ex.date <= Convert.ToDateTime(Todate).Date
                             select new expancedetailslist 
                             { 
                                 date = ex.date, 
                                 payrtpe = st.TypeName,
                                 cashamount = ex.cashpay ?? 0,
                                 bankpay = ex.bankpay ?? 0,
                                 amount = ex.Amount 
                             })
                            .Union(from ss in db.StuffSalary.ToList()  
                                   join sf in db.Stuff.ToList() on ss.StuffId equals sf.Id
                                    where ss.payDate >= Convert.ToDateTime(date).Date && ss.payDate <= Convert.ToDateTime(Todate).Date
                                   select new expancedetailslist 
                                   { 
                                       date = ss.payDate, 
                                       amount = (ss.bankPay + ss.cashPay)??0,
                                       bankpay = ss.bankPay??0, cashamount = ss.cashPay??0, 
                                       payrtpe = "Salary ( " + sf.FirstName + " " + sf.LastName + " )" 
                                   });

                s = model.ToList();
            }
            
            else
            {
                var model = (from ex in ec.Expandature.ToList()
                             join st in ec.Expancetype.ToList() on ex.Expancetype equals st.Id
                             where ex.date == DateTime.Now.Date
                             select new expancedetailslist 
                             { 
                                 date = ex.date, 
                                 payrtpe = st.TypeName, 
                                 cashamount = ex.cashpay ?? 0,
                                 bankpay = ex.bankpay ?? 0,
                                 amount = ex.Amount 
                             })
                          .Union(from ss in db.StuffSalary.ToList()
                                 join sf in db.Stuff.ToList() on ss.StuffId equals sf.Id
                                 where ss.payDate == DateTime.Now.Date
                                 select new expancedetailslist 
                                 { 
                                     date = ss.payDate,
                                     amount = (ss.bankPay + ss.cashPay)??0,
                                     bankpay = ss.bankPay??0, cashamount = ss.cashPay??0, 
                                     payrtpe = "Salary ( " + sf.FirstName + " " + sf.LastName + " )" 
                                 });

                s = model.ToList();
            }

           
            return View(s);
         
        }

        public ActionResult EmployeeSalaryList(string date,string Todate)
        {
            var s = new List<expancedetailslist>();

            if (date != null)
            {
                var model = from ss in db.StuffSalary.ToList()
                            join sf in db.Stuff.ToList() on ss.StuffId equals sf.Id
                            join pay in db.StuffPaymentDetails on ss.StuffId equals pay.StuffId
                            where ss.payDate >= Convert.ToDateTime(date).Date && ss.payDate <= Convert.ToDateTime(Todate).Date
                            select new expancedetailslist
                            {
                                date = ss.payDate,
                                amount = (ss.bankPay + ss.cashPay)??0,
                                bankpay = ss.bankPay??0,
                                cashamount = ss.cashPay??0,
                                payrtpe = "Salary ( " + sf.FirstName + " " + sf.LastName + " )",
                                status = (pay.TotalSalry == (ss.cashPay??0 + ss.bankPay??0) ? "Paid" : "Due")
                            };

                s = model.ToList();
            }
            else
            {
                var model = from ss in db.StuffSalary.ToList()
                            join sf in db.Stuff.ToList() on ss.StuffId equals sf.Id
                            join pay in db.StuffPaymentDetails on ss.StuffId equals pay.StuffId
                            where ss.payDate == DateTime.Now.Date
                            select new expancedetailslist
                            {
                                date = ss.payDate,
                                amount = (ss.bankPay + ss.cashPay)??0,
                                bankpay = ss.bankPay??0,
                                cashamount = ss.cashPay??0,
                                payrtpe = "Salary ( " + sf.FirstName + " " + sf.LastName + " )",
                                status = (pay.TotalSalry == ss.bankPay + ss.cashPay ? "Paid" : "Due")
                            };

                s = model.ToList();
            }

            return View(s);
        }

        public ActionResult ExpensePrint(DateTime? fdate, DateTime? tdate)
        {
            if(fdate!=null && tdate!=null)
            {
                fdate = fdate.Value;
                tdate = tdate.Value;
            }
            else
            {
                fdate = DateTime.Now;
                tdate = DateTime.Now;
            }
            var model = (from ex in ec.Expandature.ToList()
                         join st in ec.Expancetype.ToList() on ex.Expancetype equals st.Id
                         where ex.date >= Convert.ToDateTime(fdate).Date && ex.date <= Convert.ToDateTime(tdate).Date
                         select new expancedetailslist 
                         {
                             date = ex.date, 
                             payrtpe = st.TypeName, 
                             cashamount = ex.cashpay ?? 0,
                             bankpay = ex.bankpay ?? 0, 
                             amount = ex.Amount 
                         })
                          .Union(from ss in db.StuffSalary.ToList()
                                 join sf in db.Stuff.ToList() on ss.StuffId equals sf.Id
                                 where ss.payDate >= Convert.ToDateTime(fdate).Date && ss.payDate <= Convert.ToDateTime(tdate).Date
                                 select new expancedetailslist
                                 { 
                                     date = ss.payDate,
                                     amount = (ss.bankPay??0 + ss.cashPay??0), 
                                     bankpay = ss.bankPay??0, 
                                     cashamount = ss.cashPay??0,
                                     payrtpe = "Salary ( " + sf.FirstName + " " + sf.LastName + " )"
                                 });
          
           
            //expenseModel = model.ToList();
            var totalamount = model.Sum(x => x.amount);
            var totalacashamount = model.Sum(x => x.cashamount);
            var totalbankpay = model.Sum(x => x.bankpay);
            
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Report"), "ExpenseReport.rpt"));
            rd.SetDataSource(model);
            rd.SetParameterValue("fdate", fdate.Value.ToString("dd-MM-yyyy"));
            rd.SetParameterValue("tdate", tdate.Value.ToString("dd-MM-yyyy"));
            rd.SetParameterValue("totalamount", totalamount.ToString());
            rd.SetParameterValue("totalacashamount", totalacashamount.ToString());
            rd.SetParameterValue("totalbankpay", totalbankpay.ToString());

            rd.SummaryInfo.ReportTitle = "Expense Report";
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
