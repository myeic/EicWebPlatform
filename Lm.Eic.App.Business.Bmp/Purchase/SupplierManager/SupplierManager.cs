﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using  Lm.Eic.App.Erp.Bussiness.PurchaseManage;
using Lm.Eic.App.DomainModel.Bpm.Purchase;
using Lm.Eic.Uti.Common.YleeExtension.Conversion;
using Lm.Eic.Uti.Common.YleeDbHandler;
using Lm.Eic.Uti.Common.YleeObjectBuilder;
using Lm.Eic.Uti.Common.YleeOOMapper;
using Lm.Eic.App.DbAccess.Bpm.Repository.PurchaseRep.PurchaseSuppliesManagement;
using Lm.Eic.Uti.Common.YleeExtension.FileOperation;

namespace Lm.Eic.App.Business.Bmp.Purchase.SupplierManager
{
    public class PurSupplierManager
    {
        /// <summary>
        /// 从ERP中获取年份合格供应商清册表
        /// </summary>
        /// <param name="yearMoth">年份格式yyyyMM</param>
        /// <returns></returns>
        public List<EligibleSuppliersModel> FindQualifiedSupplierList(string yearMoth)
        {
            List<EligibleSuppliersModel> QualifiedSupplierInfo = new List<EligibleSuppliersModel>();
            //获取供应商信息
            var supplierInfoList = GetSupplierInformationListBy(yearMoth);

            if (supplierInfoList == null || supplierInfoList.Count <= 0) return QualifiedSupplierInfo;

            supplierInfoList.ForEach(supplierInfo =>
            {
                //从ERP中得到最新二次采购信息
                var SupplierLatestTwoPurchase = PurchaseDbManager.PurchaseDb.FindSupplierLatestTwoPurchaseBy(supplierInfo.SupplierId);

                QualifiedSupplierInfo.Add(new EligibleSuppliersModel
                {
                    LastPurchaseDate = SupplierLatestTwoPurchase.FirstOrDefault().PurchaseDate.Trim().ToDate(),
                    UpperPurchaseDate = SupplierLatestTwoPurchase.LastOrDefault().PurchaseDate.Trim().ToDate(),
                    PurchaseUser = SupplierLatestTwoPurchase.FirstOrDefault().PurchasePerson,
                    SupplierId = supplierInfo.SupplierId,
                    SupplierProperty = supplierInfo.SupplierProperty,
                    PurchaseType = supplierInfo.PurchaseType,
                    SupplierEmail = supplierInfo.SupplierEmail,
                    SupplierAddress = supplierInfo.SupplierAddress,
                    BillAddress = supplierInfo.BillAddress,
                    SupplierFaxNo = supplierInfo.SupplierFaxNo,
                    SupplierName = supplierInfo.SupplierName,
                    Remark = supplierInfo.Remark,
                    SupplierShortName = supplierInfo.SupplierShortName,
                    SupplierUser = supplierInfo.SupplierUser,
                    SupplierTel = supplierInfo.SupplierTel,
                    EligibleCertificate = SupplierLatestTwoPurchase.FirstOrDefault().PurchaseDate.Trim().ToDate().ToString()
                });
            });
            return QualifiedSupplierInfo.ToList();
        }

        /// <summary>
        /// 获取供应商信息表
        /// </summary>
        /// <param name="yearMoth">年份格式yyyyMM</param>
        /// <returns></returns>
        public  List<SuppliersInfoModel> GetSupplierInformationListBy(string yearMoth)
        {
            List<SuppliersInfoModel> SupplierInfoList = new List<SuppliersInfoModel>();
            //从ERP中得到此年中所有供应商Id号
            var supplierList = PurchaseDbManager.PurchaseDb.PurchaseSppuerId(yearMoth);
            if (supplierList == null || supplierList.Count <= 0) return null;
            supplierList.ForEach(supplierId =>
            {
                SupplierInfoList.Add(GetSuppplierInfoBy(supplierId));
            });
            return SupplierInfoList;
        }
        /// <summary>
        /// 获取供应商信息
        /// </summary>
        /// <param name="supplierId"></param>
        /// <returns></returns>
        public SuppliersInfoModel GetSuppplierInfoBy(string supplierId)
        {
            try
            {
                //先从已存的数据信息中找 没有找到再从ERP中找
                SuppliersInfoModel SupplierInfo = SupplierCrudFactory.SuppliersInfoCrud.GetSupplierInfoBy(supplierId);
                if (SupplierInfo == null)
                { SupplierInfo = GetErpSuppplierInfoBy(supplierId); }
                return SupplierInfo;
            }
            catch (Exception ex) { throw new Exception(ex.InnerException.Message); }
        }
       
        /// <summary>
        /// 批量保存供应商信息
        /// </summary>
        /// <param name="modelList"></param>
        /// <returns></returns>
        public OpResult SaveSupplierInfoList(List<SuppliersInfoModel> modelList)
        {
            return SupplierCrudFactory.SuppliersInfoCrud.SavaSupplierInfoList(modelList);
        }
        /// <summary>
        /// 批量保存合格供应商清册
        /// </summary>
        /// <param name="modelList"></param>
        /// <returns></returns>
        public OpResult SavaQualifiedSupplierInfoList(List<EligibleSuppliersModel> modelList)
        {
            return SupplierCrudFactory.EligibleSupplierCrud.SavaEligibleSuppliersList(modelList);
        }

