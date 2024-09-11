using MvcUMS.Models.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using VIT.DataLogicLayer;

namespace MvcUMS.Models.EntityManager
{
    public class PaymentManager
    {
        StudentContext sdb = new StudentContext();
        UMSEntities1 _std = new UMSEntities1();
        SchoolNewDBEntities _payCtx = new SchoolNewDBEntities();
        EventContext evc = new EventContext();
        PaymentContext _stdPayCtx = new PaymentContext();
        CommonManager CM = new CommonManager();

        public void AddMonthlyPaymentSetup(MonthlyStudentPaymentSetupViewModel model,
            List<StudentPaymentTypeViewModel> pType, List<StudentListViewModel> stdList,
            int[] noMonth)
        {
            MonthlyStudentPaymentSetup MPS = new MonthlyStudentPaymentSetup();
            MPS.SemesterId = model.SemesterId;
            MPS.DepartmentId = model.DepartmentId ?? 0;
            MPS.MonthId = model.MonthId;
            MPS.Total = model.TotalAmount;
            MPS.Year = model.Year;
            _payCtx.MonthlyStudentPaymentSetups.Add(MPS);

            _payCtx.SaveChanges();



            foreach (var sId in stdList)
            {
                if (sId.IsChecked == true)
                {
                    var preDueExists = _payCtx.StudentInvoiceLists.
                        Where(x => x.StudentId == sId.StudentId && x.DueAmount > 0 && x.MonthId == model.MonthId - 1).Select(d => new { d.DueAmount, d.MonthId }).FirstOrDefault();



                    StudentInvoiceList inv = new StudentInvoiceList();
                    inv.InvoiceNo = RandomString(10);
                    inv.TotalAmount = MPS.Total;
                    inv.PaidAmount = 0;
                    inv.DueAmount = MPS.Total;
                    inv.Discount = 0;
                    inv.CreatedDate = DateTime.Now;
                    inv.StudentId = sId.StudentId;
                    inv.MonthlyPaymentSetupId = MPS.Id;
                    inv.DueDate = DateTime.Now;
                    _payCtx.StudentInvoiceLists.Add(inv);

                    _payCtx.SaveChanges();

                    foreach (var item in pType)
                    {
                        if ((item.Amount > 0 && item.IsTypeChecked == true))
                        {
                            if (item.Id == 1)
                            {
                                foreach (var pMonth in noMonth)
                                {
                                    MonthlyPaymentSetupDetail MPD = new MonthlyPaymentSetupDetail();
                                    MPD.PaymentTypeId = item.Id;
                                    MPD.PaymenSetupId = inv.Id;
                                    MPD.PayMonth = pMonth;
                                    _payCtx.MonthlyPaymentSetupDetails.Add(MPD);
                                }
                            }
                            else
                            {
                                MonthlyPaymentSetupDetail MPD = new MonthlyPaymentSetupDetail();
                                MPD.PaymentTypeId = item.Id;
                                MPD.PaymenSetupId = inv.Id;
                                _payCtx.MonthlyPaymentSetupDetails.Add(MPD);
                            }

                        }


                        _payCtx.SaveChanges();

                    }


                }

            }



        }


