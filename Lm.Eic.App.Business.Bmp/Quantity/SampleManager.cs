﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lm.Eic.App.DomainModel.Bpm.Quanity;
using Lm.Eic.App.DbAccess.Bpm.Repository.QuantityRep;
using Lm.Eic.App.Erp.Bussiness.QuantityManage;
using Lm.Eic.App.Erp.Domain .QuantityModel;
using Lm.Eic.Uti.Common.YleeOOMapper;
using Lm.Eic.Uti.Common.YleeExcelHanlder;



namespace Lm.Eic.App.Business.Bmp.Quantity
{
    public class IQCSampleItemsRecordManager
    {
        IIQCSampleItemRecordReposity irep = null;
        public IQCSampleItemsRecordManager ()
        {
            irep = new IQCSampleItemRecordReposity();
        }
      
        
        /// <summary>
       /// 
       /// </summary>
       /// <param name="orderid"></param>
       /// <returns></returns>
        public  List<IQCSampleItemRecordModel> GetSamplePrintItemBy(string orderid)
        {
            return irep.Entities.Where (e=>e.OrderID ==orderid ).ToList ();
        }



        /// <summary>
        /// 得到IQC抽样项次 （单身）
        /// </summary>
        /// <param name="Orderid"></param>
        /// <param name="SampleMaterial"></param>
        /// <returns></returns>
        public  List<IQCSampleItemRecordModel> GetPringSampleItemBy(string Orderid,string SampleMaterial)
        {
            return irep.Entities.Where(e => e.OrderID == Orderid & e.SampleMaterial == SampleMaterial).ToList();
        }
     
