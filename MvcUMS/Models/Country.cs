using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MvcUMS.Models
{
    public class CountryContex  : DbContext
    {
        public CountryContex()
            : base("UMSConnection")
        {
        }
        public DbSet<Countries> Countries { get; set; }
    }
    public class Countries
    {
        public int Id { get; set; }
        public string CountryName { get; set; }

    }
}