        public void UpdateMonthlyPaymentSetup(InvoiceDetailsViewModel model, List<StudentPaymentTypeViewModel> pType, int[] noMonth)
        {
            foreach (var item in pType)
            {
                if ((item.Amount > 0 && item.IsTypeChecked == true))
                {
                    if (item.Id == 1)
                    {
                        foreach (var pMonth in noMonth)
                        {
                            MonthlyPaymentSetupDetail MPD = new MonthlyPaymentSetupDetail();
                            MPD.PaymentTypeId = item.Id;
                            MPD.PaymenSetupId = model.Id;
                            MPD.PayMonth = pMonth;
                            _payCtx.MonthlyPaymentSetupDetails.Add(MPD);
                        }
                    }
                    else
                    {
                        MonthlyPaymentSetupDetail MPD = new MonthlyPaymentSetupDetail();
                        MPD.PaymentTypeId = item.Id;
                        MPD.PaymenSetupId = 13;
                        _payCtx.MonthlyPaymentSetupDetails.Add(MPD);
                    }

                }


                _payCtx.SaveChanges();

            }

            var invoice = _payCtx.StudentInvoiceLists.Where(x => x.Id == model.Id).FirstOrDefault();

            //var payTypeId = _payCtx.MonthlyPaymentSetupDetails.Where(x => x.PaymenSetupId == invoice.Id).ToList();


            //var total = 0m;
            //foreach (var item in payTypeId)
            //{
            //    total += item. ?? 0;

            //}

            invoice.TotalAmount += model.TotalAmount;
            invoice.DueAmount = invoice.TotalAmount - invoice.PaidAmount;
            _payCtx.Entry(invoice).State = EntityState.Modified;

            _payCtx.SaveChanges();

        }

        public List<StudentInvoiceListViewModel> GetInvoiceList(string Std_id, int? SemesterId, int? MonthId)
        {
            var list = new List<StudentInvoiceListViewModel>();

            if (!string.IsNullOrEmpty(Std_id))
            {
                list = (from inv in _payCtx.StudentInvoiceLists.ToList()
                        join std in _std.Student_info.ToList() on inv.StudentId equals std.Id
                        join cai in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals cai.Std_id
                        join mps in _payCtx.MonthlyStudentPaymentSetups.ToList() on inv.MonthlyPaymentSetupId equals mps.Id
                        join mnt in _payCtx.Months.ToList() on mps.MonthId equals mnt.Id
                        where std.Std_id.Equals(Std_id)
                        select new StudentInvoiceListViewModel
                        {
                            Id = inv.Id,
                            InvoiceNo = inv.InvoiceNo,
                            StudentName = std.Std_FName + " " + std.Std_MName + " " + std.Std_LName,
                            TotalAmount = inv.TotalAmount ?? 0,
                            PaidAmount = inv.PaidAmount ?? 0,
                            PreviousDue = inv.PreviousDue ?? 0,
                            DueAmount = inv.DueAmount ?? 0,
                            DueDate = inv.DueDate.Value.ToShortDateString(),
                            Std_id = std.Std_id,
                            Department = cai.Department,
                            Semester = cai.Semester,
                            Section = cai.Section,
                            Month = mnt.MonthName,
                            GurdianMobile = std.GardianMobile
                        }).ToList();
            }
            else
            {

                list = (from inv in _payCtx.StudentInvoiceLists.ToList()
                        join std in _std.Student_info.ToList() on inv.StudentId equals std.Id
                        join cai in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals cai.Std_id
                        join mps in _payCtx.MonthlyStudentPaymentSetups.ToList() on inv.MonthlyPaymentSetupId equals mps.Id
                        join mnt in _payCtx.Months.ToList() on mps.MonthId equals mnt.Id
                        where mps.SemesterId.Equals(SemesterId) && mps.MonthId.Equals(MonthId)
                        select new StudentInvoiceListViewModel
                        {
                            Id = inv.Id,
                            InvoiceNo = inv.InvoiceNo,
                            StudentName = std.Std_FName + " " + std.Std_MName + " " + std.Std_LName,
                            TotalAmount = inv.TotalAmount ?? 0,
                            PaidAmount = inv.PaidAmount ?? 0,
                            PreviousDue = inv.PreviousDue ?? 0,
                            DueAmount = inv.DueAmount ?? 0,
                            DueDate = inv.DueDate.Value.ToShortDateString(),
                            Std_id = std.Std_id,
                            Department = cai.Department,
                            Semester = cai.Semester,
                            Section = cai.Section,
                            Month = mnt.MonthName,
                            GurdianMobile = std.GardianMobile
                        }).ToList();
            }

            return list;
        }

