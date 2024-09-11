using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcUMS.Models;
using System.IO;
using System.Xml.Linq;
using PagedList;
using CrystalDecisions.CrystalReports.Engine;
using MvcEnergyPac.Models;
using WebMatrix.WebData;
using MvcEnergyPac.Filters;
using System.Drawing;
using System.Drawing.Drawing2D;
using MvcUMS.Helpers;
using static MvcUMS.Helpers.GlobalHelper;

namespace MvcUMS.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class StuffController : Controller
    {
        private StuffContext db = new StuffContext();
        private StudentAttendances dba = new StudentAttendances();
        private PaymentContext dbpt = new PaymentContext();
        CountryContex country = new CountryContex();
        CityContex citydb = new CityContex();
        ZIPContext zipdb=new ZIPContext();
        UsersContext uc = new UsersContext();
        EventContext evc = new EventContext();
        
        //
        // GET: /Stuff/

        public ActionResult Index(string Search_Data, string Department,string Designition,int? Page_No)
        {
            ViewBag.advancedsearch = "advancedsearch";
            ViewBag.Department = new SelectList(db.StuffDepartment, "DepartmentId", "DepartmentName");

            if (Search_Data != null || Department !=null)
            {
                Page_No = 1;
            }
            
            var stuff = from stt in db.Stuff
                        join stuffPay in db.StuffPaymentDetails on stt.Id equals stuffPay.StuffId
                        join des in db.Designition on stuffPay.DesignitionId equals des.DestId
                        join stuffDep in db.StuffDepartment on stt.Department equals stuffDep.DepartmentId
                        orderby (stt.FirstName)
                        select new stuffFullDetails { Stuff = stt, StuffPaymentDetails = stuffPay, StuffDepartment = stuffDep, Designition = des };
            if (!String.IsNullOrEmpty(Search_Data))
            {
                stuff = from stt in db.Stuff
                        join stuffPay in db.StuffPaymentDetails on stt.Id equals stuffPay.StuffId
                        join des in db.Designition on stuffPay.DesignitionId equals des.DestId
                        join stuffDep in db.StuffDepartment on stt.Department equals stuffDep.DepartmentId
                        where ((stt.ContactNo + " ").ToLower().Contains(Search_Data.ToLower()))
                               || (stt.FirstName + " " + stt.LastName + " ").ToLower().Contains(Search_Data.ToLower())
                                   || (stt.Email + " ").ToLower().Contains(Search_Data.ToLower())
                        orderby (stt.FirstName)
                        select new stuffFullDetails { Stuff = stt, StuffPaymentDetails = stuffPay, StuffDepartment = stuffDep, Designition = des };
            }

            else if (!String.IsNullOrEmpty(Department) && String.IsNullOrEmpty(Designition))
            {
                int p = Convert.ToInt16(Department);
                stuff = from stt in db.Stuff
                        join stuffPay in db.StuffPaymentDetails on stt.Id equals stuffPay.StuffId
                        join des in db.Designition on stuffPay.DesignitionId equals des.DestId
                        join stuffDep in db.StuffDepartment on stt.Department equals stuffDep.DepartmentId
                        where (stt.Department.Equals(p))
                        orderby (stt.FirstName)
                        select new stuffFullDetails { Stuff = stt, StuffPaymentDetails = stuffPay, StuffDepartment = stuffDep, Designition = des };
            }
            else if (!String.IsNullOrEmpty(Designition) && !String.IsNullOrEmpty(Department))
            {
                int p = Convert.ToInt16(Designition);
                stuff = from stt in db.Stuff
                        join stuffPay in db.StuffPaymentDetails on stt.Id equals stuffPay.StuffId
                        join des in db.Designition on stuffPay.DesignitionId equals des.DestId
                        join stuffDep in db.StuffDepartment on stt.Department equals stuffDep.DepartmentId
                        where (stuffPay.DesignitionId.Equals(p))
                        orderby (stt.FirstName)
                        select new stuffFullDetails { Stuff = stt, StuffPaymentDetails = stuffPay, StuffDepartment = stuffDep, Designition = des };
            }
         
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(stuff.ToPagedList(No_Of_Page, Size_Of_Page));
          
        }

        // get desdignition

        [HttpPost]
        public JsonResult getDesignition(string id)
        {

            int s = int.Parse(id);
            var objcity = db.Designition.Where(a=>a.DepatmentId.Equals(s)).ToList();
            SelectList objdata = new SelectList(objcity, "DestId", "DesName", 0);
            return new JsonResult { Data = objdata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        //
        // GET: /Stuff/Details/5

        public ActionResult Details(Guid id)
        {
            Stuff stuff = db.Stuff.Find(id);
            TempData["Department"] = db.StuffDepartment.Where(a => a.DepartmentId.Equals(stuff.Department)).Select(a=>a.DepartmentName).FirstOrDefault();
            if (stuff == null)
            {
                return HttpNotFound();
            }
            return View(stuff);
        }

        // get stuff Salary Details

         [HttpPost]
        public PartialViewResult showSalaryDetails(Guid id)
        {
            var stu = db.StuffPaymentDetails.Where(a => a.StuffId.Equals(id)).FirstOrDefault();
            TempData["Designition"] = db.Designition.Where(a => a.DestId.Equals(stu.DesignitionId)).Select(a=>a.DesName).FirstOrDefault();
            if (stu.PaymentShceduleId == 1)
                TempData["PayS"] = "Monthly";
            else if(stu.PaymentShceduleId == 2)
                TempData["PayS"] = "Hourly";
            else
            {
                TempData["PayS"] = "Class Wise";
            }
            return PartialView(stu);
        }


        // auto complite for stuff get

        public JsonResult getStuff(string term)
        {
            IEnumerable<dynamic> users = null;
          
            if ((users = db.Stuff.Where(stf => stf.ContactNo.ToLower().Contains(term.ToLower())).OrderBy(a=>a.ContactNo).Select(a => a.ContactNo).ToList()).Count() != 0)
                return Json(users, JsonRequestBehavior.AllowGet);
            else if ((users = db.Stuff.Where(stf => (stf.FirstName + " " + stf.LastName).ToLower().Contains(term.ToLower())).OrderBy(a=>a.FirstName).Select(a => a.FirstName + " " + a.LastName).ToList()).Count() != 0)
                return Json(users, JsonRequestBehavior.AllowGet);
            else if ((users = db.Stuff.Where(stf => stf.Email.ToLower().Contains(term.ToLower())).OrderBy(a => a.Email).Select(a => a.Email).ToList()).Count() != 0)
                return Json(users, JsonRequestBehavior.AllowGet);
            return Json(users, JsonRequestBehavior.AllowGet);
        }
        //
        // GET: /Stuff/Create

        public ActionResult Create()
        {
            ViewBag.studentscripts = "specificscripts";
            ViewBag.Department = new SelectList(db.StuffDepartment, "DepartmentId", "DepartmentName");
            ViewBag.CountryList = new SelectList(country.Countries, "Id", "CountryName");

            return View();
        }

        //
        // POST: /Stuff/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Stuff stuff)
        {
            ViewBag.studentscripts = "specificscripts";
            var imagename = "";
            if (ModelState.IsValid)
            {
                if (stuff.picture != null && stuff.picture.ContentLength > 0)
                {
                    imagename = stuff.picture.FileName;
                    var uploadDir = "~/uploads";
                    var imagePath = Path.Combine(Server.MapPath(uploadDir), stuff.picture.FileName);
                    var imageUrl = Path.Combine(uploadDir, stuff.picture.FileName);
                    
                    if (System.IO.File.Exists(Server.MapPath(imageUrl)))
                    {
                        int counter = 2;
                        while (System.IO.File.Exists(Server.MapPath(imageUrl)))
                        {
                         
                            imagePath =Path.Combine(Server.MapPath(uploadDir),counter.ToString()+ stuff.picture.FileName);
                            imageUrl = Path.Combine(uploadDir,counter.ToString()+ stuff.picture.FileName);
                            imagename =counter.ToString()+stuff.picture.FileName;
                            counter++;
                        }

                    }
            
                    stuff.picture.SaveAs(imagePath);
                }
           
    
                stuff.Id = Guid.NewGuid();
                stuff.active = true;
                stuff.Image = imagename;
                stuff.MemberCode = "emp" + "00" + stuff.Id;

                WebSecurity.CreateUserAndAccount(stuff.UserName, stuff.Password);
                var userdata = $"Stuff User Name: {stuff.UserName} Pass: {stuff.Password}";
                //SaveTxtData(userdata);
                var userdetails = uc.UserProfiles.Where(a => a.UserName.Equals(stuff.UserName)).FirstOrDefault();
                userdetails.SchoolId = (int)Session["SchoolId"];
                uc.Entry(userdetails).State = EntityState.Modified;
                webpages_UsersInRoles usrRoles = new webpages_UsersInRoles();
                var rolename = RoleType.Teacher.ToString();
                var roleid = uc.webpages_Roles.Where(r => r.RoleName == rolename).FirstOrDefault().RoleId;
                usrRoles.RoleId = roleid;
                usrRoles.UserId = userdetails.UserId;
                uc.webpages_UsersInRoles.Add(usrRoles);
                uc.SaveChanges();

                db.Stuff.Add(stuff);
                db.SaveChanges();
                return RedirectToAction("StuffSalary", "Stuff", new { Department = stuff.Department,stuffId=stuff.Id });
            }
            return View(stuff);
        }

        //
        // GET: /Stuff/Edit/5

        public ActionResult Edit(Guid id)
        {
            Stuff stuff = db.Stuff.Find(id);
            stuff.PrvUserName = stuff.UserName;
            ViewBag.password="1";
            ViewBag.Cpassword = "1";

            ViewBag.Departmentlist = new SelectList(db.StuffDepartment, "DepartmentId", "DepartmentName");
            ViewBag.CountryList = new SelectList(country.Countries, "Id", "CountryName");
            ViewBag.CityList = new SelectList(citydb.City, "Id", "CityName");
            ViewBag.ZIPList = new SelectList(zipdb.ZIP, "Id", "ZIPCode");
            ViewBag.UpzilaList = new SelectList(citydb.Upazila, "Id", "UpazilaName");

            if (stuff == null)
            {
                return HttpNotFound();
            }
            return View(stuff);
        }

        //
        // POST: /Stuff/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Stuff stuff)
        {
            if (ModelState.IsValid)
            {
              
                //if (stuff.picture != null && stuff.picture.ContentLength > 0)
                //{
                //    var imagename = "";
                //    imagename = stuff.picture.FileName;
                //    var uploadDir = "~/uploads";
                //    var imagePath = Path.Combine(Server.MapPath(uploadDir), stuff.picture.FileName);
                //    stuff.Image = imagename;
                //    var imageUrl = Path.Combine(uploadDir, stuff.picture.FileName);

                //    if (System.IO.File.Exists(Server.MapPath(imageUrl)))
                //    {
                //        int counter = 2;
                //        while (System.IO.File.Exists(Server.MapPath(imageUrl)))
                //        {

                //            imagePath = Path.Combine(Server.MapPath(uploadDir), counter.ToString() + stuff.picture.FileName);
                //            imageUrl = Path.Combine(uploadDir, counter.ToString() + stuff.picture.FileName);
                //            imagename = counter.ToString() + stuff.picture.FileName;
                //            counter++;
                //        }

                //    }

                //    stuff.picture.SaveAs(imagePath);
                //}


                if (stuff.picture != null && stuff.picture.ContentLength > 0)
                {
                    string filename = "";
                    filename = Path.GetFileName(Guid.NewGuid() + "." + stuff.picture.FileName.Split('.')[1]);
                    stuff.Image = filename;
                    string targetPath = Server.MapPath("~/uploads//" + filename);
                    Stream strm = stuff.picture.InputStream;
                    var targetFile = targetPath;

                    GenerateThumbnails(0.5, strm, targetFile);
                }


                var stufflist = db.Stuff.Where(x => x.Id.Equals(stuff.Id)).FirstOrDefault();
                stufflist.FirstName=stuff.FirstName;
                stufflist.LastName = stuff.LastName;
                stufflist.UserName = stuff.UserName;
                if (stuff.Image == null)
                {
                    stufflist.Image = stufflist.Image;
                }
                else
                {
                    stufflist.Image = stuff.Image;
                }
               
                stufflist.FatherName = stuff.FatherName;
                stufflist.MotherName = stuff.MotherName;
                stufflist.NID = stuff.NID;
                stufflist.PresentAddress = stuff.PresentAddress;
                stufflist.StateDetils = stuff.StateDetils;
                stufflist.ZIPId = stuff.ZIPId;
                stufflist.Gender = stuff.Gender;
                stufflist.DOB = stuff.DOB;
                stufflist.Department = stuff.Department;
                stufflist.ContactNo = stuff.ContactNo;
                stufflist.active = true;
                stufflist.CityId = stuff.CityId;
                stufflist.JoinDate = stuff.JoinDate;
                stufflist.Email = stuff.Email;
                stufflist.Password = stuff.Password;
                stufflist.ProximateID = stuff.ProximateID;
                stufflist.ConfirmPassword = stuff.ConfirmPassword;
                db.Entry(stufflist).State = EntityState.Modified;
                var username = stuff.UserName == stuff.PrvUserName ? stuff.UserName : stuff.PrvUserName;
                var user = uc.UserProfiles.Where(u=>u.UserName == username).FirstOrDefault();
                user.UserName = stuff.UserName;
                uc.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                uc.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.Departmentlist = new SelectList(db.StuffDepartment, "DepartmentId", "DepartmentName");
            ViewBag.CountryList = new SelectList(country.Countries, "Id", "CountryName");
            ViewBag.CityList = new SelectList(citydb.City, "Id", "CityName");
            ViewBag.ZIPList = new SelectList(zipdb.ZIP, "Id", "ZIPCode");
            ViewBag.UpzilaList = new SelectList(citydb.Upazila, "Id", "UpazilaName");
            return View(stuff);
        }


        private void GenerateThumbnails(double scaleFactor, Stream sourcePath, string targetPath)
        {
            using (var image = System.Drawing.Image.FromStream(sourcePath))
            {
                int newWidth;
                int newHeight;

                newWidth = 512;
                newHeight = 384;

                var thumbnailImg = new Bitmap(newWidth, newHeight);
                var thumbGraph = Graphics.FromImage(thumbnailImg);
                thumbGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                thumbGraph.DrawImage(image, imageRectangle);
                if (System.IO.File.Exists(targetPath))
                {
                    System.IO.File.Delete(targetPath);
                }

                thumbnailImg.Save(targetPath, image.RawFormat);
            }
        }

        //
        // GET: /Stuff/Delete/5

        public ActionResult Delete(Guid id)
        {
            Stuff stuff = db.Stuff.Find(id);
            if (stuff == null)
            {
                return HttpNotFound();
            }
            return View(stuff);
        }

        //
        // POST: /Stuff/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Stuff stuff = db.Stuff.Find(id);
            db.Stuff.Remove(stuff);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // for stuff attendence get method
        public ActionResult StuffAttendence( )
        {
            StuffAttendence model = new StuffAttendence();
            ViewBag.Department = new SelectList(db.StuffDepartment, "DepartmentId", "DepartmentName");
            model.Date = DateTime.Now.Date;
            return View(model);
        }
        // POST: /Stuff/stuffattendence/
        [HttpPost]
        public ActionResult StuffAttendence(string[] ids)
        {
            StuffAttendence dba = new StuffAttendence();
            Guid[] id = null;
            if(ids!=null)
            {
                id=new Guid[ids.Length];
                int j = 0;
                foreach(string i in ids)
                {
                    Guid.TryParse(i, out id[j++]);
                }
            }
            if (id != null && id.Length > 0)
            {
                List<Stuff> stufflist = new List<Stuff>();
                stufflist = db.Stuff.Where(x => id.Contains(x.Id)).ToList();
                foreach (var i in stufflist)
                {
                    dba.Id =Guid.NewGuid();
                    dba.StuffId = i.Id;
                    dba.Date = DateTime.Now.Date;
                    dba.status = true;
                    db.StuffAttendence.Add(dba);
                    db.SaveChanges();
                   
                }
               
            }

            return RedirectToAction("Index");
        }

        //get dropdown citylist
        [HttpPost]
        public JsonResult GetCity( string id)
        {

            int s = int.Parse(id);
            List<City> city = citydb.City.ToList();
            List<CountryCity> cCity = citydb.CountryCity.Where(a=>a.CountryId.Equals(s)).ToList(); ;
            var objcity = city.Join(cCity, c => c.Id, cc => cc.CityId, (c, cc) => new { Value=cc.Id,Name=c.CityName}).ToList();
            SelectList objdata = new SelectList(objcity, "Value", "Name", 0);

            return new JsonResult { Data = objdata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        [HttpPost]
        public ActionResult GetUpazila(string idd)
        {
            int s = int.Parse(idd);
            var upazila=from u in citydb.Upazila.ToList()
                        where u.CityId.Equals(s)
                        select new 
                        {
                            Value = u.Id,
                            Name=u.UpazilaName
                        };

            SelectList objdata = new SelectList(upazila.ToList(), "Value", "Name", 0);

            return new JsonResult { Data = objdata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        //get dropdown ZIPCodelist

        [HttpPost]
        public ActionResult GetZIPCode(string idd)
        {
            int s = int.Parse(idd);
            var zips = from z in zipdb.ZIP.ToList()
                       where z.UpzilaId.Equals(s)
                       select new {Value=z.Id,
                       Name=z.ZIPCode};
        
            SelectList objdata = new SelectList(zips.ToList(), "Value", "Name", 0);

            return new JsonResult { Data = objdata, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        // email and nid varification

        public ActionResult GetData(string nid,string email)
        {
            var naid = "";
            var s="";
            if(nid!=null)
            {
                 naid = db.Stuff.Where(a => a.NID.Equals(nid)).Select(x => x.NID).SingleOrDefault();
                 if (naid == nid)
                 {
                     s = "1";
                 }
            }
            else 
            {
                 naid = db.Stuff.Where(a => a.Email.Equals(email)).Select(x => x.Email).SingleOrDefault();
                 if (naid == email)
                 {
                     s = "1";
                 }
            }


            return new JsonResult { Data = s, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
         

        }

        [HttpPost]
        public ActionResult GetGridData(int id)
        {
          
            List<Stuff> stuff = db.Stuff.Where(x=>x.Department.Equals(id)).ToList(); ;

            return PartialView("GetGridData", stuff);
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult StuffSalary(int Department = 0, Guid stuffId = default(Guid))
        {
            TempData["StuffId"] = stuffId;
            ViewBag.Designition= new SelectList(db.Designition.Where(a => a.DepatmentId.Equals(Department)), "DestId", "DesName");
            return View();
        }

        [HttpPost]
        public ActionResult StuffSalary(StuffPaymentDetails stp)
        {
            db.StuffPaymentDetails.Add(stp);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // stuff salary edit

        public ActionResult stuffSalaryUpdate(Guid id)
        {
            var stu = db.StuffPaymentDetails.Where(a => a.StuffId.Equals(id)).FirstOrDefault();
            TempData["StuffId"] = id;
            ViewBag.Designition = new SelectList(db.Designition, "DestId", "DesName");
           
            return View(stu);
        }

        [HttpPost]
        public ActionResult stuffSalaryUpdate(StuffPaymentDetails st)
        {
            db.Entry(st).State = EntityState.Modified;
            db.SaveChanges();
            TempData["saved"] = "Data successfully updated";
            return RedirectToAction("Index");
        }

        public ActionResult EmployeeSalary(string Search_Data, string Department, string Designition)
        {
            ViewBag.advancedsearch = "advancedsearch";
            ViewBag.Department = new SelectList(db.StuffDepartment, "DepartmentId", "DepartmentName");

           

            var stuff = from stt in db.Stuff
                        join stuffPay in db.StuffPaymentDetails on stt.Id equals stuffPay.StuffId
                        join des in db.Designition on stuffPay.DesignitionId equals des.DestId
                        join stuffDep in db.StuffDepartment on stt.Department equals stuffDep.DepartmentId
                        
                        orderby (stt.FirstName)
                        select new stuffFullDetails { Stuff = stt, StuffPaymentDetails = stuffPay, StuffDepartment = stuffDep, Designition = des };

            if (!String.IsNullOrEmpty(Search_Data))
            {
                stuff = from stt in db.Stuff
                        join stuffPay in db.StuffPaymentDetails on stt.Id equals stuffPay.StuffId
                        join des in db.Designition on stuffPay.DesignitionId equals des.DestId
                        join stuffDep in db.StuffDepartment on stt.Department equals stuffDep.DepartmentId
                        where ((stt.ContactNo + " ").ToLower().Contains(Search_Data.ToLower()))
                               || (stt.FirstName + " " + stt.LastName + " ").ToLower().Contains(Search_Data.ToLower())
                                              || (stt.Email + " ").ToLower().Contains(Search_Data.ToLower())
                        orderby (stt.FirstName)
                        select new stuffFullDetails { Stuff = stt, StuffPaymentDetails = stuffPay, StuffDepartment = stuffDep, Designition = des };
            }

            else if (!String.IsNullOrEmpty(Department) && String.IsNullOrEmpty(Designition))
            {
                int p = Convert.ToInt16(Department);
                stuff = from stt in db.Stuff
                        join stuffPay in db.StuffPaymentDetails on stt.Id equals stuffPay.StuffId
                        join des in db.Designition on stuffPay.DesignitionId equals des.DestId
                        join stuffDep in db.StuffDepartment on stt.Department equals stuffDep.DepartmentId
                        where (stt.Department.Equals(p))
                        orderby (stt.FirstName)
                        select new stuffFullDetails { Stuff = stt, StuffPaymentDetails = stuffPay, StuffDepartment = stuffDep, Designition = des };
            }
            else if (!String.IsNullOrEmpty(Designition) && !String.IsNullOrEmpty(Department))
            {
                int p = Convert.ToInt16(Designition);
                stuff = from stt in db.Stuff
                        join stuffPay in db.StuffPaymentDetails on stt.Id equals stuffPay.StuffId
                        join des in db.Designition on stuffPay.DesignitionId equals des.DestId
                        join stuffDep in db.StuffDepartment on stt.Department equals stuffDep.DepartmentId
                        where (stuffPay.DesignitionId.Equals(p))
                        orderby (stt.FirstName)
                        select new stuffFullDetails { Stuff = stt, StuffPaymentDetails = stuffPay, StuffDepartment = stuffDep, Designition = des };
            }

            
            return View(stuff.ToList());
        }
        
        [HttpGet]
        public ActionResult Salary(Guid id)
        {
            ViewBag.advancedsearch = "advancedsearch";
            StuffSalary model = new StuffSalary();
            TempData["Empname"] = db.Stuff.Where(x => x.Id.Equals(id)).Select(x => x.FirstName + x.LastName).FirstOrDefault();
        
            TempData["Email"] = db.Stuff.Where(x => x.Id.Equals(id)).Select(x => x.Email).FirstOrDefault();
            TempData["mobile"] = db.Stuff.Where(x => x.Id.Equals(id)).Select(x => x.ContactNo).FirstOrDefault();
            TempData["amount"] = db.StuffPaymentDetails.Where(x => x.StuffId.Equals(id)).Select(x => x.TotalSalry).FirstOrDefault();
            ViewBag.Bank = new SelectList(dbpt.Bank, "Id", "BankName");
            model.StuffId = id;            
            return View(model);
        }
        [HttpPost]
        public ActionResult Salary(StuffSalary model)
        {
            ViewBag.advancedsearch = "advancedsearch";

            if(ModelState.IsValid)
            {
                model.Id = Guid.NewGuid();
                model.StuffId = model.StuffId;
                model.payDate = DateTime.Now.Date;
                model.bankPay = model.bankPay??0;
                model.cashPay = model.cashPay??0;
                model.Due = model.Due;
                model.Remarks = model.Remarks;
                model.BankId = model.BankId;
                model.EmpId = Guid.Empty;
                db.StuffSalary.Add(model);
                db.SaveChanges();
            }
            return RedirectToAction("StuffSalaryRpt", new { Id = model.Id});
        }
        public ActionResult StuffSalaryRpt(Guid Id)
        {
            empsalary es = new empsalary();
            string userid = User.Identity.Name;
            int schoolid = uc.UserProfiles.Where(x => x.UserName.Equals(userid)).Select(x => x.SchoolId??0).SingleOrDefault();
            string schoolname = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.SchoolName).SingleOrDefault();
            string address = evc.SchoolSetup.Where(x => x.Id.Equals(schoolid)).Select(x => x.Address + " " + x.City + " " + x.Country).SingleOrDefault();
            var s = db.StuffSalary.Where(x => x.Id.Equals(Id)).Select(x => x.StuffId).SingleOrDefault();
            var stuffsalary =   from ss in db.StuffSalary
                                join st in db.Stuff on ss.StuffId equals st.Id
                                join sd in db.StuffDepartment on st.Department equals sd.DepartmentId
                                join sp in db.StuffPaymentDetails on ss.StuffId equals sp.StuffId
                                join d in db.Designition on sp.DesignitionId equals d.DepatmentId
                                where ss.Id==Id 
                                select new empsalary
                                {
                                    Date = ss.payDate,
                                    empname=st.FirstName+" "+st.LastName,
                                    email=st.Email,
                                    mobiel=st.ContactNo,
                                    department = sd.DepartmentName,
                                    designation=d.DesName,
                                    basicsalary = sp.BasicSalary,
                                    houserent = sp.HouseRent,
                                    health = sp.Health,
                                    other = sp.Others,
                                    total = sp.TotalSalry,
                                    cashpay = ss.cashPay??0,
                                    bankpay=ss.bankPay??0,
                                    Due = ss.Due??0
                                   
                                };
            es = stuffsalary.FirstOrDefault();
          
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Report"), "RptEmployeeSalary.rpt"));
            rd.SetDataSource(stuffsalary);
            rd.SetParameterValue("schoolname", schoolname.ToString());
            rd.SetParameterValue("address", address.ToString());

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public ActionResult DeptList()
        {
            ViewBag.advancedsearch = "advancedsearch";
            
            return View(db.StuffDepartment.ToList());
        }

        public ActionResult CreateDept()
        {

            return View();
        }

        [HttpPost]
        public ActionResult CreateDept(StuffDepartment model)
        {
            if (ModelState.IsValid)
            {
                db.StuffDepartment.Add(model);
                db.SaveChanges();
                return RedirectToAction("DeptList");
            }
            return View();
        }

        public ActionResult DeptEdit(int id)
        {
            ViewBag.Department = new SelectList(db.StuffDepartment, "DepartmentId", "DepartmentName");
            var Dept = db.StuffDepartment.Where(a => a.DepartmentId == id).FirstOrDefault();
            return View(Dept);
        }

        [HttpPost]
        public ActionResult DeptEdit(StuffDepartment model)
        {
            if (ModelState.IsValid)
            {
                var Dept = db.StuffDepartment.Where(a => a.DepartmentId == model.DepartmentId).FirstOrDefault();
                Dept.DepartmentName = model.DepartmentName;
                db.Entry(Dept).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("DeptList");
            }
            return View();
        }

        public ActionResult DeptDelete(int id)
        {
            var Dept = db.StuffDepartment.Where(a => a.DepartmentId == id).FirstOrDefault();
            db.StuffDepartment.Remove(Dept);
            db.SaveChanges();
            return RedirectToAction("DeptList");
        }

        public ActionResult DesignationList()
        {
            ViewBag.advancedsearch = "advancedsearch";
            ViewBag.Department = new SelectList(db.StuffDepartment, "DepartmentId", "DepartmentName");
            var designation = (from d in db.Designition.ToList()
                               join dpt in db.StuffDepartment.ToList() on d.DepatmentId equals dpt.DepartmentId
                               where d.DepatmentId== dpt.DepartmentId
                               select new stuffFullDetails { StuffDepartment = dpt, Designition = d }).ToList();
            return View(designation.ToList());
        }

        public ActionResult CreateDesignation()
        {
            
            return View();
        }

        [HttpPost]
        public ActionResult CreateDesignation(Designition model)
        {
            if (ModelState.IsValid)
            {
                db.Designition.Add(model);
                db.SaveChanges();
                return RedirectToAction("DesignationList");
            }
            return View();
        }

        public ActionResult DesignationEdit(int id)
        {
            ViewBag.Department = new SelectList(db.StuffDepartment, "DepartmentId", "DepartmentName");
            var designation = db.Designition.Where(a => a.DestId == id).FirstOrDefault();
            return View(designation);
        }

        [HttpPost]
        public ActionResult DesignationEdit(Designition model)
        {
            if (ModelState.IsValid)
            {
                var section = db.Designition.Where(a => a.DestId == model.DestId).FirstOrDefault();
                section.DesName = model.DesName;
                section.DepatmentId = model.DepatmentId;
                db.Entry(section).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("DesignationList");
            }
            return View();
        }

        public ActionResult DesignationDelete(int id)
        {
            var section = db.Designition.Where(a => a.DestId == id).FirstOrDefault();
            db.Designition.Remove(section);
            db.SaveChanges();
            return RedirectToAction("DesignationList");
        }

        public void SaveTxtData(string data)
        {
            string filePath = Server.MapPath("~/App_Data/StudentUser.txt"); // Path to your desired text file
            string existingContent = System.IO.File.ReadAllText(filePath);
            string currdata = existingContent + Environment.NewLine + data;
            // Write the data to the text file
            System.IO.File.WriteAllText(filePath, currdata);
        }
    }
}