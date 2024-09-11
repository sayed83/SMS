using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MvcUMS.Models
{
    public class PaymentContext:DbContext
    {
        public PaymentContext()
            : base("UMSConnection")
        {
        }

        public DbSet<stdPaymentType> PaymentType { get; set; }
        public DbSet<PaymentAmount> PaymentAmount { get; set; }
        public DbSet<Bank> Bank { get; set; }
    }

    [Table("stdPaymentType")]
    public class stdPaymentType
    {
        [Key]
       public int Id{get;set;}
       public string PaymentType{get;set;}
       public bool active { get; set; }
		
    }
     [Table("PaymentAmount")]
    public class PaymentAmount
    {
        [Key]
       public Guid Id{get;set;}
       public int? DepartmentId{get;set;}
       public int SemesterId{get;set;}

       public int PaymentTypeId { get; set; }
       public bool Active { get; set; }
       public Decimal Amount { get; set; }

       public Department Department { get; set; }
       public Semester Semester { get; set; }
       //public PaymentType PaymentTpe { get; set; }
		
    }
    [NotMapped]
     public class PaymentReport 
     {
         public Guid Id { get; set; }
         public string StudentId { get; set; }
         public string studentname { get; set; }
         public int classid { get; set; }
         public string classname { get; set; }
         public int roll { get; set; }
         public decimal totalfees { get; set; }
         public decimal Less { get; set; }
         public decimal Payamount { get; set; }
         public decimal Dueamount { get; set; }
         public string remarks { get; set; }
         public DateTime PaymentDate { get; set; }
         public decimal AdmissionFee { get; set; }
         public decimal ExamFee { get; set; }
         public decimal total { get; set; }
         public decimal TutionFee { get; set; }
         public decimal OtherFee { get; set; }
         public decimal Khata { get; set; }
         public decimal Diary { get; set; }
     }

    [Table("Bank")]
    public class Bank
    {
        public int Id { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsActive { get; set; }
    }
 public class ptamou
 {
     public Guid Id { get; set; }
      public string depname ;
      public string sem;
      public string paytype;
      public decimal payamount;
 }
}