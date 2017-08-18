﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lm.Eic.App.Business.Bmp.Quality.Qua8DReportManage;
using Lm.Eic.App.DomainModel.Bpm.Quanity;
using Lm.Eic.App.Business.Bmp.Quality.InspectionManage;
using Lm.Eic.Framework.ProductMaster.Model.CommonManage;
using Lm.Eic.App.Business.Bmp.WorkFlow.GeneralForm;

namespace EicWorkPlatfrom.Controllers
{
    public class Qua8DManageController : EicBaseController
    {
        //
        // GET: /Qua8DManage/

        public ActionResult Index()
        {
            return View();
        }

        #region Create8DForm
        [NoAuthenCheck]
        public ActionResult Create8DForm()
        {
            return View();
        }
        [NoAuthenCheck]
        public ContentResult GetQueryDatas(string searchModel, string orderId)
        {
            var datas = InspectionService.DataGatherManager.IqcDataGather.MasterDatasGather.GetIqcMasterContainDatasBy(orderId);
            return DateJsonResult(datas);
        }

        /// <summary>
        /// 保存 初始8Dp初始数据
        /// </summary>
        /// <param name="initiateData"></param>
        /// <returns></returns>
        [NoAuthenCheck]
        public JsonResult StoreCraet8DInitialData(Qua8DReportMasterModel initialData)
        {
            var result = Qua8DService.Qua8DManager.Qua8DMaster.StoreQua8DMaster(initialData);
            return Json(result);
        }
        /// <summary>
        /// 自动生成8D单单号
        /// </summary>
        /// <param name="mmm"></param>
        /// <returns></returns>
        [NoAuthenCheck]
        public JsonResult AutoBuildingReportId(string discoverPosition)
        {
            var data = Qua8DService.Qua8DManager.Qua8DMaster.AutoBuildingReportId(discoverPosition);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Handle8DFolw
        [NoAuthenCheck]
        public ActionResult Handle8DFolw()
        {
            return View();
        }
        /// <summary>
        /// 得到Rma单的数据
        /// </summary>
        /// <param name="rmaId"></param>
        /// <returns></returns>
        [NoAuthenCheck]
        public JsonResult ShowQua8DDetailDatas(string reportId)
        {
            var ShowQua8DMasterData = Qua8DService.Qua8DManager.Qua8DMaster.Show8DReportMasterInfo(reportId);

            var Stepdatas = Qua8DService.Qua8DManager.Qua8DDatail.ShowQua8DDetailDatasBy(reportId);
            var datas = new { Stepdatas, ShowQua8DMasterData };
            return Json(datas, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 通过单号 序号得到设置模板
        /// </summary>
        /// <param name="reportId"></param>
        /// <param name="stepId"></param>
        /// <returns></returns>
        [NoAuthenCheck]
        public JsonResult GetRua8DReportStepData(string reportId, int stepId)
        {
            var data = Qua8DService.Qua8DManager.Qua8DDatail.GetQua8DDetailDatasBy(reportId, stepId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="handelData"></param>
        /// <returns></returns>
        [NoAuthenCheck]
        public JsonResult SaveQua8DHandleDatas(Qua8DReportDetailModel handelData)
        {
            var data = Qua8DService.Qua8DManager.Qua8DDatail.StoreQua8DHandleDatas(handelData);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 上传文件附件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [NoAuthenCheck]
        public JsonResult Upload8DHandleFile(HttpPostedFileBase file)
        {
            FormAttachFileManageModel dto = ConvertFormDataToTEntity<FormAttachFileManageModel>("attachFileDto");
            string filePath = this.CombinedFilePath(FileLibraryKey.FileLibrary, FileLibraryKey.Qua8DUpAttachFile, dto.ModuleName);
            string customizeFileName = GeneralFormService.InternalContactFormManager.AttachFileHandler.SetAttachFileName(dto.ModuleName, dto.FormId);
            string addchangeFileName = DateTime.Now.Day.ToString("00") + DateTime.Now.Hour.ToString("00");
            var result = this.SaveFileToServer(file, filePath, addchangeFileName);
            //string addchangeFileName = DateTime.Now.Day.ToString("00") + DateTime.Now.Hour.ToString("00");
            //string filePath = this.CombinedFilePath(FileLibraryKey.FileLibrary, FileLibraryKey.IqcInspectionGatherDataFile, DateTime.Now.ToString("yyyyMM"));
            //this.SaveFileToServer(file, filePath, addchangeFileName);
            return Json(result);
        }
        #endregion



        #region Close8DForm
        [NoAuthenCheck]
        public ActionResult Close8DForm()
        {
            return View();
        }
        #endregion

    }
}
