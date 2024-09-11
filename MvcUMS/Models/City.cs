using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcUMS.Models
{
    public class CityContex  : DbContext
    {
        public CityContex()
            : base("UMSConnection")
        {
        }
        public DbSet<City> City { get; set; }
        public DbSet<CountryCity> CountryCity { get; set; }
        public DbSet<Upazila> Upazila { get; set; }
    }
    [Table("City")]
    public class City
    {
        public int Id { get; set; }
       
        public string CityName { get; set; }
      
     
    }
     [Table("Upazila")]
    public class Upazila
    {
      public int  Id{get;set;}
      public int  CityId{get;set;}
      public string UpazilaName{get;set;}
		
    }

    [Table("CountryCity")]
    public class CountryCity
    {
        public int Id { get; set; }
        public int CountryId { get; set; }
        public int CityId { get; set; }
        

    }
}