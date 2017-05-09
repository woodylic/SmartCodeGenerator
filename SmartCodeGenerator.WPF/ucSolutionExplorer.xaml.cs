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
using System.IO;
using System.Text.RegularExpressions;
using SmartCodeGenerator.Uility;

namespace SmartCodeGenerator
{
    /// <summary>
    /// Interaction logic for ucSolutionExplorer.xaml
    /// </summary>
    public partial class ucSolutionExplorer : DockableContent
    {
        /// <summary>
        /// 
        /// </summary>
        public ucSolutionExplorer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public PropertyNodeItem CurSelectedNode;
        public PropertyNodeItem CurProject;
        public PropertyNodeItem RootNode;
        /// <summary>
        /// 
        /// </summary>
        public event ViewCodeHander SelectFileNode;
        /// <summary>
        /// ss
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvObjects_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (tvObjects.SelectedItem != null)
            {
                CurSelectedNode = (PropertyNodeItem)tvObjects.SelectedItem;
                this.CurProject = GetProjectNode(this.CurSelectedNode);
                if (SelectFileNode != null && File.Exists(CurSelectedNode.Value) &&
                    this.CurProject != null && this.CurProject.Value != CurSelectedNode.Value
                    && CurSelectedNode.Parent != null)
                {
                    SelectFileNode(CurSelectedNode);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pni"></param>
        /// <returns></returns>
        private PropertyNodeItem GetProjectNode(PropertyNodeItem pni)
        {
            if (pni.Parent == null && pni.Value.IndexOf("proj")>0)
            {
                return pni;
            }
            if (pni.Parent == null && pni.Value.IndexOf("proj") < 0)
            {
                return null;
            }
            if (pni.Parent.Parent == null && pni.Value.IndexOf("proj") > 0)
            {
                return pni;
            }
            else
            {
                return GetProjectNode(pni.Parent);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private List<PropertyNodeItem> objectTree;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectPath"></param>
        public void LoadFiles(string projectPath)
        {
            TreeViewTools.SelectObject(this.tvObjects, this.RootNode);
            objectTree = new List<PropertyNodeItem>();           
            if (File.Exists(projectPath))
            {
                FileInfo info = new FileInfo(projectPath);
                if (info.Extension.ToLower() == ".sln")
                {
                    LoadSolution(projectPath, info);
                }
                if (info.Extension.Contains("proj"))
                {
                    FileInfo pinfo = new FileInfo(projectPath);
                    PropertyNodeItem prj = new PropertyNodeItem();
                    prj.DisplayName = pinfo.Name.Replace(pinfo.Extension, string.Empty);
                    prj.Value = projectPath;
                    prj.Icon = @"images/Proj.png";
                    objectTree.Add(prj);
                    RootNode = prj;
                    LoadProjs(projectPath, prj); 
                }               
                this.tvObjects.ItemsSource = objectTree; 
            }
        }

        /// <summary>
        /// Refresh
        /// </summary>
        public void Refresh()
        {
            LoadFiles(RootNode.Value); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="info"></param>
        private void LoadSolution(string filePath, FileInfo info)
        { 
            PropertyNodeItem sln = new PropertyNodeItem();
            sln.DisplayName = info.Name.Replace(info.Extension, string.Empty);
            sln.Value = info.FullName;
            sln.Icon = @"images/Solution.png";
            objectTree.Add(sln);
            RootNode = sln;

            System.Text.RegularExpressions.MatchCollection ma = System.Text.RegularExpressions.Regex.Matches(File.ReadAllText(filePath), @"(\}""\) = "").*("", ""\{)", RegexOptions.Multiline);            
            List<string> pnList = new List<string>();
            foreach (Match item in ma)
            {
                if (item.Success)
                {
                    string tmp = item.Value.Substring(7).Replace(@"""", "");
                    string[] strList = tmp.Split(',');
                    if (strList != null && strList.Length==3)
                    {

                        string projPath = GetRealPath(info.Directory.FullName, strList[1].Trim());    //info.Directory + @"\" + strList[1];
                        if (File.Exists(projPath))
                        {
                            FileInfo pinfo = new FileInfo(projPath);
                            PropertyNodeItem prj = new PropertyNodeItem();
                            prj.DisplayName = strList[0].Trim();
                            prj.Value = projPath;
                            prj.Icon = @"images/Proj.png";
                            prj.Parent = sln;
                            sln.Children.Add(prj);
                            LoadProjs(projPath, prj);
                        }
                    }             
                }
            }           
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        private void LoadProjs(string filePath, PropertyNodeItem proj)
        {
            FileInfo finfo = new FileInfo(filePath);

            System.Text.RegularExpressions.MatchCollection ma = System.Text.RegularExpressions.Regex.Matches(File.ReadAllText(filePath), @"(\<Compile Include="").*("" /\>)", RegexOptions.Multiline);
            foreach (Match item in ma)
            {
                if (item.Success)
                {
                    int length = item.Value.Length;
                    string tmp = item.Value.Substring(18, length - 22);
                    string fPath = finfo.Directory.FullName + @"\" + tmp;
                    int fIndex = tmp.IndexOf(@"\");
                    if (fIndex > 0)
                    {
                        string fp = finfo.Directory.FullName + @"\" + tmp.Substring(0, fIndex);
                        if (Directory.Exists(fp))
                        {
                            DirectoryInfo dinfo = new DirectoryInfo(fp);
                            PropertyNodeItem d = new PropertyNodeItem();
                            d.DisplayName = tmp.Substring(0, fIndex);
                            d.Value = fp;
                            d.Icon = @"images/Folder.png";
                            d.Parent = proj;
                            proj.Children.Add(d);
                            LoadFolder(fp, d);
                        }
                    }
                    else
                    {
                        if (File.Exists(fPath))
                        {
                            FileInfo pinfo = new FileInfo(fPath);
                            PropertyNodeItem f = new PropertyNodeItem();
                            f.DisplayName = tmp;
                            f.Value = fPath;
                            f.Icon = @"images/code.png";
                            f.Parent = proj;
                            proj.Children.Add(f);
                        }
                    }

                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="Last"></param>
        private void LoadFolder(string folderPath, PropertyNodeItem Last)
        {
            DirectoryInfo dinfo = new DirectoryInfo(folderPath);

            DirectoryInfo[] dList = dinfo.GetDirectories();
            if (dList != null)
            {
                foreach (DirectoryInfo di in dList)
                {
                    PropertyNodeItem d = new PropertyNodeItem();
                    d.DisplayName = di.Name;
                    d.Value = di.FullName;
                    d.Icon = @"images/Folder.png";
                    d.Parent = Last;
                    Last.Children.Add(d);
                    LoadFolder(di.FullName, d);
                }
            }

            FileInfo[] fList = dinfo.GetFiles();
            if (fList != null)
            {
                foreach (FileInfo fi in fList)
                { 
                    PropertyNodeItem f = new PropertyNodeItem();
                    f.DisplayName = fi.Name;
                    f.Value = fi.FullName;
                    f.Icon = @"images/code.png";
                    f.Parent = Last;
                    Last.Children.Add(f);
                }
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

        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    public delegate void ViewCodeHander(PropertyNodeItem pni);
}
