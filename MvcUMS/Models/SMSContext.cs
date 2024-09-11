using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MvcUMS.Models
{
    public class SMSContext:DbContext
    {
        public SMSContext()
            : base("UMSConnection")
        {
        }
        //public DbSet<SMS> SMS { get; set; }
       
    }
    [Table("SMS")]
    public class SMS
    {
        public Guid Id { get; set; }
        public string StudentId{get;set;}
        public Guid StuffId { get; set; }
        public string phone { get; set; }
        public string Text { get; set; }
        public DateTime date { get; set; }
    }

    public class Result
    {
        public bool success;
        public string errormessage;
        public int errorcode;
    }

    public class RootObject
    {
        public string from { get; set; }
        public string to { get; set; }
        public string text { get; set; }
    }
}