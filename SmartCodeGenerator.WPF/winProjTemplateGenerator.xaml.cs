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
using ICSharpCode.SharpZipLib; 
using SmartCodeGenerator.Uility;
using System.Diagnostics;

namespace SmartCodeGenerator
{
    /// <summary>
    /// Interaction logic for winProjTemplateGenerator.xaml
    /// </summary>
    public partial class winProjTemplateGenerator : Window
    {
        /// <summary>
        /// 
        /// </summary>
        public System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
        /// <summary>
        /// 
        /// </summary>
        public winProjTemplateGenerator()
        {
            InitializeComponent();
            LoadTemplates();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvTemplates_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.tvTemplates.SelectedItem != null)
            {
                PropertyNodeItem sn = (PropertyNodeItem)this.tvTemplates.SelectedItem;
                ZipHelper zip = new ZipHelper();
                zip.FileNameZipped = sn.Value;
                folderBrowserDialog.ShowDialog();
                if (!string.IsNullOrEmpty(folderBrowserDialog.SelectedPath))
                {
                    zip.UnZipFile(sn.Value, folderBrowserDialog.SelectedPath, string.Empty);
                    Process.Start("explorer.exe", folderBrowserDialog.SelectedPath);
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Please select a template.");
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void LoadTemplates()
        {
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\SolutionTemplates\"))
            {
                List<PropertyNodeItem> itemList = new List<PropertyNodeItem>();
                string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\SolutionTemplates\");
                
                foreach(string f in files)
                {
                    FileInfo fi = new FileInfo(f);
                    if(fi.Extension.ToLower()  == ".zip")
                    {
                        PropertyNodeItem pn = new PropertyNodeItem();
                        pn.DisplayName = fi.Name;
                        pn.Value = fi.FullName;
                        pn.Icon = @"Images\SolutionTemplate.png";
                        itemList.Add(pn);
                    }
                }
                this.tvTemplates.ItemsSource = itemList; 
            }
        }
    }
}
