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
using SmartCodeGenerator.Uility;

namespace SmartCodeGenerator
{    
    /// <summary>
    /// Interaction logic for ucTemplatesExplorer.xaml
    /// </summary>
    public partial class ucTemplatesExplorer : DockableContent
    {
        /// <summary>
        /// 
        /// </summary>
        public System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        /// <summary>
        /// 
        /// </summary>
        public ucTemplatesExplorer()
        {
            InitializeComponent();
            LoadTemplates();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadTemplate_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog dlg = new OpenFileDialog();
            if (folderBrowserDialog == null)
            {
                folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
                folderBrowserDialog.SelectedPath = AppDomain.CurrentDomain.BaseDirectory;
            }            
            folderBrowserDialog.Description = "Select an folder, and load all templates below it.";
            System.Windows.Forms.DialogResult dr = folderBrowserDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                List<PropertyNodeItem> itemList = new List<PropertyNodeItem>();
                if (Directory.Exists(folderBrowserDialog.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(folderBrowserDialog.SelectedPath);
                    PropertyNodeItem rootn = new PropertyNodeItem(); 
                    rootn.DisplayName = di.Name;
                    rootn.Value = folderBrowserDialog.SelectedPath;
                   // this.tvTemplates.GetItemFromObject(rootn).IsExpanded = true;
                    GetFolders(folderBrowserDialog.SelectedPath, rootn);
                    itemList.Add(rootn);
                }

                this.tvTemplates.ItemsSource = itemList;                
            }
            //MessageBox.Show(dialog.SelectedPath);
        }

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
                    i.Children.Add(subn);
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
        public PropertyNodeItem CurSelectedNode;
        /// <summary>
        /// 
        /// </summary>
        public event TemplateHander SelectTemplateNode;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvTemplates_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            CurSelectedNode = (PropertyNodeItem)tvTemplates.SelectedItem;
            if (SelectTemplateNode != null && File.Exists(CurSelectedNode.Value))
            {
                SelectTemplateNode(CurSelectedNode.Value, CurSelectedNode.DisplayName);
            }
        }

    }
    /// <summary>
    /// 
    /// </summary>
    public class PropertyNodeItem 
    {
        public string Icon { get; set; }
        public string DisplayName { get; set; }
        public string Value { get; set; }
        public PropertyNodeItem Parent { get; set; }
        public List<PropertyNodeItem> Children { get; set; }
        public PropertyNodeItem()
        {
            Children = new List<PropertyNodeItem>();
        } 
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    public delegate void TemplateHander(string filePath,string fileName);
}
