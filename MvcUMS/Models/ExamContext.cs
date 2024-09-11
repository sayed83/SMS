using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;
using MvcUMS.Models.ViewModels;
namespace MvcUMS.Models
{
    public class ExamContext : DbContext
    {

        public ExamContext()
            : base("UMSConnection")
        {
        }
        public DbSet<ExamType> ExamType { get; set; }
        public DbSet<Exam> Exam { get; set; }
        public DbSet<Marks> Marks { get; set; }
        public DbSet<MarkType> MarkType { get; set; }
        public DbSet<SubjectMarks> SubjectMarks { get; set; }
        public DbSet<GPAType> GPAType { get; set; }
        public DbSet<StudentSubjectMarks> StudentSubjectMarks { get; set; }
        public DbSet<SubjectHighestMarks> SubjectHighestMarks { get; set; }

        public DbSet<CourseMarkTypeSetup> CourseMarkSetup { get; set; }

        public DbSet<StudentObtainMarksDetails> StudentObtainMarksDetails { get; set; }
        public DbSet<StudentObtainMarks> StudentObtainMarks { get; set; }

        public DbSet<MarksheetType> MarksheetType { get; set; }

        public DbSet<StudentMarksSummery> StudentMarksSummery { get; set; }
    }
    [Table("StudentSubjectMarks")]
    public class StudentSubjectMarks
    {
        public Guid Id { get; set; }
        public int SubjectCode { get; set; }
        public string StudentId { get; set; }
        public int ExamName { get; set; }
        public decimal Written { get; set; }
        public decimal MCQ { get; set; }
        public decimal ClassTest { get; set; }
        public decimal Practical { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal GradePoint { get; set; }
        public string LetterGrade { get; set; }
    }


    [Table("SubjectHighestMarks")]
    public class SubjectHighestMarks
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public int SubjectCode { get; set; }
        public decimal HM { get; set; }
        public decimal HGP { get; set; }
    }

