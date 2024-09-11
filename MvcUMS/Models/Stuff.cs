using PagedList;
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
    public class StuffContext : DbContext
    {
        public StuffContext()
            : base("UMSConnection")
        {
        }
        public DbSet<Stuff> Stuff { get; set; }
        public DbSet<StuffAttendence> StuffAttendence { get; set; }
        public DbSet<Designition> Designition { get; set; }
        public DbSet<StuffPaymentDetails> StuffPaymentDetails { get; set; }
        public DbSet<StuffDepartment> StuffDepartment { get; set; }
        public DbSet<StuffSalary> StuffSalary { get; set; }
    }

    [Table("Stuff")]
    public class Stuff
    {
        public Guid Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string FatherName { get; set; }
        [Required]
        public string MotherName { get; set; }
        [Required]
        public int Department { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; }
        [Required]
        public string NID { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string UserName { get; set; }
        [NotMapped]
        public string PrvUserName { get; set; }
        [NotMapped]
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [NotMapped]
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Image { get; set; }
        [Required]
        public string StateDetils { get; set; }
        //public int ZIPId{get;set;}
        public string ZIPId { get; set; }
        public int CityId { get; set; }
        //public int? UzId { get; set; }
        public string UzId { get; set; }
        public int CountryId { get; set; }
        public bool active { get; set; }
        [Required]
        public string PresentAddress { get; set; }
        public string Comments { get; set; }
        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Invalid")]
        public string ContactNo { get; set; }
        public string MemberCode { get; set; }

        [NotMapped]
        public HttpPostedFileBase picture { get; set; }
        public int ProximateID { get; set; }



    }

    [Table("StuffAttendence")]
    public class StuffAttendence
    {
        [Key]
        public Guid Id { get; set; }

        public Guid StuffId { get; set; }

        public DateTime Date { get; set; }
        public bool status { get; set; }
        public Stuff stuff { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> Designations { get; set; }
    }


    [Table("Designition")]
    public class Designition
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int DestId { get; set; }
        public string DesName { get; set; }
        public int DepatmentId { get; set; }

    }

    [Table("StuffPaymentDetails")]
    public class StuffPaymentDetails
    {
        [Key]
        public Guid StuffId { get; set; }
        [Required(ErrorMessage = "Please select a post", AllowEmptyStrings = false)]
        public int DesignitionId { get; set; }
        [Required(ErrorMessage = "Select a Payment Shcedule", AllowEmptyStrings = false)]
        public int PaymentShceduleId { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal HouseRent { get; set; }
        public decimal Health { get; set; }
        public decimal Others { get; set; }
        public decimal TotalSalry { get; set; }
    }
    [Table("StuffSalary")]
    public class StuffSalary
    {
        public Guid Id { get; set; }
        public Guid StuffId { get; set; }
        public DateTime payDate { get; set; }
        public decimal? cashPay { get; set; }
        public decimal? bankPay { get; set; }
        public decimal? Due { get; set; }
        public string Remarks { get; set; }
        public int? BankId { get; set; }
        public Guid EmpId { get; set; }
    }


    [Table("StuffDepartment")]
    public class StuffDepartment
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }

    }

    public class stuffFullDetails
    {
        public Stuff Stuff { get; set; }
        public StuffPaymentDetails StuffPaymentDetails { get; set; }
        public StuffDepartment StuffDepartment { get; set; }
        public Designition Designition { get; set; }
    }

}