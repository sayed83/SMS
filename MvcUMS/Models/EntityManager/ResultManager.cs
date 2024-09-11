using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Web;
using VIT.DataLogicLayer;

namespace MvcUMS.Models.EntityManager
{
    public class ResultManager
    {
        ExamContext _xm = new ExamContext();

        EventContext evntCtx = new EventContext();
        //ExamContext xmCtx = new ExamContext();
        UMSEntities1 sdbs = new UMSEntities1();
        static ExamContext ec = new ExamContext();
        StudentContext sc = new StudentContext();


        public List<SubjectMarksCourse> GetStudentResult(string StudentId, string Semester, int Examtypeid)
        {
            var shm = ec.SubjectHighestMarks.Where(x => x.ExamId.Equals(Examtypeid)).ToList();
            var totallist = (from ca in ec.StudentSubjectMarks.ToList()
                             join c in sc.Course.ToList() on ca.SubjectCode equals c.Id
                             join ex in shm.ToList() on ca.SubjectCode equals ex.SubjectCode
                             orderby c.Id
                             where ca.StudentId.Equals(StudentId) && ca.ExamName.Equals(Examtypeid)
                             select new SubjectMarksCourse
                             {
                                 ssm = ca,
                                 c = c,
                                 CMS = (from cm in _xm.CourseMarkSetup.ToList()
                                        join mt in _xm.MarkType on cm.MarkTypeId equals mt.Id
                                        where cm.CourseId == c.Id
                                        select new CourseMarkTypeSetupViewModel
                                        {
                                            Id = cm.Id,
                                            CourseId = c.CourseId,
                                            MarkTypeId = cm.MarkTypeId,
                                            MarkType = mt.Name,
                                            PassMarks = cm.PassMarks,
                                            TotalMarks = cm.TotalMarks
                                        }).ToList(),
                                 highestmarks = ex.HM,
                                 hightgrade = ex.HGP,
                                 TotalFullMarks = (from t in sc.Course.ToList()
                                                   where t.Semester.Equals(Semester)
                                                   select t.FullMarks).Sum(),
                                 TotalObMarks = (from m in ec.StudentSubjectMarks.ToList()
                                                 where m.StudentId.Equals(StudentId) && m.ExamName.Equals(Examtypeid)
                                                 select m.TotalMarks).Sum(),
                                 GPA = (from m in ec.StudentSubjectMarks.ToList()
                                        where m.StudentId.Equals(StudentId) && m.ExamName.Equals(Examtypeid)
                                        select m.GradePoint).Sum()


                             }).OrderByDescending(x => x.TotalObMarks).ToList();

            return totallist;

        }


        private List<ResultReport> ExamMarks(string stdId, int Examtypeid)
        {
            var shm = ec.SubjectHighestMarks.Where(x => x.ExamId.Equals(Examtypeid)).ToList();
            var totallist = from ca in ec.StudentSubjectMarks.ToList()
                            where ca.ExamName.Equals(Examtypeid)
                            join ex in shm.ToList() on ca.SubjectCode equals ex.SubjectCode
                            join c in sc.Course.ToList() on ex.SubjectCode equals c.Id
                            orderby ca.SubjectCode
                            where ca.StudentId.Equals(stdId) && ca.ExamName.Equals(Examtypeid)
                            select new ResultReport
                            {
                                SubjectCode = c.CourseId,
                                SubjectTitle = c.CourseTitle,
                                fullmarks = c.FullMarks,
                                passmarks = c.MinimumPassMarks,
                                classtest = ca.ClassTest,
                                subjective = ca.Written,
                                objective = ca.MCQ,
                                practical = ca.Practical,
                                totalmarks = ca.TotalMarks,
                                gradepoint = ca.GradePoint,
                                lettergrade = ca.LetterGrade,
                                hightestmarks = ex.HM
                            };
            return totallist.ToList();
        }

