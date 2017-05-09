using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace SmartCodeGenerator.Uility
{
    public class CodeGenerateHelper
    {
        /// <summary>
        /// 
        /// </summary>
        SqlDBHelper sqldbHelper;

        /// <summary>
        /// 
        /// </summary>
        public CodeGenerateHelper()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqldbhelper"></param>
        public CodeGenerateHelper(SqlDBHelper sqldbhelper)
        {
            sqldbHelper = sqldbhelper;
        }

        /// <summary>
        /// Clear Database Tables Heard 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public DataTable ClearHeard(DataTable dt)
        {
            dt.Columns.Remove("NumericScale");
            dt.Columns.RemoveAt(1);
            dt.Columns.Remove("NumericPrecision");
            dt.Columns.Remove("IsUnique");
            //dt.Columns.Remove("IsKey");
            dt.Columns.Remove("BaseServerName");
            dt.Columns.Remove("BaseCatalogName");
            dt.Columns.Remove("BaseSchemaName");
            dt.Columns.Remove("BaseTableName");
            dt.Columns.Remove("AllowDBNull");
            dt.Columns.Remove("ProviderType");
            dt.Columns.Remove("IsAliased");
            dt.Columns.Remove("IsExpression");
            dt.Columns.Remove("IsIdentity");
            dt.Columns.Remove("IsAutoIncrement");
            dt.Columns.Remove("IsRowVersion");
            dt.Columns.Remove("IsHidden");
            dt.Columns.Remove("IsReadOnly");
            dt.Columns.Remove("ProviderSpecificDataType");
            dt.Columns.Remove("XmlSchemaCollectionDatabase");
            dt.Columns.Remove("XmlSchemaCollectionOwningSchema");
            dt.Columns.Remove("XmlSchemaCollectionName");
            dt.Columns.Remove("UdtAssemblyQualifiedName");
            dt.Columns.Remove("IsLong");
            dt.Columns.Remove("DataType");
            dt.Columns.Remove("NonverSionedProviderType");
            dt.Columns.Remove("BaseColumnName");
            dt.Columns.Remove("IsColumnSet");

            return dt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="pkdt"></param>
        /// <returns></returns>
        public DataTable GetDtColumnsInfo(string tableName)
        {
            DataTable dtSchema = sqldbHelper.GetTableSchema(tableName);
            dtSchema.Columns.Add("IsPK");
            DataTable dtpk = sqldbHelper.GetPKs(tableName);
            foreach (DataRow r in dtSchema.Rows)
            {
                int count = 0;
                foreach (DataRow rr in dtpk.Rows)
                {
                    if (rr["COLUMN_NAME"].ToString() == r["ColumnName"].ToString())
                    {
                        r["IsPK"] = "Y";
                        break;
                    }
                    count++;
                }
                if (count == dtpk.Rows.Count)
                {
                    r["IsPK"] = "N";
                }
            }
            return dtSchema;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public string GenerateValidateString(DataRow dr)
        {
            string retStr = string.Empty;
            string dtName = conver(dr["DataTypeName"].ToString());
            int colSize = (int)dr["ColumnSize"];
            bool allowDBNull = (bool)dr["AllowDBNull"];
            if(!allowDBNull)
            {
                retStr = "[NotNullValidator()]";
            }

            if (dtName.ToLower() == "string")
            {
                if(retStr.Length >0)
                {
                    retStr += "\n		";
                }
                retStr += "[StringLengthValidator(0, "+colSize.ToString()+", MessageTemplate = \""+dtName+" length must between 0 and "+ colSize.ToString()+".\")]";
            }
            return retStr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="pkdt"></param>
        /// <returns></returns>
        public DataTable GetColumnsInfo(DataTable dt, DataTable pkdt)
        {

            dt.Columns.Remove("NumericScale");
            dt.Columns.RemoveAt(1);
            dt.Columns.Remove("NumericPrecision");
            dt.Columns.Remove("IsUnique");
            dt.Columns.Remove("IsKey");
            dt.Columns.Remove("BaseServerName");
            dt.Columns.Remove("BaseCatalogName");
            dt.Columns.Remove("BaseSchemaName");
            dt.Columns.Remove("BaseTableName");
            dt.Columns.Remove("AllowDBNull");
            dt.Columns.Remove("ProviderType");
            dt.Columns.Remove("IsAliased");
            dt.Columns.Remove("IsExpression");
            dt.Columns.Remove("IsIdentity");
            dt.Columns.Remove("IsAutoIncrement");
            dt.Columns.Remove("IsRowVersion");
            dt.Columns.Remove("IsHidden");
            dt.Columns.Remove("IsReadOnly");
            dt.Columns.Remove("ProviderSpecificDataType");
            dt.Columns.Remove("XmlSchemaCollectionDatabase");
            dt.Columns.Remove("XmlSchemaCollectionOwningSchema");
            dt.Columns.Remove("XmlSchemaCollectionName");
            dt.Columns.Remove("UdtAssemblyQualifiedName");
            dt.Columns.Remove("IsLong");
            dt.Columns.Remove("DataType");
            dt.Columns.Remove("NonverSionedProviderType");
            dt.Columns.Remove("BaseColumnName");
            dt.Columns.Add("IsPK");
            foreach (DataRow r in dt.Rows)
            {
                int count = 0;
                foreach (DataRow rr in pkdt.Rows)
                {
                    if (rr["COLUMN_NAME"].ToString() == r["ColumnName"].ToString())
                    {
                        r["IsPK"] = "Y";
                        break;
                    }
                    count++;
                }
                if (count == pkdt.Rows.Count)
                {
                    r["IsPK"] = "N";
                }
            }
            return dt;
        }

        /// <summary>
        /// Type Convert between sql server and .Net
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string conver(string str)
        {
            if (str == "nvarchar")
            {
                return "string";
            }
            else if (str == "char")
            {
                return "string";
            }
            else if (str == "nchar")
            {
                return "string";
            }
            else if (str == "varchar")
            {
                return "string";
            }
            else if (str == "int")
            {
                return "int";
            }
            else if (str == "tinyint")
            {
                return "byte";
            }
            else if (str == "smallint")
            {
                return "short";
            }
            else if (str == "bigint")
            {
                return "long";
            }
            else if (str == "bit")
            {
                return "int";
            }
            else if (str == "float")
            {
                return "double";
            }
            else if (str == "real") 
            {
                return "double";
            }
            else if (str == "decimal")
            {
                return "double";
            }
            else if (str == "richTextBox1eric")
            {
                return "double";
            }
            else if (str == "money")
            {
                return "decimal";
            }
            else if (str == "smallmoney")
            {
                return "decimal";
            }
            else if (str == "datetime")
            {
                return "DateTime";
            }
            else if (str == "smalldatetime")
            {
                return "DateTime";
            }
            else if (str == "binary")
            {
                return "byte[]";
            }
            else if (str == "varbinary")
            {
                return "byte[]";
            }
            else if (str == "image")
            {
                return "byte[]";
            }
            else if (str.ToLower() == "uniqueidentifier")
            {
                return "Guid";
            }
            return "string";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string convertToDbType(string str)
        {
            if (str == "nvarchar")
            {
                return "String";
            }
            else if (str == "char")
            {
                return "String";
            }
            else if (str == "nchar")
            {
                return "String";
            }
            else if (str == "varchar")
            {
                return "String";
            }
            else if (str == "int")
            {
                return "Int32";
            }
            else if (str == "tinyint")
            {
                return "Byte";
            }
            else if (str == "smallint")
            {
                return "Int16";
            }
            else if (str == "bigint")
            {
                return "Int64";
            }
            else if (str == "bit")
            {
                return "Int32";
            }
            else if (str == "float")
            {
                return "Double";
            }
            else if (str == "real")
            {
                return "Double";
            }
            else if (str == "decimal")
            {
                return "Double";
            }
            else if (str == "richTextBox1eric")
            {
                return "Double";
            }
            else if (str == "money")
            {
                return "Decimal";
            }
            else if (str == "smallmoney")
            {
                return "Decimal";
            }
            else if (str == "datetime")
            {
                return "DateTime";
            }
            else if (str == "smalldatetime")
            {
                return "DateTime";
            }
            else if (str == "binary")
            {
                return "Byte[]";
            }
            else if (str == "varbinary")
            {
                return "Byte[]";
            }
            else if (str == "image")
            {
                return "Byte[]";
            }
            else if (str.ToLower() == "uniqueidentifier")
            {
                return "Guid";
            }
            return "String";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string converForVB(string str)
        {
            if (str == "nvarchar")
            {
                return "string";
            }
            else if (str == "char")
            {
                return "string";
            }
            else if (str == "nchar")
            {
                return "string";
            }
            else if (str == "varchar")
            {
                return "string";
            }
            else if (str == "int")
            {
                return "Integer";
            }
            else if (str == "tinyint")
            {
                return "byte";
            }
            else if (str == "smallint")
            {
                return "short";
            }
            else if (str == "bigint")
            {
                return "long";
            }
            else if (str == "bit")
            {
                return "Integer";
            }
            else if (str == "float")
            {
                return "double";
            }
            else if (str == "real")
            {
                return "double";
            }
            else if (str == "decimal")
            {
                return "double";
            }
            else if (str == "richTextBox1eric")
            {
                return "double";
            }
            else if (str == "money")
            {
                return "decimal";
            }
            else if (str == "smallmoney")
            {
                return "decimal";
            }
            else if (str == "datetime")
            {
                return "DateTime";
            }
            else if (str == "smalldatetime")
            {
                return "DateTime";
            }
            else if (str == "binary")
            {
                return "byte[]";
            }
            else if (str == "varbinary")
            {
                return "byte[]";
            }
            else if (str == "image")
            {
                return "byte[]";
            }
            else if (str.ToLower() == "uniqueidentifier")
            {
                return "Guid";
            }
            return "string";
        }

        /// <summary>
        /// Write Config Xml File
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public string GetConfigXml(string[] strName, string[] strValue)
        {
            StringBuilder str = new StringBuilder();
            str.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine);
            str.Append("<configuration>" + Environment.NewLine);
            str.Append("  <appSettings>" + Environment.NewLine);
            int rows = strName.Length;
            for (int i = 0; i < rows; i++)
            {
                str.Append("      <add key=\"" + strName[i] + "\" value=\"" + strValue[i] + "\" />" + Environment.NewLine);
            }
            str.Append("  </appSettings>" + Environment.NewLine);
            str.Append("</configuration>" + Environment.NewLine);
            return str.ToString();
        }

        /// <summary>
        /// Get Config Operation Class
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public string GetConfigClass(string[] strName, string[] strValue)
        {
            StringBuilder str = new StringBuilder();
            str.Append("using System;" + Environment.NewLine);
            str.Append("using System.Collections.Generic;" + Environment.NewLine);
            str.Append("using System.Text;" + Environment.NewLine);
            str.Append("using System.Configuration;" + Environment.NewLine);
            str.Append("using System.Xml;" + Environment.NewLine + Environment.NewLine);
            str.Append("namespace Angel.Haibao" + Environment.NewLine);
            str.Append("{" + Environment.NewLine);
            str.Append("    /// <summary>" + Environment.NewLine);
            str.Append("    /// System Config Class" + Environment.NewLine);
            str.Append("    /// </summary>" + Environment.NewLine);
            str.Append("    public class SystemConfig" + Environment.NewLine);
            str.Append("    {" + Environment.NewLine);
            str.Append("        public SystemConfig(string path)" + Environment.NewLine);
            str.Append("        {" + Environment.NewLine);
            str.Append("            XmlDocument doc = new XmlDocument();" + Environment.NewLine);
            str.Append(Environment.NewLine + "            doc.Load(path);" + Environment.NewLine);
            str.Append("            XmlNodeList nodes = doc.GetElementsByTagName(\"add\");" + Environment.NewLine);
            int rows = strName.Length;
            for (int i = 0; i < rows; i++)
            {
                str.Append("            " + strName[i] + " = nodes[" + i.ToString() + "].Attributes[\"value\"].Value;" + Environment.NewLine);
            }
            str.Append("        }" + Environment.NewLine + Environment.NewLine);

            str.Append(Environment.NewLine + "        public void SaveAllConfig(string path)" + Environment.NewLine);
            str.Append("        {" + Environment.NewLine);
            str.Append("            XmlDocument doc = new XmlDocument();" + Environment.NewLine + Environment.NewLine);
            str.Append("            doc.Load(path);" + Environment.NewLine);
            str.Append("            XmlNodeList nodes = doc.GetElementsByTagName(\"add\");" + Environment.NewLine);

            for (int i = 0; i < rows; i++)
            {
                str.Append("            nodes[" + i.ToString() + "].Attributes[\"value\"].Value = this." + strName[i] + ";" + Environment.NewLine);
            }

            str.Append(Environment.NewLine + "            doc.Save(path);" + Environment.NewLine);
            str.Append("        }" + Environment.NewLine + Environment.NewLine);


            for (int i = 0; i < rows; i++)
            {
                str.Append("        private string " + strName[i].ToLower() + ";" + Environment.NewLine);

                str.Append("        /// <summary>" + Environment.NewLine);
                str.Append("        /// Get or Set " + strName[i] + Environment.NewLine);
                str.Append("        /// </summary>" + Environment.NewLine);
                str.Append("        public " + "string" + " " + strName[i] + Environment.NewLine);
                str.Append("        {" + Environment.NewLine);

                str.Append("            get" + Environment.NewLine);
                str.Append("            {" + Environment.NewLine);
                str.Append("                return this." + strName[i].ToLower() + ";" + Environment.NewLine);
                str.Append("            }" + Environment.NewLine);

                str.Append("            set" + Environment.NewLine);
                str.Append("            {" + Environment.NewLine);
                str.Append("                this." + strName[i].ToLower() + " = value;" + Environment.NewLine);
                str.Append("            }" + Environment.NewLine);

                str.Append("        }" + Environment.NewLine);
            }
            str.Append("    }" + Environment.NewLine);
            str.Append("}");


            return str.ToString();
        }

    }
}
