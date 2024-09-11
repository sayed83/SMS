using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using MvcEnergyPac.Filters;
using MvcEnergyPac.Models;
using MvcUMS.Models;
using MvcUMS.Filters;
using System.Data;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net;

namespace MvcEnergyPac.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login
        private StuffContext db = new StuffContext();
        private UsersContext _usrContext = new UsersContext();
        EventContext sch = new EventContext();

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                var userid = WebSecurity.GetUserId(model.UserName);
                var schoolid = _usrContext.UserProfiles.Where(u => u.UserId == userid).FirstOrDefault().SchoolId;
                var roleid = _usrContext.webpages_UsersInRoles.Where(r => r.UserId == userid).FirstOrDefault().RoleId;
                Session["userId"] = model.UserName;
                Session["Password"] = model.Password;
                Session["SchoolId"] = schoolid;
                var sid = Session["SchoolId"];
                Session["RoleId"] = roleid;
                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("FrontPage", "Home");

        }

        //GET: /Account/Register

        //[AuthorizeRoles("Admin")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult Register()
        {
            var roles = from c in _usrContext.webpages_Roles select c;
            ViewBag.userRole = new SelectList(roles, "RoleId", "RoleName");
            ViewBag.SchoolId = new SelectList(sch.SchoolSetup.ToList(), "Id", "SchoolName");
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        //[AuthorizeRoles("Admin")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
                    var userdata = $"User Name: {model.UserName} Pass: {model.Password}";
                    //SaveTxtData(userdata);
                    webpages_UsersInRoles usrRoles = new webpages_UsersInRoles();
                    ManagementProfile mp = new ManagementProfile();
                    UserProfile up = new UserProfile();
                    var userdetails = _usrContext.UserProfiles.Where(a => a.UserName.Equals(model.UserName)).FirstOrDefault();
                    userdetails.SchoolId = model.SchoolId;
                    _usrContext.Entry(userdetails).State = EntityState.Modified;

                    usrRoles.RoleId = model.role;
                    usrRoles.UserId = userdetails.UserId;
                    _usrContext.webpages_UsersInRoles.Add(usrRoles);

                    //management profile
                    //if (model.role == 5)
                    //{
                    //    mp.Designation = model.Designation;
                    //    mp.Name = model.Name;
                    //    mp.Phone = model.Phone;
                    //    mp.UserId = _usrContext.UserProfiles.Max(x => x.UserId);
                    //    _usrContext.ManagementProfile.Add(mp);

                    //}

                    //var gaurdian = new GuardianProfile();
                    //gaurdian.UserId = userdetails.UserId;
                    //gaurdian.UserName = model.UserName;
                    //_usrContext.GuardianProfile.Add(gaurdian);
                    _usrContext.SaveChanges();

                    return RedirectToAction("management_list", "account");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return RedirectToAction("Register");
        }

        public ActionResult Management_List()
        {
            ViewBag.basicdatatable = "basicdatatable";
            return View(_usrContext.ManagementProfile.ToList());
        }

        public ActionResult Management_Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Management_Create(ManagementProfile model)
        {
            if (ModelState.IsValid)
            {
                string filename = "";

                if (model.image != null && model.image.ContentLength > 0)
                {
                    filename = Path.GetFileName(Guid.NewGuid() + "." + model.image.FileName.Split('.')[1]);
                    string targetPath = Server.MapPath("../uploads//" + filename);
                    Stream strm = model.image.InputStream;
                    var targetFile = targetPath;

                    GenerateThumbnails(0.5, strm, targetFile);
                }
                model.Photo = filename;
                _usrContext.ManagementProfile.Add(model);
                _usrContext.SaveChanges();
                return RedirectToAction("Management_List");
            }
            return View();
        }

        public ActionResult ManagementDetails(int id)
        {
            var details = _usrContext.ManagementProfile.Where(s => s.Id.Equals(id)).FirstOrDefault();

            return View(details);
        }

        public ActionResult Management_Edit(int id)
        {
            var managmnt = _usrContext.ManagementProfile.Where(s => s.Id == id).FirstOrDefault();
            return View(managmnt);
        }

        [HttpPost]
        public ActionResult Management_Edit(ManagementProfile models)
        {
            if (ModelState.IsValid)
            {

                if (models.image != null && models.image.ContentLength > 0)
                {
                    string filename = "";
                    filename = Path.GetFileName(Guid.NewGuid() + "." + models.image.FileName.Split('.')[1]);
                    models.Photo = filename;
                    string targetPath = Server.MapPath("~/uploads//" + filename);
                    Stream strm = models.image.InputStream;
                    var targetFile = targetPath;

                    GenerateThumbnails(0.5, strm, targetFile);
                }

                var managmnt = _usrContext.ManagementProfile.Where(s => s.Id == models.Id).FirstOrDefault();
                managmnt.Name = models.Name;
                managmnt.Designation = models.Designation;
                managmnt.Phone = models.Phone;
                managmnt.Photo = models.Photo;
                _usrContext.Entry(managmnt).State = EntityState.Modified;
                _usrContext.SaveChanges();
                return RedirectToAction("Management_List");
            }

            return View();
        }

        public ActionResult Management_Delete(int id)
        {
            var managmnt = _usrContext.ManagementProfile.Where(s => s.Id == id).FirstOrDefault();

            _usrContext.ManagementProfile.Remove(managmnt);

            _usrContext.SaveChanges();
            return RedirectToAction("Management_List");
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

        [HttpGet]
        public ActionResult UserCreation()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UserCreation(string st)
        {
            if (ModelState.IsValid)
            {
                User_Registration user = new User_Registration();

                //    user. = true;
                //    stuff.Image = imagename;
                //    stuff.MemberCode = "emp" + "00" + stuff.Id;
                //    db.Stuff.Add(stuff);
                //    db.SaveChanges();
                //    return RedirectToAction("Index");
                //}
                //return View(stuff);

            }
            return View();
        }

        ////
        //// POST: /Account/Disassociate

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Disassociate(string provider, string providerUserId)
        //{
        //    string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
        //    ManageMessageId? message = null;

        //    // Only disassociate the account if the currently logged in user is the owner
        //    if (ownerAccount == User.Identity.Name)
        //    {
        //        // Use a transaction to prevent the user from deleting their last login credential
        //        using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
        //        {
        //            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
        //            if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
        //            {
        //                OAuthWebSecurity.DeleteAccount(provider, providerUserId);
        //                scope.Complete();
        //                message = ManageMessageId.RemoveLoginSuccess;
        //            }
        //        }
        //    }

        //    return RedirectToAction("Manage", new { Message = message });
        //}

        ////
        //// GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                //: message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }


            // If we got this far, something failed, redisplay form
            return View(model);
        }


        // GET: /AccountAdmin/ResetPassword
        public ActionResult ResetPassword(string id)
        {
            string username = "";


            //string testMe = "not a guid";
            if (GuidEx.IsGuid(id))
            {
                Guid stdid = new Guid(id);
                username = db.Stuff.Where(x => x.Id == stdid).Select(x => x.Email).FirstOrDefault();
            }
            string role = "";
            role = (from u in _usrContext.UserProfiles
                    join r in _usrContext.webpages_UsersInRoles on u.UserId equals r.UserId
                    join ru in _usrContext.webpages_Roles on r.RoleId equals ru.RoleId
                    where u.UserName == username
                    select ru.RoleName).FirstOrDefault();


            if (role == "Teacher")
            {
                id = username;
            }

            if (id == null)
            {
                ViewBag.ErorMessage = "Something went wrong!";
                return Redirect(Request.UrlReferrer.PathAndQuery);

            }
            ResetPasswordViewModel model = new ResetPasswordViewModel() { Id = id };
            return View(model);
        }

        //
        // POST: /AccountAdmin/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string username = "";
            if (GuidEx.IsGuid(model.Id))
            {
                Guid stdid = new Guid(model.Id);
                username = db.Stuff.Where(x => x.Id == stdid).Select(x => x.Email).FirstOrDefault();
            }
            string role = "";
            role = (from u in _usrContext.UserProfiles
                    join r in _usrContext.webpages_UsersInRoles on u.UserId equals r.UserId
                    join ru in _usrContext.webpages_Roles on r.RoleId equals ru.RoleId
                    where u.UserName == username
                    select ru.RoleName).FirstOrDefault();


            if (role == "Teacher")
            {
                model.Id = username;
            }


            if (model.Id == null)
            {
                ViewBag.ErorMessage = "Something went wrong!";

            }

            var token = WebSecurity.GeneratePasswordResetToken(model.Id);

            var resetPassword = WebSecurity.ResetPassword(token, model.NewPassword);
            if (resetPassword)
            {
                ViewBag.Message = "Successfully Changed";
            }
            else
            {
                ViewBag.Message = "Something went horribly wrong!";
            }

            return View();
        }



        #region Helpers

        public static class GuidEx
        {
            public static bool IsGuid(string value)
            {
                Guid x;
                return Guid.TryParse(value, out x);
            }
        }
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion

        public JsonResult GetData(string email)
        {
            var naid = "";
            var s = "";


            naid = db.Stuff.Where(a => a.Email.Equals(email)).Select(x => x.Email).SingleOrDefault();
            if (naid == email)
            {
                s = "1";
            }

            return new JsonResult { Data = s, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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