        public List<StudentInvoiceListViewModel> GetDueInvoiceList(string Std_id, int? SemesterId, int? MonthId, int? MonthIdTo)
        {
            var list = new List<StudentInvoiceListViewModel>();

            if (!string.IsNullOrEmpty(Std_id))
            {
                list = (from inv in _payCtx.StudentInvoiceLists.ToList()
                        join std in _std.Student_info.ToList() on inv.StudentId equals std.Id
                        join cai in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals cai.Std_id
                        join mps in _payCtx.MonthlyStudentPaymentSetups.ToList() on inv.MonthlyPaymentSetupId equals mps.Id
                        join mnt in _payCtx.Months.ToList() on mps.MonthId equals mnt.Id
                        where std.Std_id.Equals(Std_id) && inv.DueAmount > 0
                        select new StudentInvoiceListViewModel
                        {
                            Id = inv.Id,
                            InvoiceNo = inv.InvoiceNo,
                            StudentName = std.Std_FName + " " + std.Std_MName + " " + std.Std_LName,
                            TotalAmount = inv.TotalAmount ?? 0,
                            PaidAmount = inv.PaidAmount ?? 0,
                            PreviousDue = inv.PreviousDue ?? 0,
                            DueAmount = inv.DueAmount ?? 0,
                            DueDate = inv.DueDate.Value.ToShortDateString(),
                            Std_id = std.Std_id,
                            Department = cai.Department,
                            Semester = cai.Semester,
                            Section = cai.Section,
                            Month = mnt.MonthName
                        }).ToList();
            }
            else
            {
                list = (from inv in _payCtx.StudentInvoiceLists.ToList()
                        join std in _std.Student_info.ToList() on inv.StudentId equals std.Id
                        join cai in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals cai.Std_id
                        join mps in _payCtx.MonthlyStudentPaymentSetups.ToList() on inv.MonthlyPaymentSetupId equals mps.Id
                        join mnt in _payCtx.Months.ToList() on mps.MonthId equals mnt.Id
                        where mps.SemesterId.Equals(SemesterId) && mps.MonthId >= MonthId && mps.MonthId <= MonthIdTo && inv.DueAmount > 0
                        select new StudentInvoiceListViewModel
                        {
                            Id = inv.Id,
                            InvoiceNo = inv.InvoiceNo,
                            StudentName = std.Std_FName + " " + std.Std_MName + " " + std.Std_LName,
                            TotalAmount = inv.TotalAmount ?? 0,
                            PaidAmount = inv.PaidAmount ?? 0,
                            PreviousDue = inv.PreviousDue ?? 0,
                            DueAmount = inv.DueAmount ?? 0,
                            DueDate = inv.DueDate.Value.ToShortDateString(),
                            Std_id = std.Std_id,
                            Department = cai.Department,
                            Semester = cai.Semester,
                            Section = cai.Section,
                            Month = mnt.MonthName
                        }).ToList();
            }

            return list;
        }

