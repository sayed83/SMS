using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MvcUMS.Models
{
    public class ScolarshipContext:DbContext
    {
    public ScolarshipContext()
            : base("UMSConnection")
        {
        }
        public DbSet<ScholarshipType> ScholarshipType { get; set; }
        public DbSet<StudentScholarship> StudentScholarship { get; set; }
      
    }
    [Table("ScholarshipType")]
    public class ScholarshipType
    {
         public int Id	{get;set;}
         public string TypeName{get;set;}
         public bool  isactive{get;set;}
         public DateTime date{get;set;}
		
     
    }
     [Table("StudentScholarship")]
    public class StudentScholarship
    {
         public Guid Id	{get;set;}
         public string StudentId {get;set;}
         public int ScholarshipTypeId {get;set;}
         public decimal Amount {get;set;}
         public Guid StuffId {get;set;}
         public DateTime Date {get;set;}
         public string Remarks {get;set;}
         public bool isactive {get;set;}
	
     
    }
    [NotMapped]

    public class studentScholarshiptype
    {
        public ScholarshipType type { get; set; }
        public StudentScholarship scholarship { get; set; }
        public Student_info stuinfo { get; set; }
        public CurrentAcademicInfo cruainfo { get; set; }
    }
 
   
}