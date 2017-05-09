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
using System.Windows.Shapes;
using System.IO;
using SmartCodeGenerator.Uility;
using System.Data;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.VisualStudio.TextTemplating;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;

namespace SmartCodeGenerator
{
    /// <summary>
    /// Interaction logic for winBatchGenerator.xaml
    /// </summary>
    public partial class winBatchGenerator : Window
    {
        /// <summary>
        /// 
        /// </summary>
        public SqlDBHelper dbHelper = new SqlDBHelper();
        public PropertyNodeItem CurSelectedNode;
        /// <summary>
        /// 
        /// </summary>
        public System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();

        /// <summary>
        /// 
        /// </summary>
        private BackgroundWorker worker;

        /// <summary>
        /// 
        /// </summary>
        public winBatchGenerator(SqlDBHelper dbhelper,string serverName)
        {
            InitializeComponent();
            this.dbHelper = dbhelper;
            this.lblServerName.Content = serverName;
            LoadTemplates();
            LoadDatabases();
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadTemplates()
        {
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Templates\"))
            {
                List<PropertyNodeItem> itemList = new List<PropertyNodeItem>();
                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Templates\"))
                {
                    DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Templates\");
                    PropertyNodeItem rootn = new PropertyNodeItem();
                    rootn.DisplayName = di.Name;
                    rootn.Value = AppDomain.CurrentDomain.BaseDirectory + @"\Templates\"; 
                    GetFolders(AppDomain.CurrentDomain.BaseDirectory + @"\Templates\", rootn);
                    itemList.Add(rootn);                    
                }
                this.tvTemplates.ItemsSource = itemList;   
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="i"></param>
        private void GetFolders(string path, PropertyNodeItem i)
        {
            string[] folders = Directory.GetDirectories(path);

            GetFiles(path, i);
            if (folders.Length > 0)
            {
                i.Icon = @"Images\FolderOpen.png";
                foreach (string folder in folders)
                {
                    DirectoryInfo di = new DirectoryInfo(folder);
                    PropertyNodeItem subn = new PropertyNodeItem();
                    subn.DisplayName = di.Name;
                    subn.Value = path + @"\" + di.Name;
                    subn.Parent = i;
                    i.Children.Add(subn);                    
                    GetFolders(folder, subn);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="i"></param>
        private void GetFiles(string path, PropertyNodeItem i)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] fis = di.GetFiles();
            int count = 0;
            foreach (FileInfo info in fis)
            {
                if (info.Extension.ToLower() == ".tt")
                {
                    PropertyNodeItem subn = new PropertyNodeItem();
                    subn.Icon = @"images\file-manager.png";
                    subn.DisplayName = info.Name;
                    subn.Value = info.FullName;
                    subn.Parent = i;
                    i.Children.Add(subn);
                    this.totalCount++;
                    count++;
                }
            }
            if (count == 0 && i.Icon == null)
            {
                i.Icon = @"Images\FolderClosed.png";
            }
            else
            {
                i.Icon = @"Images\FolderOpen.png";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadDatabases()
        {
            DataTable dt = dbHelper.GetDatabases();
            if (dt != null && dt.Rows.Count > 0)
            {
                this.cboxDatabases.Items.Clear();
                foreach (DataRow r in dt.Rows)
                {
                    this.cboxDatabases.Items.Add(r["Name"].ToString());
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboxDatabase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Text.RegularExpressions.MatchCollection ma = System.Text.RegularExpressions.Regex.Matches(dbHelper.SqlConnectionString, @"Database=.*?;");
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
                dbHelper.SqlConnectionString = dbHelper.SqlConnectionString.Replace("Database=;", "Database=" + this.cboxDatabases.SelectedItem.ToString() + ";");
            }
            else
            {
                dbHelper.SqlConnectionString =  dbHelper.SqlConnectionString.Replace(databaseName, cboxDatabases.SelectedItem.ToString());
            }
            DataTable tList =  dbHelper.GetTables();
            this.listLeftTables.Items.Clear();
            foreach (DataRow r in tList.Rows)
            {
                this.listLeftTables.Items.Add("T."+r["Name"].ToString());
            }
            DataTable vList = dbHelper.GetViews();
            foreach (DataRow r in vList.Rows)
            {
                this.listLeftTables.Items.Add("V." + r["Name"].ToString());
            }
            this.listRightTables.Items.Clear();
            SetAddDelBtn();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddAll_Click(object sender, RoutedEventArgs e)
        {
            if (this.listLeftTables.Items.Count > 0)
            {
                for (int i = 0; i < this.listLeftTables.Items.Count; i++)
                {
                    this.listRightTables.Items.Add(this.listLeftTables.Items[i]);
                }
                this.listLeftTables.Items.Clear();
            }
            SetAddDelBtn();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddSelected_Click(object sender, RoutedEventArgs e)
        {
            if (this.listLeftTables.SelectedItems.Count > 0)
            {
                for (int i = 0; i < this.listLeftTables.SelectedItems.Count; i++)
                {
                    this.listRightTables.Items.Add(this.listLeftTables.SelectedItems[i]);
                }
                for (int i = 0; i < this.listLeftTables.SelectedItems.Count; i++)
                {
                    this.listLeftTables.Items.Remove(this.listLeftTables.SelectedItems[i]);
                }

            }
            else
            {
                MessageBox.Show("Please select tables.");
            }
            SetAddDelBtn();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            if (this.listRightTables.SelectedItems.Count > 0)
            {
                for (int i = 0; i < this.listRightTables.SelectedItems.Count; i++)
                {
                    this.listLeftTables.Items.Add(this.listRightTables.SelectedItems[i]);
                }
                for (int i = 0; i < this.listRightTables.SelectedItems.Count; i++)
                {
                    this.listRightTables.Items.Remove(this.listRightTables.SelectedItems[i]);
                }
            }
            else
            {
                MessageBox.Show("Please select tables.");
            }
            SetAddDelBtn();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemoveAll_Click(object sender, RoutedEventArgs e)
        {
            if (this.listRightTables.Items.Count > 0)
            {
                for (int i = 0; i < this.listRightTables.Items.Count; i++)
                {
                    this.listLeftTables.Items.Add(this.listRightTables.Items[i]);
                }
                this.listRightTables.Items.Clear();
            }
            SetAddDelBtn();
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetAddDelBtn()
        {
            if (this.listLeftTables.Items.Count > 0)
            {
                this.btnAddAll.IsEnabled  = true;
                this.btnAddSelected.IsEnabled = true;
            }
            else
            {
                this.btnAddAll.IsEnabled = false;
                this.btnAddSelected.IsEnabled = false;
            }
            if (this.listRightTables.Items.Count > 0)
            {
                this.btnRemoveAll.IsEnabled = true;
                this.btnRemoveSelected.IsEnabled = true;
            }
            else
            {
                this.btnRemoveAll.IsEnabled = false;
                this.btnRemoveSelected.IsEnabled = false;
            }
            this.listRightTables.SelectedIndex = -1;
            this.listLeftTables.SelectedIndex = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvTemplates_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.CurSelectedNode = (PropertyNodeItem)this.tvTemplates.SelectedItem;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtExportPath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.folderBrowserDialog.ShowDialog();
            this.txtExportPath.Text = this.folderBrowserDialog.SelectedPath;
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (this.listRightTables.Items.Count == 0)
            {
                MessageBox.Show("Please select object(s)");
                return;
            }
            if (this.txtNameSpaceRoot.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please input root Namespace");
                this.txtNameSpaceRoot.Focus();
                return;
            }
            if (this.tvTemplates.SelectedItem == null)
            {
                MessageBox.Show("Please select template package or template.");
                return;
            }
            if (!Directory.Exists(this.txtExportPath.Text + @"\" + this.cboxDatabases.Text))
            {
                Directory.CreateDirectory(this.txtExportPath.Text + @"\" + this.cboxDatabases.Text);
            }

            PropertyNodeItem sn = (PropertyNodeItem)this.tvTemplates.SelectedItem;
                                 
            this.proCount = 0;
            this.pbarBatchGenerate.Maximum = totalCount;
            this.pbarBatchGenerate.Minimum = 0;
            this.pbarBatchGenerate.Value = 0;
            updatePbDelegate = new UpdateProgressBarDelegate(pbarBatchGenerate.SetValue); 
            //Start to Generate Codes
            GenerateByTreeNode(sn);
             
            Process.Start("explorer.exe", this.txtExportPath.Text);          
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tn"></param>
        private void GenerateByTreeNode(PropertyNodeItem sn)
        {
            foreach (PropertyNodeItem n in sn.Children)
            {
                if (n.Children.Count == 0)
                {
                    //UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(this.pbarBatchGenerate.SetValue);
                    BatchGenerate(n);
                    //Dispatcher.Invoke(updatePbDelegate,
                    //System.Windows.Threading.DispatcherPriority.Background,
                    //new object[] { ProgressBar.ValueProperty, proCount });
                }
                else
                {
                    string tmpDir = this.txtExportPath.Text + @"\" + this.cboxDatabases.Text + @"\" + this.GetFolder(n);
                    if (!System.IO.Directory.Exists(tmpDir))
                    {
                        Directory.CreateDirectory(tmpDir);
                    }
                    GenerateByTreeNode(n);
                }
            }
        }
        /// <summary>
        /// /
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private string GetFolder(PropertyNodeItem n)
        {
            if (n.Parent.Value == CurSelectedNode.Value)
            {
                return string.Empty;
            }
            else if (n.Children.Count == 0)
            {
                return this.GetPath(n.Parent);
            }
            else
            {
                return GetPath(n.Parent) + @"\" +n.DisplayName;
            }
        }
        /// <summary>
        /// /
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private string GetPath(PropertyNodeItem n)
        {

            if (n.Value == CurSelectedNode.Value)
            {
                return n.DisplayName;
            }
            else
            {
                return GetPath(n.Parent) + @"\" + n.DisplayName;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tempTreeNode"></param>
        private void BatchGenerate(PropertyNodeItem sn)
        {
            CreateTopDirectory(sn);

            string strTmp = GetNamespace(sn);
                   
            for (int i = 0; i < this.listRightTables.Items.Count; i++)
            {                
                try
                {
                    TextTemplatingEngineHost host = new TextTemplatingEngineHost();
                    host.TemplateFileValue = AppDomain.CurrentDomain.BaseDirectory +  @"\Templates\"+ GetPath(sn);
                    Engine engine = new Engine();
                    host.Session = new TextTemplatingSession();

                    string inputTemplate = File.ReadAllText(host.TemplateFileValue);
                    string oName = this.listRightTables.Items[i].ToString();

                    Parameter connStrparameter = new Parameter() { Text = "ConnectionStr", Value =  dbHelper.SqlConnectionString };
                    host.Session.Add("ConnectionStr", connStrparameter);

                    Parameter nameSpaceParameter = new Parameter() { Text = "NameSpace", Value = this.txtNameSpaceRoot.Text };//+ strTmp.Replace("Templates." + this.CurSelectedNode.DisplayName, string.Empty)
                    host.Session.Add("NameSpace", nameSpaceParameter);
                    
                    Parameter tableNameParameter = new Parameter() { Text = "TableName", Value = oName.Substring(2)};
                    host.Session.Add("TableName", tableNameParameter);
                    if ((oName.Substring(0, 1) == "T" && sn.DisplayName.Substring(2, 1) != "T" && sn.DisplayName.Substring(2, 1) != "A" && sn.DisplayName.Substring(2, 1) != "N") ||
                        (oName.Substring(0, 1) == "V" && sn.DisplayName.Substring(2, 1) != "V" && sn.DisplayName.Substring(2, 1) != "A" && sn.DisplayName.Substring(2, 1) != "N")  
                        )
                    {
                        continue;
                    }
                    string strCodes = engine.ProcessTemplate(inputTemplate, host);

                    string filefullname = string.Empty;// this.txtExportPath.Text + @"\" + this.cboxDatabases.Text + @"\" + GetFolder(sn).Replace("Templates", string.Empty) + @"\" + this.listRightTables.Items[i].ToString() + "." + this.GetOutputExtension(inputTemplate);
                    
                    if (sn.DisplayName.Substring(2, 1) == "N")
                    {
                        filefullname = this.txtExportPath.Text + @"\" + this.cboxDatabases.Text + @"\" + GetFolder(sn).Replace("Templates", string.Empty) + @"\" + sn.DisplayName.Substring(4, sn.DisplayName.Length - 7) + "." + this.GetOutputExtension(inputTemplate);
                    }
                    else
                    {
                        filefullname = this.txtExportPath.Text + @"\" + this.cboxDatabases.Text + @"\" + GetFolder(sn).Replace("Templates", string.Empty) + @"\" + this.listRightTables.Items[i].ToString().Substring(2) + sn.DisplayName.Substring(4, sn.DisplayName.Length - 7) + "." + this.GetOutputExtension(inputTemplate);
                    }

                    if (string.IsNullOrEmpty(filefullname)) return;

                    FileStream fs;

                    if (!File.Exists(filefullname))
                    {
                        fs = new FileStream(filefullname, FileMode.Create);
                    }
                    else
                    {
                        fs = new FileStream(filefullname, FileMode.Open);
                    }
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(strCodes);
                    sw.Close();
                    fs.Close();
                    
                    if (sn.DisplayName.Substring(0, 1) == "1")
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }               
            }
            proCount+=1;
            if (this.pbarBatchGenerate.Value != pbarBatchGenerate.Maximum)
            {
                Dispatcher.Invoke(updatePbDelegate,
                       System.Windows.Threading.DispatcherPriority.Background,
                       new object[] { ProgressBar.ValueProperty, proCount });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tempTreeNode"></param>
        private void BaseGenerate(PropertyNodeItem sn)
        {
            try
            {

                CreateTopDirectory(sn);

                string strTmp = GetNamespace(sn);

                TextTemplatingEngineHost host = new TextTemplatingEngineHost();
                host.TemplateFileValue = AppDomain.CurrentDomain.BaseDirectory + @"\Templates\"+ GetPath(sn);
                Engine engine = new Engine();
                host.Session = new TextTemplatingSession();

                string inputTemplate = File.ReadAllText(host.TemplateFileValue);

                Parameter connStrparameter = new Parameter() { Text = "ConnectionStr", Value =dbHelper.SqlConnectionString };
                host.Session.Add("ConnectionStr", connStrparameter);

                Parameter nameSpaceParameter = new Parameter() { Text = "NameSpace", Value = this.txtNameSpaceRoot.Text + strTmp.Replace("Templates." + this.CurSelectedNode.DisplayName, string.Empty) };
                host.Session.Add("NameSpace", nameSpaceParameter);
                Parameter tableNameParameter = new Parameter() { Text = "TableName", Value = string.Empty };
                host.Session.Add("TableName", tableNameParameter);

                string strCodes = engine.ProcessTemplate(inputTemplate, host);

                string filefullname = this.txtExportPath.Text + @"\" + this.cboxDatabases.Text + @"\" + GetFolder(sn).Replace("Templates", string.Empty) + @"\" + sn.DisplayName.Trim().Replace(".tt", string.Empty) + "." + this.GetOutputExtension(inputTemplate);
                if (string.IsNullOrEmpty(filefullname)) return;

                FileStream fs;

                if (!File.Exists(filefullname))
                {
                    fs = new FileStream(filefullname, FileMode.Create);
                }
                else
                {
                    fs = new FileStream(filefullname, FileMode.Open);
                }
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(strCodes);
                sw.Close();
                fs.Close();
                ++proCount;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tn"></param>
        /// <returns></returns>
        private string GetNamespace(PropertyNodeItem sn)
        {
            if (sn.Parent == null)
            {
                return string.Empty;
            }
            else if (sn.Children.Count == 0)
            {
                return GetPath(sn.Parent);
            }
            else
            {
                return GetPath(sn.Parent) + @"." + sn.DisplayName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        private string GetOutputExtension(string inputTemplate)
        {
            System.Text.RegularExpressions.MatchCollection ma = System.Text.RegularExpressions.Regex.Matches(inputTemplate, @"<#@ output extension="".*?"" ");
            foreach (Match item in ma)
            {
                if (item.Success)
                {
                    return item.Value.Substring(item.Value.IndexOf(@"""")).Replace(@"""", "");
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tn"></param>
        /// <returns></returns>
        private int GetTemplatesCount(PropertyNodeItem sn)
        {
            int count = 0;
            foreach (PropertyNodeItem n in sn.Children )
            {
                if (n.Children.Count == 0 && n.DisplayName.LastIndexOf(".tt") == n.DisplayName.Length - 3)
                {
                    if (n.Parent.DisplayName == "Base" || n.DisplayName.ToLower().IndexOf("proj") == 0)
                    {
                        count++;
                    }
                    else
                    {
                        count += this.listRightTables.Items.Count;
                    }
                }
                else
                {

                    count += GetTemplatesCount(n);
                }
            }
            return count; 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tn"></param>
        private void CreateTopDirectory(PropertyNodeItem sn)
        {

            if (sn.Parent != null && sn.Parent.DisplayName != "Templates")
            {
                if (!Directory.Exists(this.txtExportPath.Text + @"\" + this.cboxDatabases.Text + @"\" + GetFolder(sn.Parent).Replace("Templates", string.Empty)))
                {
                    CreateTopDirectory(sn.Parent);
                }
                else
                {
                    Directory.CreateDirectory(this.txtExportPath.Text + @"\" + this.cboxDatabases.Text + @"\" + GetFolder(sn).Replace("Templates", string.Empty));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private double proCount = 0;
        private double totalCount = 0;
        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        private UpdateProgressBarDelegate updatePbDelegate ;  
    }
}
