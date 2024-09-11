using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MvcUMS.Models
{
    public class AccountingContext:DbContext
    {
        public AccountingContext()
            : base("UMSConnection")
        {

        }
        public DbSet<OpenningBalance> OpenningBalance { get; set; }
        public DbSet<BalanceType> BalanceType { get; set; }
        public DbSet<PaymentSatup> PaymentSatup { get; set; }
        public DbSet<StudentPaymentDetails> StudentPaymentDetails { get; set; }
        public DbSet<PaymentList> PaymentList { get; set; }
        public DbSet<TutionFees> TutionFees { get; set; }
        public DbSet<StudentDue> StudentDue { get; set; }
        public DbSet<StudentsPayment> StudentsPayment { get; set; }
     

    }
   
    [Table("OpenningBalance")]
    public class OpenningBalance
    {
        public int Id { get; set; }
        public int BalanceType { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public int BankId { get; set; }
    }
       [Table("BalanceType")]
    public class BalanceType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

     [Table("PaymentSatup")]
    public class PaymentSatup
    {
         [Key]
         [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
         public int Id { get; set; }
         public string Std_id { get; set; }
         public decimal TutionFeePerMonth { get; set; }
         public decimal LabFee { get; set; }
         public decimal AdmissionFee { get; set; }
         public decimal DevelopmentFee { get; set; }
         public decimal ExamFee { get; set; }
         public decimal LibraryFee { get; set; }
         public decimal Other { get; set; }
         public decimal Total { get; set; }
         public DateTime SessionStartMonth { get; set; }
         public int NoMonth { get; set; }
         public int NoExam { get; set; }
    }

     [Table("StudentsPayment")]
     public class StudentsPayment
     {
         public Guid Id { get; set; }
         public string StudentId { get; set; }
         public int classid { get; set; }
         public int roll { get; set; }
         public decimal totalfees { get; set; }
         public decimal Less { get; set; }
         public decimal Payamount { get; set; }
         public decimal Dueamount { get; set; }
         public string remarks { get; set; }
         public DateTime PaymentDate { get; set; }
         public decimal AdmissionFee { get; set; }
         public decimal ExamFee { get; set; }
         public decimal TutionFee { get; set; }
         public decimal OtherFee { get; set; }
         public decimal Khata { get; set; }
         public decimal Diary { get; set; }
       
     }
    [Table("StudentDue")]
    public class StudentDue
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public decimal TotalDueamount { get; set; }
    }

    public class studentpaydetaisl
    {
       public decimal Tution{get;set;}
           public decimal Exam{get;set;}
           public decimal Others { get; set; }
           public decimal Admision { get; set; }
           public decimal Late { get; set; }
           public decimal Less { get; set; }
           public decimal totalfees { get; set; }
           public decimal textCash { get; set; }
           public decimal dueamount { get; set; }
           public string Remarks { get; set; }
           public string StudentId { get; set; }
           public string studentname{get;set;}
           public string classname{get;set;}
           public int rollno { get; set; }
           public decimal predue { get; set; }
           public decimal Khata { get; set; }
           public decimal Diary { get; set; }
    }


    public class PaymentTypeViewModel
    {
        public int Id { get; set; }
        public string PaymentType { get; set; }
        public decimal Amount { get; set; }
        public bool IsActive { get; set; }
        public bool IsChecked { get; set; }
    }

     [Table("StudentPaymentDetails")]
     public class StudentPaymentDetails
     {
         [Key]
         [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
         public int Id { get; set; }
         public string std_id { get; set; } 
         public decimal  BankPay { get; set; }
         public decimal CashPay { get; set; }
         public string Remarks { get; set; }
         public DateTime PayDate { get; set; }
     }

     [Table("PaymentList")]
     public class PaymentList
     {
         [Key]
         [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
         public int Id { get; set; }
         public int PayId { get; set; }
         public string PayType { get; set; }
         public decimal Amount { get; set; }
     }

     [Table("TutionFees")]
     public class TutionFees
     {
         [Key]
         [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
         public int id { get; set; }
         public int PayListId { get; set; }
         public DateTime PayMonth { get; set; }
     }

    public class PaymentTypeList
    {

    }

}