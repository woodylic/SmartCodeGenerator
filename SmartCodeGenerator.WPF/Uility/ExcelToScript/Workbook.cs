using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace SmartCodeGenerator.Uility.ExcelToScript
{
    class Workbook
    {
        public string WorkbookPath { get; set; }
        public Dictionary<string, DataTable> Tables { get; protected set; }

        public Workbook(string bookPath)
        {
            Init(bookPath);
        }

        private void Init(string bookPath)
        {
            if (!File.Exists(bookPath))
            {
                throw new ArgumentException("Invalid workbook path.", "bookPath");
            }

            this.WorkbookPath = bookPath;
            this.Tables = new Dictionary<string, DataTable>();

            using (var conn = WorkbookConnectionFactory.CreateOleDbConnection(bookPath))
            {
                var schema = conn.GetSchema("Tables");
                if (null != schema)
                {
                    foreach (DataRow row in schema.Rows)
                    {
                        var tableName = row["TABLE_NAME"].ToString();
                        if (!string.IsNullOrEmpty(tableName))
                        {
                            var tableValue = GetTableDirect(conn, tableName);
                            Tables.Add(tableName, tableValue);
                        }
                    }
                }
            }
        }

        private static DataTable GetTableDirect(OleDbConnection conn, string tableName)
        {
            var tableCmd = conn.CreateCommand();
            tableCmd.CommandText = tableName;
            tableCmd.CommandType = CommandType.TableDirect;
            DataTable dt;
            using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(tableCmd))
            {
                dt = new DataTable();
                dataAdapter.Fill(dt);
            }
            return dt;
        }

        private static OleDbConnection CreateOleDbConnection(string workbookPath)
        {
            throw new NotImplementedException();
        }
    }
}