    [Table("StudentMarksSummery")]
    public class StudentMarksSummery
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public int ExamId { get; set; }
        public decimal TotalObtainMarks { get; set; }
        public decimal TotalGradePoint { get; set; }
        public decimal GPA { get; set; }
        public decimal MeritPosition { get; set; }
        public bool IsPassed { get; set; }
    }

    [Table("ExamType")]
    public class ExamType
    {
        public int Id { get; set; }

        public string Examtype { get; set; }
        public DateTime date { get; set; }
        public bool isactive { get; set; }
    }

    [Table("MarksheetType")]
    public class MarksheetType
    {
        public int Id { get; set; }

        public string MarksheetFor { get; set; }

        public bool IsActive { get; set; }
    }

    [Table("CourseMarkTypeSetup")]
    public class CourseMarkTypeSetup
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int MarkTypeId { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal PassMarks { get; set; }
    }

    [Table("Exam")]
    public class Exam
    {
        public Guid Id { get; set; }
        public int ExamtypeId { get; set; }
        [Required]
        public int CourseId { get; set; }
         
        [Required]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }
        [Required]
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Type { get; set; }
        public Guid StuffId { get; set; }
        [NotMapped]
        public string CourseCode { get; set; }
    }

    [Table("MarkType")]
    public class MarkType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [NotMapped]
    public class ExamAndType
    {
        public Exam ex { get; set; }
        public ExamType et { get; set; }
        public Course co { get; set; }

        public Stuff stuff { get; set; }
    }
    [Table("Marks")]
    public class Marks
    {
        public Guid Id { get; set; }
        public string StudentId { get; set; }
        public Guid ExamId { get; set; }
        public decimal Mark { get; set; }
        public Guid TeacherId { get; set; }
        public DateTime Date { get; set; }
    }
    [Table("SubjectMarks")]
    public class SubjectMarks
    {
        public Guid Id { get; set; }
        public string StudentId { get; set; }
        public string SubjectCode { get; set; }
        public decimal Attendence { get; set; }
        public decimal ClassTest { get; set; }
        public decimal MidTerm { get; set; }
        public decimal FinalMark { get; set; }
        public string GPA { get; set; }
        public string Remarks { get; set; }
    }
    [Table("GPAType")]
    public class GPAType
    {
        public int Id { get; set; }
        public decimal Startmark { get; set; }
        public decimal endmark { get; set; }
        public decimal GradePoint { get; set; }
        public string GPA { get; set; }
    }

    public class SubjectMarksCourse
    {
        public StudentDetailsForMarksheetViewModel std { get; set; }
        public SubjectMarks sm { get; set; }
        public StudentSubjectMarks ssm { get; set; }
        public Course c { get; set; }
        public List<CourseMarkTypeSetupViewModel> CMS { get; set; }
        public string CourseTitle { set; get; }
        public decimal FullMarks { get; set; }
        public decimal highestmarks { get; set; }
        public decimal hightgrade { get; set; }
        public decimal TotalFullMarks { get; set; }
        public decimal TotalObMarks { get; set; }
        public decimal ObtainGPA { get; set; }
        public string LG { get; set; }

        public decimal GPA { get; set; }
        public int MeritPosition { get; set; }


        public List<MarkNameViewModel> MarkNameViewModel { get; set; }
    }


    public class ResultReport
    {
        public string SubjectCode;
        public string SubjectTitle;
        public decimal fullmarks;
        public decimal passmarks;
        public decimal classtest;
        public decimal subjective;
        public decimal objective;
        public decimal practical;
        public decimal totalmarks;
        public decimal gradepoint;
        public string lettergrade;
        public decimal hightestmarks;
        public decimal HYTotalObMarks { get; set; }

        public decimal fclasstest;
        public decimal fsubjective;
        public decimal fobjective;
        public decimal fpractical;
        public decimal ftotalmarks;
        public decimal fgradepoint;
        public string flettergrade;
        public decimal fhightestmarks;
        public decimal ANTotalObMarks { get; set; }

    }
    public class ExamRoutineView
    {
        public string time;
        public string first;
        public string second;
        public string third;
        public string fourth;
        public string fifth;
        public string six;
        public string seven;
        public string eight;
    }

    public class CourseMarkTypeViewModel
    {
        public CourseMarkTypeViewModel()
        {
            this.MarkTypes = new List<MarkTypeViewModel>() { new MarkTypeViewModel() };
        }
        public int Id { get; set; }
        public string CourseId { set; get; }
        [Required]
        public string CourseTitle { set; get; }
        public string Semester { set; get; }
        public string Department { set; get; }
        public Boolean IsActive { get; set; }
        public decimal FullMarks { get; set; }
        public decimal MinimumPassMarks { get; set; }
        public bool IsOptional { get; set; }

        public bool ExcludeFromResult { get; set; }

        public List<MarkTypeViewModel> MarkTypes { get; set; }
    }

    public class MarkTypeViewModel
    {
        public int MarkTypeId { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal PassMarks { get; set; }
    }

    [Table("StudentObtainMarks")]
    public class StudentObtainMarks
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Guid StudentId { get; set; }
        public int ExamId { get; set; }
        public decimal TotalMarks { get; set; }

        public ICollection<StudentObtainMarksDetails> MarksDetails { get; set; }
    }

    [Table("StudentObtainMarksDetails")]
    public class StudentObtainMarksDetails
    {
        public int Id { get; set; }
        public int MarkTypeId { get; set; }
        public decimal Marks { get; set; }
        public int StudentObtainMarksId { get; set; }

        public StudentObtainMarks Mark { get; set; }
    }

    public class StudenMarkInputViewModel
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string Section { get; set; }
        public decimal TotalMarks { get; set; }
        public string MarkType { get; set; }
    }

    public class CourseMarkViewModel
    {
        public int MarkTypeId { get; set; }
        public string MarkType { get; set; }
        public decimal Marks { get; set; }
    }


    public class MarkFormViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public int CourseId { get; set; }
        public int ExamId { get; set; }
        public decimal TotalMarks { get; set; }
        public List<MarkDetailsViewModel> MarkDetails { get; set; }
    }

    public class MarkInputView
    {
        public List<MarkFormViewModel> MarkFormViewModel { get; set; }
        public List<MarkDetailsViewModel> MarkDetailsViewModel { get; set; }
    }

    public class StudenInputViewModel
    {

        public StudenMarkInputViewModel StudenMarkInputViewModel { get; set; }
        public List<Dictionary<string, object>> Marks { get; set; }
    }



    public class MarkDetailsViewModel
    {
        public int MarkTypeId { get; set; }
        public decimal Marks { get; set; }
        public decimal MCQ { get; set; }
        public decimal Written { get; set; }
        public decimal ClassTest { get; set; }
        public int StudentObtainMarksId { get; set; }
    }


}