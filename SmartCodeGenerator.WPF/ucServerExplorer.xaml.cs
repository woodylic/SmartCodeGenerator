using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AvalonDock;
using Microsoft.Win32;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;
using SmartCodeGenerator.Uility;

namespace SmartCodeGenerator
{
    /// <summary>
    /// Interaction logic for ucServerExplorer.xaml
    /// </summary>
    public partial class ucServerExplorer : DockableContent
    {
        /// <summary>
        /// 
        /// </summary>
        public winConnector connector;
        /// <summary>
        /// 
        /// </summary>
        public ucServerExplorer()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectServer_Click(object sender, RoutedEventArgs e)
        {
            //if (connector == null )
            //{
                connector = new winConnector();
            //}
            connector.ShowDialog();
            if (connector.IsConnected)
            {
                DataTable dt = connector.dbHelper.GetDatabases();
                if (dt != null && dt.Rows.Count > 0)
                {
                    this.cboxDatabases.Items.Clear();
                    foreach (DataRow r in dt.Rows)
                    {
                        this.cboxDatabases.Items.Add(r["Name"].ToString());
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboxDatabases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.cboxDatabases.SelectedValue.ToString()))
            {
                this.LoadTables();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void LoadTables()
        { 
            System.Text.RegularExpressions.MatchCollection ma = System.Text.RegularExpressions.Regex.Matches(connector.dbHelper.SqlConnectionString, @"Database=.*?;");
            string databaseName = string.Empty;
            foreach (Match item in ma)
            {
                if (item.Success)
                {
                    databaseName = item.Value.Replace("Database=", string.Empty).Replace(";", string.Empty);
                    break;
                }
            }
            if (databaseName == string.Empty)
            {
                connector.dbHelper.SqlConnectionString = connector.dbHelper.SqlConnectionString.Replace("Database=;", "Database=" + this.cboxDatabases.SelectedItem.ToString() + ";");
            }
            else
            {
                connector.dbHelper.SqlConnectionString = connector.dbHelper.SqlConnectionString.Replace(databaseName, cboxDatabases.SelectedItem.ToString());
            }

            List<PropertyNodeItem> niList = new List<PropertyNodeItem>();

            PropertyNodeItem sRoot = new PropertyNodeItem();
            sRoot.Icon = @"images/display.png";
            sRoot.DisplayName = this.cboxDatabases.SelectedItem.ToString();
            sRoot.Value = "@ServerRootNode";
            niList.Add(sRoot);
             
            DataTable tList = connector.dbHelper.GetTables();


            PropertyNodeItem tRoot = new PropertyNodeItem();
            tRoot.Icon = @"images/Table.png";
            tRoot.DisplayName = "Tables";
            tRoot.Value = "@TableRootNode";
            sRoot.Children.Add(tRoot);

            foreach (DataRow r in tList.Rows)
            {
                PropertyNodeItem i = new PropertyNodeItem();
                i.Icon = @"images/Table.png";
                i.DisplayName = r["Name"].ToString();
                i.Value = "@TableNode";
                tRoot.Children.Add(i);  
            }
            DataTable vList = connector.dbHelper.GetViews();
            PropertyNodeItem vRoot = new PropertyNodeItem();
            vRoot.Icon = @"images/View.png";
            vRoot.DisplayName = "Views";
            vRoot.Value = "@ViewRootNode";
            sRoot.Children.Add(vRoot);

            foreach (DataRow r in vList.Rows)
            {
                PropertyNodeItem i = new PropertyNodeItem();
                i.Icon = @"images/View.png";
                i.DisplayName = r["Name"].ToString();
                i.Value = "@ViewNode";
                vRoot.Children.Add(i);  
            }

            this.tvObjects.ItemsSource = niList;

            this.tvObjects.GetItemFromObject(sRoot).IsExpanded = true;
             
        }

        /// <summary>
        /// 
        /// </summary>
        public PropertyNodeItem CurSelectedNode;
        /// <summary>
        /// 
        /// </summary>
        public event SelectedServerObjectHander SelectServerObject;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvObjects_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            CurSelectedNode = (PropertyNodeItem)tvObjects.SelectedItem;

            if (CurSelectedNode.DisplayName != "Tables" &&
                CurSelectedNode.DisplayName != "Views" &&
                CurSelectedNode.DisplayName != this.cboxDatabases.Text)
            {
                List<CustomProperty> pniList = new List<CustomProperty>();

                CustomProperty conn = new CustomProperty();
                conn.Name = "ConnectionStr";
                conn.Value = this.connector.dbHelper.SqlConnectionString;
                pniList.Add(conn);

                CustomProperty nspace = new CustomProperty();
                nspace.Name = "NameSpace";
                nspace.Value = this.cboxDatabases.Text;
                pniList.Add(nspace);

                CustomProperty tName = new CustomProperty();
                tName.Name = "TableName";
                tName.Value = CurSelectedNode.DisplayName;
                pniList.Add(tName);

                if (SelectServerObject != null && pniList.Count > 0)
                {
                    SelectServerObject(pniList);
                }
            } 
        }


    }

    public delegate void SelectedServerObjectHander(List<CustomProperty> pniList);
}
