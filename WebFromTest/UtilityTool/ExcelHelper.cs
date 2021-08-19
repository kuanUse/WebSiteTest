using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using OfficeOpenXml;

namespace UtilityTool
{
    public class ExcelHelper
    {
        //Excel欄位對應資訊
        public class ExcelColMap
        {
            public string propertyName { get; set; }
            public int excelIndex { get; set; }
        }
        public class CellTextFormat
        {
            public string dateTimeFormat { get; set; } = "mm/dd/yyyy hh:mm";
            public string decimalFormat { get; set; } = "0.00";
        }
        /// <summary>
        /// DataTable轉ExcelPackage
        /// </summary>
        /// <param name="dt">資料集</param>
        /// <param name="dbColIndex">範本中DataTable欄位名所在的列</param>
        /// <param name="printDtCol">是否印出colNameMap或DataTable欄位名</param>
        /// <param name="templatePath">範本路徑或檔案名稱，預設值為""../DocTemplate/" + 範本檔案名稱"</param>
        /// <param name="sheetIdx">範本寫入的Sheet Index</param>
        /// <param name="cellTextFormat">日期時間及數字型態顯示格式;預設:{"mm/dd/yyyy hh:mm", "0.00"}</param>
        /// <param name="colNameMap">DataTable與Excel欄位對應資訊</param> 
        public ExcelPackage ExportDS(DataTable dt, int dbColIndex = -1, string templatePath = "", int sheetIdx = 1, bool printDtCol = false, ExcelPackage ep = null, CellTextFormat cellTextFormat = null, List<ExcelColMap> colNameMap = null)
        {
            ExcelWorksheet sheet = null;
            cellTextFormat = cellTextFormat ?? new CellTextFormat();

            //取得ExcelPackage內容
            if (string.IsNullOrWhiteSpace(templatePath))
            {
                ep = new ExcelPackage();
            }
            //若templatePath不為空值，則嘗試載入範本檔案
            else
            {
                var reportName = Path.GetFileName(templatePath);
                reportName = reportName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ? reportName : reportName.Trim() + ".xlsx";
                if (!File.Exists(templatePath))
                {
                    templatePath = HttpContext.Current.Server.MapPath(@"~/DocTemplate/" + reportName);
                    if (!File.Exists(templatePath))
                    {
                        throw new Exception("查無報表檔案");
                    }
                }
                using (FileStream fs = new FileStream(templatePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    //載入Excel檔案
                    ep = new ExcelPackage(fs);
                }
            }
           
            sheet = ep.Workbook.Worksheets.FirstOrDefault(x => x.Index == sheetIdx);//取得Sheet
            if (sheet == null)
                sheet = ep.Workbook.Worksheets.Add("Sheet" + ep.Workbook.Worksheets.Count + 1);//若無指定sheet，建立Sheet1

            colNameMap = colNameMap ?? new List<ExcelColMap>();
            if (dbColIndex > 0 && !colNameMap.Any())
            {
                int startColumn = sheet.Dimension.Start.Column;//開始欄編號，從1算起
                int endColumn = sheet.Dimension.End.Column;//結束欄編號，從1算起
                for (int col = startColumn; col <= endColumn; col++)
                {
                    string cellValue = sheet.Cells[dbColIndex, col].Text;
                    if (!string.IsNullOrEmpty(cellValue)
                            && !colNameMap.Exists(e => e.propertyName.Equals(cellValue, StringComparison.OrdinalIgnoreCase)))
                    {
                        colNameMap.Add(new ExcelColMap()
                        {
                            excelIndex = col,
                            propertyName = cellValue.ToUpper()
                        });
                    }
                }
                sheet.DeleteRow(dbColIndex, 1, true);
            }
            else if (!colNameMap.Any())
            {
                foreach (DataColumn col in dt.Columns)
                {
                    colNameMap.Add(new ExcelColMap()
                    {
                        excelIndex = col.Ordinal,
                        propertyName = col.ColumnName
                    });
                }
            }

            if (printDtCol)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    sheet.Cells[1, col.Ordinal + 1].Value = col.ColumnName;
                }
            }