        public List<ResultReport> GetStudentFullResult(string StudentId, string Semester, int Examtypeid)
        {
            var HyExammarklist = ExamMarks(StudentId, 1).ToList();
            var FExammarklist = ExamMarks(StudentId, 2).ToList();
            var allExamMarks = from fe in FExammarklist.ToList()
                               join hy in HyExammarklist.ToList() on fe.SubjectCode equals hy.SubjectCode
                               select new ResultReport
                               {
                                   SubjectCode = hy.SubjectCode,
                                   SubjectTitle = hy.SubjectTitle,
                                   fullmarks = hy.fullmarks,
                                   passmarks = hy.passmarks,
                                   classtest = hy.classtest,
                                   subjective = hy.subjective,
                                   objective = hy.objective,
                                   practical = hy.practical,
                                   totalmarks = hy.totalmarks,
                                   gradepoint = hy.gradepoint,
                                   lettergrade = hy.lettergrade,
                                   hightestmarks = hy.hightestmarks,
                                   HYTotalObMarks = (from m in HyExammarklist.ToList()
                                                     select m.totalmarks).Sum(),

                                   fclasstest = fe.classtest,
                                   fsubjective = fe.subjective,
                                   fobjective = fe.objective,
                                   fpractical = fe.practical,
                                   ftotalmarks = fe.totalmarks,
                                   fgradepoint = fe.gradepoint,
                                   flettergrade = fe.lettergrade,
                                   fhightestmarks = fe.hightestmarks,
                                   ANTotalObMarks = (from m in FExammarklist.ToList()
                                                     select m.totalmarks).Sum()
                               };

            return allExamMarks.ToList();


        }

        public static string GPAmark(decimal Marks)
        {
            //string s = string.Empty;
            //var m = ec.GPAType.ToList();
            //foreach (var i in m)
            //{
            //    if (Marks >= i.Startmark && Marks <= i.endmark)
            //    {
            //        s = i.GPA;
            //    }
            //    else if (Marks < 33)
            //    {
            //        s = "F";
            //    }
            //}

            //return s;

            string s;
            if (Marks >= 80 && Marks <= 100)
            {
                s = "A+";
            }
            else if (Marks >= 70 && Marks < 80)
            {
                s = "A";
            }
            else if (Marks >= 60 && Marks < 70)
            {
                s = "A-";
            }
            else if (Marks >= 50 && Marks < 60)
            {
                s = "B";
            }
            else if (Marks >= 40 && Marks < 50)
            {
                s = "C";
            }
            else if (Marks >= 33 && Marks < 40)
            {
                s = "D";
            }
            else
            {
                s = "F";
            }
            return s;
        }

        public static string GPA(decimal Marks)
        {
            string s;
            if (Marks == 5)
            {
                s = "A+";
            }
            else if (Marks >= 4 && Marks < 5)
            {
                s = "A";
            }
            else if (Marks >= 3.5m && Marks < 4)
            {
                s = "A-";
            }
            else if (Marks >= 3 && Marks < 3.5m)
            {
                s = "B";
            }
            else if (Marks >= 2 && Marks < 3)
            {
                s = "C";
            }
            else if (Marks >= 1 && Marks < 2)
            {
                s = "D";
            }
            else
            {
                s = "F";
            }
            return s;
        }


        public static decimal getGP(decimal Marks)
        {
            //decimal s = 0;
            //var m = ec.GPAType.ToList();
            //foreach (var i in m)
            //{
            //    if (Marks >= i.Startmark && Marks <= i.endmark)
            //    {
            //        s = i.GradePoint;
            //    }
            //}

            //return s;

            decimal s;
            if (Marks >= 80 && Marks <= 100)
            {
                s = 5;
            }
            else if (Marks >= 70 && Marks < 80)
            {
                s = 4;
            }
            else if (Marks >= 60 && Marks < 70)
            {
                s = 3.5m;
            }
            else if (Marks >= 50 && Marks < 60)
            {
                s = 3;
            }
            else if (Marks >= 40 && Marks < 50)
            {
                s = 2;
            }
            else if (Marks >= 33 && Marks < 40)
            {
                s = 1;
            }
            else
            {
                s =0;
            }
            return s;
        }