        public List<StudentDueListViewModel> GetDueList(string Std_id, int? SemesterId, int? MonthId, int? MonthIdTo)
        {
            var list = new List<StudentDueListViewModel>();

            if (!string.IsNullOrEmpty(Std_id))
            {
                list = (from inv in _payCtx.StudentInvoiceLists.ToList()
                        join std in _std.Student_info.ToList() on inv.StudentId equals std.Id
                        join cai in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals cai.Std_id
                        join mps in _payCtx.MonthlyStudentPaymentSetups.ToList() on inv.MonthlyPaymentSetupId equals mps.Id
                        join mnt in _payCtx.Months.ToList() on mps.MonthId equals mnt.Id
                        where std.Std_id.Equals(Std_id)
                        group new { inv, std } by new { inv.StudentId, std } into g
                        select new StudentDueListViewModel
                        {
                            StudentName = g.Key.std.Std_FName + " " + g.Key.std.Std_MName + " " + g.Key.std.Std_LName,
                            TotalDue = g.Sum(s => s.inv.DueAmount) ?? 0,
                            StudentId = g.Key.std.Id,
                            StdId = g.Key.std.Std_id,
                            StudentInvoiceList = GetDueInvoiceList(Std_id, SemesterId, MonthId, MonthIdTo)
                        }).ToList();
            }
            else
            {



                list = (from inv in _payCtx.StudentInvoiceLists.ToList()
                        join std in _std.Student_info.ToList() on inv.StudentId equals std.Id
                        join cai in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals cai.Std_id
                        join mps in _payCtx.MonthlyStudentPaymentSetups.ToList() on inv.MonthlyPaymentSetupId equals mps.Id
                        join mnt in _payCtx.Months.ToList() on mps.MonthId equals mnt.Id
                        where mps.SemesterId.Equals(SemesterId) && mps.MonthId.Equals(MonthId)
                        select new StudentDueListViewModel
                        {
                            StudentName = std.Std_FName + " " + std.Std_MName + " " + std.Std_LName,
                            TotalDue = inv.TotalAmount ?? 0,
                            StudentId = std.Id,
                            StdId = std.Std_id
                        }).ToList();
            }

            return list;
        }

        public List<MonthlyStudentPaymentListViewModel> MonthlyPaymentList()
        {
            var list = (from mps in _payCtx.MonthlyStudentPaymentSetups.ToList()
                        join smt in sdb.Semester.ToList() on mps.SemesterId equals smt.Id
                        join mnt in _payCtx.Months.ToList() on mps.MonthId equals mnt.Id
                        join dpt in sdb.Department.ToList() on mps.DepartmentId equals dpt.Id into gdpt
                        from dpt in gdpt.DefaultIfEmpty()
                        select new MonthlyStudentPaymentListViewModel
                        {
                            Semester = smt.Semester_name,
                            Department = dpt == null ? "" : dpt.Dep_name,
                            DepartmentId = dpt == null ? 0 : dpt.Id,
                            Month = mnt.MonthName,
                            Total = mps.Total ?? 0,
                            Year = mps.Year ?? 0
                        }).ToList();

            return list;
        }


        public List<StudentPaymentTypeViewModel> getAllPaymentType(string Semester = null)
        {
            List<StudentPaymentTypeViewModel> List = new List<StudentPaymentTypeViewModel>();
            //var endList = _payCtx.StudentPaymentTypes.ToList();
            var endList = from p in _payCtx.StudentPaymentTypes.ToList()
                          join amt in _payCtx.StudentPaymentAmounts on p.Id equals amt.PaymentTypeId
                          join smt in sdb.Semester on amt.SemesterId equals smt.Id
                          where smt.Semester_name.Equals(Semester)
                          select new StudentPaymentTypeViewModel
                          {
                              Id = p.Id,
                              PaymentType = p.PaymentType,
                              Amount = amt.Amount ?? 0
                          };
            foreach (var item in endList)
            {

                List.Add(new StudentPaymentTypeViewModel()
                {
                    Id = item.Id,
                    PaymentType = item.PaymentType,
                    Amount = item.Amount
                });
            }
            return List;



            //var list = from p in _payCtx.StudentPaymentTypes.ToList()
            //           join amt in _payCtx.StudentPaymentAmounts on p.Id equals amt.PaymentTypeId
            //           join smt in sdb.Semester on amt.SemesterId equals smt.Id
            //           where smt.Semester_name.Equals(Semester)
            //           select new StudentPaymentTypeViewModel
            //           {
            //               Id = p.Id,
            //               PaymentType = p.PaymentType,
            //               Amount = amt.Amount??0
            //           };

            //return list.ToList();



            //var list = from p in _payCtx.PaymentTypes.ToList()
            //           select new PaymentTypeViewModel
            //           {
            //               Id = p.Id,
            //               PaymentType = p.PaymentType1,
            //               Amount = p.Amount ?? 0
            //           };

            //return list.ToList();
        }

