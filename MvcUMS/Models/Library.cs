using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MvcUMS.Models
{
    public class Library:DbContext
    {
        public Library()
            : base("UMSConnection")
        {

        }
        public DbSet<Book> Book { get; set; }
        public DbSet<BookCatagory> BookCatagory { get; set; }
        public DbSet<Auther> Auther { get; set; }
        public DbSet<BookIssue> BookIssue { get; set; }
        public DbSet<CatagoryAuther> CatagoryAuther { get; set; }
        public DbSet<IndividualBookCode> IndividualBookCode { get; set; }
        
    }


    [Table("Book")]
    public class Book
    {
        public Guid Id { get; set; }
        public string BookId { get; set; }
        public string BookName { get; set; }
        public int AutherId { get; set; }
        public int Catagory { get; set; }
        public int ClassFor { get; set; }
        public int BookCopy { get; set; }
        public Decimal Price { get; set; }
        public string ISBN { get; set; }
       
        public string BookAddition { get; set; }
        public DateTime PublishDate { get; set; }
        public string Subject { get; set; }
        public Guid RegisteredBy { get; set; }
        public DateTime RegisterDate { get; set; }
        public bool isactive { get; set; }
    }
     [Table("IndividualBookCode")]
    public class IndividualBookCode
    {
         [Key]
     public Guid Id { get; set; }
     public string  BookId{get;set;}	
     public string BookCode{get;set;}
     public int Code { get; set; }
    }
    [Table("BookCatagory")]
    public class BookCatagory
    {
        public int Id { get; set; }
        public string CatagoryName { get; set; }
        public bool IsActive { get; set; }
        public DateTime Date { get; set; }

    }
    [Table("Auther")]
    public class Auther
    {
        [Key]
        public int Id { get; set; }
        public string authername { get; set; }
        public bool isactive { get; set; }
        public DateTime createdate { get; set; }

    }
     [Table("BookIssue")]
    public class BookIssue
    {
        [Key]
        public Guid Id { get; set; }
        public string BookId { get; set; }
        public string StudentId { get; set; }
        public bool Type { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime SubmissionDate { get; set; }
        public Guid EmployeeId{get;set;}
        public string BookCode { get; set; }
        public Guid StuffId { get; set; }

    }



    [Table("CatagoryAuther")]
    public class CatagoryAuther
    {
        public int Id { get; set; }
        public int CatagoryId{get;set;}
        public int AutherId { get; set; }
    }

  public class  cataauther
  {
      public string catagoryname;
      public string authername;
  }
  
   [NotMapped]
    public class autherbookBookIssue
    {
         public Book Book { get; set; }
         public Auther Auther { get; set; }
         public BookIssue BookIssue { get; set; }
    }

   [NotMapped]
   public class bookbookissuestudent
   {
       public Book Book { get; set; }
       public BookIssue BookIssue { get; set; }
       public Student_info Student_info { get; set; }
       public CurrentAcademicInfo CurrentAcademicInfo { get; set; }
   }

   [NotMapped]
   public class stuffbookissue
   {
      
       public BookIssue BookIssue { get; set; }
       public Stuff Stuff { get; set; }
      
   }


    [NotMapped]
    public class BookList
    {
        public string BookName{get;set;}
        public string BookId{get;set;}
        public string authername{get;set;}
        public int BookCopy{get;set;}
        public int GetBook{get;set;}
        public int Copy{get;set;}
        public decimal Prise { get; set; }
        public string Id { get; set; }

        public bool isactive { get; set; }
    }



     [NotMapped]
    public class IssuedBookList
    {
         public string Id { get; set; }
        public string Name{get;set;}
        public string ContactNumber{get;set;}
        public string Type{get;set;}
        public int TotalBook { get; set; }
       
    }
     [NotMapped]
     public class bookid
     {
         public string BookId { get; set; }
         public string BookCode { get; set; }
      

     }
   
}