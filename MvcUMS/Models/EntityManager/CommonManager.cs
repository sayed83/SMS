using MvcUMS.Models.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace MvcUMS.Models.EntityManager
{
    public class CommonManager
    {
        EventContext evntCtx = new EventContext();
        ExamContext xmCtx = new ExamContext();
        UMSEntities1 sdbs = new UMSEntities1();
        ExamContext ec = new ExamContext();
        StudentContext sc = new StudentContext();

        public SchoolViewModel GetSchoolInfo()
        {
            var data = (from s in evntCtx.SchoolSetup.ToList()
                        select new SchoolViewModel
                        {
                            Id=s.Id,
                            SchoolName=s.SchoolName,
                            Address=s.Address,
                            City=s.City,
                            Logo=s.Logo
                        }).FirstOrDefault();

            return data;
        }

        public List<GPAViewModel> GetGPAInfo()
        {
            var data = (from s in xmCtx.GPAType.ToList()
                        select new GPAViewModel
                        {
                            Range=Convert.ToInt32(s.Startmark) + "-" + Convert.ToInt32(s.endmark),
                            Grade = s.GradePoint,
                            GPA=s.GPA
                        }).ToList();

            return data;
        }

        public StudentDetailsForMarksheetViewModel GetStudentInfo(string StudentId)
        {
            var stdList = (from s in sdbs.Student_info.ToList()
                           join c in sc.CurrentAcademicInfo.ToList() on s.Std_id equals c.Std_id
                           join sm in ec.StudentSubjectMarks.ToList() on s.Std_id equals sm.StudentId
                           join xm in ec.ExamType.ToList() on sm.ExamName equals xm.Id
                           where s.Std_id.Equals(StudentId)
                           select new StudentDetailsForMarksheetViewModel
                           {
                               Name=s.Std_FName + " " + s.Std_MName + " " + s.Std_LName,
                               FatherName=s.Father_Name,
                               MotherName=s.Mother_Nmae,
                               Class=c.Semester,
                               Section=c.Section,
                               Roll=c.RollNo,
                               StdId=s.Std_id,
                               Photo=s.Picture,
                               Group=c.Department,
                               Exam=xm.Examtype,
                               Session=c.AcademicYear
                           }).FirstOrDefault();

            return stdList;
        }

        public RootObject SmsSend(string sender, string to, string message)
        {
            using (var http = new HttpClient())
            {
                // Define authorization headers here, if any
                http.DefaultRequestHeaders.Add("Authorization", "Basic Z2xvYmFsc29mdDpnbG9iYWxzb2Z0dGVjaG5vbG9neQ==");

                var data = new RootObject
                {
                    from = sender,
                    to = to,
                    text = message
                };

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var request = http.PostAsync("http://107.20.199.106/restapi/sms/1/text/single", content);

                var response = request.Result.Content.ReadAsStringAsync().Result;

                return JsonConvert.DeserializeObject<RootObject>(response);
            }
        }
       
    }
}