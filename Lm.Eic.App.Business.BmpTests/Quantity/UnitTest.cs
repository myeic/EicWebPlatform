﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lm.Eic.App.Business.Bmp.Quantity;


namespace Lm.Eic.App.Business.BmpTests.Quantity
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void QsTestMethod()
        {
            //var n = QuantityService.IQCSampleItemsRecordManager.GetSamplePrintItemBy("32AAP00001200RM");
            //  /// 测工单从ERP中得到物料信息
            //int m = QuantityService .IQCSampleItemsRecordManager.GetPuroductSupplierInfo("591-1607032").Count;
            var mm = QuantityService.IQCSampleItemsRecordManager.GetPringSampleItemBy("591-1607032", "32AAP00001200RM");
           System.IO.MemoryStream stream=  QuantityService.IQCSampleItemsRecordManager.ExportPrintToExcel(mm, "IQCPrint");
            #region 输出到Excel
           string path = @"C:\\lmSpc\\System\\ProductSizeSpecPicture\\品保课\\IQC.xls";
            using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                byte[] bArr = stream.ToArray();
                fs.Write(bArr, 0, bArr.Length);
                fs.Flush();
            }
            #endregion

   
           
            Assert.Fail();
        }



     
    }


}