        public DataTable GetStudentMark(int CourseId, string Semester)
        {
            ArrayList altParams = new ArrayList();
            altParams.Add(new SqlParameter("@CourseId", CourseId));
            altParams.Add(new SqlParameter("@strSmt", Semester));
            return DatabaseManager.GetInstance().ExecuteStoredProcedureDataTable("StudentMarkInput", altParams);
        }



        public List<Dictionary<string, object>> Read(DbDataReader reader)
        {
            List<Dictionary<string, object>> expandolist = new List<Dictionary<string, object>>();
            foreach (var item in reader)
            {
                IDictionary<string, object> expando = new ExpandoObject();
                foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(item))
                {
                    var obj = propertyDescriptor.GetValue(item);
                    expando.Add(propertyDescriptor.Name, obj);
                }
                expandolist.Add(new Dictionary<string, object>(expando));
            }
            return expandolist;
        }


        public DataSet makePurchaseTable(int CourseId, string Semester)
        {
            DataSet ds = new DataSet();

            ds.DataSetName = "dsMarkInput";
            DataTable dt = GetStudentMark(CourseId, Semester);

            dt.TableName = "tblMarkInput";
            if (dt.Rows.Count > 0)
                ds.Tables.Add(dt);

            return ds;
        }





        public int InsertUpdatePurchase(DataSet PurchaseData, int CourseId, int ExamId)
        {

            string Data = PurchaseData.GetXml();
            Data = Data.Replace("'", "''");
            Data = Data.Replace("T00:00:00+06:00", "");
            Data = Data.Replace("T00:00:00+07:00", "");
            Data = Data.Replace("T18:00:00+06:00", "");
            Data = Data.Replace("T05:00:00+06:00", "");
            Data = Data.Replace("T05:00:00+06:00", "");
            Data = Data.Replace("T06:00:00+06:00", "");
            Data = Data.Replace("T05:00:00+06:00", "");
            Data = Data.Replace("T04:00:00+06:00", "");

            return Insert(Data, CourseId, ExamId);
        }


        public int Insert(string PurchaseData, int CourseId, int ExamId)
        {
            int success = 0;
            ArrayList altParams = new ArrayList();
            altParams.Add(new SqlParameter("@Purchasexml", PurchaseData));
            altParams.Add(new SqlParameter("@CourseId", CourseId));
            altParams.Add(new SqlParameter("@ExamId", ExamId));
            DataTable dt = DatabaseManager.GetInstance().ExecuteStoredProcedureDataTable("Inv_sp_PurchaseBulkInsert", altParams);
            if (dt.Rows.Count > 0)
            {
                success = Convert.ToInt32(dt.Rows[0][0].ToString());
            }
            return success;
        }





        public void AddMarks(MarkFormViewModel model)
        {

            var exits = _xm.StudentObtainMarks.Where(s => s.CourseId == model.CourseId
                && s.StudentId == model.StudentId
                && s.ExamId == model.ExamId).FirstOrDefault();

            var existMarks = exits.MarksDetails.Where(x => x.StudentObtainMarksId == exits.Id).FirstOrDefault();

            if (exits != null)
            {
                foreach (var item in model.MarkDetails)
                {
                    existMarks.Marks = item.Marks;
                    exits.TotalMarks += item.Marks;
                    _xm.Entry(exits).State = EntityState.Modified;
                    _xm.Entry(existMarks).State = EntityState.Modified;
                }

                _xm.SaveChanges();
            }

            StudentObtainMarks SOM = new StudentObtainMarks();
            SOM.CourseId = model.CourseId;
            SOM.StudentId = model.StudentId;
            SOM.ExamId = model.ExamId;
            _xm.StudentObtainMarks.Add(SOM);
            _xm.SaveChanges();

            foreach (var item in model.MarkDetails)
            {
                StudentObtainMarksDetails SOMD = new StudentObtainMarksDetails();
                SOMD.MarkTypeId = item.MarkTypeId;
                SOMD.Marks = item.Marks;
                SOMD.StudentObtainMarksId = SOM.Id;
                SOM.TotalMarks += item.Marks;
                _xm.StudentObtainMarksDetails.Add(SOMD);
                _xm.Entry(SOM).State = EntityState.Modified;

            }

            _xm.SaveChanges();
        }
    }
}