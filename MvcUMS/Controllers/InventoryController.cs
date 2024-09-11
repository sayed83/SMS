using MvcUMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Data;

namespace MvcUMS.Controllers
{
    public class InventoryController : Controller
    {
        //
        // GET: /Inventory/
        InventoryContext ic = new InventoryContext();

        public ActionResult Index(int? Page_No)
        {
            ViewBag.advancedsearch = "advancedsearch";
            var inventorylist = from i in ic.Inventory.ToList()
                                join it in ic.AssetType.ToList() on i.AssetTypeId equals it.Id
                                where i.isactive.Equals(true)
                                orderby i.Id
                                select new InventoryType
                                {
                                    I = i,
                                    IT = it
                                };
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(inventorylist.ToPagedList(No_Of_Page, Size_Of_Page));
           
        }

        public ActionResult InventoryCreate()
        {
            ViewBag.advancedsearch = "advancedsearch";
            ViewBag.TypeList = new SelectList(ic.AssetType.Where(x=>x.isactive.Equals(true)), "Id", "Name");
            return View();
        }
        [HttpPost]
        public ActionResult InventoryCreate(Inventory model, string companyname, string BoucherNumber)
        {
            var s = ic.Inventory.Where(x => x.Name.Equals(model.Name) && x.AssetTypeId.Equals(model.AssetTypeId)).Count();
            if (s > 0)
            {
                TempData["error"] = "Alrady Created.";
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var quantity = ic.Inventory.Where(x => x.Name.Equals(model.Name) && x.AssetTypeId.Equals(model.AssetTypeId)).Select(x => x.TotalQuantity).SingleOrDefault();
                    var prize = ic.Inventory.Where(x => x.Name.Equals(model.Name) && x.AssetTypeId.Equals(model.AssetTypeId)).Select(x => x.UnitPrize).SingleOrDefault();
                    if(quantity==0)
                    {
                        model.TotalQuantity = 0 + model.TotalQuantity;
                    }
                    else { model.TotalQuantity = quantity + model.TotalQuantity; }
                    if (prize == 0) { model.UnitPrize = 0 + model.UnitPrize; }
                    else { model.TotalQuantity = model.TotalQuantity; }
                    model.isactive = true;
                    model.date = model.date.Date;
                    ic.Inventory.Add(model);
                    ic.SaveChanges();
                    if (model.UnitPrize > 0 && model.TotalQuantity > 0)
                    {
                        var id = ic.Inventory.Where(x => x.Name.Equals(model.Name)).Select(x => x.Id).SingleOrDefault();
                        Purchase purchase = new Purchase();
                        purchase.OrderId = Guid.NewGuid();
                        purchase.Date = model.date.Date;
                        purchase.Prize = model.UnitPrize;
                        purchase.Quantity = model.TotalQuantity;
                        purchase.ProductId = id;
                        purchase.CompanyName = companyname;
                        purchase.BoucherNumber = BoucherNumber;
                        ic.Purchase.Add(purchase);
                        ic.SaveChanges();
                    }

                    return RedirectToAction("Index");
                }
            }
            ViewBag.TypeList = new SelectList(ic.AssetType.Where(x => x.isactive.Equals(true)), "Id", "Name");
            return View();
        }

        [HttpGet]
        public ActionResult PurchaseInventory(int id)
        {
            var model = new Inventory();
            model.Id = id;
            model.Name = ic.Inventory.Where(x => x.Id.Equals(id)).Select(s => s.Name).SingleOrDefault();
            model.AssetTypeId = ic.Inventory.Where(x => x.Id.Equals(id)).Select(s => s.AssetTypeId).SingleOrDefault();
            return View(model);
        }

        [HttpPost]
        public ActionResult PurchaseInventory(Inventory model, string companyname,string BoucherNumber)
        {
             var  b = ic.Inventory.Where(x => x.Id.Equals(model.Id)).SingleOrDefault();
          
            b.TotalQuantity = b.TotalQuantity + model.TotalQuantity;
            b.UnitPrize = model.UnitPrize;
            ic.Entry(b).State = EntityState.Modified;
            ic.SaveChanges();
           
            Purchase purchase = new Purchase();
            purchase.OrderId = Guid.NewGuid();
            purchase.Date = DateTime.Now.Date;
            purchase.CompanyName=companyname;
            purchase.BoucherNumber = BoucherNumber;
            purchase.Prize = model.UnitPrize;
            purchase.Quantity = model.TotalQuantity;
            purchase.ProductId = model.Id;
            ic.Purchase.Add(purchase);
            ic.SaveChanges();
            return RedirectToAction("Index");
        }



        public ActionResult PurchaseReport(string fromdate,string todate)
        {
            ViewBag.advancedsearch = "advancedsearch";
            if (!String.IsNullOrEmpty(fromdate) && !String.IsNullOrEmpty(todate))
            {
                TempData["list"] = "List";
                
            }
            return View();
        }
        public ActionResult PurchaseList(string fromdate, string todate, int? Page_No)
        {
            var purchlist = from i in ic.Inventory.ToList()
                            join it in ic.AssetType.ToList() on i.AssetTypeId equals it.Id
                            join p in ic.Purchase.ToList() on i.Id equals p.ProductId
                            where p.Date >= Convert.ToDateTime(fromdate).Date && p.Date <= Convert.ToDateTime(todate).Date
                            orderby p.OrderId
                            select new InventoryType
                            {
                                I=i,
                                IT=it,
                                p=p
                            };
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(purchlist.ToPagedList(No_Of_Page, Size_Of_Page));
        }

    }
}
