using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MvcUMS.Models
{
    public class StudentAttendenceContex : DbContext
    {
        public StudentAttendenceContex()
            : base("UMSConnection")
        {
        }
        public DbSet<StudentAttendances> StudentAttendances { get; set; }
        public DbSet<StdAttendance> StdAttendances { get; set; }
        public DbSet<StudentPass> StudentPasses { get; set; }
    }
    public class StudentAttendances
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int AttendId { set; get; }
        public string Course_id { set; get; }
        public string Std_id { set; get; }

        public Nullable<System.DateTime> AttendanceDate { set; get; }
        public Boolean IsPresent { set; get; }
        public Boolean Isfirst { get; set; }
    }

    public class StudentPass
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }
        public string Std_id { set; get; }

        public Nullable<System.DateTime> Date { set; get; }
        public Boolean IsPass { set; get; }
    }


    public class StdAttendance
    {
        public int Id { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public DateTime CheckDate { get; set; }
        public string StdId { get; set; }
        public int ProximateID { get; set; }
    }

  

    public class ShowStdAttendence
    {
        public string StdId { set; get; }
        public string StuffName { get; set; }
        public string StdName { set; get; }
        public int rollNo { get; set; }
        public string StdFather { set; get; }
        public string courseTitle { get; set; }
        public int totalPresent { get; set; }
        public int totalAbsent { get; set; }
        public string ContactNo { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public Boolean isPresent { set; get; }
        public string Section { get; set; }
        public string Type { get; set; }
    }

    public class ShowStdPass
    {
        public string StdId { set; get; }
        public string StuffName { get; set; }
        public string StdName { set; get; }
        public int rollNo { get; set; }
        public string StdFather { set; get; }
        public string courseTitle { get; set; }
        public int totalPass { get; set; }
        public int totalFail { get; set; }
        public string ContactNo { get; set; }
        public Boolean isPass { set; get; }
        public string Section { get; set; }
        public string Type { get; set; }
    }

    [NotMapped]
    public class Chartview
    {
        public string className { get; set; }
        public int TotalPresent {get;set;}
        public int Total { get; set; }
        
    }
}