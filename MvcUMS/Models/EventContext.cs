using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MvcUMS.Models
{
    public class EventContext:DbContext
    {
        public EventContext()
            : base("UMSConnection")
        {

        }
        public DbSet<Events> Events { get; set; }
        public DbSet<Donation> Donation { get; set; }
        public DbSet<SchoolSetup> SchoolSetup { get; set; }

        public DbSet<Notice> Notices { get; set; }
        
    }
     [Table("Events")]
    public class Events
    {
         [Key]
          public Guid Id { get; set; }
          public string  EventName{get;set;}	
     public string EventFor{get;set;}
     public DateTime OrganizeDate { get; set; }
           public string Description{get;set;}
          public Guid StuffId { get; set; }
          public string Image { get; set; }


          [NotMapped]
          public HttpPostedFileBase picture { get; set; }
    }

    [Table("Donation")]
    public class Donation
    {  
        [Key]
        public int Id { get; set; }
        public string ProviderName { get; set; }
        public DateTime Date { get; set; }
        public string ReciverName { get; set; }
        public decimal CashPay { get; set; }
        public decimal BankPay { get; set; }
        public string Checknumber { get; set; }
        public string BankName { get; set; }
        public Guid StuffId { get; set; }
    }
    [Table("SchoolSetup")]
    public class SchoolSetup
    {
        public int Id { get; set; }
        public string SchoolName { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
        public string Logo { get; set; }
        public string opcityimg { get; set; }
        public DateTime Date { get; set; }
        public DateTime crdate { get; set; }
        [NotMapped]
        public HttpPostedFileBase picture { get; set; }

    }

    [Table("Notice")]
    public class Notice
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string NoticeFor { get; set; }

        public string Image { get; set; }

        [NotMapped]
        public HttpPostedFileBase picture { get; set; }
    }



}