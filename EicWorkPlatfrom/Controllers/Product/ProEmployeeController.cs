﻿using Lm.Eic.App.Business.Bmp.Hrm.Archives;
using Lm.Eic.App.Business.Bmp.Hrm.WorkOverHours;
using Lm.Eic.App.Business.Bmp.Pms.LeaveAsk;
using Lm.Eic.App.Business.Mes.Optical.Authen;
using Lm.Eic.App.DomainModel.Bpm.Hrm.Archives;
using Lm.Eic.App.DomainModel.Bpm.Hrm.WorkOverHours;
using Lm.Eic.App.DomainModel.Bpm.Pms.LeaveAsk;
using Lm.Eic.Framework.Authenticate.Business;
using Lm.Eic.Framework.ProductMaster.Business.Config;
using Lm.Eic.Framework.ProductMaster.Model;
using Lm.Eic.Uti.Common.YleeExtension.FileOperation;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EicWorkPlatfrom.Controllers.Product
{
    public class ProEmployeeController : EicBaseController
    {
       
        // GET: /ProEmployee/

        public ActionResult Index()
        {
            return View();
        }
        #region WorkerInfo
        /// <summary>
        /// 人员信息管理
        /// </summary>
        /// <returns></returns>
        public ActionResult RegistWorkerInfo()
        {
            return View();
        }

        [NoAuthenCheck]
        public JsonResult GetWorkers()
        {
            var workers = AuthenManager.User.GetWorkers();
            LoginUser currentUser = OnLineUser;
            var currentWorker = workers.FirstOrDefault(e => e.WorkerId == currentUser.UserId);
            if (currentWorker != null)
                currentUser.Department = currentWorker.Department;
            var departments = ArchiveService.ArchivesManager.DepartmentMananger.Departments;
            var roles = AuthenService.RoleManager.Roles.Where(e => e.RoleLevel > currentUser.Role.RoleLevel);
            var datas = new { user = currentUser, workers = workers, departments = departments, roles = roles };
            return Json(datas, JsonRequestBehavior.AllowGet);
        }

        [NoAuthenCheck]
        public JsonResult GetWorkerBy(string workerId)
        {
            var workers = ArchiveService.ProWorkerManager.GetWorkerBy(workerId);
            return Json(workers, JsonRequestBehavior.AllowGet);
        }

        [NoAuthenCheck]
        public JsonResult RegistWorker(ProWorkerInfo worker)
        {
            var result = ArchiveService.ProWorkerManager.RegistUser(worker);
            return Json(result);

        }
        #endregion

        #region 考勤管理

        #region 加班管理
        public ActionResult ProWorkOverHoursManage()
        {
            return View();
        }
        [NoAuthenCheck]
        //获取信息
        public ContentResult GetWorkOverHoursData(DateTime workDate, string departmentText, int mode)
        {
            WorkOverHoursDto dto = new WorkOverHoursDto()
            {
                WorkDate = workDate,
                DepartmentText = departmentText,
                SearchMode = mode
            };
            var datas = WorkOverHoursService.WorkOverHoursManager.FindRecordBy(dto);
            return DateJsonResult(datas);
        }
        [NoAuthenCheck]
        public ContentResult GetWorkOverHoursSum(string qrydate, string departmentText, int mode)
        {
            WorkOverHoursDto qryDto = new WorkOverHoursDto()
            {
                QryDate = qrydate,
                DepartmentText = departmentText,
                ParentDataNodeText = departmentText,
                SearchMode = mode
            };
            var datas = WorkOverHoursService.WorkOverHoursManager.FindRecordBySum(qryDto);
            TempData["WorkOverHourDatasBySum"] = datas;        
            return DateJsonResult(datas);
        }
        [NoAuthenCheck]
        public ContentResult GetWorkOverHourSumsByWorkId(string qrydate, string departmentText, string workId, int mode)
        {
            WorkOverHoursDto qryDto = new WorkOverHoursDto()
            {
                QryDate = qrydate,
                DepartmentText = departmentText,
                WorkId = workId,
                SearchMode = mode
            };
            var datas = WorkOverHoursService.WorkOverHoursManager.FindRecordBySum(qryDto);
            TempData["WorkOverHourDatasBySum"] = datas;
            return DateJsonResult(datas);
        }
        [NoAuthenCheck]
        public ContentResult GetWorkOverHoursWorkIdBydetail(string qrydate, string departmentText, int mode)
        {
            WorkOverHoursDto qryDto = new WorkOverHoursDto()
            {
                QryDate = qrydate,
                DepartmentText = departmentText,        
                SearchMode = mode
            };
            var datas = WorkOverHoursService.WorkOverHoursManager.FindRecordByDetail(qryDto);
            TempData["WorkOverHourDatasBySum"] = datas;     
            return DateJsonResult(datas);
        }
        [NoAuthenCheck]
        /// <summary>
        /// 载入模板
        /// </summary>
        /// <param name="workDate"></param>
        /// <param name="departmentText"></param>
        /// <returns></returns>
        public ContentResult GetWorkOverHoursMode(string departmentText,string postNature, DateTime workDate,int mode)
        {
            var datas = WorkOverHoursService.WorkOverHoursManager.FindRecordByModel(departmentText,postNature,workDate,mode);

            foreach (var item in datas)
            {
                if (item.OpSign == "edit")
                {
                    item.OpSign = "add";
                }             
            }
            TempData["WorkOverHoursDatas"] = datas;
            return DateJsonResult(datas);
        }
        [NoAuthenCheck]
        [HttpPost]

        public JsonResult HandlWorkOverHoursDt(List<WorkOverHoursMangeModels> workOverHours)
        {
            var result = WorkOverHoursService.WorkOverHoursManager.HandleWorkOverHoursDatas(workOverHours);
            return Json(result);
        }
        /// <summary>
        /// 导入EXCEL数据到DataSets
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [NoAuthenCheck]
        public ContentResult ImportWorkOverHoursDatas(HttpPostedFileBase file)
        {
            List<WorkOverHoursMangeModels> datas = null;
            string filePath = this.CombinedFilePath(FileLibraryKey.FileLibrary, FileLibraryKey.Temp);
            SaveFileToServer(file, filePath, () =>
            {
                string fileName = Path.Combine(filePath, file.FileName);
                datas = WorkOverHoursService.WorkOverHoursManager.ImportWorkOverHoursListBy(fileName);
                //System.IO.File.Delete(fileName);
            });
            return DateJsonResult(datas);
        }
        [NoAuthenCheck]
        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <returns></returns>
        public FileResult WorkOverHoursDatasToExcel()
        {     
            try
            {
                string postNature="";
                //double workhours = 0;
                var datasList = new List<WorkOverHoursMangeModels>();
                var datas = TempData["WorkOverHoursDatas"] as List<WorkOverHoursMangeModels>;
                if (datas == null || datas.Count == 0)
                {
                    new DownLoadFileModel().Default();
                }
                //else
                //{
                //    foreach (var item in datas)
                //    {
                //        postNature = item.PostNature;
                //        workhours = item.WorkOverHours;
                //        var temModel = new WorkOverHoursMangeModels();
                //        temModel.WorkerId = item.WorkerId;
                //        temModel.WorkerName = item.WorkerName;
                //        temModel.DepartmentText = item.DepartmentText;
                //        temModel.WorkoverType = item.WorkoverType;
                //        temModel.WorkDate = item.WorkDate;
                //        temModel.WorkOverHours = item.WorkOverHours;
                //        temModel.WorkReason = item.WorkReason;
                //        temModel.WorkClassType = item.WorkClassType;
                //        temModel.WorkDayTime = item.WorkDayTime;
                //        temModel.WorkDayTime1 = item.WorkDayTime1;
                //        temModel.WorkNightTime = item.WorkNightTime;
                //        temModel.WorkNightTime1 = item.WorkNightTime1;
                //        temModel.PostNature = item.PostNature;
                //        temModel.Remark = item.Remark;
                //        temModel.WorkStatus = item.WorkStatus;
                //        datasList.Add(temModel);
                //    }
                //}  
                else
                {
                    foreach (var item in datas)
                    {
                        postNature = item.PostNature;

                        break;
                    }
                }
                if (postNature=="间接")
                {
                    string filePath = SiteRootPath + @"FileLibrary\WorkOverHours\加班数据模板间接.xls";
                    string fileName = "加班数据模板间接.xls";               
                    if (datas == null || datas.Count == 0)
                    {
                        new DownLoadFileModel().Default();
                    }
                    var dlfm = WorkOverHoursService.WorkOverHoursManager.WorkOverHoursDatasDLFM(datas, SiteRootPath, filePath, fileName);
                    return this.DownLoadFile(dlfm);
                }
                else
                {
                    string filePath = SiteRootPath + @"FileLibrary\WorkOverHours\加班数据模板直接.xls";
                    string fileName = "加班数据模板直接.xls";
                    if (datasList == null || datasList.Count == 0)
                    {
                        new DownLoadFileModel().Default();
                    }
                    var dlfm = WorkOverHoursService.WorkOverHoursManager.WorkOverHoursDatasDLFM_direct(datas, SiteRootPath, filePath, fileName);
                    return this.DownLoadFile(dlfm);
                }
               
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [NoAuthenCheck]
        public FileResult WorkOverHoursDatasSumToExcel()
        {
            try
            {
                string filePath1 = SiteRootPath + @"FileLibrary\WorkOverHours\加班汇总表.xls";
                string fileName1 = "加班汇总表.xls";
                var datas = TempData["WorkOverHourDatasBySum"] as List<WorkOverHoursMangeModels>;
                if (datas == null || datas.Count == 0)
                {
                    new DownLoadFileModel().Default();
                }
                var dlfm = WorkOverHoursService.WorkOverHoursManager.WorkOverHoursDatasSumDLFM(datas, SiteRootPath, filePath1, fileName1);
                return this.DownLoadFile(dlfm);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [NoAuthenCheck]
        public FileResult WorkOverHoursDatasSumToExcelDayCount()
        {
            try
            {
                string filePath1 = SiteRootPath + @"FileLibrary\WorkOverHours\加班汇总表1.xls";
                string fileName1 = "加班汇总表1.xls";
                var datas = TempData["WorkOverHourDatasBySum"] as List<WorkOverHoursMangeModels>;
                if (datas == null || datas.Count == 0)
                {
                    new DownLoadFileModel().Default();
                }
                var dlfm = WorkOverHoursService.WorkOverHoursManager.WorkOverHoursDatasSumDLFM1(datas, SiteRootPath, filePath1, fileName1);
                return this.DownLoadFile(dlfm);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// 后台保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [NoAuthenCheck]
        public JsonResult StoreWorkOverHoursRecordSingle(WorkOverHoursMangeModels model)
        {
            try
            {
                var opresult = WorkOverHoursService.WorkOverHoursManager.StoreWorkOverHours(model);
                return Json(opresult);
            }
            catch (Exception ex)
            {
                throw new System.Exception(ex.Message);
            }
        }
        [NoAuthenCheck]
        public ContentResult GetDepartment(string datanodeName)
        {
            try
            {
                var datas = PmConfigService.DataDicManager.GetConfigDataDepartment("Organization", "HrBaseInfoManage", datanodeName);

                return DateJsonResult(datas);

            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

        }
        /// <summary>
        /// 批量删除后台数据
        /// </summary>
        /// <param name="departmentText"></param>
        /// <param name="workDate"></param>
        /// <returns></returns>
        [HttpPost]
        [NoAuthenCheck]
        public JsonResult HandlDeleteWorkOverHoursDt(string departmentText, DateTime workDate)
        {
            try
            {
               var opresult = WorkOverHoursService.WorkOverHoursManager.HandleDeleteWorkOverHours(departmentText, workDate);
               return Json(opresult);
            }
            catch (Exception ex)
            {
               throw new Exception(ex.Message);
            }

        }
  
        #endregion

        #region 请假管理
        public ActionResult ProAskLeaveManage()
        {
            return View();
        }
        /// <summary>
        /// 获取请假类别
        /// </summary>
        /// <returns></returns>
        [NoAuthenCheck]
        public  JsonResult GetLeaveTypesConfigs()
        {               
            List<ConfigDataDictionaryModel> leaveConfigTypes = PmConfigService.DataDicManager.LoadConfigDatasBy("AttendanceConfig", "AskForLeaveType");
            return Json(leaveConfigTypes, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 保存请假数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [NoAuthenCheck]
        public JsonResult StoreLeaveAskManagerDatas(LeaveAskManagerModels model)
        {
           
           try
           {
                var opresult = LeaveAskService.LeaveAskManager.StoreLeaveAskDatas(model);          
                return Json(opresult);
           }
            catch (System.Exception ex)
           {

                throw new Exception(ex.Message);
           }
        }
        /// <summary>
        /// 查询请假信息
        /// </summary>
        /// <param name="workerId"></param>
        /// <param name="leaveSate"></param>
        /// <param name="department"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        [HttpGet]
        [NoAuthenCheck]
        
        public ContentResult GetLeaveAskManagerDatas(string workerId,string leaveSate,string department,string leaveType,int mode)
        {
            var datas = LeaveAskService.LeaveAskManager.FindByWorkerId(new LeaveAskManagerModelDto()
            {
                WorkerId = workerId,
                LeaveSate=leaveSate,
                Department=department,
                LeaveType=leaveType,
                SearchMode=mode     
            });
            return DateJsonResult(datas);
        }

        /// <summary>
        /// 计算两个日期的差值 
        /// </summary>
        /// <param name="date2"></param>
        /// <param name="date1"></param>
        /// <returns></returns>
        [HttpGet]
        [NoAuthenCheck]
        public JsonResult CalculateDate(DateTime date2,DateTime date1)
        {
            TimeSpan ts = date2.Subtract(date1);
            var hours = Math.Round(ts.TotalHours, 2);
            return  Json(hours, JsonRequestBehavior.AllowGet); ;
        }
        #endregion

        #endregion


    }
}