            int index = sheet.Dimension == null ? 1 : sheet.Dimension.End.Row + 1;
            foreach (DataRow dr in dt.Rows)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    var colMap = colNameMap.FirstOrDefault(e => e.propertyName.Equals(col.ColumnName, StringComparison.OrdinalIgnoreCase));
                    if (colMap == null)
                    {
                        continue;
                    }
                    var val = dr[col];
                    var cell = sheet.Cells[index, colMap.excelIndex];
                    cell.Value = val;
                    switch (Type.GetTypeCode(col.DataType.GetType()))
                    {
                        case TypeCode.DateTime:
                            cell.Style.Numberformat.Format = cellTextFormat.dateTimeFormat;
                            break;
                        case TypeCode.Double:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Decimal:
                            cell.Style.Numberformat.Format = cellTextFormat.decimalFormat;
                            break;
                        default:
                            string valStr = Convert.ToString(val);
                            //轉換網頁換行語法為文本換行格式
                            if (valStr.Contains("<br>"))
                            {
                                valStr = valStr.Replace("<br>", "\r\n");
                            }
                            cell.Value = valStr;

                            int warpCount = Regex.Matches(valStr, "\r\n").Count;
                            cell.Style.WrapText = warpCount > 0 ? true : false;

                            double newRowHeight = 20 + warpCount * 15;
                            double rowHeight = sheet.Row(index).Height;
                            if (newRowHeight > rowHeight)
                            {
                                sheet.Row(index).Height = newRowHeight;
                            }

                            cell.Style.Numberformat.Format = "@";
                            break;
                    }

                }
                index++;
            }
            sheet.Calculate();
            return ep;
        }
        /// <summary>
        /// 將Excel資料轉換為對應的物件List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="worksheet">Excel Sheet</param>
        /// <param name="propertyNameStartRow">Object屬性名稱所在的列</param>
        /// <param name="dataStartRow">資料起始列</param>
        /// <param name="map">指定Object屬性與Excel欄位的對應關係</param>
        /// <returns></returns>
        public List<T> ConvertSheetToObjects<T>(ExcelWorksheet worksheet, int propertyNameStartRow = 1, int dataStartRow = 2, List<ExcelColMap> map = null) where T : new()
        {
            //DateTime Conversion
            var convertDateTime = new Func<double, DateTime>(excelDate =>
            {
                if (excelDate < 1)
                    throw new ArgumentException("Excel dates cannot be smaller than 0.");

                var dateOfReference = new DateTime(1900, 1, 1);

                if (excelDate > 60d)
                    excelDate = excelDate - 2;
                else
                    excelDate = excelDate - 1;
                return dateOfReference.AddDays(excelDate);
            });

            var props = typeof(T).GetProperties()
                .Select(prop =>
                {
                    var displayAttribute = (DisplayAttribute)prop.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
                    return new
                    {
                        Name = prop.Name,
                        DisplayName = displayAttribute == null ? prop.Name : displayAttribute.Name,
                        Order = displayAttribute == null || !displayAttribute.GetOrder().HasValue ? 999 : displayAttribute.Order,
                        PropertyInfo = prop,
                        PropertyType = prop.PropertyType,
                        HasDisplayName = displayAttribute != null
                    };
                })
            .Where(prop => !string.IsNullOrWhiteSpace(prop.DisplayName))
            .ToList();

            var retList = new List<T>();
            var columns = new List<ExcelColMap>();
            var start = worksheet.Dimension.Start;
            var end = worksheet.Dimension.End;
            var startCol = start.Column;
            var endCol = end.Column;
            var endRow = end.Row;

            // Assume first row has column names
            if (map == null)
            {
                for (int col = startCol; col <= endCol; col++)
                {
                    var cellValue = (worksheet.Cells[propertyNameStartRow, col].Value ?? string.Empty).ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(cellValue) && !columns.Any(e => e.propertyName.Equals(cellValue, StringComparison.OrdinalIgnoreCase)))
                    {
                        columns.Add(new ExcelColMap()
                        {
                            propertyName = cellValue,
                            excelIndex = col
                        });
                    }
                }
            }
            else
            {
                columns = map;
            }

            // Now iterate over all the rows
            for (int rowIndex = dataStartRow; rowIndex <= endRow; rowIndex++)
            {
                var item = new T();
                columns.ForEach(column =>
                {
                    var value = worksheet.Cells[rowIndex, column.excelIndex].Value;
                    var valueStr = value == null ? string.Empty : value.ToString().Trim();
                    var prop = props.First(p => p.Name.Equals(column.propertyName, StringComparison.OrdinalIgnoreCase));

                    // Excel stores all numbers as doubles, but we're relying on the object's property types
                    if (prop != null)
                    {
                        var propertyType = prop.PropertyType;
                        object parsedValue = null;

                        if (propertyType == typeof(int?) || propertyType == typeof(int))
                        {
                            int val;
                            if (!int.TryParse(valueStr, out val))
                            {
                                val = default(int);
                            }

                            parsedValue = val;
                        }
                        else if (propertyType == typeof(short?) || propertyType == typeof(short))
                        {
                            short val;
                            if (!short.TryParse(valueStr, out val))
                                val = default(short);
                            parsedValue = val;
                        }
                        else if (propertyType == typeof(long?) || propertyType == typeof(long))
                        {
                            long val;
                            if (!long.TryParse(valueStr, out val))
                                val = default(long);
                            parsedValue = val;
                        }
                        else if (propertyType == typeof(decimal?) || propertyType == typeof(decimal))
                        {
                            decimal val;
                            if (!decimal.TryParse(valueStr, out val))
                                val = default(decimal);
                            parsedValue = val;
                        }
                        else if (propertyType == typeof(double?) || propertyType == typeof(double))
                        {
                            double val;
                            if (!double.TryParse(valueStr, out val))
                                val = default(double);
                            parsedValue = val;
                        }
                        else if (propertyType == typeof(DateTime?) || propertyType == typeof(DateTime))
                        {
                            parsedValue = convertDateTime((double)value);
                        }
                        else if (propertyType.IsEnum)
                        {
                            try
                            {
                                parsedValue = Enum.ToObject(propertyType, int.Parse(valueStr));
                            }
                            catch
                            {
                                parsedValue = Enum.ToObject(propertyType, 0);
                            }
                        }
                        else if (propertyType == typeof(string))
                        {
                            parsedValue = valueStr;
                        }
                        else
                        {
                            try
                            {
                                parsedValue = Convert.ChangeType(value, propertyType);
                            }
                            catch
                            {
                                parsedValue = valueStr;
                            }
                        }
                        prop.PropertyInfo.SetValue(item, parsedValue, null);

                    }
                });

                retList.Add(item);
            }

            return retList;
        }
    }
}