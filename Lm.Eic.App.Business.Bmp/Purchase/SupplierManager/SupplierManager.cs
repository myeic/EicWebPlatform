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
namespace Lm.Eic.App.Business.Bmp.Purchase.SupplierManager
{

    public class PurSupplierManager
    {
       
     public purSupplierInputManager InPutManage
        {
            get { return OBulider.BuildInstance<purSupplierInputManager>(); }
        }
    }
    public class purSupplierInputManager
    {
        /// <summary>
        /// 从ERP中获取年份合格供应商信息
        /// </summary>
        /// <param name="yearMoth">年份格式yyyyMM</param>
        /// <returns></returns>
        public List<QualifiedSupplierModel> FindQualifiedSupplierList(string yearMoth)
        {
            List<QualifiedSupplierModel> QualifiedSupplierInfo = new List<QualifiedSupplierModel>();
            //获取供应商信息
            var supplierInfoList = FindSupplierInformationList(yearMoth);

            if (supplierInfoList == null || supplierInfoList.Count <= 0) return QualifiedSupplierInfo;

            supplierInfoList.ForEach(supplierInfo =>
            {
                //从ERP中得到最新二次采购信息
                var SupplierLatestTwoPurchase = PurchaseDbManager.PurchaseDb.FindSupplierLatestTwoPurchaseBy(supplierInfo.SupplierId);

                QualifiedSupplierInfo.Add(new QualifiedSupplierModel
                {
                    LastPurchaseDate = SupplierLatestTwoPurchase.FirstOrDefault().PurchaseDate.Trim().ToDate(),
                    UpperPurchaseDate = SupplierLatestTwoPurchase.LastOrDefault().PurchaseDate.Trim().ToDate(),
                    PurchaseUser = SupplierLatestTwoPurchase.FirstOrDefault().PurchasePerson,
                    SupplierId = supplierInfo.SupplierId,
                    SupplierEmail = supplierInfo.SupplierEmail,
                    SupplierAddress = supplierInfo.SupplierAddress,
                    BillAddress = supplierInfo.BillAddress,
                    SupplierFaxNo = supplierInfo.SupplierFaxNo,
                    SupplierName = supplierInfo.SupplierName,
                    SupplierShortName = supplierInfo.SupplierShortName,
                    SupplierUser = supplierInfo.PurchaseUser,
                    SupplierTel = supplierInfo.SupplierTel
                });
            });
            return QualifiedSupplierInfo.Take(10).ToList();
        }
        /// <summary>
        /// 从ERP中获取年份供应商信息
        /// </summary>
        /// <param name="yearMoth">年份格式yyyyMM</param>
        /// <returns></returns>
        public List<SupplierInfoModel> FindSupplierInformationList(string yearMoth)
        {
            List<SupplierInfoModel> SupplierInfoList = new List<SupplierInfoModel>();
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
        public SupplierInfoModel GetSuppplierInfoBy(string supplierId)
        {
            try
            {
                //先从已存的数据信息中找 没有找到再从ERP中找
                SupplierInfoModel SupplierInfo = SupplierCrudFactory.SuppliersInfoCrud.GetSupplierInfoBy(supplierId);
                if (SupplierInfo == null)
                { SupplierInfo = GetErpSuppplierInfoBy(supplierId); }
                return SupplierInfo;
            }
            catch (Exception ex) { throw new Exception(ex.InnerException.Message); }
        }
        /// <summary>
        /// 从ERP中得到供应商信息
        /// </summary>
        /// <param name="supplierId"></param>
        /// <returns></returns>
        private SupplierInfoModel GetErpSuppplierInfoBy(string supplierId)
        {
            var erpSupplierInfo = PurchaseDbManager.SupplierDb.FindSpupplierInfoBy(supplierId);
            if (erpSupplierInfo == null) return null;
            return new SupplierInfoModel
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
        /// <summary>
        /// 批量保存供应商信息
        /// </summary>
        /// <param name="modelList"></param>
        /// <returns></returns>
        public OpResult SaveSupplierInfos(List<SupplierInfoModel> modelList)
        {
            return SupplierCrudFactory.SuppliersInfoCrud.SavaSupplierInfoList(modelList);
        }
        /// <summary>
        /// 批量保存合格供应商信息
        /// </summary>
        /// <param name="modelList"></param>
        /// <returns></returns>
        public OpResult SavaQualifiedSupplierInfoS(List<QualifiedSupplierModel> modelList)
        {
            return SupplierCrudFactory.QualifiedSupplierCrud.SavaQualifiedSupplierInfoList(modelList);
        }
         OpResult SaveSupplierInfoModel(SupplierInfoModel model)
        {

            try
            {
                decimal findId_key = 0;
                if (SupplierCrudFactory.SuppliersInfoCrud.IsExistSupperid(model.SupplierId, out findId_key))
                {
                    model.OpSign = "eidt";
                    model.Id_key = findId_key;
                }
                else model.OpSign = "add";

                return SupplierCrudFactory.SuppliersInfoCrud.Store(model);
            }
            catch (Exception ex) { throw new Exception(ex.InnerException.Message); }


            
        }
        /// <summary>
        /// 批量保存证书信息
        /// </summary>
        /// <param name="modelList"></param>
        /// <returns></returns>
        public OpResult SavaEditSpplierPutInt(List<PutIntSupplieInfoModel> modelList)
        {

            //判断列表是否为空
            if (modelList == null || modelList.Count <= 0) return new OpResult("数据列表不能为空",true);
            //通过SupplierId得到供应商信息
            var supplierInfoModel = GetErpSuppplierInfoBy(modelList.FirstOrDefault().SupplierId);
            //赋修供应商属性和采购性质
            if (supplierInfoModel!=null)
            {
                if ((supplierInfoModel.PurchaseType ==null)|| (supplierInfoModel.PurchaseType==string.Empty) )
                {
                    supplierInfoModel.PurchaseType = modelList.FirstOrDefault().PurchaseType;
                }
                if ((supplierInfoModel.SupplierProperty == null) || (supplierInfoModel.SupplierProperty  == string.Empty))
                {
                    supplierInfoModel.SupplierProperty = modelList.FirstOrDefault().SupplierProperty;
                }
               
            }
            //更新保存数据
            if (SaveSupplierInfoModel(supplierInfoModel).Result)
            {
                List<SupplierEligibleCertificateModel> certificateModelList = new List<SupplierEligibleCertificateModel>();
                //保存证书数据
                modelList.ForEach(e =>
                {
                    SupplierEligibleCertificateModel model = new SupplierEligibleCertificateModel()
                    {
                        SupplierId = e.SupplierId,
                        EligibleCertificate = e.EligibleCertificate,
                        FilePath = e.FilePath,
                        DateOfCertificate = e.DateOfCertificate,
                        IsEfficacy = e.IsEfficacy,
                        OpSign = "add"
                    };
                    certificateModelList.Add(model);
                });

                return SupplierCrudFactory.SupplierEligibleCrud.SavaSupplierEligibleList(certificateModelList);
            }
            else return new OpResult("数据保存失败");
        }
    }
   
}