        public IList<StudentPaymentTypeViewModel> GetPaymentTypeList()
        {

            List<StudentPaymentTypeViewModel> List = new List<StudentPaymentTypeViewModel>();
            var endList = _payCtx.StudentPaymentTypes.ToList();
            foreach (var item in endList)
            {

                List.Add(new StudentPaymentTypeViewModel()
                {
                    Id = item.Id,
                    PaymentType = item.PaymentType
                });
            }
            return List;
        }

        public IList<PaymentMethodViewModel> GetPaymentMethodList()
        {

            List<PaymentMethodViewModel> List = new List<PaymentMethodViewModel>();
            var pMList = _payCtx.PaymentMethods.ToList();
            foreach (var item in pMList)
            {

                List.Add(new PaymentMethodViewModel()
                {
                    Id = item.Id,
                    MethodName = item.MethodName

                });
            }
            return List;
        }

        public IList<StudentListViewModel> GetStudentList(string sem = null)
        {
            var studentList = (from s in _std.Student_info.ToList()
                               join cInfo in sdb.CurrentAcademicInfo.ToList() on s.Std_id equals cInfo.Std_id
                               join smt in sdb.Semester.ToList() on cInfo.Semester equals smt.Semester_name
                               where cInfo.Semester.Equals(sem)
                               select new StudentListViewModel
                               {
                                   StdId = s.Std_id,
                                   Name = s.Std_FName + s.Std_MName + s.Std_LName,
                                   StudentId = s.Id,
                                   SemesterId = smt.Id
                               }).ToList();

            return studentList;
        }

        public IList<BankVM> GetBankList()
        {

            List<BankVM> List = new List<BankVM>();
            var BnkList = _payCtx.BankLists.ToList();
            foreach (var item in BnkList)
            {

                List.Add(new BankVM()
                {
                    Id = item.Id,
                    BankName = item.BankName,
                    Branch = item.Branch

                });
            }
            return List;
        }




        public InvoiceDetailsViewModel invoiceInsertedData(int id)
        {
            var query = (from inv in _payCtx.StudentInvoiceLists.ToList()
                         join std in _std.Student_info.ToList() on inv.StudentId equals std.Id
                         join crr in sdb.CurrentAcademicInfo on std.Std_id equals crr.Std_id
                         join mps in _payCtx.MonthlyStudentPaymentSetups.ToList() on inv.MonthlyPaymentSetupId equals mps.Id

                         where inv.Id == id
                         select new InvoiceDetailsViewModel
                         {
                             Id = inv.Id,
                             InvoiceNo = inv.InvoiceNo,
                             StudentName = std.Std_FName + " " + std.Std_MName + " " + std.Std_LName,
                             TotalAmount = inv.TotalAmount ?? 0,
                             PaidAmount = inv.PaidAmount ?? 0,
                             PreviousDue = inv.PreviousDue ?? 0,
                             DueAmount = inv.DueAmount ?? 0,
                             DueDate = inv.DueDate.ToString(),
                             Mobile=std.Contact_no,
                             Address=std.Parmenent_address,
                             Session=crr.AcademicYear,
                             PaymentTypeList = (from mpd in _payCtx.MonthlyPaymentSetupDetails.ToList()
                                                join p in _payCtx.StudentPaymentTypes.ToList() on mpd.PaymentTypeId equals p.Id
                                                join amt in _payCtx.StudentPaymentAmounts on p.Id equals amt.PaymentTypeId
                                                join smt in sdb.Semester on amt.SemesterId equals smt.Id
                                                join pM in _payCtx.Months on mpd.PayMonth equals pM.Id into gMp
                                                from pM in gMp.DefaultIfEmpty()
                                                where mpd.PaymenSetupId.Equals(id)
                                                group new { mpd, amt, pM } by new { mpd.PaymentTypeId, p } into g
                                                select new StudentPaymentTypeViewModel
                                                {
                                                    PaymentTypeId = g.Key.PaymentTypeId,
                                                    PaymentType = g.Key.p.PaymentType,
                                                    Amount = g.Sum(x => x.amt.Amount) ?? 0,
                                                    PayMonths = g.Key.p.Id.Equals(1) ? (from mp in _payCtx.MonthlyPaymentSetupDetails.ToList()
                                                                                        join m in _payCtx.Months.ToList() on mp.PayMonth equals m.Id into gm
                                                                                        from m in gm.DefaultIfEmpty()
                                                                                        where mp.PaymenSetupId == id
                                                                                        select m).ToList() : null
                                                }).ToList(),
                             Collections = (from pc in _payCtx.StudentPaymentCollections.ToList()
                                            join siv in _payCtx.StudentInvoiceLists.ToList() on pc.StudentInvoiceId equals siv.Id
                                            join pm in _payCtx.PaymentMethods.ToList() on pc.PaymentMethodId equals pm.Id
                                            where pc.StudentInvoiceId == id
                                            select new PaymentCollectionViewModel
                                            {
                                                PayDate = pc.PayDate.Value.ToShortDateString(),
                                                Amount = pc.Amount,
                                                PaymentMehtod = pm.MethodName
                                            }).ToList(),
                             SchoolViewModel = CM.GetSchoolInfo()
                         }).FirstOrDefault();


            return query;
        }

