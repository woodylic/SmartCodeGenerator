using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace SmartCodeGenerator.Uility.ExcelToScript
{
    class SqlGenerator
    {
        private const int BUNCH_SIZE = 100;

        public static void GenerateInsertStatements(string tableName, DataTable tbl,ref StringBuilder writer)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName");
            if (null == tbl) throw new ArgumentNullException("tbl");
            if (null == writer) throw new ArgumentNullException("writer");

            var colQuery = from col in tbl.Columns.Cast<DataColumn>()
                           select col.ColumnName;
            var columns = string.Join(", ", colQuery.ToArray());

            int bunchCounter = 0;
            foreach (DataRow row in tbl.Rows)
            {
                bunchCounter++;
                var valuesList = new List<string>();
                foreach (DataColumn col in tbl.Columns)
                {
                    valuesList.Add(GetObjectString(row[col], col));
                }
                var values = string.Join(", ", valuesList.ToArray());
                 
                writer.Append(string.Format("INSERT INTO {0}({1}) VALUES({2})", tableName, columns, values) + Environment.NewLine);
                if (bunchCounter >= BUNCH_SIZE)
                {
                    writer.Append("GO" + Environment.NewLine);
                    bunchCounter = 0;
                }
            }
        }

        public static void GenerateUpdateStatements(string tableName, string[] pks, DataTable tbl,ref  StringBuilder writer)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName");
            if (null == pks || 0 == pks.Length) throw new ArgumentNullException("pks");
            if (null == tbl) throw new ArgumentNullException("tbl");
            if (null == writer) throw new ArgumentNullException("writer");

            int bunchCounter = 0;
            foreach (DataRow row in tbl.Rows)
            {
                bunchCounter++;
                var valueSetters = new List<string>();
                foreach (DataColumn col in tbl.Columns)
                {
                    if (pks.Contains(col.ColumnName)) continue;
                    var valueString = GetObjectString(row[col], col);
                    valueSetters.Add(string.Format("{0} = {1}", col.ColumnName, valueString));
                }
                var valueSetClause = string.Join(", ", valueSetters.ToArray());

                var pkIdentifiers = new List<string>();
                foreach (var pkCol in pks)
                {
                    var valueString = GetObjectString(row[pkCol], tbl.Columns[pkCol]);
                    pkIdentifiers.Add(string.Format("{0} = {1}", pkCol, valueString));
                }

                var pkClause = string.Join(" AND ", pkIdentifiers.ToArray()); 
                writer.Append(string.Format("UPDATE {0} SET {1} WHERE {2}", tableName, valueSetClause, pkClause) + Environment.NewLine);
                if (bunchCounter >= BUNCH_SIZE)
                {
                    writer.Append("GO" + Environment.NewLine);
                    bunchCounter = 0;
                }
            }
        }

        private static string GetObjectString(object value, DataColumn col)
        {
            if (null == value || DBNull.Value == value || "NULL" == value.ToString()) return "NULL";

            if (col.DataType == typeof(DateTime))
            {
                DateTime dtValue = (DateTime)value;
                return string.Format("'{0}'", dtValue.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }

            if (col.DataType == typeof(string))
            {
                var stringValue = value.ToString();
                stringValue = stringValue.Replace("'", "''");
                return string.Format("'{0}'", stringValue);
            }

            return value.ToString();
        }
    }
}
