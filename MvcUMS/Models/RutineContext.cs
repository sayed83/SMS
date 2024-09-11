using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MvcUMS.Models
{
    public class RutineContext:DbContext
    {
        public RutineContext()
            : base("UMSConnection")
        {
        }
        public DbSet<Rutine> Rutine { get; set; }
        public DbSet<Week> Week { get; set; }
        public DbSet<TimeShedule> TimeShedule { get; set; }
    }

    [Table("TimeShedule")]
    public class TimeShedule
    {
        public int Id{get;set;}
        
         public string StartTime { get; set; }
        
         public string EndTime { get; set; }
         public string  Duration{get;set;}
         [Required]
         [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
         public DateTime? Date { get; set; }
	
    }

    [Table("Rutine")]
    public class Rutine
    {
        [Key]
        public Guid Id { get; set; }
     
        [Required]
        public Guid SubjectTeacherId { get; set; }
        public Guid RoomId { get; set; }
        [Required]
        public int WeekId { get; set; }
        public DateTime CreationDate { get; set; }
        [Required]
        public int timeId { get; set; }
        //public string endTime { get; set; }
        public Guid StuffId { get; set; }
        [NotMapped]
        public Guid TeacherId { get; set; }
    }
      [Table("Week")]
    public class Week
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class routineweekcourse
    {
        public Rutine ru { get; set; }
        public Week we { get; set; }
        public Course c { get; set; }
        public Stuff s { get; set; }
    }

    [NotMapped]
    public class RoutineView
    {
        public int timeId { get; set; }
        public string time { get; set; }
        public string Saturday { get; set; }
        public string Sunday { get; set; }
        public string Monday { get; set; }
        public string Tuesday { get; set; }
        public string Wednesday { get; set; }
        public string Thusday { get; set; }
        public string Friday { get; set; }
    }
}