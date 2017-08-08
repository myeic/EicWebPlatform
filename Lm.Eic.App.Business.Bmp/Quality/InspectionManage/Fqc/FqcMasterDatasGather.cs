﻿using Lm.Eic.App.DomainModel.Bpm.Quanity;
using Lm.Eic.Uti.Common.YleeOOMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lm.Eic.App.Business.Bmp.Quality.InspectionManage
{
    public class FqcMasterDatasGather : InspectionDateGatherManageBase
    {
        /// <summary>
        /// 物料得到 MasterDatas
        /// </summary>
        /// <param name="marterialId"></param>
        /// <returns></returns>
        public List<InspectionFqcMasterModel> GetFqcMasterModeListlBy(string marterialId)
        {
            return InspectionManagerCrudFactory.FqcMasterCrud.GetFqcInspectionMasterListBy(marterialId);
        }
        /// <summary>
        ///  得到Mater列表数据
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public List<InspectionFqcMasterModel> GetFqcMasterOrderIdDatasBy(string orderId)
        {
            try
            {
                return InspectionManagerCrudFactory.FqcMasterCrud.GetFqcInspectionMasterModelListBy(orderId);
            }
            catch (Exception ex)
            { return null; throw new Exception(ex.InnerException.Message); }
        }
        /// <summary>
        /// 存储主表信息
        /// </summary>
        /// <param name="masterModel"></param>
        /// <returns></returns>
        public OpResult storeInspectionMasterial(InspectionFqcMasterModel masterModel)
        {
            var haveStoreMasterModel = InspectionManagerCrudFactory.FqcMasterCrud.GetStroeOldModel(masterModel.OrderId, masterModel.OrderIdNumber, masterModel.MaterialId);
            if (haveStoreMasterModel == null) return InspectionManagerCrudFactory.FqcMasterCrud.Store(masterModel);
            ///初始化数据
            if (masterModel.OpPerson == "StartSetValue") return OpResult.SetSuccessResult("初始已经保存", true);

            string inspecitonItem = masterModel.InspectionItems != null ? masterModel.InspectionItems.Trim() : string.Empty;
            if (!haveStoreMasterModel.InspectionItems.Contains(inspecitonItem) && inspecitonItem != string.Empty)
                masterModel.InspectionItems = haveStoreMasterModel.InspectionItems + "," + inspecitonItem;
            var detailDatas = InspectionManagerCrudFactory.FqcDetailCrud.GetFqcDetailDatasBy(masterModel.OrderId, masterModel.OrderIdNumber);
            if (detailDatas != null && detailDatas.Count > 0)
            {
                if (detailDatas.Count() == GetHaveFinishDataNumber(masterModel.InspectionItems))
                {
                    masterModel.InspectionStatus = "待审核";
                    masterModel.InspectionResult = (detailDatas.Count(e => e.InspectionItemResult == "NG") > 0 ? "NG" : "OK");
                }
                else masterModel.InspectionStatus = "未完工";
            }
            /// 如果 是新增 只添加一次 
            masterModel.Id_Key = haveStoreMasterModel.Id_Key;
            masterModel.OpSign = OpMode.Edit;
            return InspectionManagerCrudFactory.FqcMasterCrud.Store(masterModel);

        }
        /// <summary>
        /// 获取已经检验的数量
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public double GetFqcMasterHaveInspectionCountBy(string orderId)
        {
            var listdatas = GetFqcMasterOrderIdDatasBy(orderId);
            return (listdatas == null || listdatas.Count <= 0) ? 0 : listdatas.Sum(e => e.InspectionCount);
        }
    }

}
