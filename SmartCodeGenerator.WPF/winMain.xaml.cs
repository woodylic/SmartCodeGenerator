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
using System.CodeDom.Compiler; 
using System.Diagnostics;
using System.Windows.Forms.Integration;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.VisualStudio.TextTemplating; 
using AvalonDock;
using SmartCodeGenerator.Uility; 
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Data;

namespace SmartCodeGenerator
{
    /// <summary>
    /// Interaction logic for winMain.xaml
    /// </summary>
    public partial class winMain : Window
    {
        /// <summary>
        /// Open File Dialog
        /// </summary>
        public System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
        /// <summary>
        /// Save File Dialog
        /// </summary>
        public System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
        /// <summary>
        /// Folder Browser Dialog
        /// </summary>
        public System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
        /// <summary>
        /// 
        /// </summary>
        public winBatchGenerator batchGenerator ;
        /// <summary>
        /// 
        /// </summary>
        public winProjTemplateGenerator projTemGenerator;

        /// <summary>
        /// 
        /// </summary>
        public winExcelToScript excelToScriptor;
        /// <summary>
        /// Win main
        /// </summary>
        public winMain()
        {

            ThemeFactory.ChangeTheme("classic");
            InitializeComponent();
            //Select Template In Template Explorer
            this.lucTemplatesExplorer.SelectTemplateNode += new TemplateHander(lucTemplatesExplorer_SelectTemplateNode);
            //Select File In Solution Explorer
            this.lucSolutionExplorer.SelectFileNode += new ViewCodeHander(lucSolutionExplorer_SelectFileNode);
            //
            this.lucServerExplorer.SelectServerObject += new SelectedServerObjectHander(lucServerExplorer_SelectServerObject);
            //
            this.InitNewDocument.LoadProperties(this.lucDocumentProperties);

            //CodeGenerateHelper cghelper = new CodeGenerateHelper();
            //SqlDBHelper sqlHelper = new SqlDBHelper( "Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=srmgateway;Data Source=EBIZSQLD3.amer.corp.eds.com");            
            //DataTable dt = sqlHelper.GetTables("select * from SRM_ErrorLog");
            //DataType          System.String
            //DataTypeName      String
            //ColumnSize        
            //IsKey
            //AllowDBNull        False/True
            //ColumnName
        }
        /// <summary>
        /// Select Server Object
        /// </summary>
        /// <param name="pniList"></param>
        void lucServerExplorer_SelectServerObject(List< CustomProperty> pniList)
        {
            if (this.DocumentHost.ContainsActiveDocument)
            {
                ((ucBaseDocument)this.DocumentHost.SelectedItem).SetParameterValues(pniList);
            }
        }
        /// <summary>
        /// Select File Node
        /// </summary>
        /// <param name="pni"></param>
        void lucSolutionExplorer_SelectFileNode(PropertyNodeItem pni)
        {
            if (this.DocumentHost.Items.Count > 0)
            {
                foreach (Control c in this.DocumentHost.Items)
                {
                    if (((ucBaseDocument)c).DocPath != null && ((ucBaseDocument)c).DocPath.ToLower() == pni.Value.ToLower())
                    {
                        ((ucBaseDocument)c).Activate();
                        return;
                    }
                }
            }
            if (File.Exists(pni.Value))
            {
                FileInfo finfo = new FileInfo(pni.Value);
                ucBaseDocument doc = new ucBaseDocument();
                doc.DocExt = finfo.Extension;
                doc.DocPath = pni.Value;
                doc.DocType = DocumentType.CodeDocument;
                doc.DocChanged = false;
                doc.Title = finfo.Name;                
                TextEditor tb = new TextEditor();
                tb.IsReadOnly =  true;                
                tb.SyntaxHighlighting = tb.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(finfo.Extension);
                tb.Load(pni.Value);
                doc.Content = tb;
                doc.LoadProperties(this.lucDocumentProperties);
                this.DocumentHost.Items.Add(doc);
                doc.Activate();
            }
        }
        /// <summary>
        /// Select Template Node
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        void lucTemplatesExplorer_SelectTemplateNode(string filePath,string fileName)
        {
            if (this.DocumentHost.Items.Count > 0)
            {
                foreach (Control c in this.DocumentHost.Items)
                {
                    if (((ucBaseDocument)c).DocPath != null && ((ucBaseDocument)c).DocPath.ToLower() == filePath.ToLower())
                    {
                        ((ucBaseDocument)c).Activate();
                        return;
                    }
                }
            }

            if (filePath.IndexOf(".tt") > 0)
            {
                ucBaseDocument doc = new ucBaseDocument();
                doc.DocExt = ".tt";
                doc.DocPath = filePath;
                doc.DocType = DocumentType.TemplateDocument;
                doc.DocChanged = false;                 
                doc.Title = fileName;                
                TextEditor tb = new TextEditor();
                tb.IsReadOnly  = false;                
                tb.Load(filePath);
                tb.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition( ToolHelper.GetHighlightingByExtension( GetOutputExtension(tb.Text).Replace(".",string.Empty) ) );
                doc.Content = tb;
                doc.LoadProperties(this.lucDocumentProperties);
                this.DocumentHost.Items.Add(doc);
                doc.Activate(); 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaded(object sender, EventArgs e)
        {
             
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuSingleExplorer_Click(object sender, RoutedEventArgs e)
        {
   
        }
        /// <summary>
        /// Show DockableContent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShowDockableContent(object sender, RoutedEventArgs e)
        {
            var selectedContent = ((MenuItem)e.OriginalSource).DataContext as DockableContent;
            if (selectedContent.State != DockableContentState.Docked)
            {
                selectedContent.Show(DockManager, AnchorStyle.Right);
            }
            selectedContent.Activate();
        }
        /// <summary>
        /// Show DocumentContent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShowDocumentContent(object sender, RoutedEventArgs e)
        {
            var selectedDocument = ((MenuItem)e.OriginalSource).DataContext as DocumentContent;
            selectedDocument.Activate();
        }

        /// <summary>
        /// Close All Docs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemCloseAllDocs_Click(object sender, RoutedEventArgs e)
        {
            this.DocumentHost.Items.Clear();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutMenu_Click(object sender, RoutedEventArgs e)
        {
            winAboutSmartCodeGenerator winAbout = new winAboutSmartCodeGenerator();
            winAbout.ShowDialog();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCreateNewDocumentContent(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeStandardTheme(object sender, RoutedEventArgs e)
        { 
            string name = (string)((MenuItem)sender).Tag;
            ThemeFactory.ChangeTheme(name);
        }

        /// <summary>
        /// SelectionChanged in Document Host
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DocumentHost_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ucBaseDocument doc = ((ucBaseDocument)DocumentHost.SelectedItem);
            if (doc != null && this.lucDocumentProperties != null) doc.LoadProperties(this.lucDocumentProperties);
        }

        #region - Tool Base - 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenDoc_Click(object sender, RoutedEventArgs e)
        {
            if (openFileDialog == null) this.openFileDialog = new System.Windows.Forms.OpenFileDialog();

            this.openFileDialog.ShowDialog();
            if (this.DocumentHost.Items.Count > 0)
            {
                foreach (Control c in this.DocumentHost.Items)
                {
                    if (((ucBaseDocument)c).DocPath != null && ((ucBaseDocument)c).DocPath.ToLower() == openFileDialog.FileName.ToLower())
                    {
                        ((ucBaseDocument)c).Activate();
                        return;
                    }
                }
            }

            if (openFileDialog.CheckFileExists)
            {
                FileInfo finfo = new FileInfo(openFileDialog.FileName);
                ucBaseDocument doc = new ucBaseDocument()
                {
                    Title = finfo.Name,
                    Content = new TextEditor()
                    { 
                        Text = File.ReadAllText(openFileDialog.FileName),
                        SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(finfo.Extension),
                        Encoding =  System.Text.UnicodeEncoding.UTF8
                    }
                };
                doc.Title = finfo.Name;
                doc.DocExt =finfo.Extension;
                doc.DocPath = finfo.FullName;
                if (doc.DocExt == ".tt")
                {
                    doc.DocType = DocumentType.TemplateDocument;
                }
                else
                {
                    doc.DocType = DocumentType.CodeDocument;
                }
                doc.DocChanged = false;
                doc.LoadProperties(this.lucDocumentProperties);                    
                  
                this.DocumentHost.Items.Add(doc);
                doc.Activate();
                 
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNewDoc_Click(object sender, RoutedEventArgs e)
        {
            ucBaseDocument doc = new ucBaseDocument()
            {
                Title = string.Format("Doc_{0}", DateTime.Now.ToString("yyyyMMdd")),
                Content = new TextEditor()
                {
                    IsReadOnly= false,
                    SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("txt") 
                } 
            };
            doc.DocType = DocumentType.CodeDocument;
            doc.DocChanged = false;
            this.DocumentHost.Items.Add(doc);
            doc.Activate();
            doc.LoadProperties(this.lucDocumentProperties);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveDoc_Click(object sender, RoutedEventArgs e)
        {
            if (this.DocumentHost.ContainsActiveContent)
            {
                ucBaseDocument doc = ((ucBaseDocument)this.DocumentHost.SelectedItem);

                if (File.Exists(doc.DocPath))
                {
                    this.saveFileDialog.FileName = doc.DocPath;
                }
                if (!string.IsNullOrEmpty(doc.DocExt))
                {
                    this.saveFileDialog.Filter = doc.DocExt + "|*." + doc.DocExt;
                }
                else
                {
                    this.saveFileDialog.Filter = string.Empty;
                }
                this.saveFileDialog.ShowDialog();
                
                if (!string.IsNullOrEmpty(this.saveFileDialog.FileName))
                {
                    FileStream fs;
                    if (!File.Exists(this.saveFileDialog.FileName))
                    {
                        fs = new FileStream(this.saveFileDialog.FileName, FileMode.Create);
                    }
                    else
                    {
                        fs = new FileStream(this.saveFileDialog.FileName, FileMode.Open);
                    }
                    StreamWriter sw = new StreamWriter(fs);

                    sw.WriteLine(doc.Content.ToString());

                    sw.Close();
                    fs.Close();
                    FileInfo finfo = new FileInfo(this.saveFileDialog.FileName);
                    doc.DocPath = this.saveFileDialog.FileName;
                    doc.DocExt = finfo.Extension;
                    doc.LoadProperties(this.lucDocumentProperties);
                }
            }
        }
        /// <summary>
        /// /
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteDoc_Click(object sender, RoutedEventArgs e)
        {
            if (this.DocumentHost.ContainsActiveContent)
            {
                ucBaseDocument doc = ((ucBaseDocument)this.DocumentHost.SelectedItem);

                if (File.Exists(doc.DocPath))
                {
                    MessageBoxResult mbr = MessageBox.Show("Confirm to delect current active document?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                    if (mbr == MessageBoxResult.Yes)
                    {
                        File.Delete(doc.DocPath);
                    }
                    doc.Close();
                }                
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDocAddToProj_Click(object sender, RoutedEventArgs e)
        {
            if(this.DocumentHost.Items.Count == 0)
            {
                MessageBox.Show("Please new or open a code document add to current selected project.");
            }
            if (this.lucSolutionExplorer.CurProject == null)
            {
                MessageBox.Show("Please select target project or a file below target project.");
            }

            ucBaseDocument doc = this.DocumentHost.SelectedItem as ucBaseDocument;

            MessageBoxResult mbr = MessageBox.Show("Are you sure to add " + doc.Title + " to " + lucSolutionExplorer.CurProject.DisplayName + " ?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (mbr == MessageBoxResult.No)
            {
                return;
            }

            TextEditor txtEditor = doc.Content as TextEditor;
            FileInfo finfo = new FileInfo(lucSolutionExplorer.CurProject.Value);

            string newPaht = finfo.Directory.FullName + @"\" + doc.Title;
            FileStream fs;
            if (!File.Exists(newPaht))
            {
                fs = new FileStream(newPaht, FileMode.Create);
            }
            else
            {
                fs = new FileStream(newPaht, FileMode.Open);
            }
            StreamWriter sw = new StreamWriter(fs);

            sw.WriteLine(txtEditor.Text);

            sw.Close();
            fs.Close();
            FileInfo newinfo = new FileInfo(newPaht);
            doc.DocPath = newPaht;
            doc.DocExt = finfo.Extension;
            doc.LoadProperties(this.lucDocumentProperties);

            UpdateProjectFile(doc, lucSolutionExplorer.CurProject.Value);

            this.lucSolutionExplorer.Refresh();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="projFilePath"></param>
        public void UpdateProjectFile(ucBaseDocument doc, string projFilePath)
        {
            DataSet ds = new DataSet();
            ds.ReadXml(projFilePath);

            DataTable dt = ds.Tables["Compile"];
            if (dt != null)
            {
                DataRow dr = dt.NewRow();
                dr["Include"] = doc.Title;
                dr["ItemGroup_Id"] = 1;
                dt.Rows.Add(dr);
            }
            ds.WriteXml(projFilePath);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projNode"></param>
        /// <param name="selectedNode"></param>
        /// <returns></returns>
        public string GetFloderPathToProj(PropertyNodeItem projNode, PropertyNodeItem selectedNode)
        {
            if (File.Exists(selectedNode.Value))
            {
                FileInfo finfo = new FileInfo(selectedNode.Value);
                if (finfo.Directory.Name == projNode.DisplayName)
                {
                    return @"\" + finfo.Directory.Name;
                }
                else
                {
                    return GetFloderPathToProj(projNode, selectedNode.Parent) + @"\" + finfo.Directory.Name;
                }
            }
            if (Directory.Exists(selectedNode.Value))
            {
                DirectoryInfo finfo = new DirectoryInfo(selectedNode.Value);
                if (finfo.Name == projNode.DisplayName)
                {
                    return @"\" + finfo.Name;
                }
                else
                {
                    return GetFloderPathToProj(projNode, selectedNode.Parent) + @"\" + finfo.Name;
                }
            }
            return string.Empty;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDocRemoveFormProj_Click(object sender, RoutedEventArgs e)
        {

            if (lucSolutionExplorer.CurSelectedNode == null ||
                lucSolutionExplorer.CurSelectedNode.Children.Count > 0 ||
                lucSolutionExplorer.CurSelectedNode.Value == lucSolutionExplorer.CurProject.Value )
            {
                MessageBox.Show("Please select one code file.");
                return;
            }

            MessageBoxResult mbr = MessageBox.Show("Are you sure to remove "+lucSolutionExplorer.CurSelectedNode.DisplayName +" from "+lucSolutionExplorer.CurProject.DisplayName +" ?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (mbr == MessageBoxResult.No)
            {
                return;
            }

            if (this.lucSolutionExplorer.CurProject != null && File.Exists(lucSolutionExplorer.CurProject.Value))
            {
                FileInfo finfo = new FileInfo(lucSolutionExplorer.CurProject.Value);

                DataSet ds = new DataSet();
                ds.ReadXml(lucSolutionExplorer.CurProject.Value);
                
                DataTable dt = ds.Tables["Compile"];
                if (dt != null)
                {
                    string tmpStr = string.Empty;
                    foreach (DataRow r in dt.Rows)
                    {
                        if (r["Include"].ToString().IndexOf(this.lucSolutionExplorer.CurSelectedNode.DisplayName)>=0)
                        {
                            tmpStr = r["Include"].ToString();
                            dt.Rows.Remove(r);
                            break;
                        }
                    }
                    string realPath = GetRealPath(finfo.Directory.FullName, tmpStr);
                    if (File.Exists(realPath))
                    {
                        File.Delete(realPath);
                    }
                }
                ds.WriteXml(lucSolutionExplorer.CurProject.Value);
                //this.lucSolutionExplorer.Refresh();
                //lucSolutionExplorer.LoadFiles(lucSolutionExplorer.RootNode.Value);
                this.lucSolutionExplorer.Refresh();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curDirectory"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetRealPath(string curDirectory, string path)
        {
            DirectoryInfo dinfo = new DirectoryInfo(curDirectory);
            if (path.IndexOf(@"..\") == 0)
            {
                return GetRealPath(dinfo.Parent.FullName, path.Substring(3));
            }
            else
            {
                return curDirectory + @"\" + path;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProjOpen_Click(object sender, RoutedEventArgs e)
        {
            this.openFileDialog.DefaultExt = ".sln";
            this.openFileDialog.ShowDialog();
            if (this.openFileDialog.CheckFileExists)
            {
                lucSolutionExplorer.LoadFiles(this.openFileDialog.FileName);
            }
            else
            {
                MessageBox.Show("Please select a valid Solution File.");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProjNew_Click(object sender, RoutedEventArgs e)
        {
            projTemGenerator = new winProjTemplateGenerator();
            projTemGenerator.ShowDialog();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProjSave_Click(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProjClose_Click(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGenerateCode_Click(object sender, RoutedEventArgs e)
        {
            if (!this.DocumentHost.ContainsActiveDocument)
            {
                MessageBox.Show("Please open a template document.");
                return;
            }
            ucBaseDocument doc = ((ucBaseDocument)this.DocumentHost.SelectedItem);
            if (doc.DocType == DocumentType.CodeDocument)
            {
                MessageBox.Show("Please select a template document.");
                return;
            }

            TextTemplatingEngineHost host = new TextTemplatingEngineHost();
            Engine engine = new Engine();

            host.Session = new TextTemplatingSession();
            host.TemplateFileValue = doc.DocPath == null ? string.Empty : doc.DocPath.ToString();

            TextEditor tbox = (TextEditor)doc.Content;
            string inputTemplate = tbox == null ?string.Empty:tbox.Text;//.Replace("System.Windows.Controls.TextBox: ",string.Empty);

            List<CustomProperty> cpList = doc.GetParamters();

            foreach (CustomProperty cp in cpList)
            {
                Parameter par = new Parameter() { Text = cp.Name.Trim(), Value = cp.Value.ToString() };
                host.Session.Add(cp.Name.Trim(), par);
            }

            string strCodes = engine.ProcessTemplate(inputTemplate, host);
            string docExt = "." + GetOutputExtension(inputTemplate);
            CustomProperty tncp = doc.GetProperty("TableName");
            string tname = tncp == null ? string.Empty : tncp.Value.ToString();
            string dtitle = doc.Title.Substring(4, doc.Title.Length - 7);
            ucBaseDocument codedoc = new ucBaseDocument()
            {
                Title = tname + dtitle == string.Empty ? string.Format("Doc_{0}", DateTime.Now.ToString("yyyyMMdd")) : tname + dtitle + docExt,

                Content = new TextEditor()
                {
                    IsReadOnly = false,
                    SyntaxHighlighting = HighlightingManager.Instance.GetDefinition( ToolHelper.GetHighlightingByExtension( GetOutputExtension(inputTemplate).Trim())) ,
                    Text = strCodes
                }
            };
            codedoc.DocType = DocumentType.CodeDocument;
            codedoc.DocChanged = false;
            codedoc.DocExt = docExt; 
            this.DocumentHost.Items.Add(codedoc);
            codedoc.Activate();
            codedoc.LoadProperties(this.lucDocumentProperties);
        }        


        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputTemplate"></param>
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
        

        private void btnBatchGenerateCode_Click(object sender, RoutedEventArgs e)
        {
            if (lucServerExplorer.connector != null)
            {
               // if (batchGenerator == null)
                batchGenerator = new winBatchGenerator(lucServerExplorer.connector.dbHelper, lucServerExplorer.connector.ServerName); 
                batchGenerator.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please Connect With A Database Server in Server Explorer.");
            }

        }

        private void btnExceltoScript_Click(object sender, RoutedEventArgs e)
        {
            excelToScriptor = new winExcelToScript(this);
            excelToScriptor.ShowDialog();
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public void CreateDoc(ucBaseDocument doc)
        {
            doc.DocChanged = false;
            this.DocumentHost.Items.Add(doc);
            doc.Activate();
            doc.LoadProperties(this.lucDocumentProperties);
        }


    }
 
}