        /// <summary>
        /// 得到抽样物料信息 （单头）
        /// </summary>
        /// <param name="orderid"></param>
        /// <returns></returns>
        public List<MaterialModel> GetPuroductSupplierInfo(string orderid)
        {
            return   QuantityDBManager.QuantityPurchseDb.FindMaterialBy(orderid);
        }

        
        /// <summary>
        ///  IQC 导出Excel 数据流
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="xlsSheetName"></param>
        /// <returns></returns>
        public  System.IO.MemoryStream   ExportPrintToExcel(List<IQCSampleItemRecordModel> dataSource, string xlsSheetName) 
        {
            //return NPOIHelper.ExportToExcel<IQCSampleItemRecordModel>(dataSource, xlsSheetName);
            //数据为Null时返回数值
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            if (dataSource == null || dataSource.Count == 0) return stream;
            NPOI.HSSF.UserModel.HSSFWorkbook workbook = new NPOI.HSSF.UserModel.HSSFWorkbook();
            NPOI.SS.UserModel.ISheet sheet = workbook.CreateSheet(xlsSheetName);
            try
            {
                #region 填充列头区域
               

                SetISheetTitle(sheet, dataSource.FirstOrDefault());

                #endregion 填充列头区域

                #region 填充内容区域

               
                int ValueStartIndex = 8;
                int StartRowIndex =6;
                //数据源第一项赋值 
                for (int rowIndex = 0; rowIndex < dataSource.Count; rowIndex++)
                {
                    IQCSampleItemRecordModel entity = dataSource[rowIndex];
                    // ROHS打印测试项除掉
                    //if (entity.SampleItem.Contains("ROHS"))
                    //{ continue; }


                    NPOI.SS.UserModel.IRow rowContent = sheet.CreateRow(rowIndex + StartRowIndex + 1); ; 
                  
                 
                    Type tentity = entity.GetType();
                    System.Reflection.PropertyInfo[] tpis = tentity.GetProperties();
                 
                    ///每一项每一列赋值 
                    for (int colColumnIndex = 0; colColumnIndex < tpis.Length - ValueStartIndex-1; colColumnIndex++)
                    {
                        /// 单元格的行
                        NPOI.SS.UserModel.ICell cellContent = rowContent.CreateCell(colColumnIndex);
                        // 从第ValueStartIndex 起始列开始取值 
                        object value = tpis[colColumnIndex + ValueStartIndex].GetValue(entity, null);
                        if (value == null) value = "";
                        //对不同类型的值做调整
                        Type type = value.GetType();
                        //对不同类型的值处理赋值
                        OperationSetCellvalue(cellContent, value, type);
                    }
                }

                #endregion 填充内容区域

                sheet.ForceFormulaRecalculation = true;
                workbook.Write(stream);
                return stream;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        private static Dictionary<int, NPOI.SS.UserModel.IRow> thisMyRow = new Dictionary<int, NPOI.SS.UserModel.IRow>();
        private static Dictionary<int, NPOI.SS.UserModel.ICell> thisMyCell = new Dictionary<int, NPOI.SS.UserModel.ICell>();
        private  NPOI.SS.UserModel.ICell ReturnIcell(NPOI.SS.UserModel.ISheet sheet,  NPOI.SS.UserModel.IRow row, int StartColumnIndex)
        {
            //  单元格的列
            NPOI.SS.UserModel.ICell cellContent = null ;

            if (thisMyCell.Keys.Contains(StartColumnIndex))
            {
                cellContent = thisMyCell[StartColumnIndex];
            }
            else
            {
                cellContent = row.CreateCell(StartColumnIndex); ;
                thisMyCell.Add(StartColumnIndex, cellContent);
            }


            return cellContent;
        }


        #region    ExportToExcelPrintOP
        /// <summary>
          /// 处理每个类型的格式
          /// </summary>
          /// <param name="cellSytleDate"></param>
          /// <param name="cellContent"></param>
          /// <param name="value"></param>
          /// <param name="type"></param>
        private  void OperationSetCellvalue( NPOI.SS.UserModel.ICell cellContent, object value, Type type)
        {
            switch (type.ToString())
            {

                case "System.String"://字符串类型
                    cellContent.SetCellValue(value.ToString());
                    break;

                case "System.DateTime"://日期类型
                    DateTime dateV;
                    DateTime.TryParse(((DateTime)value).ToString("yyyy-MM-dd"), out dateV);
                    cellContent.SetCellValue(dateV);
                    break;

                case "System.Boolean"://布尔型
                    bool boolV = false;
                    if (bool.TryParse(value.ToString(), out boolV))
                        cellContent.SetCellValue(boolV);
                    else
                        cellContent.SetCellValue("解析布尔型数据错误");
                    break;

                case "System.Int16"://整型
                    Int16 Int16V = 0;
                    if (Int16.TryParse(value.ToString(), out Int16V))
                        cellContent.SetCellValue(Int16V);
                    else
                        cellContent.SetCellValue("解析16位数据型错误");
                    break;
                case "System.Int32":
                    Int32 Int32V = 0;
                    if (Int32.TryParse(value.ToString(), out Int32V))
                        cellContent.SetCellValue(Int32V);
                    else
                        cellContent.SetCellValue("解析32位数据型错误");
                    break;
                case "System.Int64":
                    Int64 Int64V = 0;
                    if (Int64.TryParse(value.ToString(), out Int64V))
                        cellContent.SetCellValue(Int64V);
                    else
                        cellContent.SetCellValue("解析64位数据型错误");
                    break;
                case "System.Byte":
                    int intV = 0;
                    if (int.TryParse(value.ToString(), out intV))
                        cellContent.SetCellValue(intV);
                    else
                        cellContent.SetCellValue("解析整型数据错误");
                    break;

                case "System.Decimal"://浮点型
                    double DecimalV = 0;
                    if (double.TryParse(value.ToString(), out DecimalV))
                    {
                        Math.Round(DecimalV, 2);
                        cellContent.SetCellValue(DecimalV);
                    }
                    else
                        cellContent.SetCellValue("解析浮点型数据错误");
                    break;
                case "System.Double":
                    double doubV = 0;
                    if (double.TryParse(value.ToString(), out doubV))
                    {
                        Math.Round(doubV, 2);
                        cellContent.SetCellValue(doubV);
                    }
                    else
                        cellContent.SetCellValue("解析浮点型或双精度型数据错误");
                    break;

                case "System.DBNull"://空值处理
                    cellContent.SetCellValue("");
                    break;

                default:
                    cellContent.SetCellValue("");
                    break;
            }
        }


        private NPOI.SS.UserModel.ICellStyle OpDateSytle(NPOI.HSSF.UserModel.HSSFWorkbook workbook)
        {  
            NPOI.SS.UserModel.ICellStyle cellSytleDate = workbook.CreateCellStyle();
            NPOI.SS.UserModel.IDataFormat format = workbook.CreateDataFormat();
            cellSytleDate.DataFormat = format.GetFormat("yyyy年mm月dd日");
            return cellSytleDate;
        }




        #region   上下值判断条件
        private bool JudgeNoNull(string up, string down)
        {
            if (up == null || down == null) return false;
            if (up.Length > 0 || down.Length > 0)
            { return true; }
            else { return false; }
        }
        private bool JudgeMach(string up, string down)
        {
            try
            {
                double upValue = Convert.ToDouble(up);
                double downValue = Convert.ToDouble(down);
                return true;
            }
            catch { return false; }

        }
        private bool JudgeAbs(double up, double down)
        {
            if (Math.Abs(up) == Math.Abs(down))
            { return true; }
            else { return false; }
        }
        private bool JudgeNoEqual(string UpValue, string DownValue)
        {
            if (JudgeNoNull(UpValue, DownValue))
            {
                if (JudgeMach(UpValue, DownValue))
                {
                    double upValue = Convert.ToDouble(UpValue);
                    double downValue = Convert.ToDouble(DownValue);
                    if (!JudgeAbs(upValue, downValue))
                    { return true; }
                    return false;
                }
                return false;
            }
            return false;
        }


        private void JudgeUpDown(NPOI.SS.UserModel.ISheet ISheet, int IdNumStartRowIndex, int RowIndex, string SizeSpec, string SizeSpecUP, string SizeSpecDown)
        {
            try
            {
                int IdNumStopRowIndex = IdNumStartRowIndex + RowIndex - 1;
                string UpValue = SizeSpecUP;
                string DownValue = SizeSpecDown;
                if (JudgeNoNull(UpValue, DownValue) & JudgeMach(UpValue, DownValue))
                {
                    double upValue = Convert.ToDouble(UpValue);
                    double downValue = Convert.ToDouble(DownValue);
                    if (JudgeAbs(upValue, downValue))
                    {
                        string Value = SizeSpec + "±" + Math.Abs(upValue).ToString();
                        setMergeValueToXlsCell(ISheet, 2, 3, IdNumStartRowIndex, IdNumStopRowIndex, Value);
                    }
                    else
                    {
                        for (int i = 0; i < RowIndex / 2; i++)
                        {
                            int StartRowIndex = IdNumStartRowIndex + i * 2;
                            setBCValueToXlsCell(ISheet, StartRowIndex, SizeSpecUP, SizeSpecDown, SizeSpec);
                        }
                    }
                }
                else
                {
                    string Value = SizeSpec + UpValue + DownValue;
                    setMergeValueToXlsCell(ISheet,2,3, IdNumStartRowIndex, IdNumStopRowIndex, Value);
                    
                }
            }

            catch (Exception ex)
            {
                OpResult.SetResult(ex.ToString(), true);
            }

        }
         /// <summary>
         /// 合并行 填充值
         /// </summary>
         /// <param name="xlsSheet"></param>
         /// <param name="firstRow"></param>
         /// <param name="lastRow"></param>
         /// <param name="StartMergeRowIndex"></param>
         /// <param name="EndMergeRowIndex"></param>
         /// <param name="StandardValue"></param>
        private void setMergeValueToXlsCell(NPOI.SS.UserModel.ISheet xlsSheet, int firstRow, int lastRow, int StartMergeRowIndex, int EndMergeRowIndex, string StandardValue)
        {
              //合并行，列
             mergeCell(xlsSheet, firstRow, lastRow, StartMergeRowIndex, EndMergeRowIndex);
             //填充值
             cellVaule(xlsSheet, firstRow, StartMergeRowIndex, StandardValue);
        }

        private void setBCValueToXlsCell(NPOI.SS.UserModel.ISheet xlsSheet, int StartRowIndex, string ValueUp, string ValueDown, string StandardValue)
        {
            //"B"
            InsertStandardValue(xlsSheet, StandardValue, StartRowIndex, 2, 2);
            //"C"
            InserUpDownValue(xlsSheet, ValueUp, ValueDown, StartRowIndex, 3);

            int StopRowIndex = StartRowIndex + 1;
          
        }

        private void InsertStandardValue(NPOI.SS.UserModel.ISheet xlsSheet, string StandardValue, int StartRowIndex, int StartColumnIndex, int StopColumnIndex)
        {
            int StopRowIndex = StartRowIndex + 1;


            setMergeValueToXlsCell(xlsSheet, StartRowIndex, StopRowIndex, StartColumnIndex, StopColumnIndex, StandardValue);

        }

        private void InserUpDownValue(NPOI.SS.UserModel.ISheet xlsSheet, string ValueUp, string ValueDown, int StartRowIndex, int Column)
        {
            int StopRowIndex = StartRowIndex + 1;
            cellVaule(xlsSheet, StartRowIndex, Column, ValueUp);
            cellVaule(xlsSheet, StopRowIndex, Column, ValueDown);
       
        }


        #endregion
        /// <summary>
        /// 填充 IQC打印条
        /// </summary>
        /// <param name="ISheet"></param>
        /// <param name="models"></param>
        private void SetISheetTitle(NPOI.SS.UserModel.ISheet ISheet, IQCSampleItemRecordModel models)
        {
              
            cellVaule(ISheet, 0, 0, "进料检验记录表");
            cellVaule(ISheet, 1, 15, "打印日期：" + DateTime.Now.ToString("yyyy-MM-dd"));
            cellVaule(ISheet, 1, 0, "日期：");
            cellVaule(ISheet, 1, 1, DateTime .Now .Date .ToString ("yyyy-MM-dd"));
            cellVaule(ISheet, 1, 8, "品号：" + models.SampleMaterial);
            cellVaule(ISheet, 2, 17, "NO:" + models.OrderID);
            cellVaule(ISheet, 3, 0, "品名：" + models.SampleMaterialName);
            cellVaule(ISheet, 3, 4, "规格：" + models.SampleMaterialSpec);
            cellVaule(ISheet, 3, 10, "数量：" + models.SampleMaterialNumber.ToString());
            cellVaule(ISheet, 3, 12, "ROHS结果：□OK □NG  □NA");
            cellVaule(ISheet, 3, 13, "膜厚");
            cellVaule(ISheet, 3, 14, "□ OK   □NG  □NA");
            cellVaule(ISheet, 4, 0, "供应商:" + models.SampleMaterialSupplier);
            cellVaule(ISheet, 4, 4, "图号/检验规范：" + models.SampleMaterialDrawID);
            cellVaule(ISheet, 4, 10, "检验方式：" + models.CheckWay);


        }
        /// <summary>
        ///  给单元格赋值
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="row">row</param>
        /// <param name="column">cell</param>
        /// <param name="value"></param>
        public void cellVaule(NPOI.SS.UserModel.ISheet sheet, int row, int column, string value)
        {
               NPOI.SS.UserModel.IRow Row = null;
                 if ( thisMyRow .Keys .Contains (row) )
                {
                    Row = thisMyRow[row];
                }
                 else 
                 {
                     Row = sheet.CreateRow(row);
                     thisMyRow.Add(row, Row);
                 }
            
            Row.CreateCell(column, NPOI.SS.UserModel.CellType.String).SetCellValue(value);//创建第row行第cell列string类型表格，并赋值  value
        }

        private NPOI.SS.UserModel.IRow ReturnRow(NPOI.SS.UserModel.ISheet sheet, int row)
        {
            NPOI.SS.UserModel.IRow Row = null;
            if (thisMyRow.Keys.Contains(row))
            {
                Row = thisMyRow[row];
            }
            else
            {
                Row = sheet.CreateRow(row);
                thisMyRow.Add(row, Row);
            }
            return Row;
        }

        /// <summary>
        /// 设置单元格为下拉框并限制输入值
        /// <param name="sheet"></param>
        /// <param name="SelectVaules">下拉框数值</param>
        /// </summary>
        private void setCellDropdownlist(NPOI.SS.UserModel.ISheet sheet, string[] SelectVaules)
        {
            //设置生成下拉框的行和列
            var cellRegions = new NPOI.SS.Util.CellRangeAddressList(0, 65535, 0, 0);
            //设置 下拉框内容
            NPOI.HSSF.UserModel.DVConstraint constraint = NPOI.HSSF.UserModel.DVConstraint.CreateExplicitListConstraint(
                SelectVaules);

            //绑定下拉框和作用区域，并设置错误提示信息
            NPOI.HSSF.UserModel.HSSFDataValidation dataValidate = new NPOI.HSSF.UserModel.HSSFDataValidation(cellRegions, constraint);
            dataValidate.CreateErrorBox("输入不合法", "请输入下拉列表中的值。");
            dataValidate.ShowPromptBox = true;

            sheet.AddValidationData(dataValidate);
        }


        /// <summary>
        /// 合并单元格
        /// </summary>
        /// <param name="sheet">表格接口</param>
        /// <param name="firstRow">启始列</param>
        /// <param name="lastRow">结束列</param>
        /// <param name="firstCell">启始行</param>
        /// <param name="lastCell">结束行</param>
        private void mergeCell(NPOI.SS.UserModel.ISheet sheet, int firstRow, int lastRow, int firstCell, int lastCell)
        {
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(firstRow, lastRow, firstCell, lastCell));//2.0使用 2.0以下为Region
        }
        /// <summary>
        /// 设置单元格样式
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="cell">单元格</param>
        private void setCellStyle(NPOI.HSSF.UserModel.HSSFWorkbook workbook, NPOI.SS.UserModel.ICell cell, double FontHeight)
        {
            NPOI.HSSF.UserModel.HSSFCellStyle fCellStyle = (NPOI.HSSF.UserModel.HSSFCellStyle)workbook.CreateCellStyle();
            NPOI.HSSF.UserModel.HSSFFont ffont = (NPOI.HSSF.UserModel.HSSFFont)workbook.CreateFont();
            ffont.FontHeight = FontHeight;
            ffont.FontName = "宋体";
            ffont.Color = NPOI.HSSF.Util.HSSFColor.Yellow.Index;
            fCellStyle.SetFont(ffont);
            fCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;//垂直对齐 居中
            fCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Right;//水平对齐 靠左
            cell.CellStyle = fCellStyle;
        }

        #endregion

    }
  
