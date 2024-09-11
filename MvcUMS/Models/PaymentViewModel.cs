using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MvcUMS.Models
{
    public class StudentListForPayment
    {
        public int? CourseId { get; set; }
        public string CourseName { get; set; }
        public int SemesterId { get; set; }
        public string SemesterName { get; set; }
        public System.Guid Id { get; set; }
        public int StdID { get; set; }
        public string StdNameEnglish { get; set; }
        public string StdFatherNameEnglish { get; set; }
        public string StdMotherNameEnglish { get; set; }
        public string ExamRollNo { get; set; }
        public string Email { get; set; }
        public string Picture { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PresentAddress { get; set; }
        public string PermanentAddress { get; set; }
        public decimal TotalSemesterFees { get; set; }

        public string ExamName { get; set; }
        public string GroupName { get; set; }
        public string BoardName { get; set; }
        public int? YearOfPassing { get; set; }
        public string RollOfExam { get; set; }
        public string Grade { get; set; }
    }

    public class PaymentViewModel
    {
        public Guid Id { get; set; }
        public int AutoId { get; set; }
        public string InvoiceNo { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Due { get; set; }

        public decimal Discount { get; set; }
        public DateTime PaymentDate { get; set; }
        public int PaymentMethodId { get; set; }
        public string PaymentMethod { get; set; }
        public int BankId { get; set; }
        public string BankName { get; set; }
        public string CheckNo { get; set; }
        public DateTime CheckDate { get; set; }

        public string StdID { get; set; }
        public string CourseName { get; set; }
        public string SemesterName { get; set; }
        public string Mobile { get; set; }
        public string Session { get; set; }

        public List<PaymentDetailsViewModel> PaymentDetailsViewModel { get; set; }
    }

    public class PaymentDetailsViewModel
    {

        public int Id { get; set; }
        public Guid studentId { get; set; }
        public int PaymentTypeID { get; set; }
        public string PaymentTypeName { get; set; }
        public decimal Fees { get; set; }
        public decimal PayAmount { get; set; }
        public Guid PaymentsId { get; set; }
        public string PayMonth { get; set; }
    }

    public class PaymentMethods
    {
        public int Id { get; set; }
        public string MethodName { get; set; }
    }

    public class PaymentTypeViewModels
    {
        public int Id { get; set; }
        public string PaymentType { get; set; }
        public decimal Amount { get; set; }
        public bool IsActive { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PaymentTypeListVM
    {
        public List<PaymentTypeViewModel> PaymentTypeItem { get; set; }
    }


    public class PaymentTypeName
    {
        public Guid Id { get; set; }

        public string InvoiceNo { get; set; }
        public Guid StudentId { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Due { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-mm-yyyy}")]

        public DateTime PaymentDate { get; set; }
        public int PaymentMethodId { get; set; }
        public int BankId { get; set; }
        public string CheckNo { get; set; }
        public DateTime CheckDate { get; set; }
        public int TypeId { get; set; }
        public decimal AdmissionFees { get; set; }
        public bool admissionSelected { get; set; }
        public decimal MonthlyFeesToPay { get; set; }
        public bool MonthlyFeesToPaySelected { get; set; }
        public decimal PartialPayment { get; set; }
        public decimal SemesterFeesToPay { get; set; }

        public bool SemesterFeesToPaySelected { get; set; }
        public decimal MonthlyFees { get; set; }
        public decimal SemesterFees { get; set; }
        public decimal ExamFees { get; set; }
        public bool ExamFeesSelected { get; set; }
        public decimal BooksOthers { get; set; }
        public bool BooksOthersSelected { get; set; }
        public decimal LateFees { get; set; }
        public bool LateFeesSelected { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalSemesterFees { get; set; }
        public bool TotalSemesterFeesSelected { get; set; }
        public decimal dueamount { get; set; }
        public decimal Amount { get; set; }
        public decimal predue { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

    }


    public class PaymentMethodViewModel
    {
        public int Id { get; set; }
        public string MethodName { get; set; }
    }

    public class BankVM
    {
        public int Id { get; set; }
        public string BankName { get; set; }

        public string Branch { get; set; }
    }

    public class SemesterMonthVM
    {
        public int Id { get; set; }
        public string MonthName { get; set; }
    }
}