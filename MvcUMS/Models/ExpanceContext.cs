using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MvcUMS.Models
{
    public class ExpanceContext:DbContext
    {
        public ExpanceContext()
            : base("UMSConnection")
        {

        }
        public DbSet<Expancetype> Expancetype { get; set; }
        public DbSet<Expandature> Expandature { get; set; }
    }
    [Table("Expancetype")]
    public class Expancetype
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public DateTime date { get; set; }
        public bool isactive { get; set; }
    }
      [Table("Expandature")]
    public class Expandature
    {
          [Key]
          public Guid Id{get;set;}
        public int Expancetype { get; set; }
        public string Recipant { get; set; }
          public string Remarks { get; set; }
          public decimal Amount{get;set;}
          public decimal? cashpay{get;set;}
          public decimal? bankpay{get;set;}
          
        public DateTime date { get; set; }
        public Guid StuffId { get; set; }
       
    }

    public class expancetypeExpandature
    {
        public Expancetype et { get; set; }
        public Expandature ed { get; set; }
    }

    public class empsalary
    {
       public DateTime Date{get;set;}
       public string empname{get;set;}
                                    public string email{get;set;}
                                    public string mobiel{get;set;}
                                   public string  department{get;set;}
                                   public string  designation{get;set;}
                                    public decimal basicsalary{get;set;}
                                    public decimal houserent{get;set;}
                                    public decimal health{get;set;}
                                    public decimal other{get;set;}
                                    public decimal total{get;set;}
                                     public decimal cashpay{get;set;}
                                  public decimal   bankpay{get;set;}
                                  public decimal Due { get; set; }
    }
   public class expancedetailslist
   {
       public DateTime date { get; set; }
       public string payrtpe { get; set; }
       public decimal cashamount { get; set; }
       public decimal bankpay { get; set; }
       public decimal amount { get; set; }
       public string status { get; set; }
       
   }
    public class expancelist
    {
        public decimal cashpay { get; set; }
        public decimal bankpay { get; set; }
        public string particulars { get;set; }
        public DateTime date { get; set; }
        public decimal totalamount { get; set; }
    }
    public class Incomelist
    {
        public decimal cashpay { get; set; }
        public decimal bankpay { get; set; }
        public string particulars { get;set; }
        public DateTime date { get; set; }
        public decimal totalamount { get; set; }
    }
    public class incomemodel
    {
        public decimal cashpay { get; set; }
        public decimal bankpay { get; set; }
        public string particulars { get; set; }
        public DateTime date { get; set; }
        public decimal totalamount { get; set; }
        public string name{ get; set; }
        public int index{ get; set; }
        public decimal debitamount{ get; set; }
        public decimal creditamount{ get; set; }
        public decimal balance{ get; set; }
        public string sdate{ get; set; }
    }
    
}