    public class MaterialSampleItemManager
    {
        IMaterialSampleSetReposity irep = null;
        public MaterialSampleItemManager ()
        {
            irep = new MaterialSampleSetReposity();
        }
        public List<MaterialSampleSet> GetMaterilalSampleItem(string SampleMaterialProductID)
        {
            return irep.Entities.Where(e => e.SampleMaterial == SampleMaterialProductID).ToList();
        }
    }


   /// <summary>
   ///抽验查询对像
   /// </summary>
   public class SampleQueries
    {
        #region
        /// <summary>
        /// 样品订单号
        /// </summary>
        public string OrderId
        { set; get; }
        /// <summary>
        /// 样品料号
        /// </summary>
        public string Material
        { set; get; }
        /// <summary>
        /// 样品名称
        /// </summary>
        public string MaterialName
        { set; get; }
        /// <summary>
        /// 样品规格
        /// </summary>
        public string MaterialSpec
        { set; get; }
        /// <summary>
        /// 样品的供应商
        /// </summary>
        public string MaterialSupplier
        { set; get; }
        /// <summary>
        /// 样品购入日期
        /// </summary>
        public string MaterialInDate
        { set; get; }
        /// <summary>
        /// 样品提供ERP中图号
        /// </summary>
        public string MaterialDrawID
        { set; get; }
        /// <summary>
        /// 抽样数量
        /// </summary>
        public string MaterialNumber
        { set; get; }
        #endregion
    }
}
