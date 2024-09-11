using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcUMS.Models
{
    public class StudentContext : DbContext
    {
        public StudentContext()
            : base("UMSConnection")
        {
        }

        public DbSet<CurrentAcademicInfo> CurrentAcademicInfo { get; set; }
        public DbSet<StudentPreviousAcademicInfo> StudentPreviousAcademicInfo { get; set; }
        public DbSet<Class> Class { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<Semester> Semester { get; set; }
        public DbSet<Section> Section { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<Course> Course { get; set; }
        public DbSet<StudentPayment> StudentPayment { get; set; }
        public DbSet<TeacherSubject> TeacherSubject { get; set; }
       



    }


    [Table("StudentPayment")]
    public class StudentPayment
    {
        [Key]
        public Guid Id { get; set; }
        public string StudentId { get; set; }

        public int PaymentTypeId { get; set; }
        public DateTime Date { get; set; }
        public Guid EmployeeId { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public Decimal Amount { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> types { get; set; }

        //public Student_info StudentInfo { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> Students { get; set; }

        public Decimal PayAnount { get; set; }
        [NotMapped]
        public Decimal TotalAnount { get; set; }

        [NotMapped]
        public Decimal DueAnount { get; set; }
        [NotMapped]
        public Decimal advancepay { get; set; }

        [NotMapped]
        public Decimal fineAnount { get; set; }
        public ICollection<PaymentType> PaymentType { get; set; }

        public Decimal AdmissionFee { get; set; }
        public Decimal LibraryFee { get; set; }
        public Decimal LabFee { get; set; }
        public Decimal DevelopmentFee { get; set; }
        public Decimal TransportFee { get; set; }
        public Decimal SMSAlartFee { get; set; }
        public Decimal BankPay { get; set; }
        public Decimal CashPay { get; set; }
        public string Remarks { get; set; }
        public string bankname { get; set; }


    }

    public class StudentPaytypeStuff
    {
        public Student_info studentinfo { get; set; }
        public Stuff stuff { get; set; }
        public stdPaymentType paymenttype { get; set; }
        public StudentPayment studentpayment { get; set; }
    }

    public class StudentPaytypeStudentinfo
    {
        public Student_info studentinfo { get; set; }
        public CurrentAcademicInfo currentacademicinfo { get; set; }
        public PaymentType paymenttype { get; set; }
        public StudentPayment studentpayment { get; set; }
    }


    [Table("CurrentAcademicInfo")]
    public class CurrentAcademicInfo
    {
        [Key]
        public string Std_id { get; set; }
        //[Required(ErrorMessage = "Please select a department", AllowEmptyStrings = false)]
        public string Department { get; set; }
        [Required(ErrorMessage = "Please select a Semester", AllowEmptyStrings = false)]
        public string Semester { get; set; }
        [Required(ErrorMessage = "Please select a Section", AllowEmptyStrings = false)]
        public string Section { get; set; }
        [Required(ErrorMessage = "Please select a Status", AllowEmptyStrings = false)]
        public string Status { get; set; }
        public int RollNo { get; set; }

        public string RegNo { get; set; }

        public int ProximateID { get; set; }

        public int AcademicYear { get; set; }

        [NotMapped]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        
        [NotMapped]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }



    [Table("StudentPreviousAcademicInfo")]
    public class StudentPreviousAcademicInfo
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        //[Required(ErrorMessage = "Please select a department", AllowEmptyStrings = false)]
        public int RollNo { get; set; }
        public string Department { get; set; }
        [Required(ErrorMessage = "Please select a Semester", AllowEmptyStrings = false)]
        public string Semester { get; set; }
        [Required(ErrorMessage = "Please select a Section", AllowEmptyStrings = false)]
        public string Section { get; set; }
        [Required(ErrorMessage = "Please select a Status", AllowEmptyStrings = false)]
        public string Status { get; set; }
        public string Result { get; set; }
        public string Remarks { get; set; }
        public string StdId { get; set; }
    }

    [Table("Class")]
    public class Class
    {
        [Key]
        public int Id { get; set; }
        public string ClassName { get; set; }
    }


    [Table("Department")]
    public class Department
    {
        [Key]
        public int Id { get; set; }
        public string Dep_name { get; set; }
    }

    [Table("Semester")]
    public class Semester
    {
        [Key]
        public int Id { get; set; }
        public string Semester_name { get; set; }
    }

    [Table("Section")]
    public class Section
    {
        [Key]
        public int id { get; set; }
        public string section_name { get; set; }
        public string semester_Name { get; set; }
        public string deparment_Name { get; set; }
    }

    [Table("Status")]
    public class Status
    {
        [Key]
        public int id { get; set; }
        public string Std_status { get; set; }
    }
    [Table("Course")]
    public class Course
    {
        public int Id { get; set; }
        [Required]
        public string CourseId { set; get; }
        [Required]
        public string CourseTitle { set; get; }

        public string Semester { set; get; }
        public string Department { set; get; }
        public Boolean IsActive { get; set; }
        public decimal FullMarks { get; set; }
        public decimal MinimumPassMarks { get; set; }
        public bool? IsOptional { get; set; }
        public bool? ExcludeFromResult { get; set; }
    }

    [Table("TeacherSubject")]
    public class TeacherSubject
    {
        public Guid Id { get; set; }
        public Guid TeacherId { get; set; }
        public string SubjectCode { get; set; }
        public string Section { get; set; }
        public string Department { get; set; }
        public bool IsActive { get; set; }
    }

    //public class stdPay
    //{
    //    public DateTime Date { set; get; }
    //    public string TypeName { set; get; }

    //    public decimal PayAnount { set; get; }
    //}
    [NotMapped]
    public class PaymentStudent
    {
        public Student_info student { get; set; }
        public StudentPayment studentpayment { get; set; }
        public CurrentAcademicInfo curacainfo { get; set; }
    }


    public class StudentInfoForPayment
    {
        public string stdId { get; set; }
        public string FullName { get; set; }
        public string FatherName { get; set; }
        public string Class { get; set; }
        public string Section { get; set; }
        public int RolNo { get; set; }
    }
    public class CourseAssignList
    {
        public string courseCode { get; set; }
        public string courseName { get; set; }
        public string className { get; set; }
        public string sectionName { get; set; }
        public string teacherName { get; set; }
        public Guid teacherCode { get; set; }
        public Guid Id { get; set; }
    }

    public class studentsubjectmarks
    {
        public int Subjectid { get; set; }
        public int RollNo { get; set; }
        public string studentid { get; set; }
        public string stuentname { get; set; }
        public string section { get; set; }

        public List<MarkNameViewModel> MarkNameViewModel { get; set; }

       
        public decimal ObtainMark { get; set; }

        public decimal Written { get; set; }
        public decimal MCQ { get; set; }
        public decimal ClassTest { get; set; }
        public decimal Practical { get; set; }
    }

    public class MarkNameViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public decimal Mark { get; set; }
        public decimal PassMark { get; set; }

        [Remote("IsObtainMarksGreater", "Result", ErrorMessage = "Given Marks is greater then Subject Marks")]
        public decimal ObtainMark { get; set; }
        
    }

    public class CourseViewModel
    {
        public CourseViewModel()
        {
            AvailableMarkType = new List<SelectListItem>();
        }

        public int Id { get; set; }
        public string CourseId { set; get; }
        public string CourseTitle { set; get; }

        public string Semester { set; get; }

        public string semester_Name { get; set; }

        public string Department { set; get; }
        public Boolean IsActive { get; set; }
        public decimal FullMarks { get; set; }
        public decimal MinimumPassMarks { get; set; }
        public bool IsOptional { get; set; }

        public bool ExcludeFromResult { get; set; }

        public List<CourseMarkTypeSetupViewModel> CoourseDetails   { get; set; }

        public List<SelectListItem> AvailableMarkType { get; set; }
    }


    public class CourseMarkTypeSetupViewModel
    {
        public int Id { get; set; }
        public string CourseId { get; set; }
        public int MarkTypeId { get; set; }
        public string MarkType { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal PassMarks { get; set; }

    }


    public class StudentListForPromotion
    {
        public string Std_id { get; set; }
        public string Std_FName { get; set; }
        public string Std_MName { get; set; }
        public string Std_LName { get; set; }
        public string FullName { get; set; }
        public int RollNo { get; set; }
        public string CourseTitle { get; set; }
        public string Department { get; set; }
        public string Semester { get; set; }
        public string Section { get; set; }
        public string CourseId { get; set; }
        public string ContactNo { get; set; }
        public bool IsPromotion { get; set; }
    }



}