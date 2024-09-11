using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Twilio;
using MvcUMS.Models;
using System.Net;
using RestSharp;
using RestSharp.Authenticators;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;

//using ASPSnippets.SmsAPI;


namespace MvcUMS.Controllers
{
    public class SMSController : Controller
    {
        ExamContext ec = new ExamContext();
        StudentContext sc = new StudentContext();
        UMSEntities1 sdbs = new UMSEntities1();
        //
        // GET: /SMS/

        public ActionResult Index()
        {
            return View();
        }

      

        public ActionResult SendSms(string id, string contactno)
        {
            var model = new SMS();
            TempData["studentid"] = id;
            model.StudentId = id;
            model.phone = contactno;
            return View(model);
        }

        [HttpPost]
        public ActionResult SendSms(SMS model, string[] ids)
        {

            string mobilenumber = "+88" + Convert.ToString(model.phone);

            string sender = "TSTSMS";
            Result results = SmsResult(sender, mobilenumber, model.Text);
            if (results != null)
            {
                TempData["success"] = "Message Send successfully.";
            }


            ModelState.Clear();
            return View();
        }

        public Result SmsResult(string msgsender, string destinationaddr, string message)
        {
            string url;
            string serverResult = "";
            //long l;
            //int resultvaule = 1;
            Result result;
            try
            {

                string apikey = "3591FF7C93435B";
                //string username = "swopnil.it";
                //string password = "swopnil.it";

                url = "http://xsms.dianahost.com/app/smsapi/index.php?"
                + "key=" + HttpUtility.UrlEncode(apikey) + "&routeid=100015" + "&type=text"
                + "&contacts=" + HttpUtility.UrlEncode(destinationaddr)
                + "&senderid=" + HttpUtility.UrlEncode(msgsender)
                + "&msg=" + HttpUtility.UrlEncode(message);


                //if (long.TryParse(msgsender, out l))
                //{
                //    url = url + "&sourceAddr=" + msgsender;
                //}
                //else
                //{
                //    url = url + "&fromAlpha=" + msgsender;
                //}

                serverResult = DownloadString(url);
            }
            catch (Exception ex)
            {

            }


            result = ParseServerResult(serverResult);


            return result;
        }


        private string DownloadString(string URL)
        {
            using (System.Net.WebClient wlc = new System.Net.WebClient())
            {
                // Create WebClient instanse.
                try
                {
                    // Download and return the xml response
                    return wlc.DownloadString(URL);
                }
                catch (WebException ex)
                {
                    // Failed to connect to server. Throw an exception with a customized text.
                    throw new WebException("Error occurred while connecting to server. " + ex.Message, ex);
                }
            }
        }

        private Result ParseServerResult(string ServerResult)
        {
            System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
            System.Xml.XmlNode ack;
            Result result = new Result();
            //xDoc.LoadXml(ServerResult);
            //ack = xDoc.GetElementsByTagName("ack")[0];
            //result.errorcode = int.Parse(ack.Attributes["errorcode"].Value);
            //result.errormessage = ack.InnerText;
            //result.success = (result.errorcode == 0);
            return result;
        }

        //private string ParseServerResult(string ServerResult)
        //{
        //    string result;
        //    string apikey = "3591FF7C93435B";
        //    string shootkey =Convert.ToString(ServerResult.Split('/')[1]);
        //    string url = "http://isms.dianahost.com/app/miscapi/" + apikey + "/getDLR/" + shootkey;
        //    result = DownloadString(url);
        //    //System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
        //    //System.Xml.XmlNode ack;
        //    //Result result = new Result();
        //    //xDoc.LoadXml(ServerResult);
        //    //ack = xDoc.GetElementsByTagName("ack")[0];
        //    //result.errorcode = int.Parse(ack.Attributes["errorcode"].Value);
        //    //result.errormessage = ack.InnerText;
        //    //result.success = (result.errorcode == 0);
        //    return result;
        //}

        public ActionResult ExamReportSend(string id)
        {
            var model = new SMS();
            //var dc = new MvcUMS.Models.UMSSearchEntities();
            //var s = sdbs.Student_info.Where(x => x.Std_id.Equals(id)).Select(x => x.Contact_no).FirstOrDefault();
            var totallist = from ca in ec.SubjectMarks.ToList()
                            join c in sc.Course.ToList() on ca.SubjectCode equals c.CourseId
                            orderby c.CourseId
                            where ca.StudentId.Equals(id)
                            select new
                            {
                                subject = c.CourseTitle,
                                Mark = ca.Attendence + ca.ClassTest + ca.MidTerm + ca.FinalMark,
                                GPA = ca.GPA

                            };
            var text = string.Empty;
            foreach (var i in totallist)
            {
                text += i.subject + " " + i.Mark + " " + i.GPA + " ";
            }
            model.phone = sdbs.Student_info.Where(x => x.Std_id.Equals(id)).Select(x => x.Contact_no).FirstOrDefault();
            model.Text = text;

            string AccountSid = "AC7d5461f5f05cacc5c52f1395d68e037f";
            string AuthToken = "da8ed4385d0fa236f8c73ff738e7b601";
            var twilio = new TwilioRestClient(AccountSid, AuthToken);

            twilio.SendMessage("(201) 500-3230 ", "01823837373", text);


            return View();
        }
    }
}