        /// <summary>
        /// 保存编辑的供应商证书信息
        /// </summary>
        /// <param name="modelList"></param>
        /// <returns></returns>
        public OpResult SavaEditSpplierCertificate(List<InPutSupplieCertificateInfoModel> modelList)
        {
            //判断列表是否为空
            if (modelList == null || modelList.Count <= 0) return new OpResult("数据列表不能为空",true);
            var model = modelList[0];
            //通过SupplierId得到供应商信息
            var supplierInfoModel = GetErpSuppplierInfoBy(model.SupplierId);
            //判断是否为空
            if (supplierInfoModel == null) return new OpResult(string.Format("没有{0}供应商编号", model.SupplierId), true);
            //赋值 供应商属性和采购性质
            supplierInfoModel.PurchaseType = model.PurchaseType;
            supplierInfoModel.SupplierProperty = model.SupplierProperty;
            string isExistCertificateFileName = string.Empty;
            //更新保存数据
            if (SaveSupplierInfoModel(supplierInfoModel).Result)
            {
                List<SuppliersQualifiedCertificateModel> certificateModelList = new List<SuppliersQualifiedCertificateModel>();
             
                //保存证书数据
                modelList.ForEach(e =>
                {

                   if(SupplierCrudFactory.SupplierQualifiedCertificateCrud.IsExistCertificateFileName(e.CertificateFileName))
                    {
                        isExistCertificateFileName += e.CertificateFileName + ",";
                    }
                    SuppliersQualifiedCertificateModel savemodel = new SuppliersQualifiedCertificateModel()
                    {
                        SupplierId = e.SupplierId,
                        EligibleCertificate = e.EligibleCertificate,
                        CertificateFileName =e.CertificateFileName,
                        FilePath = e.FilePath,
                        DateOfCertificate =e.DateOfCertificate.ToDate(),
                        IsEfficacy = "是",
                        OpSign = "add"
                    };
                    certificateModelList.Add(savemodel);
                });
                if (isExistCertificateFileName != String.Empty) return new OpResult("此"+isExistCertificateFileName+"文档已经存在，数据保存失败");
               else  return SupplierCrudFactory.SupplierQualifiedCertificateCrud.SavaSupplierEligibleList(certificateModelList);
            }
            else return new OpResult("数据保存失败");
        }
        /// <summary>
        /// 删除供应商证书
        /// </summary>
        /// <param name="model">实体</param>
        /// <param name="rootPath">根路经</param>
        /// <returns></returns>
        public OpResult DelEditSpplierCertificate(SuppliersQualifiedCertificateModel model, string rootPath)
        {
            if (model == null || model.FilePath == string.Empty)  return new OpResult("此文档实体路经不能空");
            if (rootPath == null || rootPath == string.Empty) return new OpResult("此根路经发生错误");
            var fileDocumentPath = rootPath + model.FilePath.Replace("/", @"\"); 
            if (fileDocumentPath.DeleteFileDocumentation())
            {
                return SupplierCrudFactory.SupplierQualifiedCertificateCrud.DeleteSupplierCertificate(model);
            }
            else return new OpResult("此" + fileDocumentPath+"文档不存在或路经不对");
        }
        /// <summary>
        ///获取供应商证书列表
        /// </summary>
        /// <param name="suppliersId">供应商Id</param>
        /// <returns></returns>
        public List<SuppliersQualifiedCertificateModel> GetSupplierQualifiedCertificateListBy(string supplierId)
        {
            return SupplierCrudFactory.SupplierQualifiedCertificateCrud.GetQualifiedCertificateListBy(supplierId);
        }


        #region   internal


        /// <summary>
        /// 更新并保存供应商信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        OpResult SaveSupplierInfoModel(SuppliersInfoModel model)
        {

            try
            {
                decimal findId_key = 0;
                if (SupplierCrudFactory.SuppliersInfoCrud.IsExistSupperid(model.SupplierId, out findId_key))
                {
                    model.OpSign = "edit";
                    model.Id_key = findId_key;
                }
                else model.OpSign = "add";

                return SupplierCrudFactory.SuppliersInfoCrud.Store(model);
            }
            catch (Exception ex) { throw new Exception(ex.InnerException.Message); }



        }
        /// <summary>
        /// 从ERP中得到供应商信息
        /// </summary>
        /// <param name="supplierId"></param>
        /// <returns></returns>
        SuppliersInfoModel GetErpSuppplierInfoBy(string supplierId)
        {
            var erpSupplierInfo = PurchaseDbManager.SupplierDb.FindSpupplierInfoBy(supplierId);
            if (erpSupplierInfo == null) return null;
            return new SuppliersInfoModel
            {
                SupplierId = supplierId,
                SupplierEmail = erpSupplierInfo.Email,
                SupplierAddress = erpSupplierInfo.Address,
                BillAddress = erpSupplierInfo.BillAddress,
                SupplierFaxNo = erpSupplierInfo.FaxNo,
                SupplierName = erpSupplierInfo.SupplierName,
                SupplierShortName = erpSupplierInfo.SupplierShortName,
                SupplierUser = erpSupplierInfo.Contact,
                SupplierTel = erpSupplierInfo.Tel,
                PayCondition = erpSupplierInfo.PayCondition
            };
        }
        #endregion
    }

}
