using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CIMC.Helper
{

    public class ExcelHelper
    {       

        #region Excel导出

        /// <summary>
        /// 实体类集合导出到EXCLE
        /// </summary>
        /// <param name="cellHeard">单元头的Key和Value：{ { "UserName", "姓名" }, { "Age", "年龄" } };</param>
        /// <param name="dataList">数据源</param>
        /// <param name="sheetName">工作表名称</param>
        /// <param name="downloadUrl">文件的下载地址</param>
        /// <returns></returns>
        public static (bool Success, string Message) ListToExcel<T>(Dictionary<string, string> cellHeard, List<T> dataList, string downloadUrl, string sheetName = "Sheet1") where T : new()
        {
            string msg = string.Empty;
            try
            {
                // 1.检测是否存在文件夹，若不存在就建立个文件夹
                string directoryName = Path.GetDirectoryName(downloadUrl);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                using (var package = new ExcelPackage())
                {
                    FileInfo file = new FileInfo(downloadUrl);
                    //添加Sheets
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetName);
                    List<string> keys = cellHeard.Keys.ToList();

                    #region 表头

                    for (int i = 0; i < keys.Count; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = cellHeard[keys[i]]; //第一行列名 为Key的值
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;//字体为粗体
                        worksheet.Cells[1, i + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;//水平居中
                        worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;//设置样式类型
                        worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(159, 197, 232));//设置单元格背景色
                    }
                    #endregion

                    int rowIndex = 2; // 从第二行开始赋值(第一行已设置为单元头)
                    foreach (var en in dataList)
                    {
                        for (int i = 0; i < keys.Count; i++) // 根据指定的属性名称，获取对象指定属性的值
                        {
                            string cellValue = ""; // 单元格的值
                            object properotyValue = null; // 属性的值
                            PropertyInfo properotyInfo = null; // 属性的信息

                            // 3.2 若不是子类的属性，直接根据属性名称获取对象对应的属性
                            properotyInfo = en.GetType().GetProperty(keys[i]);
                            if (properotyInfo != null)
                            {
                             
                                properotyValue = properotyInfo.GetValue(en, null);
                            }

                            // 3.3 属性值经过转换赋值给单元格值
                            if (properotyValue != null)
                            {
                                cellValue = properotyValue.ToString();
                                // 3.3.1 对时间初始值赋值为空
                                if (cellValue.Trim() == "1976/1/1 0:00:00" || cellValue.Trim() == "1976/1/1 23:59:59")
                                {
                                    cellValue = "";
                                }
                            }

                            // 3.4 填充到Excel的单元格里                        
                            worksheet.Cells[rowIndex, i + 1].Value = cellValue;
                        }
                        rowIndex++;
                    }
                    package.SaveAs(file);

                    msg = "导入成功";
                    //log.Info($"导入文件{sheetName}成功");
                    return (true, msg);
                }
            }
            catch (Exception ex)
            {
                msg = "导入异常：" + ex.Message;
                //log.Error($"导入文件{sheetName}异常:{ex.Message}");

                return (false, msg);
            }
        }

        #endregion Excel导出

        #region Excel导入

        /// <summary>
        /// 从Excel取数据并记录到List集合里
        /// </summary>
        /// <param name="cellHeard">单元头的Key和Value：{ { "UserName", "姓名" }, { "Age", "年龄" } };</param>
        /// <param name="uploadPath">保存文件绝对路径</param>    
        /// <param name="cellIndex">第几行数据开始</param>
        /// <returns>转换好的List对象集合</returns>
        public static (List<T> List, string Message) ExcelToList<T>(Dictionary<string, string> cellHeard, string uploadPath, int cellIndex = 2) where T : new()
        {
            StringBuilder errorMsg = new StringBuilder(); // 错误信息,Excel转换到实体对象时，会有格式的错误信息
            List<T> enlist = new List<T>(); // 转换后的集合
            List<string> keys = cellHeard.Keys.ToList(); // 要赋值的实体对象属性名称

            FileInfo file = new FileInfo(uploadPath);
            using (var package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int i = cellIndex; i <= rowCount; i++) // 从第2行开始，第1行为单元头
                {
                    T en = new T();
                    string errStr = ""; // 当前行转换时，是否有错误信息，格式为：第1行数据转换异常：XXX列；
                    for (int j = 0; j < keys.Count; j++)
                    {
                        // 3.给指定的属性赋值
                        PropertyInfo properotyInfo = en.GetType().GetProperty(keys[j]);
                        if (properotyInfo != null)
                        {
                            try
                            {
                                // Excel单元格的值转换为对象属性的值，若类型不对，记录出错信息
                                //properotyInfo.SetValue(en, worksheet.Cells[i, j + 1].Value, null);
                                properotyInfo.SetValue(en, GetExcelCellToProperty(properotyInfo.PropertyType, worksheet.Cells[i, j + 1]), null);
                            }
                            catch (Exception e)
                            {
                                errStr += cellHeard[keys[j + 1]] + "列；";
                                if (errStr.Length == 0)
                                {
                                    errStr = "第" + i + "行数据转换异常：" + e.Message;
                                }

                                //log.Error($"导入文件异常:{e.Message}");
                            }
                        }
                    }
                    // 若有错误信息，就添加到错误信息里
                    if (errStr.Length > 0)
                    {
                        errorMsg.AppendLine(errStr);
                    }
                    enlist.Add(en);
                }
            }
            return (enlist, errorMsg.ToString());
        }

        #endregion Excel导入

        /// <summary>
        /// 读取合并单元格
        /// </summary>
        /// <param name="wSheet"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static string GetMegerValue(ExcelWorksheet wSheet, int row, int column)
        {
            string range = wSheet.MergedCells[row, column];
            if (range == null)
                if (wSheet.Cells[row, column].Value != null)
                    return wSheet.Cells[row, column].Value.ToString();
                else
                    return "";
            object value = wSheet.Cells[(new ExcelAddress(range)).Start.Row, (new ExcelAddress(range)).Start.Column].Value;
            if (value != null)
                return value.ToString().Trim();
            else
                return "";

        }

        /// <summary>
        /// 列属性赋值
        /// </summary>
        /// <param name="distanceType"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        private static object GetExcelCellToProperty(Type distanceType, ExcelRange cell)
        {
            object rs = distanceType.IsValueType ? Activator.CreateInstance(distanceType) : null;
            // 1.判断传递的单元格是否为空
            if (cell == null || string.IsNullOrEmpty(cell.Value.ToString()))
            {
                return rs;
            }

            string valueDataType = distanceType.Name;

            switch (valueDataType.ToLower())
            {
                case "string":
                    rs = Convert.ChangeType(cell.GetValue<string>(), distanceType);
                    break;
                case "int16":
                    rs = Convert.ChangeType(cell.GetValue<int>(), distanceType);
                    break;
                case "int32":
                    rs = Convert.ChangeType(cell.GetValue<int>(), distanceType);
                    break;
                case "int64":
                    rs = Convert.ChangeType(cell.GetValue<long>(), distanceType);
                    break;
                case "decimal":
                    rs = Convert.ChangeType(cell.GetValue<decimal>(), distanceType);
                    break;
                case "double":
                    rs = Convert.ChangeType(cell.GetValue<double>(), distanceType);
                    break;
                case "datetime":
                    rs = Convert.ChangeType(cell.GetValue<DateTime>(), distanceType);
                    break;
                //case "boolean":                  
                //    rs = Convert.ChangeType(cell.GetValue<bool>(), distanceType);
                //    break;
                //case "byte":
                //    rs = Convert.ChangeType(cell.GetValue<byte>(), distanceType);
                //    break;
                //case "char":
                //    rs = Convert.ChangeType(cell.GetValue<char>(), distanceType);
                //    break;
                case "single":
                    rs = Convert.ChangeType(cell.GetValue<Single>(), distanceType);
                    break;
                case "guid":
                    rs = Convert.ChangeType(cell.GetValue<Guid>(), distanceType);
                    break;
                default:
                    rs = Convert.ChangeType(cell.GetValue<string>(), distanceType);
                    break;
            }

            return rs;
        }


        #region Excel中数字时间转换成时间格式
        /// <summary>
        /// Excel中数字时间转换成时间格式
        /// </summary>
        /// <param name="timeStr">数字,如:42095.7069444444/0.650694444444444</param>
        /// <returns>日期/时间格式</returns>
        private static DateTime ToDateTimeValue(string strNumber)
        {
            if (!string.IsNullOrWhiteSpace(strNumber))
            {
                Decimal tempValue;
                DateTime tempret;
                if (DateTime.TryParse(strNumber, out tempret))
                {
                    return tempret;
                }
                if (strNumber.Length == 8 && strNumber.Contains(".") == false)//20160430
                {

                    strNumber = strNumber.Insert(4, "-").Insert(6 + 1, "-");
                    if (DateTime.TryParse(strNumber, out tempret))
                    {
                        return tempret;
                    }
                    else return default(DateTime);
                }
                //先检查 是不是数字;
                if (Decimal.TryParse(strNumber, out tempValue))
                {
                    //天数,取整
                    int day = Convert.ToInt32(Math.Truncate(tempValue));
                    //这里也不知道为什么. 如果是小于32,则减1,否则减2
                    //日期从1900-01-01开始累加 
                    // day = day < 32 ? day - 1 : day - 2;
                    DateTime dt = new DateTime(1900, 1, 1).AddDays(day < 32 ? (day - 1) : (day - 2));

                    //小时:减掉天数,这个数字转换小时:(* 24) 
                    Decimal hourTemp = (tempValue - day) * 24;//获取小时数
                    //取整.小时数
                    int hour = Convert.ToInt32(Math.Truncate(hourTemp));
                    //分钟:减掉小时,( * 60)
                    //这里舍入,否则取值会有1分钟误差.
                    Decimal minuteTemp = Math.Round((hourTemp - hour) * 60, 2);//获取分钟数
                    int minute = Convert.ToInt32(Math.Truncate(minuteTemp));

                    //秒:减掉分钟,( * 60)
                    //这里舍入,否则取值会有1秒误差.
                    Decimal secondTemp = Math.Round((minuteTemp - minute) * 60, 2);//获取秒数
                    int second = Convert.ToInt32(Math.Truncate(secondTemp));
                    if (second >= 60)
                    {
                        second -= 60;
                        minute += 1;
                    }
                    if (minute >= 60)
                    {
                        minute -= 60;
                        hour += 1;
                    }

                    //时间格式:00:00:00
                    string resultTimes = string.Format("{0}:{1}:{2}",
                            (hour < 10 ? ("0" + hour) : hour.ToString()),
                            (minute < 10 ? ("0" + minute) : minute.ToString()),
                            (second < 10 ? ("0" + second) : second.ToString()));
                    var str = string.Format("{0} {1}", dt.ToString("yyyy-MM-dd"), resultTimes);
                    try
                    {
                        return DateTime.Parse(str);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("DateTime.Parse出错,str：" + str, ex);
                    }

                }
            }
            return default(DateTime);
        }
        #endregion
    }
}
