﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lm.Eic.App.Business.Bmp.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lm.Eic.Uti.Common.YleeExtension.Conversion;

namespace Lm.Eic.App.Business.Bmp.Ast.Tests
{
    [TestClass()]
    public class EquipmentManagerTests
    {
        [TestMethod()]
        public void BuildAssetNumberTest()
        {
            EquipmentManager tem2 = new EquipmentManager();

            Lm.Eic.App.DomainModel.Bpm.Ast.EquipmentModel model = new DomainModel.Bpm.Ast.EquipmentModel();
            model.AssetNumber = "I169001";
            model.EquipmentName = "Test1";
            // model.Id_Key = 4;

            //修改
            //var ttt = tem.ChangeStorage(model, 1);

            string i = tem2.BuildAssetNumber("生产设备", "低值易耗品", "保税");
            string i2 = tem2.BuildAssetNumber("生产设备", "低值易耗品", "非保税");
            string i3 = tem2.BuildAssetNumber("生产设备", "固定资产", "保税");

            //  Assert.Fail();
        }

        [TestMethod()]
        public void StoreTest()
        {
            DomainModel.Bpm.Ast.EquipmentModel model = new DomainModel.Bpm.Ast.EquipmentModel();
            model.AssetNumber = AstService.EquipmentManager.BuildAssetNumber("生产设备", "低值易耗品", "保税");
            model.EquipmentType = "生产设备";
            model.AssetNumber = "低值易耗品";
            model.TaxType = "保税";
            model.EquipmentName = "Test";
            model.MaintenanceDate = DateTime.Now.ToDate();
            model.MaintenanceInterval = 10;
            model.CheckDate = DateTime.Now.ToDate();
            model.CheckInterval = 6;
            model.OpSign = "add";
            var tem = AstService.EquipmentManager.Store(model);
            Assert.Fail();
        }
    }
}