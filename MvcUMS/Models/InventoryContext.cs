using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MvcUMS.Models
{
    public class InventoryContext:DbContext
    {
        public InventoryContext()
            : base("UMSConnection")
        {

        }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<AssetType> AssetType { get; set; }
        public DbSet<Purchase> Purchase { get; set; }
    }
    [Table("Inventory")]
    public class Inventory
    {  
        [Key]
        public int Id { get; set; }
        public int AssetTypeId { get; set; }
        public string Name { get; set; }
        public bool isactive { get; set; }
        public DateTime date { get; set; }
        public int TotalQuantity{get;set;}
        public decimal UnitPrize { get; set; }
        public Guid StuffId { get; set; }
    }
    [Table("AssetType")]
    public class AssetType
    {  
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public bool isactive { get; set; }
    }
    [Table("Purchase")]
    public class Purchase
    {   
        [Key]
        public Guid OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal Prize { get; set; }
        public int Quantity { get; set; }
        public Guid StuffId { get; set; }
        public DateTime Date { get; set; }
        public string BoucherNumber { get; set; }
        public string CompanyName { get; set; }
    }



    public class InventoryType
    {
        public Inventory I { get; set; }
        public AssetType IT { get; set; }
        public Purchase p { get; set; }
    }
    public class reportlist
    {

        public string Name { get; set; }
        public decimal Amount { get; set; }
    }
    public class incomelist
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }
   
}