        public DataTable GetPaymentReport(string ID)
        {

            ArrayList altParams = new ArrayList();
            altParams.Add(new SqlParameter("@StudentId", ID));
            return DatabaseManager.GetInstance().ExecuteStoredProcedureDataTable("GetPaymentDetails", altParams);
        }

        public DataTable GetStdPaymentList(string Department, string Semester, string Section)
        {
            ArrayList altParams = new ArrayList();
            altParams.Add(new SqlParameter("@dept", Department));
            altParams.Add(new SqlParameter("@semesterId", Semester));
            altParams.Add(new SqlParameter("@section", Section));
            return DatabaseManager.GetInstance().ExecuteStoredProcedureDataTable("GetStdPaymentDueList", altParams);
        }

        public DataTable GetPaymentReport(Guid ID)
        {

            ArrayList altParams = new ArrayList();
            altParams.Add(new SqlParameter("@StudentId", ID));
            return DatabaseManager.GetInstance().ExecuteStoredProcedureDataTable("GetPaymentDetails", altParams);
        }

        public IList<SemesterMonthVM> GetMonths(int id)
        {
            List<SemesterMonthVM> List = new List<SemesterMonthVM>();
            var eMList = (from mpd in _payCtx.Months.ToList()
                          where !(from o in _payCtx.MonthlyPaymentSetupDetails.ToList()
                                  where o.PaymenSetupId.Equals(id)
                                  select o.PayMonth).Contains(mpd.Id)
                          select mpd).ToList();


            foreach (var item in eMList)
            {

                List.Add(new SemesterMonthVM()
                {
                    Id = item.Id,
                    MonthName = item.MonthName

                });
            }
            return List;
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "0123456789MMTRADING";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public void CreatePaymentCollection(PaymentCollectionViewModel model)
        {
            StudentPaymentCollection SPC = new StudentPaymentCollection();
            SPC.Amount = model.Amount;
            SPC.PayDate = Convert.ToDateTime(model.PayDate);
            SPC.PaymentMethodId = model.PaymentMethodId;
            if (model.PaymentMethodId == 2)
            {
                SPC.BankId = model.BankId;
                SPC.CheckNo = model.CheckNo;
                SPC.CheckDate = model.CheckDate;
            }
            else
            {
                SPC.BankId = 0;
                SPC.CheckNo = null;
                SPC.CheckDate = null;
            }

            SPC.StudentInvoiceId = model.Id;
            _payCtx.StudentPaymentCollections.Add(SPC);
            _payCtx.SaveChanges();

            var invoice = _payCtx.StudentInvoiceLists.Where(x => x.Id == model.Id).FirstOrDefault();

            var collectionId = _payCtx.StudentPaymentCollections.Where(x => x.StudentInvoiceId == invoice.Id).ToList();


            var total = 0m;
            foreach (var item in collectionId)
            {
                total += item.Amount ?? 0;

            }

            invoice.PaidAmount = total;
            invoice.DueAmount = invoice.TotalAmount - invoice.PaidAmount;

            _payCtx.Entry(invoice).State = EntityState.Modified;

            _payCtx.SaveChanges();

        }

        public Guid AddPayment(PaymentTypeName model, List<PaymentTypeViewModel> ptvm, int[] noMonth)
        {
            Payment payment = new Payment();
            payment.Id = Guid.NewGuid();
            payment.InvoiceNo = RandomString(5);
            payment.StudentId = model.StudentId;
            payment.GrandTotal = model.TotalSemesterFees;
            payment.Discount = model.Discount;
            payment.PaidAmount = model.PaidAmount;
            payment.Due = model.Due;
            payment.PaymentDate = model.PaymentDate;
            payment.PaymentMethodId = model.PaymentMethodId;
            if (model.PaymentMethodId == 2)
            {
                payment.BankId = model.BankId;
                payment.CheckNo = model.CheckNo;
                payment.CheckDate = model.CheckDate;
            }
            else
            {
                payment.BankId = 0;
                payment.CheckNo = null;
                payment.CheckDate = null;
            }

            _payCtx.Payments.Add(payment);

            var stdSemester = from std in _std.Student_info.ToList()
                              join smt in sdb.CurrentAcademicInfo.ToList() on std.Std_id equals smt.Std_id
                              where std.Id == model.StudentId
                              select new { SemesterId = smt.Semester };

            foreach (var item in ptvm)
            {
                if ((item.Amount > 0 && item.IsChecked == true))
                {
                    if (item.Id == 3)
                    {
                        foreach (var pMonth in noMonth)
                        {
                            PaymentDetail pd = new PaymentDetail();
                            pd.PaymentTypeID = item.Id;
                            pd.PayAmount = item.Amount;
                            pd.PaymentsId = payment.Id;
                            pd.PayMonth = pMonth;
                            _payCtx.PaymentDetails.Add(pd);
                        }
                    }
                    else
                    {
                        PaymentDetail pd = new PaymentDetail();
                        pd.PaymentTypeID = item.Id;
                        pd.PayAmount = item.Amount;
                        pd.PaymentsId = payment.Id;
                        _payCtx.PaymentDetails.Add(pd);
                    }

                }

            }

            try
            {
                _payCtx.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                Console.WriteLine(e);
            }


            return payment.Id;


        }

        public List<PaymentViewModel> PaymentList(string id)
        {
            var list = new List<PaymentViewModel>();
            var PayList = from p in _payCtx.Payments.ToList()
                          join v in _std.Student_info.ToList() on p.StudentId equals v.Id
                          where p.StudentId.Equals(new Guid(id))
                          select new PaymentViewModel
                          {
                              Id = p.Id,
                              AutoId = p.AutoId,
                              StudentId = v.Id,
                              StudentName = v.Std_FName + " " + v.Std_LName,
                              InvoiceNo = p.InvoiceNo,
                              PaymentDate = p.PaymentDate ?? DateTime.Now,
                              GrandTotal = p.GrandTotal ?? 0,
                              PaidAmount = p.PaidAmount ?? 0,
                              Discount = _payCtx.PaymentDetails.Where(x => x.PaymentsId == p.Id && x.PaymentTypeID == 8).Select(x => x.PayAmount ?? 0).FirstOrDefault(),
                              Due = p.Due ?? 0
                          };
            list = PayList.ToList();
            return list;
        }

        public List<SchoolSetup> getSchoolList()
        {
            return evc.SchoolSetup.ToList();
        }


        public static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " Million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }



    }
}