using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.IO;

namespace SmartCodeGenerator.Uility.ExcelToScript
{
    public static class WorkbookConnectionFactory
    {
        public static OleDbConnection CreateOleDbConnection(string workbookPath)
        {
            string fileExt = Path.GetExtension(workbookPath);
            string connStr;
            if (string.Equals(fileExt, ".xlsx", StringComparison.InvariantCultureIgnoreCase))
            {
                connStr = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=1\";", workbookPath);
            }
            else if (string.Equals(fileExt, ".xls", StringComparison.InvariantCultureIgnoreCase))
            {
                connStr = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=\"Excel 8.0;HDR=NO;IMEX=1\";", workbookPath);
            }
            else
            {
                throw new ArgumentException("Invalid workbook path.", "workbookPath");
            }

            var workbookConn = new OleDbConnection(connStr);
            workbookConn.Open();
            return workbookConn;
        }

        public static OleDbConnection CreateUpdatableOleDbConnection(string workbookPath)
        {
            string fileExt = Path.GetExtension(workbookPath);
            string connStr;
            if (string.Equals(fileExt, ".xlsx", StringComparison.CurrentCultureIgnoreCase))
            {
                connStr = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=NO;IMEX=2\";", workbookPath);
            }
            else if (string.Equals(fileExt, ".xls", StringComparison.CurrentCultureIgnoreCase))
            {
                connStr = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=\"Excel 8.0;HDR=NO;IMEX=2\";", workbookPath);
            }
            else
            {
                throw new ArgumentException("Invalid workbook path.", "workbookPath");
            }

            var workbookConn = new OleDbConnection(connStr);
            workbookConn.Open();
            return workbookConn;
        }
    }
}
