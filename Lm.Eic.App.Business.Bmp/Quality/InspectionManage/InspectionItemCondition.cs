﻿using Lm.Eic.App.DomainModel.Bpm.Quanity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lm.Eic.App.Business.Bmp.Quality.InspectionManage
{
    public class InspectionItemCondition
    {
        /// <summary>
        /// 加载所有的测试项目
        /// </summary>
        /// <param name="materialId"></param>
        /// <returns></returns>
        public List<InspectionIqcItemConfigModel> getIqcNeedInspectionItemDatas(string orderId, string materialId, DateTime materialInDate)
        {
            List<InspectionIqcItemConfigModel> needInsepctionItems = InspectionManagerCrudFactory.IqcItemConfigCrud.FindIqcInspectionItemConfigDatasBy(materialId);
            /// 针对所有需测试的项
            var item = InspectionManagerCrudFactory.IqcItemConfigCrud.FindIqcInspectionItemConfigDatasBy("AllMaterialId").FirstOrDefault();
            if (item != null) item.MaterialId = materialId;
            bool IsAddAllMaterialId = true;

            if (needInsepctionItems == null || needInsepctionItems.Count <= 0) return new List<InspectionIqcItemConfigModel>();
            var isAddOrRemoveItemDic = JudgeIsAddOrRemoveItemDic(orderId, materialId, materialInDate);
            needInsepctionItems.ForEach(m =>
            {
                if (isAddOrRemoveItemDic.ContainsKey(m.InspectionItem))
                {
                    if (isAddOrRemoveItemDic[m.InspectionItem])
                    {
                        needInsepctionItems.Remove(m);
                    }
                }
                ///是否包函 ROHS  如果不包括  依条件添加
                if (m.InspectionItem.Contains("ROHS"))
                {
                    IsAddAllMaterialId = false;
                }

            });
            ///判定是否应该 添加 AllMaterial
            if (IsAddAllMaterialId)
            {
                if (isAddOrRemoveItemDic.ContainsKey("AllMaterialId"))
                {
                    if (isAddOrRemoveItemDic["AllMaterialId"])
                    {
                        if (item != null)
                            needInsepctionItems.Add(item);
                    }
                }
            }
            
            return needInsepctionItems;
        }

        /// <summary>
        ///  判定是否需要测试 盐雾测试
        /// </summary>
        /// <param name="materialId">物料料号</param>
        /// <param name="materialInDate">当前物料进料日期</param>
        /// <returns></returns>
        public Dictionary<string, bool> JudgeIsAddOrRemoveItemDic(string orderId, string materialId, DateTime materialInDate)
        {
            /// true 要删除的 
            Dictionary<string, bool> itemDic = new Dictionary<string, bool>();
            var datas = InspectionManagerCrudFactory.IqcDetailCrud.GetIqcInspectionDetailDatasBy(orderId, materialId);
            itemDic.Add("盐雾测试", JudgeYwTest(materialInDate, datas));
            itemDic.Add("全尺寸", JudgeMaterialTwoYearIsRecord(datas));
            itemDic.Add("ROHS", false);
            itemDic.Add("NOT ROHS", true);
            itemDic.Add("AllMaterialId", JudgeROHSTest(datas));
            return itemDic;
        }



        /// <summary>
        /// 盐雾测试  false(要测)  ture（不要测删除）
        /// </summary>
        /// <param name="materialInDate">进货日期</param>
        /// <param name="Datas">抽样的数据</param>
        /// <returns></returns>
        private bool JudgeYwTest(DateTime materialInDate, List<InspectionIqcDetailModel> datas)
        {
            try
            {
                //调出此物料所有打印记录项

                //如果第一次打印 
                if (datas == null | datas.Count() <= 0)
                    return false;
                // 进料日期后退30天 抽测打印记录
                var inspectionItemsMonthRecord = (from t in datas
                                                  where t.MaterialInDate >= (materialInDate.AddDays(-30))
                                                        & t.MaterialInDate <= materialInDate
                                                  select t.InspecitonItem).Distinct<string>().ToList();
                //没有 测
                if (inspectionItemsMonthRecord == null) return false;
                // 有  每项中是否有测过  盐雾测试 如果有  侧不用测试
                foreach (var n in inspectionItemsMonthRecord)
                {
                    if (n.Contains("盐雾")) return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message);
            }
        }

        /// <summary>
        /// 判定些物料在二年内是否有录入记录 
        /// </summary>
        /// <param name="materialInDate"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool JudgeMaterialTwoYearIsRecord(List<InspectionIqcDetailModel> datas)
        {

            if (datas == null | datas.Count() <= 0)
                return false;
            var returnitem = datas.Where(e => e.MaterialInDate >= DateTime.Now.AddYears(-2));
            if (returnitem != null && returnitem.Count() > 0)
                return true;
            return false;
        }

        /// <summary>
        /// ROHS 抽样规则 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>

        private bool JudgeROHSTest(List<InspectionIqcDetailModel> datas)
        {
            if (datas == null | datas.Count() <= 0)
                return true;
            if (datas.Count() / 2 == 0)
                return true;
            else return false;
        }
        /// <summary>
        ///ROT抽样规则 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool JudgeROTest(InspectionIqcItemConfigModel item)
        {
            return true;
        }
    }

}
