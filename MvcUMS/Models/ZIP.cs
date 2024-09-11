using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MvcUMS.Models
{
    public class ZIPContext : DbContext
    {
        public ZIPContext()
            : base("UMSConnection")
        {
        }
        public DbSet<ZIP> ZIP { get; set; }
        public DbSet<ZIPCountryCity> ZIPCountryCity { get; set; }
    }
    [Table("ZIP")]
    public class ZIP
    {
        public int Id { get; set; }
        public string ZIPCode { get; set; }
        public int UpzilaId { get; set; }
    
    }

    [Table("ZIPCountryCity")]
    public class ZIPCountryCity
    {
        public int Id { get; set; }
        public int ZIPId { get; set; }
        public int CountryCityId { get; set; }

    }
}