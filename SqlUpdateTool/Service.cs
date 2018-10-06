using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Windows.Threading;

namespace SqlUpdateTool
{
    public static class Service
    {
        public static void ProcessUiTasks()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate (object parameter) {
                frame.Continue = false;
                return null;
            }), null);
            Dispatcher.PushFrame(frame);
        }

        public static string GetFileName()
        {
            var fname = ShowFileDialog.DisplayForFileSelection("Excel|*.xls;*.xlsx", null, 
                "Please select the file to import.");
            if (!IsFile(fname))
                return null;

            return fname;
        }

        public static bool IsFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
                return false;

            return true;
        }

        public static DataTable ExcelToDataTable(string pathName, string sheetName)
        {
            DataTable tbContainer = new DataTable();
            string strConn = string.Empty;

            if (!IsFile(pathName))
                return null;

            if (string.IsNullOrEmpty(sheetName)) { sheetName = "Sheet1"; }

            FileInfo file = new FileInfo(pathName);
            if (!file.Exists) { throw new Exception("Error, file doesn't exists!"); }

            string extension = file.Extension;
            switch (extension)
            {
                case ".xls":
                    strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathName + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";
                    break;
                case ".xlsx":
                    strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + pathName + ";Extended Properties='Excel 12.0;HDR=Yes;IMEX=1;'";
                    break;
                default:
                    strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathName + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";
                    break;
            }
            OleDbConnection cnnxls = new OleDbConnection(strConn);
            OleDbDataAdapter oda = new OleDbDataAdapter($"select * from [{sheetName}$]", cnnxls);
            DataSet ds = new DataSet();

            try
            {
                oda.Fill(tbContainer);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains($"'{sheetName}$' is not a valid name"))
                    return null;

                throw;
            }

            return tbContainer;
        }

        public class TableColumn
        {
            public TableColumn(string dtColumnName, string table, string column)
            {
                DtColumnName = dtColumnName;
                Table = table;
                Column = column;
            }

            public string DtColumnName;
            public string Table;
            public string Column;
        }

        public delegate void StatusHandler(string status);

        public static void ProcessDatatable(DataTable dt, StatusHandler errorHandler, StatusHandler sqlHandler, bool singleOnly, bool updateNull)
        {
            if (errorHandler == null)
            {
                Console.WriteLine("WTF mate?");
                return;
            }

            if (dt == null)
            {
                errorHandler("No DataTable!?");
                return;
            }

            var col = new List<string>();
            foreach (DataColumn column in dt.Columns)
                col.Add(column.ColumnName);

            var tables = new List<string>();

            var keyCol = new List<TableColumn>();
            foreach (var kc in col.Where(x => x.StartsWith("Keys#")))
            {
                var tc = kc.Replace("Keys#", "");
                var tcArray = tc.Split(new [] { "#" }, StringSplitOptions.None);
                if (tcArray.Length != 2)
                {
                    errorHandler($"Table and column not specified for key column: {kc}. Process terminated.");
                    return;
                }

                var tNames = tcArray[0].Split(new[] { "|" }, StringSplitOptions.None);
                foreach (var t in tNames)
                {
                    keyCol.Add(new TableColumn(kc, t, tcArray[1]));
                    if (!tables.Contains(t))
                        tables.Add(t);
                }
            }

            var dataCol = new List<TableColumn>();
            foreach (var dc in col.Where(x => !x.StartsWith("Keys#")))
            {
                var tcArray = dc.Split(new[] { "#" }, StringSplitOptions.None);
                if (tcArray.Length != 2)
                {
                    errorHandler($"Table and column not specified for data column: {dc}. Process terminated.");
                    return;
                }
                
                dataCol.Add(new TableColumn(dc, tcArray[0], tcArray[1]));
            }

            int i = 0;
            foreach (DataRow dr in dt.Rows)
            {
                i++;

                foreach (var t in tables)
                {
                    var tKeys = keyCol.Where(x => x.Table == t).ToList();
                    var tData = dataCol.Where(x => x.Table == t).ToList();
                    if (!tData.Any())
                        continue;

                    if (singleOnly)
                    {
                        var sqlQuery = $"SELECT * FROM {t} +" +
                                       " WHERE " + string.Join(", ", tKeys.Select(x => x.Column + "='" + dr[x.DtColumnName] + "'"));

                        //var qDt = Database.ExecuteDataReader(connectionString, sqlQuery);
                        var qDt = new DataTable();
                        if (qDt == null)
                            continue; // errMsg

                        if (qDt.Rows.Count > 1)
                            continue; // errMsg
                    }
                    
                    var nullKeyVal = tKeys.Where(y => 
                        string.IsNullOrWhiteSpace(dr[y.DtColumnName].ToString())).ToList();
                    if (nullKeyVal.Any())
                    {
                        errorHandler($"Row {i} (1-indexed): Null key value found for column(s): {string.Join(",", nullKeyVal.Select(x => x.Column))}. Skipping record.");
                        continue;
                    }

                    var nullDataCol = tData.Where(y =>
                        string.IsNullOrWhiteSpace(dr[y.DtColumnName].ToString()) ||
                        dr[y.DtColumnName].ToString().Trim().ToUpper() == "NULL").ToList();
                    if (nullDataCol.Any() && !updateNull)
                    {
                        tData = tData.Except(nullDataCol).ToList();
                        errorHandler($"Row {i} (1-indexed): Null data value found for column(s): {string.Join(",", nullDataCol.Select(x => x.Column))}.");
                    }
                    
                    var sqlUpdate = $"UPDATE {t} SET " + 
                              string.Join(", ", tData.Select(x => x.Column + "='" + dr[x.DtColumnName] + "'")) + // need to address nulls here
                              " WHERE " +
                              string.Join(" AND ", tKeys.Select(x => x.Column + "='" + dr[x.DtColumnName] + "'"));

                    sqlHandler(sqlUpdate);
                }
            }
        }

    }
}